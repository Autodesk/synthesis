""" Generate all the front-end command inputs and GUI elements.
    links the Configuration Command seen when pressing the Synthesis button in the Addins Panel
"""

from enum import Enum
from ..general_imports import *
from ..configure import NOTIFIED, write_configuration
from ..Analytics.alert import showAnalyticsAlert
from . import Helper, FileDialogConfig, OsHelper, CustomGraphics, IconPaths
from ..Parser.ParseOptions import (
    Gamepiece,
    Mode,
    ParseOptions,
    _Joint,
    _Wheel,
    JointParentType,
)
from .Configuration.SerialCommand import SerialCommand

from .MenuItems.JointCommandGroup import *

import adsk.core, adsk.fusion, traceback, logging, os
from types import SimpleNamespace

from .UiCallbacks import *

# ====================================== CONFIG COMMAND ======================================

"""
INPUTS_ROOT (adsk.fusion.CommandInputs):
    - Provides access to the set of all commandInput UI elements in the panel
"""
INPUTS_ROOT = None

"""
These lists are crucial, and contain all of the relevent object selections.
- WheelListGlobal: list of wheels (adsk.fusion.Occurrence)
- JointListGlobal: list of joints (adsk.fusion.Joint)
- GamepieceListGlobal: list of gamepieces (adsk.fusion.Occurrence)
"""
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


