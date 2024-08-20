import adsk.core
import adsk.fusion

from src.Logging import logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Types import Gamepiece, UnitSystem
from src.UI.CreateCommandInputsHelper import (
    createBooleanInput,
    createTableInput,
    createTextBoxInput,
)
from src.Util import convertMassUnitsFrom, convertMassUnitsTo, getFusionUnitSystem


class GamepieceConfigTab:
    selectedGamepieceList: list[adsk.fusion.Occurrence] = []
    selectedGamepieceEntityIDs: set[str] = set()
    gamepieceConfigTab: adsk.core.TabCommandInput
    gamepieceTable: adsk.core.TableCommandInput
    previousAutoCalcWeightCheckboxState: bool
    previousSelectedUnitDropdownIndex: int

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs, exporterOptions: ExporterOptions) -> None:
        inputs = args.command.commandInputs
        self.gamepieceConfigTab = inputs.addTabCommandInput("gamepieceSettings", "Gamepiece Settings")
        self.gamepieceConfigTab.tooltip = "Field gamepiece configuration options."
        gamepieceTabInputs = self.gamepieceConfigTab.children

        createBooleanInput(
            "autoCalcGamepieceWeight",
            "Auto Calculate Gamepiece Weight",
            gamepieceTabInputs,
            checked=exporterOptions.autoCalcGamepieceWeight,
            tooltip="Approximate the weight of each selected gamepiece.",
        )
        self.previousAutoCalcWeightCheckboxState = exporterOptions.autoCalcGamepieceWeight

        self.gamepieceTable = createTableInput(
            "gamepieceTable",
            "Gamepiece",
            gamepieceTabInputs,
            4,
            "8:5:12",
        )

        self.gamepieceTable.addCommandInput(
            createTextBoxInput("gamepieceNameHeader", "Name", gamepieceTabInputs, "Name", bold=False), 0, 0
        )
        fusUnitSystem = getFusionUnitSystem()
        self.gamepieceTable.addCommandInput(
            createTextBoxInput(
                "gamepieceWeightHeader",
                "Weight",
                gamepieceTabInputs,
                f"Weight {'(lbs)' if fusUnitSystem is UnitSystem.IMPERIAL else '(kg)'}",
                bold=False,
            ),
            0,
            1,
        )
        self.gamepieceTable.addCommandInput(
            createTextBoxInput(
                "frictionHeader",
                "Gamepiece Friction Coefficient",
                gamepieceTabInputs,
                "Gamepiece Friction Coefficient",
                background="#d9d9d9",
            ),
            0,
            2,
        )
        gamepieceSelectCancelButton = gamepieceTabInputs.addBoolValueInput(
            "gamepieceSelectCancelButton", "Cancel", False
        )
        gamepieceSelectCancelButton.isEnabled = gamepieceSelectCancelButton.isVisible = False

        gamepieceAddButton = gamepieceTabInputs.addBoolValueInput("gamepieceAddButton", "Add", False)
        gamepieceRemoveButton = gamepieceTabInputs.addBoolValueInput("gamepieceRemoveButton", "Remove", False)
        gamepieceAddButton.isEnabled = gamepieceRemoveButton.isEnabled = True

        gamepieceSelect = gamepieceTabInputs.addSelectionInput(
            "gamepieceSelect", "Selection", "Select the unique gamepieces in your field."
        )
        gamepieceSelect.addSelectionFilter("Occurrences")
        gamepieceSelect.setSelectionLimits(0)
        gamepieceSelect.isEnabled = gamepieceSelect.isVisible = False  # Visibility is triggered by `gamepieceAddButton`

        self.gamepieceTable.addToolbarCommandInput(gamepieceAddButton)
        self.gamepieceTable.addToolbarCommandInput(gamepieceRemoveButton)
        self.gamepieceTable.addToolbarCommandInput(gamepieceSelectCancelButton)

        gamepieceTabInputs.addTextBoxCommandInput("gamepieceTabSpacer", "", "", 1, True)

        self.reset()

    @property
    def isVisible(self) -> bool:
        return self.gamepieceConfigTab.isVisible

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.gamepieceConfigTab.isVisible = value

    @property
    def autoCalculateWeight(self) -> bool:
        autoCalcWeightButton: adsk.core.BoolValueCommandInput = self.gamepieceConfigTab.children.itemById(
            "autoCalcGamepieceWeight"
        )
        return autoCalcWeightButton.value

    @logFailure
    def weightInputs(self) -> list[adsk.core.ValueCommandInput]:
        gamepieceWeightInputs = []
        for row in range(1, self.gamepieceTable.rowCount):  # Row is 1 indexed
            gamepieceWeightInputs.append(self.gamepieceTable.getInputAtPosition(row, 1))

        return gamepieceWeightInputs

    @logFailure
    def addGamepiece(self, gamepiece: adsk.fusion.Occurrence, synGamepiece: Gamepiece | None = None) -> bool:
        if gamepiece.entityToken in self.selectedGamepieceEntityIDs:
            return False

        def addChildOccurrences(childOccurrences: adsk.fusion.OccurrenceList) -> None:
            for occ in childOccurrences:
                self.selectedGamepieceEntityIDs.add(occ.entityToken)

                if occ.childOccurrences:
                    addChildOccurrences(occ.childOccurrences)

        if gamepiece.childOccurrences:
            addChildOccurrences(gamepiece.childOccurrences)
        else:
            self.selectedGamepieceEntityIDs.add(gamepiece.entityToken)

        self.selectedGamepieceList.append(gamepiece)

        commandInputs = self.gamepieceTable.commandInputs
        gamepieceName = commandInputs.addTextBoxCommandInput(
            "gamepieceName", "Occurrence Name", gamepiece.name, 1, True
        )
        gamepieceName.tooltip = gamepiece.name

        valueList = [1] + [i / 20 for i in range(20)]
        frictionCoefficient = commandInputs.addFloatSliderListCommandInput(
            "gamepieceFrictionCoefficient", "", "", valueList
        )
        frictionCoefficient.tooltip = "Friction coefficient of field element"
        frictionCoefficient.tooltipDescription = "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        if synGamepiece:
            frictionCoefficient.valueOne = synGamepiece.friction
        else:
            frictionCoefficient.valueOne = 0.5

        physical = gamepiece.component.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
        gamepieceMass = round(convertMassUnitsFrom(physical.mass), 2)
        weight = commandInputs.addValueInput(
            "gamepieceWeight", "Weight Input", "", adsk.core.ValueInput.createByString(str(gamepieceMass))
        )
        weight.tooltip = "Weight of field element"
        weight.isEnabled = not self.previousAutoCalcWeightCheckboxState

        row = self.gamepieceTable.rowCount
        self.gamepieceTable.addCommandInput(gamepieceName, row, 0)
        self.gamepieceTable.addCommandInput(weight, row, 1)
        self.gamepieceTable.addCommandInput(frictionCoefficient, row, 2)

        return True

    @logFailure
    def removeIndexedGamepiece(self, index: int) -> None:
        self.removeGamepiece(self.selectedGamepieceList[index])

    @logFailure
    def removeGamepiece(self, gamepiece: adsk.fusion.Occurrence) -> None:
        def removeChildOccurrences(childOccurrences: adsk.fusion.OccurrenceList) -> None:
            for occ in childOccurrences:
                self.selectedGamepieceEntityIDs.remove(occ.entityToken)

                if occ.childOccurrences:
                    removeChildOccurrences(occ.childOccurrences)

        if gamepiece.childOccurrences:
            removeChildOccurrences(gamepiece.childOccurrences)
        else:
            self.selectedGamepieceEntityIDs.remove(gamepiece.entityToken)

        i = self.selectedGamepieceList.index(gamepiece)
        self.selectedGamepieceList.remove(gamepiece)
        self.gamepieceTable.deleteRow(i + 1)  # Row is 1 indexed

    @logFailure
    def getGamepieces(self) -> list[Gamepiece]:
        gamepieces: list[Gamepiece] = []
        for row in range(1, self.gamepieceTable.rowCount):  # Row is 1 indexed
            gamepieceEntityToken = self.selectedGamepieceList[row - 1].entityToken
            gamepieceWeight = convertMassUnitsTo(self.gamepieceTable.getInputAtPosition(row, 1).value)
            gamepieceFrictionCoefficient = self.gamepieceTable.getInputAtPosition(row, 2).valueOne
            gamepieces.append(Gamepiece(gamepieceEntityToken, gamepieceWeight, gamepieceFrictionCoefficient))

        return gamepieces

    def reset(self) -> None:
        self.selectedGamepieceEntityIDs.clear()
        self.selectedGamepieceList.clear()

    @logFailure
    def calcGamepieceWeights(self) -> None:
        for row in range(1, self.gamepieceTable.rowCount):  # Row is 1 indexed
            weightInput: adsk.core.ValueCommandInput = self.gamepieceTable.getInputAtPosition(row, 1)
            physical = self.selectedGamepieceList[row - 1].component.getPhysicalProperties(
                adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
            )
            weightInput.value = round(convertMassUnitsFrom(physical.mass), 2)

    @logFailure
    def handleInputChanged(
        self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
    ) -> None:
        commandInput = args.input
        if commandInput.id == "autoCalcGamepieceWeight":
            autoCalcWeightButton = adsk.core.BoolValueCommandInput.cast(commandInput)
            if autoCalcWeightButton.value == self.previousAutoCalcWeightCheckboxState:
                return

            if autoCalcWeightButton.value:
                self.calcGamepieceWeights()
                for weightInput in self.weightInputs():
                    weightInput.isEnabled = False
            else:
                for weightInput in self.weightInputs():
                    weightInput.isEnabled = True

            self.previousAutoCalcWeightCheckboxState = autoCalcWeightButton.value

        elif commandInput.id == "gamepieceAddButton":
            gamepieceAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("gamepieceAddButton")
            gamepieceRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "gamepieceRemoveButton"
            )
            gamepieceSelectCancelButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "gamepieceSelectCancelButton"
            )
            gamepieceSelection: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById(
                "gamepieceSelect"
            )
            spacer: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById("gamepieceTabSpacer")

            gamepieceSelection.isVisible = gamepieceSelection.isEnabled = True
            gamepieceSelection.clearSelection()
            gamepieceAddButton.isEnabled = gamepieceRemoveButton.isEnabled = False
            gamepieceSelectCancelButton.isVisible = gamepieceSelectCancelButton.isEnabled = True
            spacer.isVisible = False

        elif commandInput.id == "gamepieceRemoveButton":
            gamepieceAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("gamepieceAddButton")
            gamepieceTable: adsk.core.TableCommandInput = args.inputs.itemById("gamepieceTable")

            gamepieceAddButton.isEnabled = True
            if gamepieceTable.selectedRow == -1 or gamepieceTable.selectedRow == 0:
                ui = adsk.core.Application.get().userInterface
                ui.messageBox("Select a row to delete.")
            else:
                self.removeIndexedGamepiece(gamepieceTable.selectedRow - 1)  # selectedRow is 1 indexed

        elif commandInput.id == "gamepieceSelectCancelButton":
            gamepieceAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("gamepieceAddButton")
            gamepieceRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "gamepieceRemoveButton"
            )
            gamepieceSelectCancelButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "gamepieceSelectCancelButton"
            )
            gamepieceSelection: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById(
                "gamepieceSelect"
            )
            spacer: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById("gamepieceTabSpacer")

            gamepieceSelection.isEnabled = gamepieceSelection.isVisible = False
            gamepieceSelectCancelButton.isEnabled = gamepieceSelectCancelButton.isVisible = False
            gamepieceAddButton.isEnabled = gamepieceRemoveButton.isEnabled = True
            spacer.isVisible = True

    @logFailure
    def handleSelectionEvent(self, args: adsk.core.SelectionEventArgs, selectedOcc: adsk.fusion.Occurrence) -> None:
        selectionInput = args.activeInput
        rootComponent = adsk.core.Application.get().activeDocument.design.rootComponent
        occurrenceList: list[adsk.fusion.Occurrence] = rootComponent.allOccurrencesByComponent(selectedOcc.component)
        for occ in occurrenceList:
            if not self.addGamepiece(occ):
                ui = adsk.core.Application.get().userInterface
                result = ui.messageBox(
                    "You have already selected this Gamepiece.\nWould you like to remove it?",
                    "Synthesis: Remove Gamepiece Confirmation",
                    adsk.core.MessageBoxButtonTypes.YesNoButtonType,
                    adsk.core.MessageBoxIconTypes.QuestionIconType,
                )

                if result == adsk.core.DialogResults.DialogYes:
                    self.removeGamepiece(occ)

        selectionInput.isEnabled = selectionInput.isVisible = False

    @logFailure
    def handlePreviewEvent(self, args: adsk.core.CommandEventArgs) -> None:
        commandInputs = args.command.commandInputs
        gamepieceAddButton: adsk.core.BoolValueCommandInput = commandInputs.itemById("gamepieceAddButton")
        gamepieceRemoveButton: adsk.core.BoolValueCommandInput = commandInputs.itemById("gamepieceRemoveButton")
        gamepieceSelectCancelButton: adsk.core.BoolValueCommandInput = commandInputs.itemById(
            "gamepieceSelectCancelButton"
        )
        gamepieceSelection: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById(
            "gamepieceSelect"
        )
        spacer: adsk.core.SelectionCommandInput = self.gamepieceConfigTab.children.itemById("gamepieceTabSpacer")

        gamepieceRemoveButton.isEnabled = self.gamepieceTable.rowCount > 1
        if not gamepieceSelection.isEnabled:
            gamepieceAddButton.isEnabled = True
            gamepieceSelectCancelButton.isVisible = gamepieceSelectCancelButton.isEnabled = False
            spacer.isVisible = True
