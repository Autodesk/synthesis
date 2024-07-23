""" Generate all the front-end command inputs and GUI elements.
    links the Configuration Command seen when pressing the Synthesis button in the Addins Panel
"""

import logging
import os
import traceback
from enum import Enum

import adsk.core
import adsk.fusion

from ..Analytics.alert import showAnalyticsAlert
from ..APS.APS import getAuth, getUserInfo, refreshAuthToken
from ..configure import NOTIFIED, write_configuration
from ..general_imports import *
from ..Parser.ExporterOptions import ExporterOptions
from ..Parser.SynthesisParser.Parser import Parser
from . import CustomGraphics, FileDialogConfig, Helper, IconPaths
from .Configuration.SerialCommand import SerialCommand
from .GamepieceConfigTab import GamepieceConfigTab
from .GeneralConfigTab import GeneralConfigTab

# Transition: AARD-1685
# In the future all components should be handled in this way.
# This import broke everything when attempting to use absolute imports??? Investigate?
from .JointConfigTab import JointConfigTab

# ====================================== CONFIG COMMAND ======================================

generalConfigTab: GeneralConfigTab
jointConfigTab: JointConfigTab
gamepieceConfigTab: GamepieceConfigTab

"""
INPUTS_ROOT (adsk.fusion.CommandInputs):
    - Provides access to the set of all commandInput UI elements in the panel
"""
INPUTS_ROOT = None

