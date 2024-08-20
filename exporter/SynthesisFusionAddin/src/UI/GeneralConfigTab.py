import adsk.core
import adsk.fusion

from src.Logging import logFailure
from src.Parser.ExporterOptions import ExporterOptions, ExportLocation, ExportMode
from src.Types import KG, UnitSystem
from src.UI.CreateCommandInputsHelper import createBooleanInput
from src.UI.GamepieceConfigTab import GamepieceConfigTab
from src.UI.JointConfigTab import JointConfigTab
from src.Util import (
    convertMassUnitsFrom,
    convertMassUnitsTo,
    designMassCalculation,
    getFusionUnitSystem,
)


class GeneralConfigTab:
    generalOptionsTab: adsk.core.TabCommandInput
    previousAutoCalcWeightCheckboxState: bool
    previousFrictionOverrideCheckboxState: bool
    previousSelectedUnitDropdownIndex: int
    previousSelectedModeDropdownIndex: int
    jointConfigTab: JointConfigTab
    gamepieceConfigTab: GamepieceConfigTab

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs, exporterOptions: ExporterOptions) -> None:
        inputs = args.command.commandInputs
        self.generalOptionsTab = inputs.addTabCommandInput("generalSettings", "General Settings")
        self.generalOptionsTab.tooltip = "General configuration options for your robot export."
        generalTabInputs = self.generalOptionsTab.children

        dropdownExportMode = generalTabInputs.addDropDownCommandInput(
            "exportModeDropdown",
            "Export Mode",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )

        dynamic = exporterOptions.exportMode == ExportMode.ROBOT
        dropdownExportMode.listItems.add("Dynamic", dynamic)
        dropdownExportMode.listItems.add("Static", not dynamic)
        dropdownExportMode.tooltip = "Export Mode"
        dropdownExportMode.tooltipDescription = "<hr>Does this object move dynamically?"
        self.previousSelectedModeDropdownIndex = int(not dynamic)

        dropdownExportLocation = generalTabInputs.addDropDownCommandInput(
            "exportLocation", "Export Location", dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
        )

        upload: bool = exporterOptions.exportLocation == ExportLocation.UPLOAD
        dropdownExportLocation.listItems.add("Upload", upload)
        dropdownExportLocation.listItems.add("Download", not upload)
        dropdownExportLocation.tooltip = "Export Location"
        dropdownExportLocation.tooltipDescription = (
            "<hr>Do you want to upload this mirabuf file to APS, or download it to your local machine?"
        )

        autoCalcWeightButton = createBooleanInput(
            "autoCalcWeightButton",
            "Auto Calculate Robot Weight",
            generalTabInputs,
            checked=exporterOptions.autoCalcRobotWeight,
            tooltip="Approximate the weight of your robot assembly.",
        )
        self.previousAutoCalcWeightCheckboxState = exporterOptions.autoCalcRobotWeight

        displayWeight = convertMassUnitsFrom(exporterOptions.robotWeight)

        fusUnitSystem = getFusionUnitSystem()
        weightInput = generalTabInputs.addValueInput(
            "weightInput",
            f"Weight {'(lbs)' if fusUnitSystem is UnitSystem.IMPERIAL else '(kg)'}",
            "",
            adsk.core.ValueInput.createByReal(displayWeight),
        )

        createBooleanInput(
            "compressOutputButton",
            "Compress Output",
            generalTabInputs,
            checked=exporterOptions.compressOutput,
            tooltip="Compress the output file for a smaller file size.",
            tooltipadvanced="<hr>Use the GZIP compression system to compress the resulting file, "
            "perfect if you want to share your robot design around.<br>",
            enabled=True,
        )

        exportAsPartButton = createBooleanInput(
            "exportAsPartButton",
            "Export As Part",
            generalTabInputs,
            checked=exporterOptions.exportAsPart,
            tooltip="Use to export as a part for Mix And Match",
            enabled=True,
        )

        frictionOverrideButton = createBooleanInput(
            "frictionOverride",
            "Friction Override",
            generalTabInputs,
            checked=exporterOptions.frictionOverride,
            tooltip="Manually override the default friction values on the bodies in the assembly.",
        )
        self.previousFrictionOverrideCheckboxState = exporterOptions.frictionOverride

        valueList = [1] + [i / 20 for i in range(20)]
        frictionCoefficient = generalTabInputs.addFloatSliderListCommandInput("frictionCoefficient", "", "", valueList)
        frictionCoefficient.valueOne = exporterOptions.frictionOverrideCoeff
        frictionCoefficient.tooltip = "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        frictionCoefficient.isVisible = exporterOptions.frictionOverride

        if exporterOptions.exportMode == ExportMode.FIELD:
            autoCalcWeightButton.isVisible = False
            exportAsPartButton.isVisible = False
            weightInput.isVisible = False
            frictionOverrideButton.isVisible = frictionCoefficient.isVisible = False

    @property
    def exportMode(self) -> ExportMode:
        exportModeDropdown: adsk.core.DropDownCommandInput = self.generalOptionsTab.children.itemById(
            "exportModeDropdown"
        )
        if exportModeDropdown.selectedItem.index == 0:
            return ExportMode.ROBOT
        else:
            assert exportModeDropdown.selectedItem.index == 1
            return ExportMode.FIELD

    @property
    def compress(self) -> bool:
        compressButton: adsk.core.BoolValueCommandInput = self.generalOptionsTab.children.itemById(
            "compressOutputButton"
        )
        return compressButton.value

    @property
    def exportAsPart(self) -> bool:
        exportAsPartButton: adsk.core.BoolValueCommandInput = self.generalOptionsTab.children.itemById(
            "exportAsPartButton"
        )
        return exportAsPartButton.value

    @property
    def robotWeight(self) -> KG:
        weightInput: adsk.core.ValueCommandInput = self.generalOptionsTab.children.itemById("weightInput")
        return convertMassUnitsTo(weightInput.value)

    @property
    def autoCalculateWeight(self) -> bool:
        autoCalcWeightButton: adsk.core.BoolValueCommandInput = self.generalOptionsTab.children.itemById(
            "autoCalcWeightButton"
        )
        return autoCalcWeightButton.value

    @property
    def exportLocation(self) -> ExportLocation:
        exportLocationDropdown: adsk.core.DropDownCommandInput = self.generalOptionsTab.children.itemById(
            "exportLocation"
        )
        if exportLocationDropdown.selectedItem.index == 0:
            return ExportLocation.UPLOAD
        else:
            assert exportLocationDropdown.selectedItem.index == 1
            return ExportLocation.DOWNLOAD

    @property
    def overrideFriction(self) -> bool:
        overrideFrictionButton: adsk.core.BoolValueCommandInput = self.generalOptionsTab.children.itemById(
            "frictionOverride"
        )
        return overrideFrictionButton.value

    @property
    def frictionOverrideCoeff(self) -> float:
        frictionSlider: adsk.core.FloatSliderCommandInput = self.generalOptionsTab.children.itemById(
            "frictionCoefficient"
        )
        return frictionSlider.valueOne

    @logFailure
    def handleInputChanged(self, args: adsk.core.InputChangedEventArgs) -> None:
        commandInput = args.input
        if commandInput.id == "exportModeDropdown":
            modeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
            autoCalcWeightButton: adsk.core.BoolValueCommandInput = args.inputs.itemById("autoCalcWeightButton")
            weightInput: adsk.core.TableCommandInput = args.inputs.itemById("weightInput")
            exportAsPartButton: adsk.core.BoolValueCommandInput = args.inputs.itemById("exportAsPartButton")
            overrideFrictionButton: adsk.core.BoolValueCommandInput = args.inputs.itemById("frictionOverride")
            frictionSlider: adsk.core.FloatSliderCommandInput = args.inputs.itemById("frictionCoefficient")
            if modeDropdown.selectedItem.index == self.previousSelectedModeDropdownIndex:
                return

            if modeDropdown.selectedItem.index == 0:
                self.jointConfigTab.isVisible = True
                self.gamepieceConfigTab.isVisible = False

                autoCalcWeightButton.isVisible = True
                weightInput.isVisible = True
                exportAsPartButton.isVisible = True
                overrideFrictionButton.isVisible = True
                frictionSlider.isVisible = overrideFrictionButton.value
            else:
                assert modeDropdown.selectedItem.index == 1
                self.jointConfigTab.isVisible = False
                self.gamepieceConfigTab.isVisible = True

                autoCalcWeightButton.isVisible = False
                weightInput.isVisible = False
                exportAsPartButton.isVisible = False
                overrideFrictionButton.isVisible = frictionSlider.isVisible = False

            self.previousSelectedModeDropdownIndex = modeDropdown.selectedItem.index

        elif commandInput.id == "autoCalcWeightButton":
            autoCalcWeightButton = adsk.core.BoolValueCommandInput.cast(commandInput)
            if autoCalcWeightButton.value == self.previousAutoCalcWeightCheckboxState:
                return

            weightInput: adsk.core.ValueCommandInput = args.inputs.itemById("weightInput")

            if autoCalcWeightButton.value:
                weightInput.value = designMassCalculation()
                weightInput.isEnabled = False
            else:
                weightInput.isEnabled = True

            self.previousAutoCalcWeightCheckboxState = autoCalcWeightButton.value

        elif commandInput.id == "frictionOverride":
            frictionOverrideButton = adsk.core.BoolValueCommandInput.cast(commandInput)
            frictionSlider: adsk.core.FloatSliderCommandInput = args.inputs.itemById("frictionCoefficient")
            if frictionOverrideButton.value == self.previousFrictionOverrideCheckboxState:
                return

            frictionSlider.isVisible = frictionOverrideButton.value
            self.previousFrictionOverrideCheckboxState = frictionOverrideButton.value
