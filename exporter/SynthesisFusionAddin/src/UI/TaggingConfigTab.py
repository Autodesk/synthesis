import adsk.core
import adsk.fusion

from src.Logging import getLogger, logFailure
from src.UI.CreateCommandInputsHelper import createTableInput, createTextBoxInput

logger = getLogger()


class TaggingConfigTab:
    # stores the types of tags available for selection
    tagTypes = ["Softbody", "Rigid", "Chain", "Spring", "Rope"]
    tagMap: dict[str, str] = {}
    tagList: list[str] = []

    taggingConfigTab: adsk.core.TabCommandInput
    taggingListTable: adsk.core.TableCommandInput
    tagTypeDropdown: adsk.core.DropDownCommandInput

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        self.taggingConfigTab = args.command.commandInputs.addTabCommandInput("taggingOptionsTab", "Tagging Options")
        self.taggingConfigTab.tooltip = "Configure tagging options for materials"
        taggingConfigTabInputs = self.taggingConfigTab.children

        self.tagTypeDropdown = taggingConfigTabInputs.addDropDownCommandInput(
            "tageTypeDropdown", "Tag Type", dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
        )
        self.tagTypeDropdown.isFullWidth = False
        for tag in self.tagTypes:
            self.tagTypeDropdown.listItems.add(tag, False)
        self.tagTypeDropdown.isEnabled = self.tagTypeDropdown.isVisible = False

        bodySelection = taggingConfigTabInputs.addSelectionInput(
            "tagBodySelect", "Select Body", "Select a single body."
        )
        bodySelection.addSelectionFilter("SolidBodies")
        bodySelection.addSelectionFilter("SurfaceBodies")

        self.taggingListTable = createTableInput("tagListTable", "Tag List", taggingConfigTabInputs, 6, "1:1")
        self.taggingListTable.addCommandInput(
            createTextBoxInput("headerBodyName", "Body", taggingConfigTabInputs, "Body Name", background="#d9d9d9"),
            0,
            0,
        )
        self.taggingListTable.addCommandInput(
            createTextBoxInput("headerTagType", "Type", taggingConfigTabInputs, "Tag Type", background="#d9d9d9"), 0, 1
        )
        self.taggingListTable.getInputAtPosition(0, 0).parentCommand.isSelectable = False
        self.taggingListTable.getInputAtPosition(0, 1).parentCommand.isSelectable = False

        addTagInputButton = taggingConfigTabInputs.addBoolValueInput("tagAddButton", "Add", False)
        removeTagInputButton = taggingConfigTabInputs.addBoolValueInput("tagRemoveButton", "Remove", False)
        cancelInputButton = taggingConfigTabInputs.addBoolValueInput("tagCancelButton", "Cancel", False)

        self.taggingListTable.addToolbarCommandInput(addTagInputButton)
        self.taggingListTable.addToolbarCommandInput(removeTagInputButton)
        self.taggingListTable.addToolbarCommandInput(cancelInputButton)

        self.toggleSelecting(False)

    @property
    def isVisible(self) -> bool:
        return self.taggingConfigTab.isVisible or False

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.taggingConfigTab.isVisible = value

    @property
    def isActive(self) -> bool:
        return self.taggingConfigTab.isActive or False

    def toggleSelecting(self, value: bool) -> None:
        tagAddButton: adsk.core.BoolValueCommandInput = self.taggingConfigTab.commandInputs.itemById("tagAddButton")
        tagRemoveButton: adsk.core.BoolValueCommandInput = self.taggingConfigTab.commandInputs.itemById(
            "tagRemoveButton"
        )
        tagBodySelection: adsk.core.SelectionCommandInput = self.taggingConfigTab.commandInputs.itemById(
            "tagBodySelect"
        )
        tagCancelButton: adsk.core.BoolValueCommandInput = self.taggingConfigTab.commandInputs.itemById(
            "tagCancelButton"
        )

        tagAddButton.isEnabled = tagRemoveButton.isEnabled = not value
        tagCancelButton.isVisible = value
        self.tagTypeDropdown.isEnabled = self.tagTypeDropdown.isVisible = value

        if not value:
            tagBodySelection.clearSelection()
            tagBodySelection.setSelectionLimits(0)
            tagBodySelection.isEnabled = tagBodySelection.isVisible = False
            for listItem in self.tagTypeDropdown.listItems:
                listItem.isSelected = False

        else:
            tagBodySelection.isVisible = tagBodySelection.isEnabled = True
            tagBodySelection.setSelectionLimits(0, 1)

    @logFailure
    def handleInputChanged(
        self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
    ) -> None:
        commandInput = args.input
        tagBodySelection: adsk.core.SelectionCommandInput = globalCommandInputs.itemById("tagBodySelect")

        if commandInput.id == "tagAddButton":
            self.toggleSelecting(True)

        elif commandInput.id == "tagRemoveButton":
            if self.taggingListTable.selectedRow == -1:
                app = adsk.core.Application.get()
                ui = app.userInterface
                ui.messageBox("No tags to remove.")
                return

            selectedRow = self.taggingListTable.selectedRow
            if (selectedRow == 0): return

            self.taggingListTable.deleteRow(selectedRow)
            self.tagMap.pop(self.tagList[selectedRow - 1])
            self.tagList.pop(selectedRow - 1)

        elif commandInput.id == "tagCancelButton":
            self.toggleSelecting(False)

        elif tagBodySelection.selectionCount == 1 and self.tagTypeDropdown.selectedItem is not None:
            commandInputs = self.taggingConfigTab.commandInputs
            row = self.taggingListTable.rowCount
            bodyName = commandInputs.addTextBoxCommandInput(
                f"bodyName_{row}", "Body Name", tagBodySelection.selection(0).entity.name, 1, True
            )
            tagType = commandInputs.addTextBoxCommandInput(
                f"tagType_{row}", "Tag Type", self.tagTypeDropdown.selectedItem.name, 1, True
            )

            row = self.taggingListTable.rowCount
            self.taggingListTable.addCommandInput(bodyName, row, 0)
            self.taggingListTable.addCommandInput(tagType, row, 1)

            self.tagMap[tagBodySelection.selection(0).entity.entityToken] = self.tagTypeDropdown.selectedItem.name
            self.tagList.append(tagBodySelection.selection(0).entity.entityToken)

            self.toggleSelecting(False)

    @logFailure
    def getTags(self) -> dict[str, str]:
        return self.tagMap
