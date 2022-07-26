from .command_group import CommandGroup
from ...general_imports import *


class ControllerCommandGroup(CommandGroup):
    def __init__(self, parent):
        super().__init__()
        self.parent = parent

    def configure(self):
        controller_settings = self.parent.advanced_settings.children.addGroupCommandInput(
            "controller_settings", "Controller Settings"
        )

        controller_settings.isExpanded = False
        controller_settings.isEnabled = True
        controller_settings.tooltip = "tooltip"  # TODO: update tooltip
        controller_settings = controller_settings.children

        self.parent.create_boolean_input(  # export signals checkbox
            "export_signals",
            "Export Signals",
            controller_settings,
            checked=True,
            tooltip="tooltip",
            enabled=True,
        )

        # clear all selections before instantiating handlers.
        gm.ui.activeSelections.clear()
