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
from ..Parser.ExporterOptions import ExporterOptions, ExportMode
from ..Parser.SynthesisParser.Parser import Parser
from . import FileDialogConfig, Helper
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

            global generalConfigTab
            generalConfigTab = GeneralConfigTab(args, exporterOptions)

            global gamepieceConfigTab
            gamepieceConfigTab = GamepieceConfigTab(args, exporterOptions)
            generalConfigTab.gamepieceConfigTab = gamepieceConfigTab

            if not exporterOptions.exportMode == ExportMode.FIELD:
                gamepieceConfigTab.isVisible = False

            if exporterOptions.gamepieces:
                for synGamepiece in exporterOptions.gamepieces:
                    fusionOccurrence = gm.app.activeDocument.design.findEntityByToken(synGamepiece.occurrenceToken)[0]
                    gamepieceConfigTab.addGamepiece(fusionOccurrence, synGamepiece)

            # ~~~~~~~~~~~~~~~~ HELP FILE ~~~~~~~~~~~~~~~~
            """
            Sets the small "i" icon in bottom left of the panel.
                - This is an HTML file that has a script to redirect to exporter workflow tutorial.
            """
            cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

            global jointConfigTab
            jointConfigTab = JointConfigTab(args)
            generalConfigTab.jointConfigTab = jointConfigTab

            if not exporterOptions.exportMode == ExportMode.ROBOT:
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

            # ~~~~~~~~~~~~~~~~ PHYSICS SETTINGS ~~~~~~~~~~~~~~~~
            """
            Physics settings group command
            """
            # physicsSettings: adsk.core.GroupCommandInput = a_input.addGroupCommandInput(
            #     "physics_settings", "Physics Settings"
            # )

            # physicsSettings.isExpanded = True
            # physicsSettings.isEnabled = True
            # physicsSettings.tooltip = "Settings relating to the custom physics of the robot, like the wheel friction"
            # physics_settings: adsk.core.CommandInputs = physicsSettings.children

            # frictionOverrideInput = self.createBooleanInput(
            #     "friction_override",
            #     "Friction Override",
            #     physics_settings,
            #     checked=True,  # object is missing attribute
            #     tooltip="Manually override the default friction values on the bodies in the assembly.",
            #     enabled=True,
            #     isCheckBox=False,
            # )
            # frictionOverrideInput.resourceFolder = IconPaths.stringIcons["friction_override-enabled"]
            # frictionOverrideInput.isFullWidth = True

            # valueList = [1]
            # for i in range(20):
            #     valueList.append(i / 20)

            # frictionCoeffSlider: adsk.core.FloatSliderCommandInput = physics_settings.addFloatSliderListCommandInput(
            #     "friction_override_coeff", "Friction Coefficient", "", valueList
            # )
            # frictionCoeffSlider.isVisible = True
            # frictionCoeffSlider.valueOne = 0.5
            # frictionCoeffSlider.tooltip = "Friction coefficient of field element."
            # frictionCoeffSlider.tooltipDescription = "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"

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

            processedFileName = gm.app.activeDocument.name.replace(" ", "_")
            # dropdownExportMode = INPUTS_ROOT.itemById("mode")
            # if dropdownExportMode.selectedItem.index == 0:
            #     isRobot = True
            # elif dropdownExportMode.selectedItem.index == 1:
            #     isRobot = False

            # processedFileName = gm.app.activeDocument.name.replace(" ", "_")
            # dropdownExportMode = INPUTS_ROOT.itemById("mode")
            # if dropdownExportMode.selectedItem.index == 0:
            #     isRobot = True
            # elif dropdownExportMode.selectedItem.index == 1:
            #     isRobot = False
            # dropdownExportLocation = INPUTS_ROOT.itemById("location")

            if True:
                savepath = FileDialogConfig.saveFileDialog(defaultPath=exporterOptions.fileLocation)

                if savepath == False:
                    # save was canceled
                    return

                updatedPath = pathlib.Path(savepath).parent
                if updatedPath != self.current.filePath:
                    self.current.filePath = str(updatedPath)
            else:
                savepath = processedFileName

            adsk.doEvents()
            # get active document
            design = gm.app.activeDocument.design
            name = design.rootComponent.name.rsplit(" ", 1)[0]
            version = design.rootComponent.name.rsplit(" ", 1)[1]

            selectedJoints, selectedWheels = jointConfigTab.getSelectedJointsAndWheels()
            selectedGamepieces = gamepieceConfigTab.getGamepieces()

            if generalConfigTab.exportMode == ExportMode.ROBOT:
                units = generalConfigTab.selectedUnits
            else:
                assert generalConfigTab.exportMode == ExportMode.FIELD
                units = gamepieceConfigTab.selectedUnits

            exporterOptions = ExporterOptions(
                savepath,
                name,
                version,
                materials=0,
                joints=selectedJoints,
                wheels=selectedWheels,
                gamepieces=selectedGamepieces,
                preferredUnits=units,
                robotWeight=generalConfigTab.robotWeight,
                exportMode=generalConfigTab.exportMode,
                exportLocation=generalConfigTab.exportLocation,
                compressOutput=generalConfigTab.compress,
                exportAsPart=generalConfigTab.exportAsPart,
                autoCalcRobotWeight=generalConfigTab.autoCalculateWeight,
            )

            _: bool = Parser(exporterOptions).export()
            exporterOptions.writeToDesign()

            # All selections should be reset AFTER a successful export and save.
            # If we run into an exporting error we should return back to the panel with all current options
            # still in tact. Even if they did not save.
            jointConfigTab.reset()
            gamepieceConfigTab.reset()
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
            jointConfigTab.handlePreviewEvent(args)
            gamepieceConfigTab.handlePreviewEvent(args)
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

            if gamepieceConfigTab.isVisible:
                self.cmd.setCursor("", 0, 0)  # Reset select cursor back to normal cursor.
                gamepieceConfigTab.handleSelectionEvent(args, args.selection.entity)

            # Transition: AARD-1685
            # This is how all handle selection events should be done in the future although it will look
            # slightly differently for each type of handle.
            if jointConfigTab.isVisible:
                self.cmd.setCursor("", 0, 0)  # Reset select cursor back to normal cursor.
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
            if not preSelected:
                self.cmd.setCursor("", 0, 0)
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

            frictionCoeff = INPUTS_ROOT.itemById("friction_override_coeff")

            if cmdInput.id == "friction_override":
                boolValue = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if boolValue.value:
                    frictionCoeff.isVisible = True
                else:
                    frictionCoeff.isVisible = False
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
            jointConfigTab.reset()
            gamepieceConfigTab.reset()

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
