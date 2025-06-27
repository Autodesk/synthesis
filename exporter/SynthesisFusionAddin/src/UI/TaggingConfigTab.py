import adsk.core
import adsk.fusion

from src.Logging import getLogger, logFailure
from src.UI.CreateCommandInputsHelper import createTableInput, createTextBoxInput

logger = getLogger()

class TaggingConfigTab:
    # stores the types of tags available for selection
    tagTypes = ["Softbody", "Rigid", "Chain", "Spring", "Rope"]

    taggingConfigTab: adsk.core.TabCommandInput
    taggingListTable: adsk.core.TableCommandInput
    tagTypeDropdown: adsk.core.DropDownCommandInput

    tagMap: dict[str, str] = {}
    tableRowTokens: list[str] = []

    @logFailure
    def __init__(self, args:adsk.core.CommandCreatedEventArgs) -> None:
        self.taggingConfigTab = args.command.commandInputs.addTabCommandInput(
            "taggingOptionsTab", "Tagging Options"
        )
        self.taggingConfigTab.tooltip = "Configure tagging options for materials"
        taggingConfigTabInputs = self.taggingConfigTab.children

        self.tagTypeDropdown = taggingConfigTabInputs.addDropDownCommandInput(
            "tagTypeDropdown",
             "Tag Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
        )
        self.tagTypeDropdown.isFullWidth = False
        for tag in self.tagTypes:
            self.tagTypeDropdown.listItems.add(tag, False)

        bodySelection = taggingConfigTabInputs.addSelectionInput(
            "tagBodySelect", 
            "Select Body", 
            "Select a single body."
        )
        bodySelection.addSelectionFilter("SolidBodies") 
        bodySelection.addSelectionFilter("SurfaceBodies")
        bodySelection.setSelectionLimits(1,1)
        bodySelection.isEnabled = bodySelection.isVisible = False 

        self.taggingListTable = createTableInput("tagListTable", "Tag List", taggingConfigTabInputs, 6, "1:1")
        self.taggingListTable.addCommandInput(
            createTextBoxInput("headerBodyName", "Body", taggingConfigTabInputs, "Body Name", background="#d9d9d9"),
            0,
            0
        )
        self.taggingListTable.addCommandInput(
            createTextBoxInput("headerTagType", "Type", taggingConfigTabInputs, "Tag Type", background="#d9d9d9"),
            0,
            1
        )
        self.taggingListTable.getInputAtPosition(0,0).parentCommand.isSelectable = False
        self.taggingListTable.getInputAtPosition(0,1).parentCommand.isSelectable = False

        addTagInputButton = taggingConfigTabInputs.addBoolValueInput("tagAddButton", "Add", False)
        removeTagInputButton = taggingConfigTabInputs.addBoolValueInput("tagRemoveButton", "Remove", False)
        cancelInputButton = taggingConfigTabInputs.addBoolValueInput("tagCancelButton", "Cancel", False)

        addTagInputButton.isEnabled = removeTagInputButton.isEnabled = True
        cancelInputButton.isVisible = False

        self.taggingListTable.addToolbarCommandInput(addTagInputButton)
        self.taggingListTable.addToolbarCommandInput(removeTagInputButton)
        self.taggingListTable.addToolbarCommandInput(cancelInputButton)

    @property
    def isVisible(self) -> bool:
        return self.taggingConfigTab.isVisible or False

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.taggingConfigTab.isVisible = value

    @property
    def isActive(self) -> bool:
        return self.taggingConfigTab.isActive or False

    @logFailure
    def handleInputChanged(self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs) -> None:
        commandInput = args.input
        tagAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("tagAddButton")
        tagRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("tagRemoveButton")
        tagCancelButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("tagCancelButton")
        tagBodySelection: adsk.core.SelectionCommandInput = globalCommandInputs.itemById("tagBodySelect")

        if (tagBodySelection.selectionCount == 1 or self.tagTypeDropdown.selectedItem is not None):
            selectedEntity = tagBodySelection.selection(0).entity
            entityToken = selectedEntity.entityToken
            tagName = self.tagTypeDropdown.selectedItem.name

            if entityToken in self.tagMap:
                app = adsk.core.Application.get()
                ui = app.userInterface
                ui.messageBox(f'The body "{selectedEntity.name}" is already tagged. Please remove the existing tag first.')
            else:
                self.tagMap[entityToken] = tagName
                
                row = self.taggingListTable.rowCount
                bodyNameInput = createTextBoxInput(f"bodyName_{row}", "Body Name", selectedEntity.name, 1, True)
                tagTypeInput = createTextBoxInput(f"tagType_{row}", "Tag Type", tagName, 1, True)
                self.taggingListTable.addCommandInput(bodyNameInput, row, 0)
                self.taggingListTable.addCommandInput(tagTypeInput, row, 1)

                self.tableRowTokens.append(entityToken)

            tagBodySelection.clearSelection()
            self.tagTypeDropdown.clearSelection()
            tagAddButton.isEnabled = tagRemoveButton.isEnabled = True
            tagBodySelection.isVisible = tagBodySelection.isEnabled = False
            tagCancelButton.isVisible = False


        if commandInput.id == "tagAddButton":
            tagBodySelection.isVisible = tagBodySelection.isEnabled = True
            tagAddButton.isEnabled = tagRemoveButton.isEnabled = False
            tagCancelButton.isVisible = True

        elif commandInput.id == "tagRemoveButton":
            selectedRow = self.taggingListTable.selectedRow
            if selectedRow == -1:
                app = adsk.core.Application.get()
                ui = app.userInterface
                ui.messageBox("No tags to remove.")
                return

            token_to_remove = self.tableRowTokens.pop(selectedRow - 1)
            if token_to_remove in self.tagMap:
                del self.tagMap[token_to_remove]

            self.taggingListTable.deleteRow(selectedRow)

        elif commandInput.id == "tagCancelButton":
            tagBodySelection.isVisible = tagBodySelection.isEnabled = False
            tagAddButton.isEnabled = tagRemoveButton.isEnabled = True
            self.taggingListTable.clearSelection()

    @logFailure
    def getTags(self) -> dict[str, str]:
        return self.tagMap
