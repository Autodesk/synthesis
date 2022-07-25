from .. import helper, file_dialog_config, os_helper, graphics_custom, icon_paths
import adsk.core, adsk.fusion, traceback, logging, os

from .command_group import CommandGroup


class WeightCommandGroup(CommandGroup):

    def __init__(self, parent):
        super().__init__(parent)
        self.wheel_list = []

    def configure(self):
        """
                    Table for weight config.
                        - Used this to align multiple commandInputs on the same row
                    """
        weight_table_input = self.parent.create_table_input(
            "weight_table",
            "Weight Table",
            self.parent.self.self.parent.inputs,
            4,
            "3:2:2:1",
            1,
        )
        weight_table_input.tablePresentationStyle = 2  # set transparent background for table

        weight_name = self.parent.inputs.addStringValueInput("weight_name", "Weight")
        weight_name.value = "Weight"
        weight_name.isReadOnly = True

        auto_calc_weight = self.parent.create_boolean_input(
            "auto_calc_weight",
            "‎",
            self.parent.inputs,
            checked=False,
            tooltip="Approximate the weight of your robot assembly.",
            tooltipadvanced="<i>This may take a moment...</i>",
            enabled=True,
            is_check_box=False
        )
        auto_calc_weight.resourceFolder = icon_paths.stringIcons["calculate-enabled"]
        auto_calc_weight.isFullWidth = True

        weight_input = self.parent.inputs.addValueInput(
            "weight_input",
            "Weight Input",
            "",
            adsk.core.ValueInput.createByString("0.0"),
        )
        weight_input.tooltip = "Robot weight"
        weight_input.tooltipDescription = """<tt>(in pounds)</tt><hr>This is the weight of the entire robot assembly."""

        weight_unit = self.parent.inputs.addDropDownCommandInput(
            "weight_unit",
            "Weight Unit",
            adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        weight_unit.listItems.add("‎", True, icon_paths.massIcons["LBS"])  # add listdropdown mass options
        weight_unit.listItems.add("‎", False, icon_paths.massIcons["KG"])  # add listdropdown mass options
        weight_unit.tooltip = "Unit of mass"
        weight_unit.tooltipDescription = "<hr>Configure the unit of mass for the weight calculation."

        weight_table_input.addCommandInput(weight_name, 0, 0)  # add command self.parent.inputs to table
        weight_table_input.addCommandInput(auto_calc_weight, 0, 1)  # add command self.parent.inputs to table
        weight_table_input.addCommandInput(weight_input, 0, 2)  # add command self.parent.inputs to table
        weight_table_input.addCommandInput(weight_unit, 0, 3)  # add command self.parent.inputs to table
