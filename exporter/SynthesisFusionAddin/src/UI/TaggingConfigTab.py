import adsk.core
import adsk.fusion

from src.Logging import logFailure, getLogger
from src.UI.CreateCommandInputsHelper import (
    createTableInput, 
    createTextBoxInput
)

logger = getLogger()

class TaggingConfigTab:
    """Tab for tagging materials"""

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

        # Dropdown for tagging options
        self.tagTypeDropdown = taggingConfigTabInputs.addDropDownCommandInput(
            "tagType",
             "Tag Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
        )
        self.tagTypeDropdown.isFullWidth = False
        for tag in self.tagTypes:
            self.tagTypeDropdown.listItems.add(tag, False)

        # Create a selection input specifically for bodies:
        self.bodySelect = taggingConfigTabInputs.addSelectionInput(
            "bodySelect", 
            "Select Body", 
            "Select a single body."
        )
        # Restrict selection to solid/surface bodies:
        self.bodySelect.addSelectionFilter("SolidBodies") 
        self.bodySelect.addSelectionFilter("SurfaceBodies")
        self.bodySelect.setSelectionLimits(1,1)

        # Table that shows all the bodies with their respective tags
        self.taggingListTable = createTableInput("tagListTable", "Tag List", taggingConfigTabInputs, 2, "1:1")
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

        # Add button that checks if body is selected and if Tag Type is selected
        addTagInputButton = taggingConfigTabInputs.addBoolValueInput("addTagButton", "Add", False)
        removeTagInputButton = taggingConfigTabInputs.addBoolValueInput("removeTagButton", "Remove", False)
        addTagInputButton.isEnabled = removeTagInputButton.isEnabled = True

        self.taggingListTable.addToolbarCommandInput(addTagInputButton)
        self.taggingListTable.addToolbarCommandInput(removeTagInputButton)

        commandInputs = self.taggingListTable.commandInputs

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
    def addTag(self) -> bool:
        # checks if something is selected
        if (not self.bodySelect.isValid()):
            logger.info("Not valid")
            return False
        logger.info("valid")
        logger.info(self.bodySelect.name)
        logger.info(self.tagTypeDropdown.name())
        return True

    @logFailure
    def handleInputChanged(self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs) -> None:
        commandInput = args.input
        tagAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("addTagButton")
        tagRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("removeTagButton")

        if commandInput.id == "addTagButton":
            if self.addTag():
                return
                # tagAddButton.isEnabled = False
                # tagRemoveButton.isEnabled = True
                # # Add the selected body and tag type to the table
                # body_name = self.bodySelect.selection(0).entity.name
                # tag_type = self.tagTypeDropdown.selectedItem.name
                # self.taggingListTable.addRow([body_name, tag_type])
                # logger.info(f"Added tag: {tag_type} to body: {body_name}")
            else:
                logger.error("Failed to add tag. Ensure a body is selected and a tag type is chosen.")

        elif commandInput.id == "bodySelect":
            selection_input = adsk.core.SelectionCommandInput.cast(commandInput)
            if selection_input.selectionCount > 0:
                selected_entity = selection_input.selection(0).entity
                if selected_entity > 0:
                    # Do something with the selected body
                    logger.info(f"Selected body: {selected_entity.name}")
        return

