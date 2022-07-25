from enum import Enum
from ..colors import Colors
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

from ..config_command import UiGlobal

import adsk.core, adsk.fusion, traceback, logging, os
from types import SimpleNamespace

from .command_group import CommandGroup


class JointCommandGroup(CommandGroup):

    class JointMotions(Enum):
        """### Corresponds to the API JointMotions enum

        Args:
            Enum (enum.Enum)
        """
        RIGID = 0
        REVOLUTE = 1
        SLIDER = 2
        CYLINDRICAL = 3
        PIN_SLOT = 4
        PLANAR = 5
        BALL = 6

    def __init__(self, parent):
        super.__init__(parent)
        super().__init__(parent)

    def configure(self):
        """
        Joint configuration group. Container for joint selection table
        """
        joint_config = self.parent.inputs.addGroupCommandInput(
            "joint_config", "Joint configuration"
        )
        joint_config.isExpanded = False
        joint_config.isVisible = True
        joint_config.tooltip = "Select and define joint occurrences in your assembly."

        joint_inputs = joint_config.children
        # ~~~~~~~~~~~~~~~~ WHEEL CONFIGURATION ~~~~~~~~~~~~~~~~
        self.parent.configureWheels()

        self.parent.configure_joint_menu()

        # JOINT SELECTION TABLE
        """
        All selection joints appear here.
        """
        joint_table_input = self.parent.create_table_input(  # create tablecommandinput using helper
            "joint_table",
            "Joint Table",
            joint_inputs,
            6,
            "1:2:2:2:2:2",
            50,
        )

        add_joint_input = joint_inputs.addBoolValueInput("joint_add", "Add", False)  # add button

        remove_joint_input = joint_inputs.addBoolValueInput(  # remove button
            "joint_delete", "Remove", False
        )

        add_joint_input.isEnabled = \
            remove_joint_input.isEnabled = True

        add_joint_input.tooltip = "Add a joint selection"  # tooltips
        remove_joint_input.tooltip = "Remove a joint selection"

        joint_select_input = joint_inputs.addSelectionInput(
            "joint_select",
            "Selection",
            "Select a joint in your drive-train assembly.",
        )

        joint_select_input.addSelectionFilter("Joints")  # only allow joint selection
        joint_select_input.setSelectionLimits(0)  # set no selection count limits
        joint_select_input.isEnabled = False
        joint_select_input.isVisible = False  # make selection box invisible

        joint_table_input.addToolbarCommandInput(add_joint_input)  # add bool inputs to the toolbar
        joint_table_input.addToolbarCommandInput(remove_joint_input)  # add bool inputs to the toolbar

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # create a textBoxCommandInput for the table header (Joint Motion), using helper
                "motion_header",
                "Motion",
                joint_inputs,
                "Motion",
                bold=False,
            ),
            0,
            0,
        )

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # textBoxCommandInput for table header (Joint Name), using helper
                "name_header", "Name", joint_inputs, "Joint name", bold=False
            ),
            0,
            1,
        )

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # another header using helper
                "parent_header",
                "Parent",
                joint_inputs,
                "Parent joint",
                background=Colors.background,  # background color
            ),
            0,
            2,
        )

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # another header using helper
                "signal_header",
                "Signal",
                joint_inputs,
                "Signal type",
                background=Colors.background,  # back color
            ),
            0,
            3,
        )

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # another header using helper
                "speed_header",
                "Speed",
                joint_inputs,
                "Joint Speed",
                background=Colors.background,  # back color
            ),
            0,
            4
        )

        joint_table_input.addCommandInput(
            self.parent.create_text_box_input(  # another header using helper
                "force_header",
                "Force",
                joint_inputs,
                "Joint Force",
                background=Colors.background,  # back color
            ),
            0,
            5
        )

        for joint in gm.app.activeDocument.design.rootComponent.allJoints:
            if (
                    joint.jointMotion.jointType == JointCommandGroup.JointMotions.REVOLUTE.value
                    or joint.jointMotion.jointType == JointCommandGroup.JointMotions.SLIDER.value
            ) and not joint.isSuppressed:
                self.add_joint_to_table(joint)

    @staticmethod
    def add_joint_to_table(joint: adsk.fusion.Joint) -> None:
        """### Adds a Joint object to its global list and joint table.

        Args:
            joint (adsk.fusion.Joint): Joint object to be added
        """
        try:
            UiGlobal.joint_list_global.append(joint)
            joint_table_input = UiGlobal.joint_table()
            cmd_inputs = adsk.core.CommandInputs.cast(joint_table_input.commandInputs)

            # joint type icons
            if joint.jointMotion.jointType == adsk.fusion.JointTypes.RigidJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Rigid", icon_paths.jointIcons["rigid"]
                )
                icon.tooltip = "Rigid joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Revolute", icon_paths.jointIcons["revolute"]
                )
                icon.tooltip = "Revolute joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.SliderJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Slider", icon_paths.jointIcons["slider"]
                )
                icon.tooltip = "Slider joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.PlanarJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Planar", icon_paths.jointIcons["planar"]
                )
                icon.tooltip = "Planar joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.PinSlotJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Pin Slot", icon_paths.jointIcons["pin_slot"]
                )
                icon.tooltip = "Pin slot joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.CylindricalJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Cylindrical", icon_paths.jointIcons["cylindrical"]
                )
                icon.tooltip = "Cylindrical joint"

            elif joint.jointMotion.jointType == adsk.fusion.JointTypes.BallJointType:
                icon = cmd_inputs.addImageCommandInput(
                    "placeholder", "Ball", icon_paths.jointIcons["ball"]
                )
                icon.tooltip = "Ball joint"

            # joint name
            name = cmd_inputs.addTextBoxCommandInput(
                "name_j", "Occurrence name", "", 1, True
            )
            name.tooltip = joint.name
            name.formattedText = "<p style='font-size:11px'>{}</p>".format(joint.name)

            jointType = cmd_inputs.addDropDownCommandInput(
                "joint_parent",
                "Joint Type",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            jointType.isFullWidth = True
            jointType.listItems.add("Root", True)

            # after each additional joint added, add joint to the dropdown of all preview rows/joints
            for row in range(joint_table_input.rowCount):
                if row != 0:
                    drop_down = joint_table_input.getInputAtPosition(row, 2)
                    drop_down.listItems.add(UiGlobal.joint_list_global[-1].name, False)

            # add all parent joint options to added joint dropdown
            for j in range(len(UiGlobal.joint_list_global) - 1):
                jointType.listItems.add(UiGlobal.joint_list_global[j].name, False)

            jointType.tooltip = "Possible parent joints"
            jointType.tooltipDescription = "<hr>The root component is usually the parent."

            signal_type = cmd_inputs.addDropDownCommandInput(
                "signal_type",
                "Signal Type",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            signal_type.listItems.add("‎", True, icon_paths.signalIcons["PWM"])
            signal_type.listItems.add("‎", False, icon_paths.signalIcons["CAN"])
            signal_type.listItems.add("‎", False, icon_paths.signalIcons["PASSIVE"])
            signal_type.tooltip = "Signal type"

            row = joint_table_input.rowCount

            joint_table_input.addCommandInput(icon, row, 0)
            joint_table_input.addCommandInput(name, row, 1)
            joint_table_input.addCommandInput(jointType, row, 2)
            joint_table_input.addCommandInput(signal_type, row, 3)

            if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                jointSpeed = cmd_inputs.addValueInput(
                    "joint_speed",
                    "Speed",
                    "deg",
                    adsk.core.ValueInput.createByReal(3.1415926)
                )
                jointSpeed.tooltip = 'Degrees per second'
                joint_table_input.addCommandInput(jointSpeed, row, 4)

            if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                jointForce = cmd_inputs.addValueInput(
                    "joint_force",
                    "Force",
                    "N",
                    adsk.core.ValueInput.createByReal(5000)
                )
                jointForce.tooltip = 'Newton-Meters***'
                joint_table_input.addCommandInput(jointForce, row, 5)


        except:
            gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.ui.ConfigCommand.addJointToTable()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )
