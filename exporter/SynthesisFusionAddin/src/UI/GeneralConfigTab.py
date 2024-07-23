import logging
import traceback

import adsk.core
import adsk.fusion

from ..general_imports import INTERNAL_ID
from ..Parser.ExporterOptions import ExporterOptions, ExportMode, PreferredUnits
from ..TypesTmp import KG, toKg, toLbs
from . import IconPaths
from .CreateCommandInputsHelper import createBooleanInput, createTableInput
from .GamepieceConfigTab import GamepieceConfigTab
from .JointConfigTab import JointConfigTab


class GeneralConfigTab:
    generalOptionsTab: adsk.core.TabCommandInput
    previousAutoCalcWeightCheckboxState: bool
    previousSelectedUnitDropdownIndex: int
    previousSelectedModeDropdownIndex: int
    currentUnits: PreferredUnits
    jointConfigTab: JointConfigTab
    gamepieceConfigTab: GamepieceConfigTab

    def __init__(self, args: adsk.core.CommandCreatedEventArgs, exporterOptions: ExporterOptions) -> None:
        try:
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

            createBooleanInput(
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
                displayWeight = exporterOptions.robotWeight * 2.2046226218
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

            createBooleanInput(
                "exportAsPartButton",
                "Export As Part",
                generalTabInputs,
                checked=exporterOptions.exportAsPart,
                tooltip="Use to export as a part for Mix And Match",
                enabled=True,
            )

        except BaseException:
            logging.getLogger("{INTERNAL_ID}.UI.GeneralConfigTab").error("Failed:\n{}".format(traceback.format_exc()))

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

    def handleInputChanged(self, args: adsk.core.InputChangedEventArgs) -> None:
        try:
            commandInput = args.input
            if commandInput.id == "exportModeDropdown":
                modeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
                if modeDropdown.selectedItem.index == self.previousSelectedModeDropdownIndex:
                    return

                if modeDropdown.selectedItem.index == 0:
                    self.jointConfigTab.isVisible = True
                    self.gamepieceConfigTab.isVisible = False
                else:
                    assert modeDropdown.selectedItem.index == 1
                    self.jointConfigTab.isVisible = False
                    self.gamepieceConfigTab.isVisible = True

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

        except BaseException:
            logging.getLogger(f"{INTERNAL_ID}.UI.GeneralConfigTab").error("Failed:\n{}".format(traceback.format_exc()))


# TODO: GH-1010 for failure logging with message box
# TODO: Perhaps move this into a different module
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
