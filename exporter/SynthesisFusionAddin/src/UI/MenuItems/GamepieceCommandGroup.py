import adsk.core
import adsk.fusion

from .CommandGroup import CommandGroup
from .. import IconPaths


class GamepieceCommandGroup(CommandGroup):

    def __init__(self, parent):
        super().__init__(parent)

    def configure(self):
        """
                  Gamepiece group command input, isVisible=False by default
                      - Container for gamepiece selection table
                  """
        gamepiece_config = self.parent.inputs.addGroupCommandInput(
            "gamepiece_config", "Gamepiece Configuration"
        )
        gamepiece_config.isExpanded = True
        gamepiece_config.isVisible = False
        gamepiece_config.tooltip = "Select and define the gamepieces in your field."
        gamepiece_inputs = gamepiece_config.children

        # GAMEPIECE MASS CONFIGURATION
        """
      Mass unit dropdown and calculation for gamepiece elements
      """
        weight_table_input_f = self.parent.create_table_input(
            "weight_table_f", "Weight Table", gamepiece_inputs, 3, "6:2:1", 1
        )
        weight_table_input_f.tablePresentationStyle = 2  # set to clear background

        weight_name_f = gamepiece_inputs.addStringValueInput("weight_name", "Weight")
        weight_name_f.value = "Unit of Mass"
        weight_name_f.isReadOnly = True

        auto_calc_weight_f = self.parent.create_boolean_input(  # CALCULATE button
            "auto_calc_weight_f",
            "‎",
            gamepiece_inputs,
            checked=False,
            tooltip="Approximate the weight of all your selected gamepieces.",
            enabled=True,
            is_check_box=False
        )
        auto_calc_weight_f.resourceFolder = IconPaths.stringIcons["calculate-enabled"]
        auto_calc_weight_f.isFullWidth = True

        weight_unit_f = gamepiece_inputs.addDropDownCommandInput(
            "weight_unit_f",
            "Unit of Mass",
            adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        weight_unit_f.listItems.add("‎", True, IconPaths.massIcons["LBS"])  # add listdropdown mass options
        weight_unit_f.listItems.add("‎", False, IconPaths.massIcons["KG"])  # add listdropdown mass options
        weight_unit_f.tooltip = "Unit of mass"
        weight_unit_f.tooltipDescription = "<hr>Configure the unit of mass for for the weight calculation."

        weight_table_input_f.addCommandInput(weight_name_f, 0, 0)  # add command inputs to table
        weight_table_input_f.addCommandInput(auto_calc_weight_f, 0, 1)  # add command inputs to table
        weight_table_input_f.addCommandInput(weight_unit_f, 0, 2)  # add command inputs to table

        # GAMEPIECE SELECTION TABLE
        """
      All selected gamepieces appear here
      """
        gamepiece_table_input = self.parent.create_table_input(
            "gamepiece_table",
            "Gamepiece",
            gamepiece_inputs,
            4,
            "1:8:5:12",
            50,
        )

        addFieldInput = gamepiece_inputs.addBoolValueInput("field_add", "Add", False)

        removeFieldInput = gamepiece_inputs.addBoolValueInput(
            "field_delete", "Remove", False
        )
        addFieldInput.isEnabled = removeFieldInput.isEnabled = True

        removeFieldInput.tooltip = "Remove a field element"
        addFieldInput.tooltip = "Add a field element"

        gamepieceSelectInput = gamepiece_inputs.addSelectionInput(
            "gamepiece_select",
            "Selection",
            "Select the unique gamepieces in your field.",
        )
        gamepieceSelectInput.addSelectionFilter("Occurrences")
        gamepieceSelectInput.setSelectionLimits(0)
        gamepieceSelectInput.isEnabled = True
        gamepieceSelectInput.isVisible = False

        gamepiece_table_input.addToolbarCommandInput(addFieldInput)
        gamepiece_table_input.addToolbarCommandInput(removeFieldInput)

        """
      Gamepiece table column headers. (the permanent captions in the first row of table)
      """
        gamepiece_table_input.addCommandInput(
            self.parent.create_text_box_input(
                "e_header",
                "Gamepiece name",
                gamepiece_inputs,
                "Gamepiece",
                bold=False,
            ),
            0,
            1,
        )

        gamepiece_table_input.addCommandInput(
            self.parent.create_text_box_input(
                "w_header",
                "Gamepiece weight",
                gamepiece_inputs,
                "Weight",
                background="#d9d9d9",
            ),
            0,
            2,
        )

        gamepiece_table_input.addCommandInput(
            self.parent.create_text_box_input(
                "f_header",
                "Friction coefficient",
                gamepiece_inputs,
                "Friction coefficient",
                background="#d9d9d9",
            ),
            0,
            3,
        )
