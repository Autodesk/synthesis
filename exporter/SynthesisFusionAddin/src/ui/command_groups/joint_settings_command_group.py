from enum import Enum
from ...general_imports import *
from ...configure import NOTIFIED, write_configuration
from ...Analytics.alert import showAnalyticsAlert
from .. import helper, file_dialog_config, os_helper, graphics_custom, icon_paths
from ...parser.parse_options import (
    Gamepiece,
    Mode,
    ParseOptions,
    _Joint,
    _Wheel,
    JointParentType,
)

from ..configuration.serial_command import SerialCommand

class JointSettingsCommandGroup(MenuItem):

    def configure(self):
        jointsSettings = a_input.addGroupCommandInput(
            "joints_settings", "Joints Settings"
        )
        jointsSettings.isExpanded = False
        jointsSettings.isEnabled = True
        jointsSettings.tooltip = "tooltip"  # TODO: update tooltip
        joints_settings = jointsSettings.children

        self.parent.create_boolean_input(
            "kinematic_only",
            "Kinematic Only",
            joints_settings,
            checked=False,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

        self.parent.create_boolean_input(
            "calculate_limits",
            "Calculate Limits",
            joints_settings,
            checked=True,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

        self.parent.create_boolean_input(
            "auto_assign_ids",
            "Auto-Assign ID's",
            joints_settings,
            checked=True,
            tooltip="tooltip",  # TODO: update tooltip
            enabled=True,
        )

