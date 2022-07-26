import logging

import adsk.core
import adsk.fusion

from .command_group import CommandGroup
from .. import os_helper, icon_paths
from ...general_imports import *

from ..config_command import UiGlobal


class WheelCommandGroup(CommandGroup):

    def __init__(self, parent):
        super().__init__()
        self.parent_menu = parent

    def configure(self):

        """
        Wheel configuration command input group
            - Container for wheel selection Table
        """
        wheel_config = self.parent_menu.inputs.addGroupCommandInput(
            "wheel_config", "Wheel configuration"
        )
        wheel_config.isExpanded = True
        wheel_config.isEnabled = True
        wheel_config.tooltip = (
            "Select and define the drive-train wheels in your assembly."
        )

        wheel_inputs = wheel_config.children

        # WHEEL SELECTION TABLE
        """
        All selected wheel occurrences appear here.
        """
        wheel_table_input = self.parent_menu.create_table_input(
            "wheel_table",
            "Wheel Table",
            wheel_inputs,
            4,
            "1:4:2:2",
            50,
        )

        add_wheel_input = wheel_inputs.addBoolValueInput("wheel_add", "Add", False)  # add button

        remove_wheel_input = wheel_inputs.addBoolValueInput(  # remove button
            "wheel_delete", "Remove", False
        )

        add_wheel_input.tooltip = "Add a wheel joint"  # tooltips
        remove_wheel_input.tooltip = "Remove a wheel joint"

        wheel_select_input = wheel_inputs.addSelectionInput(
            "wheel_select",
            "Selection",
            "Select the wheels joints in your drive-train assembly.",
        )
        wheel_select_input.addSelectionFilter("Joints")  # filter selection to only occurrences

        wheel_select_input.setSelectionLimits(0)  # no selection count limit
        wheel_select_input.isEnabled = False
        wheel_select_input.isVisible = False

        wheel_table_input.addToolbarCommandInput(add_wheel_input)  # add buttons to the toolbar
        wheel_table_input.addToolbarCommandInput(remove_wheel_input)  # add buttons to the toolbar

        """
        Algorithmic Wheel Selection Indicator
        """
        """
        algorithmicIndicator = self.parent_menu.createTextBoxInput( # wheel type header
                "algorithmic_indicator",
                "Indicator",
                wheel_inputs,
                "Algorithmic Wheel Selection",
                background="whitesmoke", # textbox header background color
                tooltip="Algorithmic Wheel Selection"
        )
        algorithmicIndicator.isFullWidth = True
        algorithmicIndicator.formattedText = "🟢"
        algorithmicIndicator.tooltipDescription = (
            "<tt>(enabled)</tt>" +
            "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" + 
            "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" + 
            "<ul>" +
                "<tt>" + 
                    "<li>Jointing the wheel differently, or</li><br>" + 
                    "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" + 
                    "<li>Disabling Algorithmic Selection.</li>" + 
                "</tt>" + 
            "</ul>"
        )

        wheel_table_input.addCommandInput(
            algorithmicIndicator,
            0,
            0,
        )
        """

        wheel_table_input.addCommandInput(  # create textbox input using helper (component name)
            self.parent_menu.create_text_box_input(
                "name_header", "Name", wheel_inputs, "Joint name", bold=False
            ),
            0,
            1,
        )

        wheel_table_input.addCommandInput(
            self.parent_menu.create_text_box_input(  # wheel type header
                "parent_header",
                "Parent",
                wheel_inputs,
                "Wheel type",
                background="#d9d9d9",  # textbox header background color
            ),
            0,
            2,
        )

        wheel_table_input.addCommandInput(
            self.parent_menu.create_text_box_input(  # Signal type header
                "signal_header",
                "Signal",
                wheel_inputs,
                "Signal type",
                background="#d9d9d9",  # textbox header background color
            ),
            0,
            3,
        )

        # AUTOMATICALLY SELECT DUPLICATES
        """
        Select duplicates?
            - creates a BoolValueCommandInput
        """
        # self.parent_menu.createBooleanInput( # create bool value command input using helper
        #     "duplicate_selection",
        #     "Select Duplicates",
        #     wheel_inputs,
        #     checked=True,
        #     tooltip="Select duplicate wheel components.",
        #     tooltipadvanced="""<hr>When this is checked, all duplicate occurrences will be automatically selected.
        #     <br>This feature may fail when duplicates are not direct copies.</br>""", # advanced tooltip
        #     enabled=True,
        # )

    @staticmethod
    def add_wheel_to_table(wheel: adsk.fusion.Joint) -> None:
        """### Adds a wheel occurrence to its global list and wheel table.

        Args:
            wheel (adsk.fusion.Occurrence): wheel Occurrence object to be added.
        """
        try:
            onSelect = gm.handlers[3]
            wheelTableInput = UiGlobal.wheel_list_global
            # def addPreselections(child_occurrences):
            #     for occ in child_occurrences:
            #         onSelect.allWheelPreselections.append(occ.entityToken)

            #         if occ.childOccurrences:
            #             addPreselections(occ.childOccurrences)

            # if wheel.childOccurrences:
            #     addPreselections(wheel.childOccurrences)
            # else:
            onSelect.allWheelPreselections.append(wheel.entityToken)

            UiGlobal.wheel_list_global.append(wheel)
            cmdInputs = adsk.core.CommandInputs.cast(wheelTableInput.commandInputs)

            icon = cmdInputs.addImageCommandInput(
                "placeholder_w", "Placeholder", icon_paths.wheelIcons["standard"]
            )

            name = cmdInputs.addTextBoxCommandInput(
                "name_w", "Joint name", wheel.name, 1, True
            )
            name.tooltip = wheel.name

            wheelType = cmdInputs.addDropDownCommandInput(
                "wheel_type_w",
                "Wheel Type",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            wheelType.listItems.add("Standard", True, "")
            wheelType.listItems.add("Omni", False, "")
            wheelType.listItems.add("Mecanum", False, "")
            wheelType.tooltip = "Wheel type"
            wheelType.tooltipDescription = "<Br>Omni-directional wheels can be used just like regular drive wheels but they have the advantage of being able to roll freely perpendicular to the drive direction.</Br>"
            wheelType.toolClipFilename = os_helper.getOSPath(".", "src", "Resources") + os.path.join("WheelIcons",
                                                                                                    "omni-wheel-preview.png")

            signalType = cmdInputs.addDropDownCommandInput(
                "signal_type_w",
                "Signal Type",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            signalType.isFullWidth = True
            signalType.listItems.add("‎", True, icon_paths.signalIcons["PWM"])
            signalType.listItems.add("‎", False, icon_paths.signalIcons["CAN"])
            signalType.listItems.add("‎", False, icon_paths.signalIcons["PASSIVE"])
            signalType.tooltip = "Signal type"

            row = wheelTableInput.rowCount

            wheelTableInput.addCommandInput(icon, row, 0)
            wheelTableInput.addCommandInput(name, row, 1)
            wheelTableInput.addCommandInput(wheelType, row, 2)
            wheelTableInput.addCommandInput(signalType, row, 3)

        except:
            logging.getLogger("{INTERNAL_ID}.ui.ConfigCommand.addWheelToTable()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )
