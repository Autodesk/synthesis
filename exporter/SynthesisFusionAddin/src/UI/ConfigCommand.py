""" Generate all the front-end command inputs and GUI elements.
    links the Configuration Command seen when pressing the Synthesis button in the Addins Panel
"""

from typing import Any

import adsk.core
import adsk.fusion

from src import gm
from src.APS.APS import getAuth, getUserInfo
from src.Logging import getLogger, logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.Parser import Parser
from src.Types import SELECTABLE_JOINT_TYPES, ExportLocation, ExportMode
from src.UI import FileDialogConfig
from src.UI.GamepieceConfigTab import GamepieceConfigTab
from src.UI.GeneralConfigTab import GeneralConfigTab
from src.UI.JointConfigTab import JointConfigTab

generalConfigTab: GeneralConfigTab
jointConfigTab: JointConfigTab
gamepieceConfigTab: GamepieceConfigTab

logger = getLogger()

INPUTS_ROOT: adsk.core.CommandInputs


class ConfigureCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    def __init__(self, configure: Any) -> None:
        super().__init__()

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        exporterOptions = ExporterOptions().readFromDesign() or ExporterOptions()
        cmd = args.command

        cmd.isAutoExecute = False
        cmd.isExecutedWhenPreEmpted = False
        cmd.okButtonText = "Export"
        cmd.setDialogSize(800, 350)

        global INPUTS_ROOT
        INPUTS_ROOT = cmd.commandInputs

        # TODO?
        # cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

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
                fusionJoints = gm.app.activeDocument.design.findEntityByToken(synJoint.jointToken)
                if len(fusionJoints):
                    jointConfigTab.addJoint(fusionJoints[0], synJoint)
        else:
            for joint in [
                *gm.app.activeDocument.design.rootComponent.allJoints,
                *gm.app.activeDocument.design.rootComponent.allAsBuiltJoints,
            ]:
                if joint.jointMotion.jointType in SELECTABLE_JOINT_TYPES and not joint.isSuppressed:
                    jointConfigTab.addJoint(joint)

        # Adding saved wheels must take place after joints are added as a result of how the two types are connected.
        # Transition: AARD-1685
        # Should consider changing how the parser handles wheels and joints to avoid overlap
        if exporterOptions.wheels:
            for wheel in exporterOptions.wheels:
                fusionJoints = gm.app.activeDocument.design.findEntityByToken(wheel.jointToken)
                if len(fusionJoints):
                    jointConfigTab.addWheel(fusionJoints[0], wheel)

        getAuth()
        user_info = getUserInfo()
        apsSettings = INPUTS_ROOT.addTabCommandInput(
            "aps_settings", f"APS Settings ({user_info.given_name if user_info else 'Not Signed In'})"
        )
        apsSettings.tooltip = "Configuration settings for Autodesk Platform Services."

        gm.ui.activeSelections.clear()
        onExecute = ConfigureCommandExecuteHandler()
        cmd.execute.add(onExecute)
        gm.handlers.append(onExecute)

        onInputChanged = ConfigureCommandInputChanged(cmd)
        cmd.inputChanged.add(onInputChanged)
        gm.handlers.append(onInputChanged)

        onExecutePreview = CommandExecutePreviewHandler()
        cmd.executePreview.add(onExecutePreview)
        gm.handlers.append(onExecutePreview)

        onSelect = MySelectHandler(cmd)
        cmd.select.add(onSelect)
        gm.handlers.append(onSelect)

        onPreSelect = MyPreSelectHandler(cmd)
        cmd.preSelect.add(onPreSelect)
        gm.handlers.append(onPreSelect)

        onPreSelectEnd = MyPreselectEndHandler(cmd)
        cmd.preSelectEnd.add(onPreSelectEnd)
        gm.handlers.append(onPreSelectEnd)

        onDestroy = MyCommandDestroyHandler()
        cmd.destroy.add(onDestroy)
        gm.handlers.append(onDestroy)


class ConfigureCommandExecuteHandler(adsk.core.CommandEventHandler):
    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandEventArgs) -> None:
        exporterOptions = ExporterOptions().readFromDesign()

        if args.executeFailed:
            logger.error("Could not execute configuration due to failure")
            return

        processedFileName = gm.app.activeDocument.name.replace(" ", "_")
        if generalConfigTab.exportLocation == ExportLocation.DOWNLOAD:
            savepath = FileDialogConfig.saveFileDialog(defaultPath=exporterOptions.fileLocation)

            if not savepath:
                # save was canceled
                return
        else:
            savepath = processedFileName

        adsk.doEvents()

        design = gm.app.activeDocument.design

        name_split: list[str] = design.rootComponent.name.split(" ")
        if len(name_split) < 2:
            gm.ui.messageBox("Please open the robot design you would like to export", "Synthesis: Error")
            return

        name = name_split[0]
        version = name_split[1]

        selectedJoints, selectedWheels = jointConfigTab.getSelectedJointsAndWheels()
        selectedGamepieces = gamepieceConfigTab.getGamepieces()

        exporterOptions = ExporterOptions(
            str(savepath),
            name,
            version,
            materials=0,
            joints=selectedJoints,
            wheels=selectedWheels,
            gamepieces=selectedGamepieces,
            robotWeight=generalConfigTab.robotWeight,
            autoCalcRobotWeight=generalConfigTab.autoCalculateWeight,
            autoCalcGamepieceWeight=gamepieceConfigTab.autoCalculateWeight,
            exportMode=generalConfigTab.exportMode,
            exportLocation=generalConfigTab.exportLocation,
            compressOutput=generalConfigTab.compress,
            exportAsPart=generalConfigTab.exportAsPart,
            frictionOverride=generalConfigTab.overrideFriction,
            frictionOverrideCoeff=generalConfigTab.frictionOverrideCoeff,
        )

        Parser(exporterOptions).export()
        exporterOptions.writeToDesign()

        # All selections should be reset AFTER a successful export and save.
        # If we run into an exporting error we should return back to the panel with all current options
        # still in tact. Even if they did not save.
        jointConfigTab.reset()
        gamepieceConfigTab.reset()


class CommandExecutePreviewHandler(adsk.core.CommandEventHandler):
    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandEventArgs) -> None:
        jointConfigTab.handlePreviewEvent(args)
        gamepieceConfigTab.handlePreviewEvent(args)


class MySelectHandler(adsk.core.SelectionEventHandler):
    def __init__(self, cmd: adsk.core.Command) -> None:
        super().__init__()
        self.cmd = cmd

    @logFailure(messageBox=True)
    def traverseAssembly(
        self, child_occurrences: adsk.fusion.OccurrenceList, jointedOcc: dict[adsk.fusion.Joint, adsk.fusion.Occurrence]
    ) -> (
        list[adsk.fusion.Joint | adsk.fusion.Occurrence] | None
    ):  # recursive traversal to check if children are jointed
        """### Traverses the entire occurrence hierarchy to find a match (jointed occurrence) in self.occurrence

        Args:
            child_occurrences (adsk.fusion.OccurrenceList): a list of child occurrences

        Returns:
            occ (Occurrence): if a match is found, return the jointed occurrence
            None: if no match is found
        """
        for occ in child_occurrences:
            for joint, value in jointedOcc.items():
                if occ in value:
                    return [joint, occ]  # occurrence that is jointed

            if occ.childOccurrences:  # if occurrence has children, traverse sub-tree
                self.traverseAssembly(occ.childOccurrences, jointedOcc)
        return None  # no jointed occurrence found

    @logFailure(messageBox=True)
    def wheelParent(self, occ: adsk.fusion.Occurrence) -> list[str | adsk.fusion.Occurrence | None]:
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

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.SelectionEventArgs) -> None:
        """### Notify member is called when a selection event is triggered.

        Args:
            args (SelectionEventArgs): A selection event argument
        """
        if gamepieceConfigTab.isVisible:
            self.cmd.setCursor("", 0, 0)  # Reset select cursor back to normal cursor.
            gamepieceConfigTab.handleSelectionEvent(args, args.selection.entity)

        if jointConfigTab.isVisible:
            self.cmd.setCursor("", 0, 0)  # Reset select cursor back to normal cursor.
            jointConfigTab.handleSelectionEvent(args, args.selection.entity)


class MyPreSelectHandler(adsk.core.SelectionEventHandler):
    """### Event fires when a entity preselection is made (mouse hovering).
    ##### When a user is selecting geometry, they move the mouse over the model and if the entity the mouse is currently over is valid for selection it will highlight indicating that it can be selected. This process of determining what is available for selection and highlighting it is referred to as the "preselect" behavior.

    Args: SelectionEventHandler
    """

    def __init__(self, cmd: adsk.core.Command) -> None:
        super().__init__()
        self.cmd = cmd

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.SelectionEventArgs) -> None:
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


class MyPreselectEndHandler(adsk.core.SelectionEventHandler):
    def __init__(self, cmd: adsk.core.Command) -> None:
        super().__init__()
        self.cmd = cmd

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.SelectionEventArgs) -> None:
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        preSelectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)
        preSelectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

        if (preSelectedOcc or preSelectedJoint) and design:
            self.cmd.setCursor("", 0, 0)  # if preselection ends (mouse off of design), reset the mouse icon to default


class ConfigureCommandInputChanged(adsk.core.InputChangedEventHandler):
    def __init__(self, cmd: adsk.core.Command) -> None:
        super().__init__()
        self.cmd = cmd

    @logFailure
    def reset(self) -> None:
        self.cmd.setCursor("", 0, 0)
        gm.ui.activeSelections.clear()

    def notify(self, args: adsk.core.InputChangedEventArgs) -> None:
        if generalConfigTab.isActive:
            generalConfigTab.handleInputChanged(args)

        if jointConfigTab.isVisible and jointConfigTab.isActive:
            jointConfigTab.handleInputChanged(args, INPUTS_ROOT)

        if gamepieceConfigTab.isVisible and gamepieceConfigTab.isActive:
            gamepieceConfigTab.handleInputChanged(args, INPUTS_ROOT)


class MyCommandDestroyHandler(adsk.core.CommandEventHandler):
    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        jointConfigTab.reset()
        gamepieceConfigTab.reset()

        for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
            group.deleteMe()
