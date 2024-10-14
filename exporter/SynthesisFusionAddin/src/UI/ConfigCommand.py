"""
Central location for which all UI is generated and handled for the main configuration panel.
"""

import os
import re
import webbrowser
from typing import Any

import adsk.core
import adsk.fusion

from src import APP_WEBSITE_URL, gm
from src.APS.APS import getAuth, getUserInfo
from src.Logging import getLogger, logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.Parser import Parser
from src.Types import SELECTABLE_JOINT_TYPES, ExportLocation, ExportMode
from src.UI import FileDialogConfig
from src.UI.GamepieceConfigTab import GamepieceConfigTab
from src.UI.GeneralConfigTab import GeneralConfigTab
from src.UI.Handlers import PersistentEventHandler
from src.UI.JointConfigTab import JointConfigTab

generalConfigTab: GeneralConfigTab
jointConfigTab: JointConfigTab
gamepieceConfigTab: GamepieceConfigTab

logger = getLogger()

INPUTS_ROOT: adsk.core.CommandInputs


class ConfigureCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    """Called when the panel is initially created."""

    def __init__(self, configure: Any) -> None:
        super().__init__()

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        cmd = args.command
        global INPUTS_ROOT
        INPUTS_ROOT = cmd.commandInputs

        gm.ui.activeSelections.clear()
        onExecute = ConfigureCommandExecuteHandler()
        cmd.execute.add(onExecute)

        onInputChanged = ConfigureCommandInputChanged()
        cmd.inputChanged.add(onInputChanged)

        onExecutePreview = CommandExecutePreviewHandler()
        cmd.executePreview.add(onExecutePreview)

        onSelect = MySelectHandler()
        cmd.select.add(onSelect)

        onPreSelectEnd = MyPreselectEndHandler(cmd)
        cmd.preSelectEnd.add(onPreSelectEnd)

        onDestroy = MyCommandDestroyHandler()
        cmd.destroy.add(onDestroy)

        exporterOptions = ExporterOptions().readFromDesign() or ExporterOptions()

        cmd.isAutoExecute = False
        cmd.isExecutedWhenPreEmpted = False
        cmd.okButtonText = "Export"
        cmd.setDialogSize(800, 350)
        cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

        global generalConfigTab
        generalConfigTab = GeneralConfigTab(args, exporterOptions)

        global gamepieceConfigTab
        gamepieceConfigTab = GamepieceConfigTab(args, exporterOptions)
        generalConfigTab.gamepieceConfigTab = gamepieceConfigTab

        global jointConfigTab
        jointConfigTab = JointConfigTab(args)
        generalConfigTab.jointConfigTab = jointConfigTab

        if not exporterOptions.exportMode == ExportMode.FIELD:
            gamepieceConfigTab.isVisible = False

        if not exporterOptions.exportMode == ExportMode.ROBOT:
            jointConfigTab.isVisible = False

        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        for synGamepiece in exporterOptions.gamepieces:
            fusionOccurrence = design.findEntityByToken(synGamepiece.occurrenceToken)[0]
            gamepieceConfigTab.addGamepiece(fusionOccurrence, synGamepiece)

        if len(exporterOptions.joints):
            for synJoint in exporterOptions.joints:
                fusionJoints = design.findEntityByToken(synJoint.jointToken)
                if len(fusionJoints):
                    jointConfigTab.addJoint(fusionJoints[0], synJoint)
        else:
            for joint in [*design.rootComponent.allJoints, *design.rootComponent.allAsBuiltJoints]:
                if joint.jointMotion.jointType in SELECTABLE_JOINT_TYPES and not joint.isSuppressed:
                    jointConfigTab.addJoint(joint)

        # Adding saved wheels must take place after joints are added as a result of how the two types are connected.
        for wheel in exporterOptions.wheels:
            fusionJoints = design.findEntityByToken(wheel.jointToken)
            if len(fusionJoints):
                jointConfigTab.addWheel(fusionJoints[0], wheel)

        getAuth()
        user_info = getUserInfo()
        apsSettings = INPUTS_ROOT.addTabCommandInput(
            "aps_settings", f"APS Settings ({user_info.given_name if user_info else 'Not Signed In'})"
        )
        apsSettings.tooltip = "Configuration settings for Autodesk Platform Services."


class ConfigureCommandExecuteHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when the `Export` button is clicked from the main configuration panel."""

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        exporterOptions = ExporterOptions().readFromDesign()

        fullName = design.rootComponent.name
        versionMatch = re.search(r"v\d+", fullName)
        docName = (fullName[: versionMatch.start()].strip() if versionMatch else fullName).replace(" ", "_")
        docVersion = versionMatch.group() if versionMatch else "v0"

        processedFileName = gm.app.activeDocument.name.replace(" ", "_")
        defaultFileName = f"{'_'.join([docName, docVersion])}.mira"
        if generalConfigTab.exportLocation == ExportLocation.DOWNLOAD:
            savepath = FileDialogConfig.saveFileDialog(exporterOptions.fileLocation, defaultFileName)
        else:
            savepath = processedFileName

        if not savepath:  # User cancelled the save dialog
            return

        adsk.doEvents()

        selectedJoints, selectedWheels = jointConfigTab.getSelectedJointsAndWheels()
        selectedGamepieces = gamepieceConfigTab.getGamepieces()

        exporterOptions = ExporterOptions(
            savepath,
            docName,
            docVersion,
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
            openSynthesisUponExport=generalConfigTab.openSynthesisUponExport,
        )

        Parser(exporterOptions).export()
        exporterOptions.writeToDesign()
        jointConfigTab.reset()
        gamepieceConfigTab.reset()

        if generalConfigTab.openSynthesisUponExport:
            res = webbrowser.open(APP_WEBSITE_URL)
            if not res:
                gm.ui.messageBox("Failed to open Synthesis in your default browser.")


class CommandExecutePreviewHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when an execute command is ready to be previewed."""

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandEventArgs) -> None:
        jointConfigTab.handlePreviewEvent(args)
        gamepieceConfigTab.handlePreviewEvent(args)


class MySelectHandler(PersistentEventHandler, adsk.core.SelectionEventHandler):
    """Called when a selection in the current design is made."""

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.SelectionEventArgs) -> None:
        if gamepieceConfigTab.isVisible:
            gamepieceConfigTab.handleSelectionEvent(args, args.selection.entity)

        if jointConfigTab.isVisible:
            jointConfigTab.handleSelectionEvent(args, args.selection.entity)


class MyPreselectEndHandler(PersistentEventHandler, adsk.core.SelectionEventHandler):
    """Called upon a pre-selection end in the current design. (Mouse hover off)"""

    def __init__(self, cmd: adsk.core.Command) -> None:
        super().__init__()
        self.cmd = cmd

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.SelectionEventArgs) -> None:
        self.cmd.setCursor("", 0, 0)  # Reset mouse icon to default


class ConfigureCommandInputChanged(PersistentEventHandler, adsk.core.InputChangedEventHandler):
    """Called when an input field in the configuration panel has been updated."""

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.InputChangedEventArgs) -> None:
        if generalConfigTab.isActive:
            generalConfigTab.handleInputChanged(args)

        if jointConfigTab.isVisible and jointConfigTab.isActive:
            jointConfigTab.handleInputChanged(args, INPUTS_ROOT)

        if gamepieceConfigTab.isVisible and gamepieceConfigTab.isActive:
            gamepieceConfigTab.handleInputChanged(args, INPUTS_ROOT)


class MyCommandDestroyHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when the configuration panel is destroyed."""

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        jointConfigTab.reset()
        gamepieceConfigTab.reset()

        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        for group in design.rootComponent.customGraphicsGroups:
            group.deleteMe()