"""
These lists are crucial, and contain all of the relevant object selections.
- GamepieceListGlobal: list of gamepieces (adsk.fusion.Occurrence)
"""
GamepieceListGlobal = []


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

            # Transition: AARD-1683
            # Working on replacing all of the general tab stuff
            global generalConfigTab
            generalConfigTab = GeneralConfigTab(args, exporterOptions)

            global gamepieceConfigTab
            gamepieceConfigTab = GamepieceConfigTab(args, exporterOptions)
            gamepieceConfigTab.isVisible = True  # TODO: Should not be visible by default.

            # ~~~~~~~~~~~~~~~~ HELP FILE ~~~~~~~~~~~~~~~~
            """
            Sets the small "i" icon in bottom left of the panel.
                - This is an HTML file that has a script to redirect to exporter workflow tutorial.
            """
            cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

            global jointConfigTab
            jointConfigTab = JointConfigTab(args)
            jointConfigTab.isVisible = False

            # Transition: AARD-1685
            # There remains some overlap between adding joints as wheels.
            # Should investigate changes to improve performance.
            if exporterOptions.joints:
                for synJoint in exporterOptions.joints:
                    fusionJoint = gm.app.activeDocument.design.findEntityByToken(synJoint.jointToken)[0]
                    jointConfigTab.addJoint(fusionJoint, synJoint)
            else:
                for joint in [
                    *gm.app.activeDocument.design.rootComponent.allJoints,
                    *gm.app.activeDocument.design.rootComponent.allAsBuiltJoints,
                ]:
                    if (
                        joint.jointMotion.jointType in (JointMotions.REVOLUTE.value, JointMotions.SLIDER.value)
                        and not joint.isSuppressed
                    ):
                        jointConfigTab.addJoint(joint)

            # Adding saved wheels must take place after joints are added as a result of how the two types are connected.
            # Transition: AARD-1685
            # Should consider changing how the parser handles wheels and joints to avoid overlap
            if exporterOptions.wheels:
                for wheel in exporterOptions.wheels:
                    fusionJoint = gm.app.activeDocument.design.findEntityByToken(wheel.jointToken)[0]
                    jointConfigTab.addWheel(fusionJoint, wheel)

            # ~~~~~~~~~~~~~~~~ GAMEPIECE CONFIGURATION ~~~~~~~~~~~~~~~~
            """
            Gamepiece group command input, isVisible=False by default
                - Container for gamepiece selection table
            """
            # gamepieceConfig = inputs.addGroupCommandInput("gamepiece_config", "Gamepiece Configuration")
            # gamepieceConfig.isExpanded = True
            # gamepieceConfig.isVisible = False
            # gamepieceConfig.tooltip = "Select and define the gamepieces in your field."
            # gamepiece_inputs = gamepieceConfig.children

            # # GAMEPIECE MASS CONFIGURATION
            # """
            # Mass unit dropdown and calculation for gamepiece elements
            # """
            # weightTableInput_f = self.createTableInput(
            #     "weight_table_f", "Weight Table", gamepiece_inputs, 3, "6:2:1", 1
            # )
            # weightTableInput_f.tablePresentationStyle = 2  # set to clear background

            # weight_name_f = gamepiece_inputs.addStringValueInput("weight_name", "Weight")
            # weight_name_f.value = "Unit of Mass"
            # weight_name_f.isReadOnly = True

            # auto_calc_weight_f = self.createBooleanInput(  # CALCULATE button
            #     "auto_calc_weight_f",
            #     "‎",
            #     gamepiece_inputs,
            #     checked=False,
            #     tooltip="Approximate the weight of all your selected gamepieces.",
            #     enabled=True,
            #     isCheckBox=False,
            # )
            # auto_calc_weight_f.resourceFolder = IconPaths.stringIcons["calculate-enabled"]
            # auto_calc_weight_f.isFullWidth = True

            # weight_unit_f = gamepiece_inputs.addDropDownCommandInput(
            #     "weight_unit_f",
            #     "Unit of Mass",
            #     adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            # )
            # weight_unit_f.listItems.add("‎", True, IconPaths.massIcons["LBS"])  # add listdropdown mass options
            # weight_unit_f.listItems.add("‎", False, IconPaths.massIcons["KG"])  # add listdropdown mass options
            # weight_unit_f.tooltip = "Unit of mass"
            # weight_unit_f.tooltipDescription = "<hr>Configure the unit of mass for for the weight calculation."

            # weightTableInput_f.addCommandInput(weight_name_f, 0, 0)  # add command inputs to table
            # weightTableInput_f.addCommandInput(auto_calc_weight_f, 0, 1)  # add command inputs to table
            # weightTableInput_f.addCommandInput(weight_unit_f, 0, 2)  # add command inputs to table

            # # GAMEPIECE SELECTION TABLE
            # """
            # All selected gamepieces appear here
            # """
            # gamepieceTableInput = self.createTableInput(
            #     "gamepiece_table",
            #     "Gamepiece",
            #     gamepiece_inputs,
            #     4,
            #     "1:8:5:12",
            #     50,
            # )

            # addFieldInput = gamepiece_inputs.addBoolValueInput("field_add", "Add", False)

            # removeFieldInput = gamepiece_inputs.addBoolValueInput("field_delete", "Remove", False)
            # addFieldInput.isEnabled = removeFieldInput.isEnabled = True

            # removeFieldInput.tooltip = "Remove a field element"
            # addFieldInput.tooltip = "Add a field element"

            # gamepieceSelectInput = gamepiece_inputs.addSelectionInput(
            #     "gamepiece_select",
            #     "Selection",
            #     "Select the unique gamepieces in your field.",
            # )
            # gamepieceSelectInput.addSelectionFilter("Occurrences")
            # gamepieceSelectInput.setSelectionLimits(0)
            # gamepieceSelectInput.isEnabled = True
            # gamepieceSelectInput.isVisible = False

            # gamepieceTableInput.addToolbarCommandInput(addFieldInput)
            # gamepieceTableInput.addToolbarCommandInput(removeFieldInput)

            # """
            # Gamepiece table column headers. (the permanent captions in the first row of table)
            # """
            # gamepieceTableInput.addCommandInput(
            #     self.createTextBoxInput(
            #         "e_header",
            #         "Gamepiece name",
            #         gamepiece_inputs,
            #         "Gamepiece",
            #         bold=False,
            #     ),
            #     0,
            #     1,
            # )

            # gamepieceTableInput.addCommandInput(
            #     self.createTextBoxInput(
            #         "w_header",
            #         "Gamepiece weight",
            #         gamepiece_inputs,
            #         "Weight",
            #         background="#d9d9d9",
            #     ),
            #     0,
            #     2,
            # )

            # gamepieceTableInput.addCommandInput(
            #     self.createTextBoxInput(
            #         "f_header",
            #         "Friction coefficient",
            #         gamepiece_inputs,
            #         "Friction coefficient",
            #         background="#d9d9d9",
            #     ),
            #     0,
            #     3,
            # )

            # Transition: AARD-1683
            # Needed to comment this out because it throws if you don't login preventing anything from working

            # getAuth()
            # user_info = getUserInfo()
            # apsSettings = INPUTS_ROOT.addTabCommandInput(
            #     "aps_settings", f"APS Settings ({user_info.given_name if user_info else 'Not Signed In'})"
            # )
            # apsSettings.tooltip = "Configuration settings for Autodesk Platform Services."
            # aps_input = apsSettings.children

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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


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

            savepath = FileDialogConfig.SaveFileDialog(defaultPath=exporterOptions.fileLocation)

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

            _exportGamepieces = []  # TODO work on the code to populate Gamepiece

            # Transition: AARD-1683
            # TODO: Implement gamepiece things
            """
            Loops through all rows in the gamepiece table to extract the input values
            """
            # gamepieceTableInput = gamepieceTable()
            # weight_unit_f = INPUTS_ROOT.itemById("weight_unit_f")
            # for row in range(gamepieceTableInput.rowCount):
            #     if row == 0:
            #         continue

            #     weightValue = gamepieceTableInput.getInputAtPosition(row, 2).value  # weight/mass input, float

            #     if weight_unit_f.selectedItem.index == 0:
            #         weightValue /= 2.2046226218

            #     frictionValue = gamepieceTableInput.getInputAtPosition(row, 3).valueOne  # friction value, float

            #     _exportGamepieces.append(
            #         Gamepiece(
            #             guid_occurrence(GamepieceListGlobal[row - 1]),
            #             weightValue,
            #             frictionValue,
            #         )
            #     )

            selectedJoints, selectedWheels = jointConfigTab.getSelectedJointsAndWheels()

            exporterOptions = ExporterOptions(
                savepath,
                name,
                version,
                materials=0,
                joints=selectedJoints,
                wheels=selectedWheels,
                gamepieces=_exportGamepieces,  # TODO
                preferredUnits=generalConfigTab.selectedUnits,
                robotWeight=generalConfigTab.robotWeight,
                exportMode=generalConfigTab.exportMode,
                compressOutput=generalConfigTab.compress,
                exportAsPart=generalConfigTab.exportAsPart,
                autoCalcRobotWeight=generalConfigTab.autoCalculateWeight,
            )

            Parser(exporterOptions).export()
            exporterOptions.writeToDesign()

            # All selections should be reset AFTER a successful export and save.
            # If we run into an exporting error we should return back to the panel with all current options
            # still in tact. Even if they did not save.
            jointConfigTab.reset()
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

            addFieldInput = INPUTS_ROOT.itemById("field_add")
            removeFieldInput = INPUTS_ROOT.itemById("field_delete")

            # Transition: AARD-1685
            # This is how all preview handles should be done in the future
            jointConfigTab.handlePreviewEvent(args)
            gamepieceConfigTab.handlePreviewEvent(args)

            # gamepieceTableInput = gamepieceTable()
            # if gamepieceTableInput.rowCount <= 1:
            #     removeFieldInput.isEnabled = auto_calc_weight_f.isEnabled = False
            # else:
            #     removeFieldInput.isEnabled = auto_calc_weight_f.isEnabled = True

            # if not addFieldInput.isEnabled or not removeFieldInput:
            #     for gamepiece in GamepieceListGlobal:
            #         gamepiece.component.opacity = 0.25
            #         CustomGraphics.createTextGraphics(gamepiece, GamepieceListGlobal)
            # else:
            #     gm.app.activeDocument.design.rootComponent.opacity = 1
        except AttributeError:
            pass
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


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
        self.allGamepiecePreselections = []  # all child gamepiece occurrences of selections

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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.traverseAssembly()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

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
                    if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]
            except:
                for joint in occ.component.joints:
                    if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]

            if parent == None:  # no parent occurrence
                # gm.ui.messageBox("Selection has no parent occurrence.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
                return [None, occ]  # return what is selected

            for joint in gm.app.activeDocument.design.rootComponent.allJoints:
                if joint.jointMotion.jointType != adsk.fusion.JointTypes.RevoluteJointType:
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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.wheelParent()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )
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

            # Transition: AARD-1683
            # TODO: Implement gamepiece things
            # if self.selectedOcc:
            #     self.cmd.setCursor("", 0, 0)
            #     if dropdownExportMode.selectedItem.index == 1:
            #         occurrenceList = gm.app.activeDocument.design.rootComponent.allOccurrencesByComponent(
            #             self.selectedOcc.component
            #         )
            #         for occ in occurrenceList:
            #             if occ not in GamepieceListGlobal:
            #                 addGamepieceToTable(occ)
            #             else:
            #                 removeGamePieceFromTable(GamepieceListGlobal.index(occ))

            #         selectionInput.isEnabled = False
            #         selectionInput.isVisible = False

            if gamepieceConfigTab.isVisible:
                self.cmd.setCursor("", 0, 0)  # Reset select cursor back to normal cursor.
                gamepieceConfigTab.handleSelectionEvent(args, args.selection.entity)

            # Transition: AARD-1685
            # This is how all handle selection events should be done in the future although it will look
            # slightly differently for each type of handle.
            if jointConfigTab.isVisible:
                jointConfigTab.handleSelectionEvent(args, args.selection.entity)
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


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

            # Transition: AARD-1683
            # TODO: Implement gamepiece things
            # dropdownExportMode = INPUTS_ROOT.itemById("mode")
            # if preSelected and design:
            #     if dropdownExportMode.selectedItem.index == 0:  # Dynamic
            #         if preSelected.entityToken in onSelect.allWheelPreselections:
            #             self.cmd.setCursor(
            #                 IconPaths.mouseIcons["remove"],
            #                 0,
            #                 0,
            #             )
            #         else:
            #             self.cmd.setCursor(
            #                 IconPaths.mouseIcons["add"],
            #                 0,
            #                 0,
            #             )

            #     elif dropdownExportMode.selectedItem.index == 1:  # Static
            #         if preSelected.entityToken in onSelect.allGamepiecePreselections:
            #             self.cmd.setCursor(
            #                 IconPaths.mouseIcons["remove"],
            #                 0,
            #                 0,
            #             )
            #         else:
            #             self.cmd.setCursor(
            #                 IconPaths.mouseIcons["add"],
            #                 0,
            #                 0,
            #             )
            # else:  # Should literally be impossible? - Brandon
            #     self.cmd.setCursor("", 0, 0)
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class ConfigureCommandInputChanged(adsk.core.InputChangedEventHandler):
    """### Gets an event that is fired whenever an input value is changed.
        - Button pressed, selection made, switching tabs, etc...

    Args: InputChangedEventHandler
    """

    def __init__(self, cmd):
        super().__init__()
        self.log = logging.getLogger(f"{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}")
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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.reset()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    def notify(self, args):
        try:
            eventArgs = adsk.core.InputChangedEventArgs.cast(args)
            cmdInput = eventArgs.input

            # Transition: AARD-1685
            # Should be how all input changed handles are done in the future
            generalConfigTab.handleInputChanged(args)

            if jointConfigTab.isVisible:
                jointConfigTab.handleInputChanged(args, INPUTS_ROOT)

            if gamepieceConfigTab.isVisible:
                gamepieceConfigTab.handleInputChanged(args, INPUTS_ROOT)

            MySelectHandler.lastInputCmd = cmdInput
            inputs = cmdInput.commandInputs
            onSelect = gm.handlers[3]

            frictionCoeff = INPUTS_ROOT.itemById("friction_coeff_override")

            gamepieceSelect = inputs.itemById("gamepiece_select")
            gamepieceTableInput = gamepieceTable()
            weightTableInput = inputs.itemById("weight_table")

            weight_input = INPUTS_ROOT.itemById("weight_input")
            gamepieceConfig = inputs.itemById("gamepiece_config")
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

                        addFieldInput.isEnabled = True

                elif modeDropdown.selectedItem.index == 1:
                    if gamepieceConfig:
                        gm.ui.activeSelections.clear()
                        gm.app.activeDocument.design.rootComponent.opacity = 1

            elif cmdInput.id == "blank_gp" or cmdInput.id == "name_gp" or cmdInput.id == "weight_gp":
                self.reset()

                gamepieceSelect.isEnabled = False
                addFieldInput.isEnabled = True

                cmdInput_str = cmdInput.id

                if cmdInput_str == "name_gp":
                    position = gamepieceTableInput.getPosition(adsk.core.TextBoxCommandInput.cast(cmdInput))[1] - 1
                elif cmdInput_str == "weight_gp":
                    position = gamepieceTableInput.getPosition(adsk.core.ValueCommandInput.cast(cmdInput))[1] - 1
                elif cmdInput_str == "blank_gp":
                    position = gamepieceTableInput.getPosition(adsk.core.ImageCommandInput.cast(cmdInput))[1] - 1
                else:
                    position = gamepieceTableInput.getPosition(adsk.core.FloatSliderCommandInput.cast(cmdInput))[1] - 1

                gm.ui.activeSelections.add(GamepieceListGlobal[position])

            elif cmdInput.id == "field_add":
                self.reset()

                gamepieceSelect.isVisible = True
                gamepieceSelect.isEnabled = True
                gamepieceSelect.clearSelection()
                addFieldInput.isEnabled = False

            elif cmdInput.id == "field_delete":
                gm.ui.activeSelections.clear()

                addFieldInput.isEnabled = True

                if gamepieceTableInput.selectedRow == -1 or gamepieceTableInput.selectedRow == 0:
                    gamepieceTableInput.selectedRow = gamepieceTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    index = gamepieceTableInput.selectedRow - 1
                    removeGamePieceFromTable(index)

            elif cmdInput.id == "gamepiece_select":
                addFieldInput.isEnabled = True

            elif cmdInput.id == "friction_override":
                boolValue = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if boolValue.value:
                    frictionCoeff.isVisible = True
                else:
                    frictionCoeff.isVisible = False

            # Transition: AARD-1683
            # TODO: Implement gamepiece things
            # elif cmdInput.id == "weight_unit_f":
            #     unitDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
            #     if unitDropdown.selectedItem.index == 0:
            #         self.isLbs_f = True

            #         for row in range(gamepieceTableInput.rowCount):
            #             if row == 0:
            #                 continue
            #             weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
            #             weightInput.tooltipDescription = "<tt>(in pounds)</tt>"
            #     elif unitDropdown.selectedItem.index == 1:
            #         self.isLbs_f = False

            #         for row in range(gamepieceTableInput.rowCount):
            #             if row == 0:
            #                 continue
            #             weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
            #             weightInput.tooltipDescription = "<tt>(in kilograms)</tt>"

            # Transition: AARD-1683
            # TODO: Implement gamepiece things???
            # Is there even any here??
            # elif cmdInput.id == "auto_calc_weight":
            #     button = adsk.core.BoolValueCommandInput.cast(cmdInput)

            #     if button.value == True:  # CALCULATE button pressed
            #         if self.allWeights.count(None) == 2:  # if button is pressed for the first time
            #             if self.isLbs:  # if pounds unit selected
            #                 self.allWeights[0] = self.weight()
            #                 weight_input.value = self.allWeights[0]
            #             else:  # if kg unit selected
            #                 self.allWeights[1] = self.weight(False)
            #                 weight_input.value = self.allWeights[1]
            #         else:  # if a mass value has already been configured
            #             if (
            #                 weight_input.value != self.allWeights[0]
            #                 or weight_input.value != self.allWeights[1]
            #                 or not weight_input.isValidExpression
            #             ):
            #                 if self.isLbs:
            #                     weight_input.value = self.allWeights[0]
            #                 else:
            #                     weight_input.value = self.allWeights[1]

            # Transition: AARD-1683
            # TODO: Implement gamepiece things
            # elif cmdInput.id == "auto_calc_weight_f":
            #     button = adsk.core.BoolValueCommandInput.cast(cmdInput)

            #     if button.value == True:  # CALCULATE button pressed
            #         if self.isLbs_f:
            #             for row in range(gamepieceTableInput.rowCount):
            #                 if row == 0:
            #                     continue
            #                 weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
            #                 physical = GamepieceListGlobal[row - 1].component.getPhysicalProperties(
            #                     adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
            #                 )
            #                 value = round(physical.mass * 2.2046226218, 2)
            #                 weightInput.value = value

            #         else:
            #             for row in range(gamepieceTableInput.rowCount):
            #                 if row == 0:
            #                     continue
            #                 weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
            #                 physical = GamepieceListGlobal[row - 1].component.getPhysicalProperties(
            #                     adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
            #                 )
            #                 value = round(physical.mass, 2)
            #                 weightInput.value = value
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


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

            jointConfigTab.reset()
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
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


# Transition: AARD-1683
# TODO: Implement gamepiece things
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
        blankIcon = cmdInputs.addImageCommandInput("blank_gp", "Blank", IconPaths.gamepieceIcons["blank"])

        type = cmdInputs.addTextBoxCommandInput("name_gp", "Occurrence name", gamepiece.name, 1, True)

        value = 0.0
        physical = gamepiece.component.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
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

        friction_coeff = cmdInputs.addFloatSliderListCommandInput("friction_coeff", "", "", valueList)
        friction_coeff.valueOne = 0.5

        type.tooltip = gamepiece.name

        weight.tooltip = "Weight of field element"
        weight.tooltipDescription = massUnitInString

        friction_coeff.tooltip = "Friction coefficient of field element"
        friction_coeff.tooltipDescription = "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        row = gamepieceTableInput.rowCount

        gamepieceTableInput.addCommandInput(blankIcon, row, 0)
        gamepieceTableInput.addCommandInput(type, row, 1)
        gamepieceTableInput.addCommandInput(weight, row, 2)
        gamepieceTableInput.addCommandInput(friction_coeff, row, 3)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addGamepieceToTable()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


# Transition: AARD-1683
# TODO: Implement gamepiece things
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
