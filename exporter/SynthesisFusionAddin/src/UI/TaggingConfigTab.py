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
    bodySelect: adsk.core.SelectionCommandInput 
    tagTypeDropdown: adsk.core.DropDownCommandInput

    @logFailure
    def __init__(self, args:adsk.core.CommandCreatedEventArgs) -> None:
        self.taggingConfigTab = args.command.commandInputs.addTabCommandInput(
            "taggingOptionsTab", "Tagging Options"
        )
        self.taggingConfigTab.tooltip = "Configure tagging options for materials"
        taggingConfigTabInputs = self.taggingConfigTab.children

        self.tagTypeDropdown = taggingConfigTabInputs.addDropDownCommandInput(
            "tagType",
             "Tag Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
        )
        self.tagTypeDropdown.isFullWidth = False
        for tag in self.tagTypes:
            self.tagTypeDropdown.listItems.add(tag, False)

        self.bodySelect = taggingConfigTabInputs.addSelectionInput(
            "bodySelect", 
            "Select Body", 
            "Select a single body."
        )
        self.bodySelect.addSelectionFilter("SolidBodies") 
        self.bodySelect.addSelectionFilter("SurfaceBodies")
        self.bodySelect.setSelectionLimits(1,1)

        self.taggingListTable = createTableInput("tagListTable", "Tag List", taggingConfigTabInputs, 6, "1:1")
        self.taggingListTable.addCommandInput(
            createTextBoxInput("bodyName", "Body", taggingConfigTabInputs, "Body Name", background="#d9d9d9"),
            0,
            0
        )
        self.taggingListTable.addCommandInput(
            createTextBoxInput("tagType", "Type", taggingConfigTabInputs, "Tag Type", background="#d9d9d9"),
            0,
            1
        )

        addTagInputButton = taggingConfigTabInputs.addBoolValueInput("addTagButton", "Add", False)
        removeTagInputButton = taggingConfigTabInputs.addBoolValueInput("removeTagButton", "Remove", False)
        addTagInputButton.isEnabled = removeTagInputButton.isEnabled = True

        self.taggingListTable.addToolbarCommandInput(addTagInputButton)
        self.taggingListTable.addToolbarCommandInput(removeTagInputButton)

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
    def addTag(self) -> None:
        if (self.bodySelect.selectionCount == 0 or self.tagTypeDropdown.selectedItem is None):
            app = adsk.core.Application.get()
            ui = app.userInterface
            ui.messageBox("Select a body and a tag type before adding a tag.")
            return

        commandInputs = self.taggingConfigTab.commandInputs
        bodyName = commandInputs.addTextBoxCommandInput("bodyName", "Body Name", self.bodySelect.selection(0).entity.name, 1, True)
        tagType = commandInputs.addTextBoxCommandInput("tagType", "Tag Type", self.tagTypeDropdown.selectedItem.name, 1, True)

        row = self.taggingListTable.rowCount
        self.taggingListTable.addCommandInput(bodyName, row, 0)
        self.taggingListTable.addCommandInput(tagType, row, 1)

        self.bodySelect.clearSelection()

    @logFailure
    def removeTag(self) -> None:
        logger.info(self.getTags()) # TODO: Remove this line
        if self.taggingListTable.selectedRow == -1:
            app = adsk.core.Application.get()
            ui = app.userInterface
            ui.messageBox("No tags to remove.")
            return
        
        self.taggingListTable.deleteRow(self.taggingListTable.selectedRow)

    @logFailure
    def handleInputChanged(self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs) -> None:
        commandInput = args.input

        if commandInput.id == "addTagButton":
            self.addTag()

        elif commandInput.id == "removeTagButton":
            self.removeTag()

    @logFailure
    def getTags(self) -> list:
        tags = []
        for row in range(self.taggingListTable.rowCount):
            bodyNameInput = self.taggingListTable.getInputAtPosition(row, 0)
            tagTypeInput = self.taggingListTable.getInputAtPosition(row, 1)

            if bodyNameInput and tagTypeInput:
                tags.append({
                    "bodyName": bodyNameInput.text,
                    "tagType": tagTypeInput.text
                })
        return tags
