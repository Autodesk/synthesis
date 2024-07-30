import adsk.core
import adsk.fusion

from ..Logging import logFailure
from ..Parser.ExporterOptions import (
    ExporterOptions,
    ExportLocation,
    ExportMode,
    PreferredUnits,
)
from ..Types import KG, toKg, toLbs
from . import IconPaths
from .CreateCommandInputsHelper import createBooleanInput, createTableInput
from .GamepieceConfigTab import GamepieceConfigTab
from .JointConfigTab import JointConfigTab


class GeneralConfigTab:
    generalOptionsTab: adsk.core.TabCommandInput
    previousAutoCalcWeightCheckboxState: bool
    previousFrictionOverrideCheckboxState: bool
    previousSelectedUnitDropdownIndex: int
    previousSelectedModeDropdownIndex: int
    currentUnits: PreferredUnits
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

        weightTableInput = createTableInput(
            "weightTable",
            "Weight Table",
            generalTabInputs,
            4,
            "2:1:1",
            1,
        )
        weightTableInput.tablePresentationStyle = 2  # Transparent background

        weightName = generalTabInputs.addStringValueInput("weightName", "Weight")
        weightName.value = "Weight"
        weightName.isReadOnly = True

        autoCalcWeightButton = createBooleanInput(
            "autoCalcWeightButton",
            "Auto Calculate Robot Weight",
            generalTabInputs,
            checked=exporterOptions.autoCalcRobotWeight,
            tooltip="Approximate the weight of your robot assembly.",
        )
        self.previousAutoCalcWeightCheckboxState = exporterOptions.autoCalcRobotWeight

        self.currentUnits = exporterOptions.preferredUnits
        imperialUnits = self.currentUnits == PreferredUnits.IMPERIAL
        if imperialUnits:
            # ExporterOptions always contains the metric value
            displayWeight = toLbs(exporterOptions.robotWeight)
        else:
            displayWeight = exporterOptions.robotWeight

        weightInput = generalTabInputs.addValueInput(
            "weightInput",
            "Weight Input",
            "",
            adsk.core.ValueInput.createByReal(displayWeight),
        )
        weightInput.tooltip = "Robot weight"
        weightInput.tooltipDescription = (
            f"<tt>(in {'pounds' if self.currentUnits == PreferredUnits.IMPERIAL else 'kilograms'})"
            "</tt><hr>This is the weight of the entire robot assembly."
        )
        weightInput.isEnabled = not exporterOptions.autoCalcRobotWeight

        weightUnitDropdown = generalTabInputs.addDropDownCommandInput(
            "weightUnitDropdown",
            "Weight Unit",
            adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )

        # Invisible white space characters are required in the list item name field to make this work.
        # I have no idea why, Fusion API needs some special education help - Brandon
        weightUnitDropdown.listItems.add("‎", imperialUnits, IconPaths.massIcons["LBS"])
        weightUnitDropdown.listItems.add("‎", not imperialUnits, IconPaths.massIcons["KG"])
        weightUnitDropdown.tooltip = "Unit of Mass"
        weightUnitDropdown.tooltipDescription = "<hr>Configure the unit of mass for the weight calculation."
        self.previousSelectedUnitDropdownIndex = int(not imperialUnits)

        weightTableInput.addCommandInput(weightName, 0, 0)
        weightTableInput.addCommandInput(weightInput, 0, 1)
        weightTableInput.addCommandInput(weightUnitDropdown, 0, 2)

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
            weightInput.isVisible = weightTableInput.isVisible = False
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
    def selectedUnits(self) -> PreferredUnits:
        return self.currentUnits

    @property
    def robotWeight(self) -> KG:
        weightInput: adsk.core.ValueCommandInput = self.generalOptionsTab.children.itemById(
            "weightTable"
        ).getInputAtPosition(0, 1)
        if self.currentUnits == PreferredUnits.METRIC:
            return KG(weightInput.value)
        else:
            assert self.currentUnits == PreferredUnits.IMPERIAL
            return toKg(weightInput.value)

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
            weightTable: adsk.core.TableCommandInput = args.inputs.itemById("weightTable")
            exportAsPartButton: adsk.core.BoolValueCommandInput = args.inputs.itemById("exportAsPartButton")
            overrideFrictionButton: adsk.core.BoolValueCommandInput = args.inputs.itemById("frictionOverride")
            frictionSlider: adsk.core.FloatSliderCommandInput = args.inputs.itemById("frictionCoefficient")
            if modeDropdown.selectedItem.index == self.previousSelectedModeDropdownIndex:
                return

            if modeDropdown.selectedItem.index == 0:
                self.jointConfigTab.isVisible = True
                self.gamepieceConfigTab.isVisible = False

                autoCalcWeightButton.isVisible = True
                weightTable.isVisible = True
                exportAsPartButton.isVisible = True
                overrideFrictionButton.isVisible = True
                frictionSlider.isVisible = overrideFrictionButton.value
            else:
                assert modeDropdown.selectedItem.index == 1
                self.jointConfigTab.isVisible = False
                self.gamepieceConfigTab.isVisible = True

                autoCalcWeightButton.isVisible = False
                weightTable.isVisible = False
                exportAsPartButton.isVisible = False
                overrideFrictionButton.isVisible = frictionSlider.isVisible = False

            self.previousSelectedModeDropdownIndex = modeDropdown.selectedItem.index

        elif commandInput.id == "weightUnitDropdown":
            weightUnitDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
            weightTable: adsk.core.TableCommandInput = args.inputs.itemById("weightTable")
            weightInput: adsk.core.ValueCommandInput = weightTable.getInputAtPosition(0, 1)
            if weightUnitDropdown.selectedItem.index == self.previousSelectedUnitDropdownIndex:
                return

            if weightUnitDropdown.selectedItem.index == 0:
                self.currentUnits = PreferredUnits.IMPERIAL
                weightInput.value = toLbs(weightInput.value)
                weightInput.tooltipDescription = (
                    "<tt>(in pounds)</tt><hr>This is the weight of the entire robot assembly."
                )
            else:
                assert weightUnitDropdown.selectedItem.index == 1
                self.currentUnits = PreferredUnits.METRIC
                weightInput.value = toKg(weightInput.value)
                weightInput.tooltipDescription = (
                    "<tt>(in kilograms)</tt><hr>This is the weight of the entire robot assembly."
                )

            self.previousSelectedUnitDropdownIndex = weightUnitDropdown.selectedItem.index

        elif commandInput.id == "autoCalcWeightButton":
            autoCalcWeightButton = adsk.core.BoolValueCommandInput.cast(commandInput)
            if autoCalcWeightButton.value == self.previousAutoCalcWeightCheckboxState:
                return

            weightTable: adsk.core.TableCommandInput = args.inputs.itemById("weightTable")
            weightInput: adsk.core.ValueCommandInput = weightTable.getInputAtPosition(0, 1)

            if autoCalcWeightButton.value:
                robotMass = designMassCalculation()
                weightInput.value = robotMass if self.currentUnits is PreferredUnits.METRIC else toLbs(robotMass)
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


# TODO: Perhaps move this into a different module
@logFailure
def designMassCalculation() -> KG:
    app = adsk.core.Application.get()
    mass = 0.0
    for body in [x for x in app.activeDocument.design.rootComponent.bRepBodies if x.isLightBulbOn]:
        physical = body.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
        mass += physical.mass

    for occ in [x for x in app.activeDocument.design.rootComponent.allOccurrences if x.isLightBulbOn]:
        for body in [x for x in occ.component.bRepBodies if x.isLightBulbOn]:
            physical = body.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
            mass += physical.mass

    return KG(round(mass, 2))
