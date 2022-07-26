from .command_group import CommandGroup
import adsk.core, adsk.fusion, traceback, logging, os


class ExportModeCommandGroup(CommandGroup):
    def __init__(self, parent):
        self.parent = parent

    def configure(self):
        dropdown_export_mode = self.parent.inputs.addDropDownCommandInput(
            "mode",
            "Export Mode",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        dropdown_export_mode.listItems.add("Dynamic", True)
        dropdown_export_mode.listItems.add("Static", False)

        dropdown_export_mode.tooltip = "Export Mode"
        dropdown_export_mode.tooltipDescription = "<hr>Does this object move dynamically?"
