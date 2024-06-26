""" Generate all the front-end command inputs and GUI elements.
    links the Configuration Command seen when pressing the Synthesis button in the Addins Panel
"""

from enum import Enum
import platform

from ..Parser.SynthesisParser.Utilities import guid_occurrence
from ..general_imports import *
from ..configure import NOTIFIED, write_configuration
from ..Analytics.alert import showAnalyticsAlert
from . import Helper, OsHelper, CustomGraphics, IconPaths, FileDialogConfig
from ..Parser.ExporterOptions import (
    Gamepiece,
    ExportMode,
    ExporterOptions,
    Wheel,
    WheelType,
    SignalType,
    PreferredUnits,
)
from .Configuration.SerialCommand import SerialCommand

# Transition: AARD-1685
# In the future all components should be handled in this way.
from .JointConfigTab import (
    createJointConfigTab,
    addJointToConfigTab,
    removeJointFromConfigTab,
    removeIndexedJointFromConfigTab,
    getSelectedJoints,
    resetSelectedJoints
)

import adsk.core
import adsk.fusion
import traceback
import logging
import os

from ..Parser.SynthesisParser.Parser import Parser

# ====================================== CONFIG COMMAND ======================================

"""
INPUTS_ROOT (adsk.fusion.CommandInputs):
    - Provides access to the set of all commandInput UI elements in the panel
"""
INPUTS_ROOT = None

"""
These lists are crucial, and contain all of the relevant object selections.
- WheelListGlobal: list of wheels (adsk.fusion.Occurrence)
- GamepieceListGlobal: list of gamepieces (adsk.fusion.Occurrence)
"""
WheelListGlobal = []
GamepieceListGlobal = []

# Default to compressed files
compress = True


def GUID(arg):
    """### Will return command object when given a string GUID, or the string GUID of an object (depending on arg value)

    Args:
        arg str | object: Either a command input object or command input GUID

    Returns:
        str | object: Either a command input object or command input GUID
    """
    if type(arg) == str:
        object = gm.app.activeDocument.design.findEntityByToken(arg)[0]
        return object
    else:  # type(obj)
        return arg.entityToken


def wheelTable():
    """### Returns the wheel table command input

    Returns:
        adsk.fusion.TableCommandInput
    """
    return INPUTS_ROOT.itemById("wheel_table")


def jointTable():
    """### Returns the joint table command input

    Returns:
        adsk.fusion.TableCommandInput
    """
    return INPUTS_ROOT.itemById("joint_table")


def gamepieceTable():
    """### Returns the gamepiece table command input

    Returns:
        adsk.fusion.TableCommandInput
    """
    return INPUTS_ROOT.itemById("gamepiece_table")


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


class FullMassCalculation:
    def __init__(self):
        self.totalMass = 0.0
        self.bRepMassInRoot()
        self.traverseOccurrenceHierarchy()

    def bRepMassInRoot(self):
        try:
            for body in gm.app.activeDocument.design.rootComponent.bRepBodies:
                if not body.isLightBulbOn:
                    continue
                physical = body.getPhysicalProperties(
                    adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                )
                self.totalMass += physical.mass
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

    def traverseOccurrenceHierarchy(self):
        try:
            for occ in gm.app.activeDocument.design.rootComponent.allOccurrences:
                if not occ.isLightBulbOn:
                    continue

                for body in occ.component.bRepBodies:
                    if not body.isLightBulbOn:
                        continue
                    physical = body.getPhysicalProperties(
                        adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                    )
                    self.totalMass += physical.mass
        except:
            pass
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

    def getTotalMass(self):
        return self.totalMass


class ConfigureCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    """### Start the Command Input Object and define all of the input groups to create our ParserOptions object.

    Notes:
        - linked and called from (@ref HButton) and linked
        - will be called from (@ref Events.py)
    """

    def __init__(self, configure):
        super().__init__()
        self.log = logging.getLogger(f"{INTERNAL_ID}.UI.{self.__class__.__name__}")
        self.designAttrs = adsk.core.Application.get().activeProduct.attributes

    def notify(self, args):
        try:
            exporterOptions = ExporterOptions().readFromDesign()

            if not Helper.check_solid_open():
                return

            global NOTIFIED  # keep this global
            if not NOTIFIED:
                showAnalyticsAlert()
                NOTIFIED = True
                write_configuration("analytics", "notified", "yes")

            if A_EP:
                A_EP.send_view("export_panel")

            eventArgs = adsk.core.CommandCreatedEventArgs.cast(args)
            cmd = eventArgs.command  # adsk.core.Command

            # Set to false so won't automatically export on switch context
            cmd.isAutoExecute = False
            cmd.isExecutedWhenPreEmpted = False
            cmd.okButtonText = "Export"  # replace default OK text with "export"
            cmd.setDialogInitialSize(400, 350)  # these aren't working for some reason...
            cmd.setDialogMinimumSize(400, 350)  # these aren't working for some reason...

            global INPUTS_ROOT  # Global CommandInputs arg
            INPUTS_ROOT = cmd.commandInputs

            # ====================================== GENERAL TAB ======================================
            """
            Creates the general tab.
                - Parent container for all the command inputs in the tab.
            """
            inputs = INPUTS_ROOT.addTabCommandInput(
                "general_settings", "General"
            ).children

            # ~~~~~~~~~~~~~~~~ HELP FILE ~~~~~~~~~~~~~~~~
            """
            Sets the small "i" icon in bottom left of the panel.
                - This is an HTML file that has a script to redirect to exporter workflow tutorial.
            """
            cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

            # ~~~~~~~~~~~~~~~~ EXPORT MODE ~~~~~~~~~~~~~~~~
            """
            Dropdown to choose whether to export robot or field element
            """
            dropdownExportMode = inputs.addDropDownCommandInput(
                "mode",
                "Export Mode",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )

            dynamic = exporterOptions.exportMode == ExportMode.ROBOT
            dropdownExportMode.listItems.add("Dynamic", dynamic)
            dropdownExportMode.listItems.add("Static", not dynamic)

            dropdownExportMode.tooltip = "Export Mode"
            dropdownExportMode.tooltipDescription = (
                "<hr>Does this object move dynamically?"
            )

            # ~~~~~~~~~~~~~~~~ WEIGHT CONFIGURATION ~~~~~~~~~~~~~~~~
            """
            Table for weight config.
                - Used this to align multiple commandInputs on the same row
            """
            weightTableInput = self.createTableInput(
                "weight_table",
                "Weight Table",
                inputs,
                4,
                "3:2:2:1",
                1,
            )
            weightTableInput.tablePresentationStyle = (
                2  # set transparent background for table
            )

            weight_name = inputs.addStringValueInput("weight_name", "Weight")
            weight_name.value = "Weight"
            weight_name.isReadOnly = True

            auto_calc_weight = self.createBooleanInput(
                "auto_calc_weight",
                "‎",
                inputs,
                checked=False,
                tooltip="Approximate the weight of your robot assembly.",
                tooltipadvanced="<i>This may take a moment...</i>",
                enabled=True,
                isCheckBox=False,
            )
            auto_calc_weight.resourceFolder = IconPaths.stringIcons["calculate-enabled"]
            auto_calc_weight.isFullWidth = True

            imperialUnits = exporterOptions.preferredUnits == PreferredUnits.IMPERIAL
            if imperialUnits:
                # ExporterOptions always contains the metric value
                displayWeight = exporterOptions.robotWeight * 2.2046226218
            else:
                displayWeight = exporterOptions.robotWeight

            weight_input = inputs.addValueInput(
                "weight_input",
                "Weight Input",
                "",
                adsk.core.ValueInput.createByReal(displayWeight),
            )
            weight_input.tooltip = "Robot weight"
            weight_input.tooltipDescription = """<tt>(in pounds)</tt><hr>This is the weight of the entire robot assembly."""

            weight_unit = inputs.addDropDownCommandInput(
                "weight_unit",
                "Weight Unit",
                adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )

            weight_unit.listItems.add("‎", imperialUnits, IconPaths.massIcons["LBS"])
            weight_unit.listItems.add("‎", not imperialUnits, IconPaths.massIcons["KG"])
            weight_unit.tooltip = "Unit of mass"
            weight_unit.tooltipDescription = (
                "<hr>Configure the unit of mass for the weight calculation."
            )

            weightTableInput.addCommandInput(
                weight_name, 0, 0
            )  # add command inputs to table
            weightTableInput.addCommandInput(
                auto_calc_weight, 0, 1
            )  # add command inputs to table
            weightTableInput.addCommandInput(
                weight_input, 0, 2
            )  # add command inputs to table
            weightTableInput.addCommandInput(
                weight_unit, 0, 3
            )  # add command inputs to table

            # ~~~~~~~~~~~~~~~~ WHEEL CONFIGURATION ~~~~~~~~~~~~~~~~
            """
            Wheel configuration command input group
                - Container for wheel selection Table
            """
            wheelConfig = inputs.addGroupCommandInput(
                "wheel_config", "Wheel Configuration"
            )
            wheelConfig.isExpanded = True
            wheelConfig.isEnabled = True
            wheelConfig.tooltip = (
                "Select and define the drive-train wheels in your assembly."
            )

            wheel_inputs = wheelConfig.children

            # WHEEL SELECTION TABLE
            """
            All selected wheel occurrences appear here.
            """
            wheelTableInput = self.createTableInput(
                "wheel_table",
                "Wheel Table",
                wheel_inputs,
                4,
                "1:4:2:2",
                50,
            )

            addWheelInput = wheel_inputs.addBoolValueInput(
                "wheel_add", "Add", False
            )  # add button

            removeWheelInput = wheel_inputs.addBoolValueInput(  # remove button
                "wheel_delete", "Remove", False
            )

            addWheelInput.tooltip = "Add a wheel joint"  # tooltips
            removeWheelInput.tooltip = "Remove a wheel joint"

            wheelSelectInput = wheel_inputs.addSelectionInput(
                "wheel_select",
                "Selection",
                "Select the wheels joints in your drive-train assembly.",
            )
            wheelSelectInput.addSelectionFilter(
                "Joints"
            )  # filter selection to only occurrences

            wheelSelectInput.setSelectionLimits(0)  # no selection count limit
            wheelSelectInput.isEnabled = False
            wheelSelectInput.isVisible = False

            wheelTableInput.addToolbarCommandInput(
                addWheelInput
            )  # add buttons to the toolbar
            wheelTableInput.addToolbarCommandInput(
                removeWheelInput
            )  # add buttons to the toolbar

            wheelTableInput.addCommandInput(  # create textbox input using helper (component name)
                self.createTextBoxInput(
                    "name_header", "Name", wheel_inputs, "Joint name", bold=False
                ),
                0,
                1,
            )

            wheelTableInput.addCommandInput(
                self.createTextBoxInput(  # wheel type header
                    "parent_header",
                    "Parent",
                    wheel_inputs,
                    "Wheel type",
                    background="#d9d9d9",  # textbox header background color
                ),
                0,
                2,
            )

            wheelTableInput.addCommandInput(
                self.createTextBoxInput(  # Signal type header
                    "signal_header",
                    "Signal",
                    wheel_inputs,
                    "Signal type",
                    background="#d9d9d9",  # textbox header background color
                ),
                0,
                3,
            )

            if exporterOptions.wheels:
                for wheel in exporterOptions.wheels:
                    wheelEntity = gm.app.activeDocument.design.findEntityByToken(wheel.jointToken)[0]
                    addWheelToTable(wheelEntity)

            # Transition: AARD-1685
            createJointConfigTab(args)
            if exporterOptions.joints:
                for synJoint in exporterOptions.joints:
                    fusionJoint = gm.app.activeDocument.design.findEntityByToken(synJoint.jointToken)[0]
                    addJointToConfigTab(fusionJoint, synJoint)
            else:
                for joint in [
                    *gm.app.activeDocument.design.rootComponent.allJoints,
                    *gm.app.activeDocument.design.rootComponent.allAsBuiltJoints
                ]:
                    if joint.jointMotion.jointType in (JointMotions.REVOLUTE.value, JointMotions.SLIDER.value) and not joint.isSuppressed:
                        addJointToConfigTab(joint)

            # ~~~~~~~~~~~~~~~~ GAMEPIECE CONFIGURATION ~~~~~~~~~~~~~~~~
            """
            Gamepiece group command input, isVisible=False by default
                - Container for gamepiece selection table
            """
            gamepieceConfig = inputs.addGroupCommandInput(
                "gamepiece_config", "Gamepiece Configuration"
            )
            gamepieceConfig.isExpanded = True
            gamepieceConfig.isVisible = False
            gamepieceConfig.tooltip = "Select and define the gamepieces in your field."
            gamepiece_inputs = gamepieceConfig.children

            # GAMEPIECE MASS CONFIGURATION
            """
            Mass unit dropdown and calculation for gamepiece elements
            """
            weightTableInput_f = self.createTableInput(
                "weight_table_f", "Weight Table", gamepiece_inputs, 3, "6:2:1", 1
            )
            weightTableInput_f.tablePresentationStyle = 2  # set to clear background

            weight_name_f = gamepiece_inputs.addStringValueInput("weight_name", "Weight")
            weight_name_f.value = "Unit of Mass"
            weight_name_f.isReadOnly = True

            auto_calc_weight_f = self.createBooleanInput(  # CALCULATE button
                "auto_calc_weight_f",
                "‎",
                gamepiece_inputs,
                checked=False,
                tooltip="Approximate the weight of all your selected gamepieces.",
                enabled=True,
                isCheckBox=False,
            )
            auto_calc_weight_f.resourceFolder = IconPaths.stringIcons["calculate-enabled"]
            auto_calc_weight_f.isFullWidth = True

            weight_unit_f = gamepiece_inputs.addDropDownCommandInput(
                "weight_unit_f",
                "Unit of Mass",
                adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            weight_unit_f.listItems.add(
                "‎", True, IconPaths.massIcons["LBS"]
            )  # add listdropdown mass options
            weight_unit_f.listItems.add(
                "‎", False, IconPaths.massIcons["KG"]
            )  # add listdropdown mass options
            weight_unit_f.tooltip = "Unit of mass"
            weight_unit_f.tooltipDescription = (
                "<hr>Configure the unit of mass for for the weight calculation."
            )

            weightTableInput_f.addCommandInput(
                weight_name_f, 0, 0
            )  # add command inputs to table
            weightTableInput_f.addCommandInput(
                auto_calc_weight_f, 0, 1
            )  # add command inputs to table
            weightTableInput_f.addCommandInput(
                weight_unit_f, 0, 2
            )  # add command inputs to table

            # GAMEPIECE SELECTION TABLE
            """
            All selected gamepieces appear here
            """
            gamepieceTableInput = self.createTableInput(
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

            gamepieceTableInput.addToolbarCommandInput(addFieldInput)
            gamepieceTableInput.addToolbarCommandInput(removeFieldInput)

            """
            Gamepiece table column headers. (the permanent captions in the first row of table)
            """
            gamepieceTableInput.addCommandInput(
                self.createTextBoxInput(
                    "e_header",
                    "Gamepiece name",
                    gamepiece_inputs,
                    "Gamepiece",
                    bold=False,
                ),
                0,
                1,
            )

            gamepieceTableInput.addCommandInput(
                self.createTextBoxInput(
                    "w_header",
                    "Gamepiece weight",
                    gamepiece_inputs,
                    "Weight",
                    background="#d9d9d9",
                ),
                0,
                2,
            )

            gamepieceTableInput.addCommandInput(
                self.createTextBoxInput(
                    "f_header",
                    "Friction coefficient",
                    gamepiece_inputs,
                    "Friction coefficient",
                    background="#d9d9d9",
                ),
                0,
                3,
            )

            # ====================================== ADVANCED TAB ======================================
            """
            Creates the advanced tab, which is the parent container for internal command inputs
            """
            advancedSettings = INPUTS_ROOT.addTabCommandInput(
                "advanced_settings", "Advanced"
            )
            advancedSettings.tooltip = "Additional Advanced Settings to change how your model will be translated into Unity."
            a_input = advancedSettings.children

            # ~~~~~~~~~~~~~~~~ EXPORTER SETTINGS ~~~~~~~~~~~~~~~~
            """
            Exporter settings group command
            """
            exporterSettings = a_input.addGroupCommandInput(
                "exporter_settings", "Exporter Settings"
            )
            exporterSettings.isExpanded = True
            exporterSettings.isEnabled = True
            exporterSettings.tooltip = "tooltip"  # TODO: update tooltip
            exporter_settings = exporterSettings.children

            self.createBooleanInput(
                "compress",
                "Compress Output",
                exporter_settings,
                checked=exporterOptions.compressOutput,
                tooltip="Compress the output file for a smaller file size.",
                tooltipadvanced="<hr>Use the GZIP compression system to compress the resulting file which will be opened in the simulator, perfect if you want to share the file.<br>",
                enabled=True,
            )

            self.createBooleanInput(
                "export_as_part",
                "Export As Part",
                exporter_settings,
                checked=exporterOptions.exportAsPart,
                tooltip="Use to export as a part for Mix And Match",
                enabled=True,
            )

            # ~~~~~~~~~~~~~~~~ PHYSICS SETTINGS ~~~~~~~~~~~~~~~~
            """
            Physics settings group command
            """
            physicsSettings = a_input.addGroupCommandInput(
                "physics_settings", "Physics Settings"
            )

            physicsSettings.isExpanded = False
            physicsSettings.isEnabled = True
            physicsSettings.tooltip = "tooltip"  # TODO: update tooltip
            physics_settings = physicsSettings.children

            # AARD-1687
            # Should also be commented out / removed?
            # This would cause problems elsewhere but I can't tell i f
            # this is even being used.
            frictionOverrideTable = self.createTableInput(
                "friction_override_table",
                "",
                physics_settings,
                2,
                "1:2",
                1,
                columnSpacing=25,
            )
            frictionOverrideTable.tablePresentationStyle = 2
            # frictionOverrideTable.isFullWidth = True

            frictionOverride = self.createBooleanInput(
                "friction_override",
                "",
                physics_settings,
                checked=False,
                tooltip="Manually override the default friction values on the bodies in the assembly.",
                enabled=True,
                isCheckBox=False,
            )
            frictionOverride.resourceFolder = IconPaths.stringIcons[
                "friction_override-enabled"
            ]
            frictionOverride.isFullWidth = True

            valueList = [1]
            for i in range(20):
                valueList.append(i / 20)

            frictionCoeff = physics_settings.addFloatSliderListCommandInput(
                "friction_coeff_override", "Friction Coefficient", "", valueList
            )
            frictionCoeff.isVisible = False
            frictionCoeff.valueOne = 0.5
            frictionCoeff.tooltip = "Friction coefficient of field element."
            frictionCoeff.tooltipDescription = (
                "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
            )

            frictionOverrideTable.addCommandInput(frictionOverride, 0, 0)
            frictionOverrideTable.addCommandInput(frictionCoeff, 0, 1)

            # ~~~~~~~~~~~~~~~~ JOINT SETTINGS ~~~~~~~~~~~~~~~~
            """
            Joint settings group command
            """

            # Transition: AARD-1689
            # Should possibly be implemented later?

            # jointsSettings = a_input.addGroupCommandInput(
            #     "joints_settings", "Joints Settings"
            # )
            # jointsSettings.isExpanded = False
            # jointsSettings.isEnabled = True
            # jointsSettings.tooltip = "tooltip"  # TODO: update tooltip
            # joints_settings = jointsSettings.children

            # self.createBooleanInput(
            #     "kinematic_only",
            #     "Kinematic Only",
            #     joints_settings,
            #     checked=False,
            #     tooltip="tooltip",  # TODO: update tooltip
            #     enabled=True,
            # )

            # self.createBooleanInput(
            #     "calculate_limits",
            #     "Calculate Limits",
            #     joints_settings,
            #     checked=True,
            #     tooltip="tooltip",  # TODO: update tooltip
            #     enabled=True,
            # )

            # self.createBooleanInput(
            #     "auto_assign_ids",
            #     "Auto-Assign ID's",
            #     joints_settings,
            #     checked=True,
            #     tooltip="tooltip",  # TODO: update tooltip
            #     enabled=True,
            # )

            # ~~~~~~~~~~~~~~~~ CONTROLLER SETTINGS ~~~~~~~~~~~~~~~~
            """
            Controller settings group command
            """

            # Transition: AARD-1689
            # Should possibly be implemented later?

            # controllerSettings = a_input.addGroupCommandInput(
            #     "controller_settings", "Controller Settings"
            # )

            # controllerSettings.isExpanded = False
            # controllerSettings.isEnabled = True
            # controllerSettings.tooltip = "tooltip"  # TODO: update tooltip
            # controller_settings = controllerSettings.children

            # self.createBooleanInput(  # export signals checkbox
            #     "export_signals",
            #     "Export Signals",
            #     controller_settings,
            #     checked=True,
            #     tooltip="tooltip",
            #     enabled=True,
            # )

            # clear all selections before instantiating handlers.
            gm.ui.activeSelections.clear()

            # ====================================== EVENT HANDLERS ======================================
            """
            Instantiating all the event handlers
            """

            onExecute = ConfigureCommandExecuteHandler()
            cmd.execute.add(onExecute)
            gm.handlers.append(onExecute)  # 0

            onInputChanged = ConfigureCommandInputChanged(cmd)
            cmd.inputChanged.add(onInputChanged)
            gm.handlers.append(onInputChanged)  # 1

            onExecutePreview = CommandExecutePreviewHandler(cmd)
            cmd.executePreview.add(onExecutePreview)
            gm.handlers.append(onExecutePreview)  # 2

            onSelect = MySelectHandler(cmd)
            cmd.select.add(onSelect)
            gm.handlers.append(onSelect)  # 3

            onPreSelect = MyPreSelectHandler(cmd)
            cmd.preSelect.add(onPreSelect)
            gm.handlers.append(onPreSelect)  # 4

            onPreSelectEnd = MyPreselectEndHandler(cmd)
            cmd.preSelectEnd.add(onPreSelectEnd)
            gm.handlers.append(onPreSelectEnd)  # 5

            onDestroy = MyCommandDestroyHandler()
            cmd.destroy.add(onDestroy)
            gm.handlers.append(onDestroy)  # 8

        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    # Transition: AARD-1685
    # Functionality will be fully moved to `CreateCommandInputsHelper` in AARD-1683
    def createBooleanInput(
        self,
        _id: str,
        name: str,
        inputs: adsk.core.CommandInputs,
        tooltip="",
        tooltipadvanced="",
        checked=True,
        enabled=True,
        isCheckBox=True,
    ) -> adsk.core.BoolValueCommandInput:
        """### Simple helper to generate all of the options for me to create a boolean command input

        Args:
            _id (str): id value of the object - pretty much lowercase name
            name (str): name as displayed by the command prompt
            inputs (adsk.core.CommandInputs): parent command input container
            tooltip (str, optional): Description on hover of the checkbox. Defaults to "".
            tooltipadvanced (str, optional): Long hover description. Defaults to "".
            checked (bool, optional): Is checked by default?. Defaults to True.

        Returns:
            adsk.core.BoolValueCommandInput: Recently created command input
        """
        try:
            _input = inputs.addBoolValueInput(_id, name, isCheckBox)
            _input.value = checked
            _input.isEnabled = enabled
            _input.tooltip = tooltip
            _input.tooltipDescription = tooltipadvanced
            return _input
        except:
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.createBooleanInput()"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    # Transition: AARD-1685
    # Functionality will be fully moved to `CreateCommandInputsHelper` in AARD-1683
    def createTableInput(
        self,
        _id: str,
        name: str,
        inputs: adsk.core.CommandInputs,
        columns: int,
        ratio: str,
        maxRows: int,
        minRows=1,
        columnSpacing=0,
        rowSpacing=0,
    ) -> adsk.core.TableCommandInput:
        """### Simple helper to generate all the TableCommandInput options.

        Args:
            _id (str): unique ID of command
            name (str): displayed name
            inputs (adsk.core.CommandInputs): parent command input container
            columns (int): column count
            ratio (str): column width ratio
            maxRows (int): the maximum number of displayed rows possible
            minRows (int, optional): the minimum number of displayed rows. Defaults to 1.
            columnSpacing (int, optional): spacing in between the columns, in pixels. Defaults to 0.
            rowSpacing (int, optional): spacing in between the rows, in pixels. Defaults to 0.

        Returns:
            adsk.core.TableCommandInput: created tableCommandInput
        """
        try:
            _input = inputs.addTableCommandInput(_id, name, columns, ratio)
            _input.minimumVisibleRows = minRows
            _input.maximumVisibleRows = maxRows
            _input.columnSpacing = columnSpacing
            _input.rowSpacing = rowSpacing
            return _input
        except:
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.createTableInput()"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    # Transition: AARD-1685
    # Functionality will be fully moved to `CreateCommandInputsHelper` in AARD-1683
    def createTextBoxInput(
        self,
        _id: str,
        name: str,
        inputs: adsk.core.CommandInputs,
        text: str,
        italics=True,
        bold=True,
        fontSize=10,
        alignment="center",
        rowCount=1,
        read=True,
        background="whitesmoke",
        tooltip="",
        advanced_tooltip="",
    ) -> adsk.core.TextBoxCommandInput:
        """### Helper to generate a textbox input from inputted options.

        Args:
            _id (str): unique ID
            name (str): displayed name
            inputs (adsk.core.CommandInputs): parent command input container
            text (str): the user-visible text in command
            italics (bool, optional): is italics? Defaults to True.
            bold (bool, optional): isBold? Defaults to True.
            fontSize (int, optional): fontsize. Defaults to 10.
            alignment (str, optional): HTML style alignment (left, center, right). Defaults to "center".
            rowCount (int, optional): number of rows in textbox. Defaults to 1.
            read (bool, optional): read only? Defaults to True.
            background (str, optional): background color (HTML color names or hex) Defaults to "whitesmoke".

        Returns:
            adsk.core.TextBoxCommandInput: newly created textBoxCommandInput
        """
        try:
            i = ["", ""]
            b = ["", ""]

            if bold:
                b[0] = "<b>"
                b[1] = "</b>"
            if italics:
                i[0] = "<i>"
                i[1] = "</i>"

            # simple wrapper for html formatting
            wrapper = """<body style='background-color:%s;'>
                         <div align='%s'>
                         <p style='font-size:%spx'>
                         %s%s{}%s%s
                         </p>
                         </body>
                      """.format(
                text
            )
            _text = wrapper % (background, alignment, fontSize, b[0], i[0], i[1], b[1])

            _input = inputs.addTextBoxCommandInput(_id, name, _text, rowCount, read)
            _input.tooltip = tooltip
            _input.tooltipDescription = advanced_tooltip
            return _input
        except:
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.createTextBoxInput()"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class ConfigureCommandExecuteHandler(adsk.core.CommandEventHandler):
    """### Called when Ok is pressed confirming the export

    Process Steps:

        1. Check for process open in explorer

        1.5. Open file dialog to allow file location save
            - Not always optimal if sending over socket for parse

        2. Check Socket bind

        3. Check Socket recv
            - if true send data about file location in temp path

        4. Parse file and focus on unity window

    """

    def __init__(self):
        super().__init__()
        self.log = logging.getLogger(f"{INTERNAL_ID}.UI.{self.__class__.__name__}")
        self.current = SerialCommand()

    def notify(self, args):
        try:
            eventArgs = adsk.core.CommandEventArgs.cast(args)
            exporterOptions = ExporterOptions().readFromDesign()

            if eventArgs.executeFailed:
                self.log.error("Could not execute configuration due to failure")
                return

            export_as_part_boolean = (
                eventArgs.command.commandInputs.itemById("advanced_settings")
                .children.itemById("exporter_settings")
                .children.itemById("export_as_part")
            ).value

            processedFileName = gm.app.activeDocument.name.replace(" ", "_")
            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            if dropdownExportMode.selectedItem.index == 0:
                isRobot = True
            elif dropdownExportMode.selectedItem.index == 1:
                isRobot = False

            if isRobot:
                savepath = FileDialogConfig.SaveFileDialog(
                    defaultPath=exporterOptions.fileLocation,
                    ext="Synthesis File (*.synth)",
                )
            else:
                savepath = FileDialogConfig.SaveFileDialog(
                    defaultPath=exporterOptions.fileLocation
                )

            if not savepath:
                # save was canceled
                return

            updatedPath = pathlib.Path(savepath).parent
            if updatedPath != self.current.filePath:
                self.current.filePath = str(updatedPath)

            adsk.doEvents()
            # get active document
            design = gm.app.activeDocument.design
            name = design.rootComponent.name.rsplit(" ", 1)[0]
            version = design.rootComponent.name.rsplit(" ", 1)[1]

            _exportWheels = []  # all selected wheels, formatted for parseOptions
            _exportGamepieces = []  # TODO work on the code to populate Gamepiece
            _robotWeight = float
            _mode = ExportMode.ROBOT

            """
            Loops through all rows in the wheel table to extract all the input values
            """
            onSelect = gm.handlers[3]
            wheelTableInput = wheelTable()
            for row in range(wheelTableInput.rowCount):
                if row == 0:
                    continue

                wheelTypeIndex = wheelTableInput.getInputAtPosition(
                    row, 2
                ).selectedItem.index  # This must be either 0 or 1 for standard or omni

                signalTypeIndex = wheelTableInput.getInputAtPosition(
                    row, 3
                ).selectedItem.index

                _exportWheels.append(
                    Wheel(
                        WheelListGlobal[row - 1].entityToken,
                        WheelType(wheelTypeIndex + 1),
                        SignalType(signalTypeIndex + 1),
                        # onSelect.wheelJointList[row-1][0] # GUID of wheel joint. if no joint found, default to None
                    )
                )

            """
            Loops through all rows in the gamepiece table to extract the input values
            """
            gamepieceTableInput = gamepieceTable()
            weight_unit_f = INPUTS_ROOT.itemById("weight_unit_f")
            for row in range(gamepieceTableInput.rowCount):
                if row == 0:
                    continue

                weightValue = gamepieceTableInput.getInputAtPosition(
                    row, 2
                ).value  # weight/mass input, float

                if weight_unit_f.selectedItem.index == 0:
                    weightValue /= 2.2046226218

                frictionValue = gamepieceTableInput.getInputAtPosition(
                    row, 3
                ).valueOne  # friction value, float

                _exportGamepieces.append(
                    Gamepiece(
                        guid_occurrence(GamepieceListGlobal[row - 1]),
                        weightValue,
                        frictionValue,
                    )
                )

            """
            Robot Weight
            """
            weight_input = INPUTS_ROOT.itemById("weight_input")
            weight_unit = INPUTS_ROOT.itemById("weight_unit")

            if weight_unit.selectedItem.index == 0:
                selectedUnits = PreferredUnits.IMPERIAL
                _robotWeight = float(weight_input.value) / 2.2046226218
            else:
                selectedUnits = PreferredUnits.METRIC
                _robotWeight = float(weight_input.value)

            """
            Export Mode
            """
            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            if dropdownExportMode.selectedItem.index == 0:
                _mode = ExportMode.ROBOT
            elif dropdownExportMode.selectedItem.index == 1:
                _mode = ExportMode.FIELD

            global compress
            compress = (
                eventArgs.command.commandInputs.itemById("advanced_settings")
                .children.itemById("exporter_settings")
                .children.itemById("compress")
            ).value

            exporterOptions = ExporterOptions(
                savepath,
                name,
                version,
                materials=0,
                joints=getSelectedJoints(),
                wheels=_exportWheels,
                gamepieces=_exportGamepieces,
                preferredUnits=selectedUnits,
                robotWeight=_robotWeight,
                exportMode=_mode,
                compressOutput=compress,
                exportAsPart=export_as_part_boolean,
            )

            Parser(exporterOptions).export()
            exporterOptions.writeToDesign()

            # All selections should be reset AFTER a successful export and save.
            # If we run into an exporting error we should return back to the panel with all current options
            # still in tact. Even if they did not save.
            resetSelectedJoints()
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


class CommandExecutePreviewHandler(adsk.core.CommandEventHandler):
    """### Gets an event that is fired when the command has completed gathering the required input and now needs to perform a preview.

    Args:
        adsk (CommandEventHandler): Command event handler that a client derives from to handle events triggered by a CommandEvent.
    """

    def __init__(self, cmd) -> None:
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        """Notify member called when a command event is triggered

        Args:
            args (CommandEventArgs): command event argument
        """
        try:
            eventArgs = adsk.core.CommandEventArgs.cast(args)
            # inputs = eventArgs.command.commandInputs # equivalent to INPUTS_ROOT global

            auto_calc_weight_f = INPUTS_ROOT.itemById("auto_calc_weight_f")

            removeWheelInput = INPUTS_ROOT.itemById("wheel_delete")
            removeJointInput = INPUTS_ROOT.itemById("jointRemoveButton")
            removeFieldInput = INPUTS_ROOT.itemById("field_delete")

            addWheelInput = INPUTS_ROOT.itemById("wheel_add")
            addJointInput = INPUTS_ROOT.itemById("jointAddButton")
            addFieldInput = INPUTS_ROOT.itemById("field_add")

            wheelTableInput = wheelTable()

            inputs = args.command.commandInputs
            jointTableInput: adsk.core.TableCommandInput = inputs.itemById("jointSettings").children.itemById("jointTable")

            gamepieceTableInput = gamepieceTable()

            if wheelTableInput.rowCount <= 1:
                removeWheelInput.isEnabled = False
            else:
                removeWheelInput.isEnabled = True

            if jointTableInput.rowCount <= 1:
                removeJointInput.isEnabled = False
            else:
                removeJointInput.isEnabled = True

            if gamepieceTableInput.rowCount <= 1:
                removeFieldInput.isEnabled = auto_calc_weight_f.isEnabled = False
            else:
                removeFieldInput.isEnabled = auto_calc_weight_f.isEnabled = True

            if not addWheelInput.isEnabled or not removeWheelInput:
                # for wheel in WheelListGlobal:
                #     wheel.component.opacity = 0.25
                #     CustomGraphics.createTextGraphics(wheel, WheelListGlobal)

                gm.app.activeViewport.refresh()
            else:
                gm.app.activeDocument.design.rootComponent.opacity = 1
                for (
                    group
                ) in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
                    group.deleteMe()

            if (
                not addJointInput.isEnabled or not removeJointInput
            ):  # TODO: improve joint highlighting
                # for joint in JointListGlobal:
                #    CustomGraphics.highlightJointedOccurrences(joint)

                # gm.app.activeViewport.refresh()
                gm.app.activeDocument.design.rootComponent.opacity = 0.15
            # else:
            # for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
            #    group.deleteMe()
            # gm.app.activeDocument.design.rootComponent.opacity = 1

            if not addFieldInput.isEnabled or not removeFieldInput:
                for gamepiece in GamepieceListGlobal:
                    gamepiece.component.opacity = 0.25
                    CustomGraphics.createTextGraphics(gamepiece, GamepieceListGlobal)
            else:
                gm.app.activeDocument.design.rootComponent.opacity = 1
        except AttributeError:
            pass
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class MySelectHandler(adsk.core.SelectionEventHandler):
    """### Event fires when the user selects an entity.
    ##### This is different from a preselection where an entity is shown as being available for selection as the mouse passes over the entity. This is the actual selection where the user has clicked the mouse on the entity.

    Args: SelectionEventHandler
    """

    lastInputCmd = None

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

        self.allWheelPreselections = []  # all child occurrences of selections
        self.allGamepiecePreselections = (
            []
        )  # all child gamepiece occurrences of selections

        self.selectedOcc = None  # selected occurrence (if there is one)
        self.selectedJoint = None  # selected joint (if there is one)

        self.wheelJointList = []
        self.algorithmicSelection = True

    def traverseAssembly(
        self, child_occurrences: adsk.fusion.OccurrenceList, jointedOcc: dict
    ):  # recursive traversal to check if children are jointed
        """### Traverses the entire occurrence hierarchy to find a match (jointed occurrence) in self.occurrence

        Args:
            child_occurrences (adsk.fusion.OccurrenceList): a list of child occurrences

        Returns:
            occ (Occurrence): if a match is found, return the jointed occurrence
            None: if no match is found
        """
        try:
            for occ in child_occurrences:
                for joint, value in jointedOcc.items():
                    if occ in value:
                        return [joint, occ]  # occurrence that is jointed

                if occ.childOccurrences:  # if occurrence has children, traverse sub-tree
                    self.traverseAssembly(occ.childOccurrences, jointedOcc)
            return None  # no jointed occurrence found
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.traverseAssembly()"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    def wheelParent(self, occ: adsk.fusion.Occurrence):
        """### Identify an occurrence that encompasses the entire wheel component.

        Process:

            1. if the selection has no parent, return the selection.

            2. if the selection is directly jointed, return the selection.

            3. else keep climbing the occurrence tree until no parent is found.

            - if a jointed occurrence is in the tree, return the selection parent.

            - if no jointed occurrence was found, return the selection.

        Args:
            occ (Occurrence): The selected child occurrence

        Returns:
            occ (Occurrence): Wheel parent
        """
        try:
            parent = occ.assemblyContext
            jointedOcc = {}  # dictionary with all jointed occurrences

            try:
                for joint in occ.joints:
                    if (
                        joint.jointMotion.jointType
                        == adsk.fusion.JointTypes.RevoluteJointType
                    ):
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]
            except:
                for joint in occ.component.joints:
                    if (
                        joint.jointMotion.jointType
                        == adsk.fusion.JointTypes.RevoluteJointType
                    ):
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]

            if parent == None:  # no parent occurrence
                # gm.ui.messageBox("Selection has no parent occurrence.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
                return [None, occ]  # return what is selected

            for joint in gm.app.activeDocument.design.rootComponent.allJoints:
                if (
                    joint.jointMotion.jointType
                    != adsk.fusion.JointTypes.RevoluteJointType
                ):
                    continue
                jointedOcc[joint.entityToken] = [
                    joint.occurrenceOne,
                    joint.occurrenceTwo,
                ]

            parentLevel = 1  # the number of nodes above the one selected
            returned = None  # the returned value of traverseAssembly()
            parentOccurrence = occ  # the parent occurrence that will be returned
            treeParent = parent  # each parent that will traverse up in algorithm.

            while treeParent != None:  # loops until reaches top-level component
                returned = self.traverseAssembly(treeParent.childOccurrences, jointedOcc)

                if returned != None:
                    for i in range(parentLevel):
                        parentOccurrence = parentOccurrence.assemblyContext

                    # gm.ui.messageBox("Joint found.\nReturning parent occurrence.\n\n" + "Selected occurrence:\n--> " + occ.name + "\nParent:\n--> " + parentOccurrence.name + "\nJoint:\n--> " + returned[0] + "\nNodes above selection:\n--> " + str(parentLevel))
                    return [returned[0], parentOccurrence]

                parentLevel += 1
                treeParent = treeParent.assemblyContext
            # gm.ui.messageBox("No jointed occurrence found.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
            return [None, occ]  # no jointed occurrence found, return what is selected
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.wheelParent()"
            ).error("Failed:\n{}".format(traceback.format_exc()))
            # gm.ui.messageBox("Selection's component has no referenced joints.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
            return [None, occ]

    def notify(self, args: adsk.core.SelectionEventArgs):
        """### Notify member is called when a selection event is triggered.

        Args:
            args (SelectionEventArgs): A selection event argument
        """
        try:
            # eventArgs = adsk.core.SelectionEventArgs.cast(args)

            self.selectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)
            self.selectedJoint = args.selection.entity

            selectionInput = args.activeInput

            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            duplicateSelection = INPUTS_ROOT.itemById("duplicate_selection")
            # indicator = INPUTS_ROOT.itemById("algorithmic_indicator")

            if self.selectedOcc:
                self.cmd.setCursor("", 0, 0)
                if dropdownExportMode.selectedItem.index == 1:
                    occurrenceList = gm.app.activeDocument.design.rootComponent.allOccurrencesByComponent(
                        self.selectedOcc.component
                    )
                    for occ in occurrenceList:
                        if occ not in GamepieceListGlobal:
                            addGamepieceToTable(occ)
                        else:
                            removeGamePieceFromTable(GamepieceListGlobal.index(occ))

                    selectionInput.isEnabled = False
                    selectionInput.isVisible = False

            elif self.selectedJoint:
                self.cmd.setCursor("", 0, 0)
                jointType = self.selectedJoint.jointMotion.jointType
                if (
                    jointType == JointMotions.REVOLUTE.value
                    or jointType == JointMotions.SLIDER.value
                ):
                    if (
                        jointType == JointMotions.REVOLUTE.value
                        and MySelectHandler.lastInputCmd.id == "wheel_select"
                    ):
                        addWheelToTable(self.selectedJoint)
                    elif (
                        jointType == JointMotions.REVOLUTE.value
                        and MySelectHandler.lastInputCmd.id == "wheel_remove"
                    ):
                        if self.selectedJoint in WheelListGlobal:
                            removeWheelFromTable(
                                WheelListGlobal.index(self.selectedJoint)
                            )
                    else:
                        # Transition: AARD-1685
                        # Should move selection handles into respective UI modules
                        if not addJointToConfigTab(self.selectedJoint):
                            result = gm.ui.messageBox(
                                "You have already selected this joint.\n"
                                "Would you like to remove it?",
                                "Synthesis: Remove Joint Confirmation",
                                adsk.core.MessageBoxButtonTypes.YesNoButtonType,
                                adsk.core.MessageBoxIconTypes.QuestionIconType,
                            )

                            if result == adsk.core.DialogResults.DialogYes:
                                removeJointFromConfigTab(self.selectedJoint)

                    selectionInput.isEnabled = False
                    selectionInput.isVisible = False
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class MyPreSelectHandler(adsk.core.SelectionEventHandler):
    """### Event fires when a entity preselection is made (mouse hovering).
    ##### When a user is selecting geometry, they move the mouse over the model and if the entity the mouse is currently over is valid for selection it will highlight indicating that it can be selected. This process of determining what is available for selection and highlighting it is referred to as the "preselect" behavior.

    Args: SelectionEventHandler
    """

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        try:
            design = adsk.fusion.Design.cast(gm.app.activeProduct)
            preSelectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)
            preSelectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

            onSelect = gm.handlers[3]  # select handler

            if (not preSelectedOcc and not preSelectedJoint) or not design:
                self.cmd.setCursor("", 0, 0)
                return

            preSelected = preSelectedOcc if preSelectedOcc else preSelectedJoint

            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            if preSelected and design:
                if dropdownExportMode.selectedItem.index == 0:  # Dynamic
                    if preSelected.entityToken in onSelect.allWheelPreselections:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["remove"],
                            0,
                            0,
                        )
                    else:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["add"],
                            0,
                            0,
                        )

                elif dropdownExportMode.selectedItem.index == 1:  # Static
                    if preSelected.entityToken in onSelect.allGamepiecePreselections:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["remove"],
                            0,
                            0,
                        )
                    else:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["add"],
                            0,
                            0,
                        )
            else:  # Should literally be impossible? - Brandon
                self.cmd.setCursor("", 0, 0)
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class MyPreselectEndHandler(adsk.core.SelectionEventHandler):
    """### Event fires when the mouse is moved away from an entity that was in a preselect state.

    Args: SelectionEventArgs
    """

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        try:
            design = adsk.fusion.Design.cast(gm.app.activeProduct)
            preSelectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)
            preSelectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

            if (preSelectedOcc or preSelectedJoint) and design:
                self.cmd.setCursor(
                    "", 0, 0
                )  # if preselection ends (mouse off of design), reset the mouse icon to default
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class ConfigureCommandInputChanged(adsk.core.InputChangedEventHandler):
    """### Gets an event that is fired whenever an input value is changed.
        - Button pressed, selection made, switching tabs, etc...

    Args: InputChangedEventHandler
    """

    def __init__(self, cmd):
        super().__init__()
        self.log = logging.getLogger(
            f"{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
        )
        self.cmd = cmd
        self.allWeights = [None, None]  # [lbs, kg]
        self.isLbs = True
        self.isLbs_f = True

    def reset(self):
        """### Process:
        - Reset the mouse icon to default
        - Clear active selections
        """
        try:
            self.cmd.setCursor("", 0, 0)
            gm.ui.activeSelections.clear()
        except:
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.reset()"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    def weight(self, isLbs=True):  # maybe add a progress dialog??
        """### Get the total design weight using the predetermined units.

        Args:
            isLbs (bool, optional): Is selected mass unit pounds? Defaults to True.

        Returns:
            value (float): weight value in specified unit
        """
        try:
            if gm.app.activeDocument.design:
                massCalculation = FullMassCalculation()
                totalMass = massCalculation.getTotalMass()

                value = float

                self.allWeights[0] = round(totalMass * 2.2046226218, 2)

                self.allWeights[1] = round(totalMass, 2)

                if isLbs:
                    value = self.allWeights[0]
                else:
                    value = self.allWeights[1]

                value = round(value, 2)  # round weight to 2 decimals places
                return value
        except:
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.weight()"
            ).error("Failed:\n{}".format(traceback.format_exc()))

    def notify(self, args):
        try:
            eventArgs = adsk.core.InputChangedEventArgs.cast(args)
            cmdInput = eventArgs.input
            MySelectHandler.lastInputCmd = cmdInput
            inputs = cmdInput.commandInputs
            onSelect = gm.handlers[3]

            frictionCoeff = INPUTS_ROOT.itemById("friction_coeff_override")

            wheelSelect = inputs.itemById("wheel_select")
            jointSelect = inputs.itemById("jointSelection")
            gamepieceSelect = inputs.itemById("gamepiece_select")

            wheelTableInput = wheelTable()

            jointTableInput: adsk.core.TableCommandInput = args.inputs.itemById("jointTable")

            gamepieceTableInput = gamepieceTable()
            weightTableInput = inputs.itemById("weight_table")

            weight_input = INPUTS_ROOT.itemById("weight_input")

            wheelConfig = inputs.itemById("wheel_config")
            jointConfig = inputs.itemById("joint_config")
            gamepieceConfig = inputs.itemById("gamepiece_config")

            addWheelInput = INPUTS_ROOT.itemById("wheel_add")
            addJointInput = INPUTS_ROOT.itemById("jointAddButton")
            addFieldInput = INPUTS_ROOT.itemById("field_add")

            indicator = INPUTS_ROOT.itemById("algorithmic_indicator")

            # gm.ui.messageBox(cmdInput.id) # DEBUG statement, displays CommandInput user-defined id

            position = int

            if cmdInput.id == "mode":
                modeDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

                if modeDropdown.selectedItem.index == 0:
                    if gamepieceConfig:
                        gm.ui.activeSelections.clear()
                        gm.app.activeDocument.design.rootComponent.opacity = 1

                        gamepieceConfig.isVisible = False
                        weightTableInput.isVisible = True

                        addFieldInput.isEnabled = wheelConfig.isVisible = (
                            jointConfig.isVisible
                        ) = True

                elif modeDropdown.selectedItem.index == 1:
                    if gamepieceConfig:
                        gm.ui.activeSelections.clear()
                        gm.app.activeDocument.design.rootComponent.opacity = 1

                        addWheelInput.isEnabled = addJointInput.isEnabled = (
                            gamepieceConfig.isVisible
                        ) = True

                        jointConfig.isVisible = wheelConfig.isVisible = (
                            weightTableInput.isVisible
                        ) = False

            elif cmdInput.id == "joint_config":
                gm.app.activeDocument.design.rootComponent.opacity = 1

            elif (
                cmdInput.id == "placeholder_w"
                or cmdInput.id == "name_w"
                or cmdInput.id == "signal_type_w"
            ):
                self.reset()

                wheelSelect.isEnabled = False
                addWheelInput.isEnabled = True

                cmdInput_str = cmdInput.id

                if cmdInput_str == "placeholder_w":
                    position = (
                        wheelTableInput.getPosition(
                            adsk.core.ImageCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )
                elif cmdInput_str == "name_w":
                    position = (
                        wheelTableInput.getPosition(
                            adsk.core.TextBoxCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )
                elif cmdInput_str == "signal_type_w":
                    position = (
                        wheelTableInput.getPosition(
                            adsk.core.DropDownCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )

                gm.ui.activeSelections.add(WheelListGlobal[position])

            elif (
                cmdInput.id == "placeholder"
                or cmdInput.id == "name_j"
                or cmdInput.id == "joint_parent"
                or cmdInput.id == "signal_type"
            ):
                self.reset()
                jointSelect.isEnabled = False
                addJointInput.isEnabled = True

            elif (
                cmdInput.id == "blank_gp"
                or cmdInput.id == "name_gp"
                or cmdInput.id == "weight_gp"
            ):
                self.reset()

                gamepieceSelect.isEnabled = False
                addFieldInput.isEnabled = True

                cmdInput_str = cmdInput.id

                if cmdInput_str == "name_gp":
                    position = (
                        gamepieceTableInput.getPosition(
                            adsk.core.TextBoxCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )
                elif cmdInput_str == "weight_gp":
                    position = (
                        gamepieceTableInput.getPosition(
                            adsk.core.ValueCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )
                elif cmdInput_str == "blank_gp":
                    position = (
                        gamepieceTableInput.getPosition(
                            adsk.core.ImageCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )
                else:
                    position = (
                        gamepieceTableInput.getPosition(
                            adsk.core.FloatSliderCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                    )

                gm.ui.activeSelections.add(GamepieceListGlobal[position])

            elif cmdInput.id == "wheel_type_w":
                self.reset()

                wheelSelect.isEnabled = False
                addWheelInput.isEnabled = True

                cmdInput_str = cmdInput.id
                position = (
                    wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )[1]
                    - 1
                )
                wheelDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

                if wheelDropdown.selectedItem.index == 0:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["standard"]
                    iconInput.tooltip = "Standard wheel"

                elif wheelDropdown.selectedItem.index == 1:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["omni"]
                    iconInput.tooltip = "Omni wheel"

                elif wheelDropdown.selectedItem.index == 2:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["mecanum"]
                    iconInput.tooltip = "Mecanum wheel"

                gm.ui.activeSelections.add(WheelListGlobal[position])

            elif cmdInput.id == "wheel_add":
                self.reset()

                wheelSelect.isVisible = True
                wheelSelect.isEnabled = True
                wheelSelect.clearSelection()
                addJointInput.isEnabled = True
                addWheelInput.isEnabled = False

            # Transition: AARD-1685
            # Functionality could potentially be moved into `JointConfigTab.py`
            elif cmdInput.id == "jointAddButton":
                self.reset()

                addWheelInput.isEnabled = True
                jointSelect.isVisible = True
                jointSelect.isEnabled = True
                jointSelect.clearSelection()
                addJointInput.isEnabled = False

            elif cmdInput.id == "field_add":
                self.reset()

                gamepieceSelect.isVisible = True
                gamepieceSelect.isEnabled = True
                gamepieceSelect.clearSelection()
                addFieldInput.isEnabled = False

            elif cmdInput.id == "wheel_delete":
                # Currently causes Internal Autodesk Error
                # gm.ui.activeSelections.clear()

                addWheelInput.isEnabled = True
                if wheelTableInput.selectedRow == -1 or wheelTableInput.selectedRow == 0:
                    wheelTableInput.selectedRow = wheelTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    index = wheelTableInput.selectedRow - 1
                    removeWheelFromTable(index)

            # Transition: AARD-1685
            # Functionality could potentially be moved into `JointConfigTab.py`
            elif cmdInput.id == "jointRemoveButton":
                gm.ui.activeSelections.clear()

                addJointInput.isEnabled = True
                addWheelInput.isEnabled = True

                if jointTableInput.selectedRow == -1 or jointTableInput.selectedRow == 0:
                    jointTableInput.selectedRow = jointTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    # Select Row is 1 indexed
                    removeIndexedJointFromConfigTab(jointTableInput.selectedRow - 1)

            elif cmdInput.id == "field_delete":
                gm.ui.activeSelections.clear()

                addFieldInput.isEnabled = True

                if (
                    gamepieceTableInput.selectedRow == -1
                    or gamepieceTableInput.selectedRow == 0
                ):
                    gamepieceTableInput.selectedRow = gamepieceTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    index = gamepieceTableInput.selectedRow - 1
                    removeGamePieceFromTable(index)

            elif cmdInput.id == "wheel_select":
                addWheelInput.isEnabled = True

            elif cmdInput.id == "jointSelection":
                addJointInput.isEnabled = True

            elif cmdInput.id == "gamepiece_select":
                addFieldInput.isEnabled = True

            elif cmdInput.id == "friction_override":
                boolValue = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if boolValue.value:
                    frictionCoeff.isVisible = True
                else:
                    frictionCoeff.isVisible = False

            elif cmdInput.id == "weight_unit":
                unitDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
                weightInput = weightTableInput.getInputAtPosition(0, 2)
                if unitDropdown.selectedItem.index == 0:
                    self.isLbs = True

                    weightInput.tooltipDescription = """<tt>(in pounds)</tt><hr>This is the weight of the entire robot assembly."""
                elif unitDropdown.selectedItem.index == 1:
                    self.isLbs = False

                    weightInput.tooltipDescription = """<tt>(in kilograms)</tt><hr>This is the weight of the entire robot assembly."""

            elif cmdInput.id == "weight_unit_f":
                unitDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
                if unitDropdown.selectedItem.index == 0:
                    self.isLbs_f = True

                    for row in range(gamepieceTableInput.rowCount):
                        if row == 0:
                            continue
                        weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                        weightInput.tooltipDescription = "<tt>(in pounds)</tt>"
                elif unitDropdown.selectedItem.index == 1:
                    self.isLbs_f = False

                    for row in range(gamepieceTableInput.rowCount):
                        if row == 0:
                            continue
                        weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                        weightInput.tooltipDescription = "<tt>(in kilograms)</tt>"

            elif cmdInput.id == "auto_calc_weight":
                button = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if button.value == True:  # CALCULATE button pressed
                    if (
                        self.allWeights.count(None) == 2
                    ):  # if button is pressed for the first time
                        if self.isLbs:  # if pounds unit selected
                            self.allWeights[0] = self.weight()
                            weight_input.value = self.allWeights[0]
                        else:  # if kg unit selected
                            self.allWeights[1] = self.weight(False)
                            weight_input.value = self.allWeights[1]
                    else:  # if a mass value has already been configured
                        if (
                            weight_input.value != self.allWeights[0]
                            or weight_input.value != self.allWeights[1]
                            or not weight_input.isValidExpression
                        ):
                            if self.isLbs:
                                weight_input.value = self.allWeights[0]
                            else:
                                weight_input.value = self.allWeights[1]

            elif cmdInput.id == "auto_calc_weight_f":
                button = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if button.value == True:  # CALCULATE button pressed
                    if self.isLbs_f:
                        for row in range(gamepieceTableInput.rowCount):
                            if row == 0:
                                continue
                            weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                            physical = GamepieceListGlobal[
                                row - 1
                            ].component.getPhysicalProperties(
                                adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                            )
                            value = round(physical.mass * 2.2046226218, 2)
                            weightInput.value = value

                    else:
                        for row in range(gamepieceTableInput.rowCount):
                            if row == 0:
                                continue
                            weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                            physical = GamepieceListGlobal[
                                row - 1
                            ].component.getPhysicalProperties(
                                adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                            )
                            value = round(physical.mass, 2)
                            weightInput.value = value
            elif cmdInput.id == "compress":
                checkBox = adsk.core.BoolValueCommandInput.cast(cmdInput)
                if checkBox.value:
                    global compress
                    compress = checkBox.value
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


class MyCommandDestroyHandler(adsk.core.CommandEventHandler):
    """### Gets the event that is fired when the command is destroyed. Globals lists are released and active selections are cleared (when exiting the panel).
        - In other words, when the OK or Cancel button is pressed...

    Args: CommandEventHandler
    """

    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            onSelect = gm.handlers[3]

            WheelListGlobal.clear()
            resetSelectedJoints()
            GamepieceListGlobal.clear()
            onSelect.allWheelPreselections.clear()
            onSelect.wheelJointList.clear()

            for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
                group.deleteMe()

            # Currently causes Internal Autodesk Error
            # gm.ui.activeSelections.clear()
            gm.app.activeDocument.design.rootComponent.opacity = 1
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(
                "{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
            ).error("Failed:\n{}".format(traceback.format_exc()))


def addWheelToTable(wheel: adsk.fusion.Joint) -> None:
    """### Adds a wheel occurrence to its global list and wheel table.

    Args:
        wheel (adsk.fusion.Occurrence): wheel Occurrence object to be added.
    """
    try:
        try:
            onSelect = gm.handlers[3]
            onSelect.allWheelPreselections.append(wheel.entityToken)
        except IndexError:
            # Not 100% sure what we need the select handler here for however it should not run when
            # first populating the saved wheel configs. This will naturally throw a IndexError as
            # we do this before the initialization of gm.handlers[]
            pass

        wheelTableInput = wheelTable()
        # def addPreselections(child_occurrences):
        #     for occ in child_occurrences:
        #         onSelect.allWheelPreselections.append(occ.entityToken)

        #         if occ.childOccurrences:
        #             addPreselections(occ.childOccurrences)

        # if wheel.childOccurrences:
        #     addPreselections(wheel.childOccurrences)
        # else:

        WheelListGlobal.append(wheel)
        cmdInputs = adsk.core.CommandInputs.cast(wheelTableInput.commandInputs)

        icon = cmdInputs.addImageCommandInput(
            "placeholder_w", "Placeholder", IconPaths.wheelIcons["standard"]
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
        wheelType.tooltip = "Wheel type"
        wheelType.tooltipDescription = "<Br>Omni-directional wheels can be used just like regular drive wheels but they have the advantage of being able to roll freely perpendicular to the drive direction.</Br>"
        wheelType.toolClipFilename = OsHelper.getOSPath(
            ".", "src", "Resources"
        ) + os.path.join("WheelIcons", "omni-wheel-preview.png")

        signalType = cmdInputs.addDropDownCommandInput(
            "signal_type_w",
            "Signal Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        signalType.isFullWidth = True
        signalType.listItems.add("‎", True, IconPaths.signalIcons["PWM"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["CAN"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["PASSIVE"])
        signalType.tooltip = "Signal type"

        row = wheelTableInput.rowCount

        wheelTableInput.addCommandInput(icon, row, 0)
        wheelTableInput.addCommandInput(name, row, 1)
        wheelTableInput.addCommandInput(wheelType, row, 2)
        wheelTableInput.addCommandInput(signalType, row, 3)

    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addWheelToTable()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def addGamepieceToTable(gamepiece: adsk.fusion.Occurrence) -> None:
    """### Adds a gamepiece occurrence to its global list and gamepiece table.

    Args:
        gamepiece (adsk.fusion.Occurrence): Gamepiece occurrence to be added
    """
    try:
        onSelect = gm.handlers[3]
        gamepieceTableInput = gamepieceTable()

        def addPreselections(child_occurrences):
            for occ in child_occurrences:
                onSelect.allGamepiecePreselections.append(occ.entityToken)

                if occ.childOccurrences:
                    addPreselections(occ.childOccurrences)

        if gamepiece.childOccurrences:
            addPreselections(gamepiece.childOccurrences)
        else:
            onSelect.allGamepiecePreselections.append(gamepiece.entityToken)

        GamepieceListGlobal.append(gamepiece)
        cmdInputs = adsk.core.CommandInputs.cast(gamepieceTableInput.commandInputs)
        blankIcon = cmdInputs.addImageCommandInput(
            "blank_gp", "Blank", IconPaths.gamepieceIcons["blank"]
        )

        type = cmdInputs.addTextBoxCommandInput(
            "name_gp", "Occurrence name", gamepiece.name, 1, True
        )

        value = 0.0
        physical = gamepiece.component.getPhysicalProperties(
            adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
        )
        value = physical.mass

        # check if dropdown unit is kg or lbs. bool value taken from ConfigureCommandInputChanged
        massUnitInString = ""
        onInputChanged = gm.handlers[1]
        if onInputChanged.isLbs_f:
            value = round(value * 2.2046226218, 2)  # lbs
            massUnitInString = "<tt>(in pounds)</tt>"
        else:
            value = round(value, 2)  # kg
            massUnitInString = "<tt>(in kilograms)</tt>"

        weight = cmdInputs.addValueInput(
            "weight_gp",
            "Weight Input",
            "",
            adsk.core.ValueInput.createByString(str(value)),
        )

        valueList = [1]
        for i in range(20):
            valueList.append(i / 20)

        friction_coeff = cmdInputs.addFloatSliderListCommandInput(
            "friction_coeff", "", "", valueList
        )
        friction_coeff.valueOne = 0.5

        type.tooltip = gamepiece.name

        weight.tooltip = "Weight of field element"
        weight.tooltipDescription = massUnitInString

        friction_coeff.tooltip = "Friction coefficient of field element"
        friction_coeff.tooltipDescription = (
            "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        )
        row = gamepieceTableInput.rowCount

        gamepieceTableInput.addCommandInput(blankIcon, row, 0)
        gamepieceTableInput.addCommandInput(type, row, 1)
        gamepieceTableInput.addCommandInput(weight, row, 2)
        gamepieceTableInput.addCommandInput(friction_coeff, row, 3)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addGamepieceToTable()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeWheelFromTable(index: int) -> None:
    """### Removes a wheel joint from its global list and wheel table.

    Args:
        index (int): index of wheel item in its global list
    """
    try:
        onSelect = gm.handlers[3]
        wheelTableInput = wheelTable()
        wheel = WheelListGlobal[index]

        # def removePreselections(child_occurrences):
        #     for occ in child_occurrences:
        #         onSelect.allWheelPreselections.remove(occ.entityToken)

        #         if occ.childOccurrences:
        #             removePreselections(occ.childOccurrences)

        # if wheel.childOccurrences:
        #     removePreselections(wheel.childOccurrences)
        # else:
        onSelect.allWheelPreselections.remove(wheel.entityToken)

        del WheelListGlobal[index]
        wheelTableInput.deleteRow(index + 1)

        # updateJointTable(wheel)
    except IndexError:
        pass
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeWheelFromTable()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeGamePieceFromTable(index: int) -> None:
    """### Removes a gamepiece occurrence from its global list and gamepiece table.

    Args:
        index (int): index of gamepiece item in its global list.
    """
    onSelect = gm.handlers[3]
    gamepieceTableInput = gamepieceTable()
    gamepiece = GamepieceListGlobal[index]

    def removePreselections(child_occurrences):
        for occ in child_occurrences:
            onSelect.allGamepiecePreselections.remove(occ.entityToken)

            if occ.childOccurrences:
                removePreselections(occ.childOccurrences)

    try:
        if gamepiece.childOccurrences:
            removePreselections(GamepieceListGlobal[index].childOccurrences)
        else:
            onSelect.allGamepiecePreselections.remove(gamepiece.entityToken)

        del GamepieceListGlobal[index]
        gamepieceTableInput.deleteRow(index + 1)
    except IndexError:
        pass
    except:
        logging.getLogger(
            "{INTERNAL_ID}.UI.ConfigCommand.removeGamePieceFromTable()"
        ).error("Failed:\n{}".format(traceback.format_exc()))