class FullMassCalculuation():
    def __init__(self):
        self.totalMass = 0.0
        self.bRepMassInRoot()
        self.traverseOccurrenceHierarchy()

    def bRepMassInRoot(self):
        try:
            for body in gm.app.activeDocument.design.rootComponent.bRepBodies:
                if not body.isLightBulbOn: continue
                physical = body.getPhysicalProperties(
                    adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                )
                self.totalMass += physical.mass
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

    def traverseOccurrenceHierarchy(self):
        try:
            for occ in gm.app.activeDocument.design.rootComponent.allOccurrences:
                if not occ.isLightBulbOn: continue

                for body in occ.component.bRepBodies:
                    if not body.isLightBulbOn: continue
                    physical = body.getPhysicalProperties(
                        adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                    )
                    self.totalMass += physical.mass
        except:
            pass
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

    def get_total_mass(self):
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

    def notify(self, args):
        try:
            if not Helper.check_solid_open():
                return

            global NOTIFIED  # keep this global
            if not NOTIFIED:
                showAnalyticsAlert()
                NOTIFIED = True
                write_configuration("analytics", "notified", "yes")

            previous = None
            saved = Helper.previouslyConfigured()

            global compress
            compress = True

            if type(saved) == str:
                try:
                    # probably need some way to validate for each usage below
                    previous = json.loads(
                        saved, object_hook=lambda d: SimpleNamespace(**d)
                    )
                except:
                    self.log.error("Failed:\n{}".format(traceback.format_exc()))
                    gm.ui.messageBox(
                        "Failed to read previous Unity Configuration\n  - Using default configuration"
                    )
                    previous = SerialCommand()
            else:
                # new file configuration
                previous = SerialCommand()

            if A_EP:
                A_EP.send_view("export_panel")

            event_args = adsk.core.CommandCreatedEventArgs.cast(args)
            cmd = event_args.command  # adsk.core.Command

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
            dropdown_export_mode = inputs.addDropDownCommandInput(
                "mode",
                "Export Mode",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            dropdown_export_mode.listItems.add("Dynamic", True)
            dropdown_export_mode.listItems.add("Static", False)

            dropdown_export_mode.tooltip = (
                "Export Mode"
            )
            dropdown_export_mode.tooltipDescription = (
                "<hr>Does this object move dynamically?"
            )

            CommandGroup.configure_menu_item(JointCommandGroup(self))

            # ~~~~~~~~~~~~~~~~ WEIGHT CONFIGURATION ~~~~~~~~~~~~~~~~

            # ~~~~~~~~~~~~~~~~ GAMEPIECE CONFIGURATION ~~~~~~~~~~~~~~~~

            # ====================================== ADVANCED TAB ======================================
            """
            Creates the advanced tab, which is the parent container for internal command inputs
            """
            advanced_settings = INPUTS_ROOT.addTabCommandInput(
                "advanced_settings", "Advanced"
            )
            advanced_settings.tooltip = "Additional Advanced Settings to change how your model will be translated " \
                                        "into Unity. "
            a_input = advanced_settings.children

            # ~~~~~~~~~~~~~~~~ EXPORTER SETTINGS ~~~~~~~~~~~~~~~~
            """
            Exporter settings group command
            """
            exporter_setings = a_input.addGroupCommandInput(
                "exporter_settings", "Exporter Settings"
            )
            exporter_setings.isExpanded = True
            exporter_setings.isEnabled = True
            exporter_setings.tooltip = "tooltip"  # TODO: update tooltip
            exporter_settings = exporter_setings.children

            self.create_boolean_input(  # algorithm wheel selection checkbox.
                "algorithmic_selection",
                "Algorithmic Wheel Selection",
                exporter_settings,
                checked=True,
                tooltip="Automatically select the entire wheel component.",
                tooltipadvanced="<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                                "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                                "<ul>" +
                                "<tt>" +
                                "<li>Jointing the wheel differently, or</li><br>" +
                                "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                                "<li>Disabling Algorithmic Selection.</li>" +
                                "</tt>" +
                                "</ul>",
                enabled=True,
            )

            self.create_boolean_input(
                "compress",
                "Compress Output",
                exporter_settings,
                checked=compress,
                tooltip="Compress the output file for a smaller file size.",
                tooltipadvanced="<hr>Use the GZIP compression system to compress the resulting file which will be opened in the simulator, perfect if you want to share the file.<br>",
                enabled=True
            )

            self.create_boolean_input(  # open synthesis checkbox
                "open_synthesis",
                "Open Synthesis",
                exporter_settings,
                checked=True,
                tooltip="Open Synthesis after the export is complete.",
                enabled=True,
            )

            # ~~~~~~~~~~~~~~~~ PHYSICS SETINGS ~~~~~~~~~~~~~~~~
            """
            Physics settings group command
            """
            physics_settings = a_input.addGroupCommandInput(
                "physics_settings", "Physics Settings"
            )

            physics_settings.isExpanded = False
            physics_settings.isEnabled = True
            physics_settings.tooltip = "tooltip"  # TODO: update tooltip
            physics_settings = physics_settings.children

            self.create_boolean_input(  # density checkbox
                "density",
                "Density",
                physics_settings,
                checked=True,
                tooltip="tooltip",  # TODO: update tooltip
                enabled=True,
            )

            self.create_boolean_input(  # SA checkbox
                "surface_area",
                "Surface Area",
                physics_settings,
                checked=True,
                tooltip="tooltip",  # TODO: update tooltip
                enabled=True,
            )

            self.create_boolean_input(  # restitution checkbox
                "restitution",
                "Restitution",
                physics_settings,
                checked=True,
                tooltip="tooltip",  # TODO: update tooltip
                enabled=True,
            )

            frictionOverrideTable = self.create_table_input(
                "friction_override_table",
                "",
                physics_settings,
                2,
                "1:2",
                1,
                column_spacing=25,
            )
            frictionOverrideTable.tablePresentationStyle = 2
            frictionOverrideTable.isFullWidth = True

            frictionOverride = self.create_boolean_input(
                "friction_override",
                "",
                physics_settings,
                checked=False,
                tooltip="Manually override the default friction values on the bodies in the assembly.",
                enabled=True,
                is_check_box=False,
            )
            frictionOverride.resourceFolder = IconPaths.stringIcons["friction_override-enabled"]
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


            # ~~~~~~~~~~~~~~~~ CONTROLLER SETTINGS ~~~~~~~~~~~~~~~~
            """
            Controller settings group command
            """
            controllerSettings = a_input.addGroupCommandInput(
                "controller_settings", "Controller Settings"
            )

            controllerSettings.isExpanded = False
            controllerSettings.isEnabled = True
            controllerSettings.tooltip = "tooltip"  # TODO: update tooltip
            controller_settings = controllerSettings.children

            self.create_boolean_input(  # export signals checkbox
                "export_signals",
                "Export Signals",
                controller_settings,
                checked=True,
                tooltip="tooltip",
                enabled=True,
            )

            # clear all selections before instantiating handlers.
            gm.ui.activeSelections.clear()

            # ====================================== EVENT HANDLERS ======================================
            """
            Instantiating all the event handlers
            """
            onExecute = ConfigureCommandExecuteHandler(
                json.dumps(previous, default=lambda o: o.__dict__, sort_keys=True, indent=1),
                previous.filePath,
            )
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

            onKeyDown = MyKeyDownHandler()
            cmd.keyDown.add(onKeyDown)
            gm.handlers.append(onKeyDown)  # 6

            onKeyUp = MyKeyUpHandler()
            cmd.keyUp.add(onKeyUp)
            gm.handlers.append(onKeyUp)  # 7

            onDestroy = MyCommandDestroyHandler()
            cmd.destroy.add(onDestroy)
            gm.handlers.append(onDestroy)  # 8

        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    @staticmethod
    def configure_menu_item(menu_item):
        menu_item.configure()
        return menu_item

    @staticmethod
    def create_boolean_input(
            _id: str,
            name: str,
            inputs: adsk.core.CommandInputs,
            tooltip="",
            tooltipadvanced="",
            checked=True,
            enabled=True,
            is_check_box=True,
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
            _input = inputs.addBoolValueInput(_id, name, is_check_box)
            _input.value = checked
            _input.isEnabled = enabled
            _input.tooltip = tooltip
            _input.tooltipDescription = tooltipadvanced
            return _input
        except:
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.createBooleanInput()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    @staticmethod
    def create_table_input(
            _id: str,
            name: str,
            inputs: adsk.core.CommandInputs,
            columns: int,
            ratio: str,
            max_rows: int,
            min_rows=1,
            column_spacing=0,
            row_spacing=0,
    ) -> adsk.core.TableCommandInput:
        """### Simple helper to generate all the TableCommandInput options.

        Args:
            _id (str): unique ID of command 
            name (str): displayed name
            inputs (adsk.core.CommandInputs): parent command input container
            columns (int): column count
            ratio (str): column width ratio
            max_rows (int): the maximum number of displayed rows possible
            min_rows (int, optional): the minumum number of displayed rows. Defaults to 1.
            column_spacing (int, optional): spacing in between the columns, in pixels. Defaults to 0.
            row_spacing (int, optional): spacing in between the rows, in pixels. Defaults to 0.

        Returns:
            adsk.core.TableCommandInput: created tableCommandInput
        """
        try:
            _input = inputs.addTableCommandInput(_id, name, columns, ratio)
            _input.minimumVisibleRows = min_rows
            _input.maximumVisibleRows = max_rows
            _input.columnSpacing = column_spacing
            _input.rowSpacing = row_spacing
            return _input
        except:
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.createTableInput()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    @staticmethod
    def create_text_box_input(
            _id: str,
            name: str,
            inputs: adsk.core.CommandInputs,
            text: str,
            italics=True,
            bold=True,
            font_size=10,
            alignment="center",
            row_count=1,
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
            font_size (int, optional): fontsize. Defaults to 10.
            alignment (str, optional): HTML style alignment (left, center, right). Defaults to "center".
            row_count (int, optional): number of rows in textbox. Defaults to 1.
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
            _text = wrapper % (background, alignment, font_size, b[0], i[0], i[1], b[1])

            _input = inputs.addTextBoxCommandInput(_id, name, _text, row_count, read)
            _input.tooltip = tooltip
            _input.tooltipDescription = advanced_tooltip
            return _input
        except:
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.createTextBoxInput()").error(
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
            value = round(
                value * 2.2046226218, 2  # lbs
            )
            massUnitInString = "<tt>(in pounds)</tt>"
        else:
            value = round(
                value, 2  # kg
            )
            massUnitInString = "<tt>(in kilograms)</tt>"

        weight = cmdInputs.addValueInput(
            "weight_gp", "Weight Input", "", adsk.core.ValueInput.createByString(str(value))
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


def removeJointFromTable(joint: adsk.fusion.Joint) -> None:
    """### Removes a joint occurrence from its global list and joint table.

    Args:
        joint (adsk.fusion.Joint): Joint object to be removed
    """
    try:
        index = JointListGlobal.index(joint)
        jointTableInput = jointTable()
        JointListGlobal.remove(joint)

        jointTableInput.deleteRow(index + 1)

        for row in range(jointTableInput.rowCount):
            if row == 0:
                continue

            dropDown = jointTableInput.getInputAtPosition(row, 2)
            listItems = dropDown.listItems

            if row > index:
                if listItems.item(index + 1).isSelected:
                    listItems.item(index).isSelected = True
                    listItems.item(index + 1).deleteMe()
                else:
                    listItems.item(index + 1).deleteMe()
            else:
                if listItems.item(index).isSelected:
                    listItems.item(index - 1).isSelected = True
                    listItems.item(index).deleteMe()
                else:
                    listItems.item(index).deleteMe()
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeJointFromTable()").error(
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
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeGamePieceFromTable()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )
