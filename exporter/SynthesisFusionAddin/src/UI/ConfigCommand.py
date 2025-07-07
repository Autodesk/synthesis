"""
Central location for which all UI is generated and handled for the main configuration panel.
"""

import importlib
import json
import os
import re
import webbrowser
from typing import Any, TypedDict, Literal

import adsk.core
import adsk.fusion

import src.Parser.SynthesisParser.Parser as Parser
import src.UI.GamepieceConfigTab as GamepieceConfigTab
import src.UI.GeneralConfigTab as GeneralConfigTab
import src.UI.JointConfigTab as JointConfigTab
from src import APP_WEBSITE_URL, gm
from src.APS.APS import getAuth, getUserInfo
from src.Logging import getLogger, logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.Parser import Parser
from src.Parser.SynthesisParser.Utilities import guid_occurrence
from src.Types import SELECTABLE_JOINT_TYPES, ExportLocation, ExportMode
from src.UI import FileDialogConfig
from src.UI.Handlers import PersistentEventHandler
from src.UI.JointConfigTab import JointConfigTab
from src.Util import designMassCalculation, convertMassUnitsTo
from src.Utils import fusionAddInUtils as futil
from src.Utils.fusionAddInUtils import log

generalConfigTab: GeneralConfigTab.GeneralConfigTab
jointConfigTab: JointConfigTab.JointConfigTab
gamepieceConfigTab: GamepieceConfigTab.GamepieceConfigTab

logger = getLogger()

INPUTS_ROOT: adsk.core.CommandInputs

PALETTE_ID="synthesis_configure"

def reload() -> None:
    """Reloads the sub modules to reflect any changes made during development."""
    importlib.reload(GeneralConfigTab)
    importlib.reload(GamepieceConfigTab)
    importlib.reload(JointConfigTab)

    importlib.reload(Parser)
    logger.info("UI modules reloaded successfully.")


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

        palettes = gm.ui.palettes
        palette = palettes.itemById(PALETTE_ID)
        if palette is None:
            palette = palettes.add(
                id=PALETTE_ID,
                name="Synthesis Exporter",
                htmlFileURL="web/dist/index.html",
                isVisible=True,
                showCloseButton=True,
                isResizable=True,
                width=800,
                height=350,
                useNewWebBrowser=True
            )
            # futil.add_handler(palette.closed, palette_closed)
            # futil.add_handler(palette.navigatingURL, palette_navigating)
            futil.add_handler(palette.incomingFromHTML, on_palette_message)
        palette.isVisible = True
        # palette.dockingState = adsk.core.PaletteDockingStates.PaletteDockStateRight

        global generalConfigTab
        generalConfigTab = GeneralConfigTab.GeneralConfigTab(args, exporterOptions)

        global gamepieceConfigTab
        gamepieceConfigTab = GamepieceConfigTab.GamepieceConfigTab(args, exporterOptions)
        generalConfigTab.gamepieceConfigTab = gamepieceConfigTab

        global jointConfigTab
        jointConfigTab = JointConfigTab.JointConfigTab(args)
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



Checkbox = Literal["on"]|Literal["off"]|None
ExportEvent = TypedDict('ExportEvent', {"mode":Literal["dynamic"]|Literal["static"],"location":Literal["upload"]|Literal["download"],"weight":float,"export_as_part":Checkbox,"friction_override":Checkbox, "friction": float, "calculate_weight":Checkbox, "compress_output":Checkbox})

@logFailure(messageBox=True)
def on_palette_message(html_args: adsk.core.HTMLEventArgs):
    data  = json.loads(html_args.data)


    if html_args.action == "init":
        html_args.returnData = json.dumps({
            "mass":convertMassUnitsTo(designMassCalculation()),
        })
    elif html_args.action == "export":
        export(data)
    elif html_args.action == "selectJoint":
        selection = gm.app.userInterface.selectEntity("Select Joints", "Joints")
        joint= adsk.fusion.Joint.cast(selection.entity)

        html_args.returnData = json.dumps({
            "name":joint.name,
            "type":selection.entity.objectType,
            "entityToken":joint.entityToken,
            "jointType":joint.jointMotion.jointType,
        })
    elif html_args.action == "selectGamepiece":
        selection = gm.app.userInterface.selectEntity("Select Gamepieces", "Occurrences")
        gamepiece= adsk.fusion.Occurrence.cast(selection.entity)
        physicalProps = gamepiece.component.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
        response = {
            "name":gamepiece.name,
            "guid": guid_occurrence(gamepiece),
            "entityToken":gamepiece.entityToken,
            "mass": physicalProps.mass,

            "entityIDs": [gamepiece.entityToken]
        }
        def addChildOccurrences(childOccurrences: adsk.fusion.OccurrenceList) -> None:
            for occ in childOccurrences:
                response["entityIDs"].append(occ.entityToken)

                if occ.childOccurrences:
                    addChildOccurrences(occ.childOccurrences)

        if gamepiece.childOccurrences:
            addChildOccurrences(gamepiece.childOccurrences)

        html_args.returnData = json.dumps(response)
    else:

        gm.ui.messageBox(f"Event {html_args.action} arrived<span>{json.dumps(data, indent=2)}</span>")






@logFailure
def export(data:ExportEvent):
    design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
    exporterOptions = ExporterOptions().readFromDesign() or ExporterOptions()

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
        robotWeight=data.get("weight"),
        autoCalcRobotWeight=(data.get("calculate_weight") == "on"),
        autoCalcGamepieceWeight=gamepieceConfigTab.autoCalculateWeight,
        exportMode=data.get("mode"),
        exportLocation=data.get("location"),
        compressOutput=(data.get("compress_output") == "on"),
        exportAsPart=(data.get("export_as_part") == "on"),
        frictionOverride=(data.get("friction_override") == "on"),
        frictionOverrideCoeff=data.get("friction"),
        openSynthesisUponExport=False,
    )

    Parser(exporterOptions).export()
    exporterOptions.writeToDesign()
    jointConfigTab.reset()
    gamepieceConfigTab.reset()


class ConfigureCommandExecuteHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when the `Export` button is clicked from the main configuration panel."""

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        exporterOptions = ExporterOptions().readFromDesign() or ExporterOptions()

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

        Parser.Parser(exporterOptions).export()
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
