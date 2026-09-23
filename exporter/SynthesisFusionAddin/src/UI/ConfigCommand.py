"""
Central location for which all UI is generated and handled for the main configuration panel.
"""

import importlib
import json
import os
import re
import traceback
import webbrowser
from typing import Any

import adsk.core
import adsk.fusion
from adsk.core import Palette

import src.Parser.ExporterOptions as moduleExporterOptions
import src.Parser.SynthesisParser.Parser as Parser
import src.UI.GamepieceConfigTab as GamepieceConfigTab
import src.UI.GeneralConfigTab as GeneralConfigTab
import src.UI.JointConfigTab as JointConfigTab
import src.UI.TaggingConfigTab as TaggingConfigTab
from src import APP_WEBSITE_URL, Logging, gm
from src.APS.APS import getAuth, getUserInfo
from src.lib.Handlers import PersistentEventHandler
from src.lib.Util import convertMassUnitsTo, designMassCalculation
from src.Logging import getLogger, logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.Utilities import guid_occurrence
from src.Types import SELECTABLE_JOINT_TYPES, ExportLocation, ExportMode
from src.UI import FileDialogConfig

generalConfigTab: GeneralConfigTab.GeneralConfigTab
jointConfigTab: JointConfigTab.JointConfigTab
gamepieceConfigTab: GamepieceConfigTab.GamepieceConfigTab
taggingConfigTab: TaggingConfigTab.TaggingConfigTab

exporterPalette: Palette
logger = getLogger()

INPUTS_ROOT: adsk.core.CommandInputs
PALETTE_ID = "synthesis_configure"
USE_NEW_UI = True
USE_OLD_UI = False  # allow both independently for testing

logger = Logging.getLogger()


def reload() -> None:
    """Reloads the sub modules to reflect any changes made during development."""
    importlib.reload(GeneralConfigTab)
    importlib.reload(GamepieceConfigTab)
    importlib.reload(JointConfigTab)
    importlib.reload(TaggingConfigTab)

    importlib.reload(moduleExporterOptions)
    importlib.reload(Parser)
    Parser.reload()


class ConfigureCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    """Called when the panel is initially created."""

    def __init__(self, configure: Any) -> None:
        super().__init__()

    @logFailure(messageBox=True)
    def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        cmd = args.command
        gm.ui.activeSelections.clear()
        if USE_OLD_UI:
            global INPUTS_ROOT
            INPUTS_ROOT = cmd.commandInputs

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

            exporterOptions = (
                moduleExporterOptions.ExporterOptions().readFromDesign() or moduleExporterOptions.ExporterOptions()
            )

            cmd.isAutoExecute = True
            cmd.isExecutedWhenPreEmpted = False
            cmd.okButtonText = "Export"
            cmd.helpFile = os.path.join(".", "src", "Resources", "HTML", "info.html")

            global generalConfigTab
            generalConfigTab = GeneralConfigTab.GeneralConfigTab(args, exporterOptions)

            global gamepieceConfigTab
            gamepieceConfigTab = GamepieceConfigTab.GamepieceConfigTab(args, exporterOptions)
            generalConfigTab.gamepieceConfigTab = gamepieceConfigTab

            global jointConfigTab
            jointConfigTab = JointConfigTab.JointConfigTab(args)
            generalConfigTab.jointConfigTab = jointConfigTab

            global taggingConfigTab
            taggingConfigTab = TaggingConfigTab.TaggingConfigTab(args)
            generalConfigTab.taggingConfigTab = taggingConfigTab

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

            if len(exporterOptions.tags):
                for token, tag in exporterOptions.tags.items():
                    fusionBody = design.findEntityByToken(token)
                    if len(fusionBody):
                        taggingConfigTab.addTag(fusionBody[0], tag)

            getAuth()
            user_info_result = getUserInfo()
            if user_info_result.is_err():
                user_name = "Not Signed In"
            else:
                user_name = user_info_result.unwrap().given_name

            apsSettings = INPUTS_ROOT.addTabCommandInput("aps_settings", f"APS Settings ({user_name})")
            apsSettings.tooltip = "Configuration settings for Autodesk Platform Services."

        if USE_NEW_UI:
            palettes = gm.ui.palettes
            global exporterPalette
            exporterPalette = palettes.itemById(PALETTE_ID)
            if exporterPalette:
                exporterPalette.deleteMe()

            exporterPalette = palettes.add(
                id=PALETTE_ID,
                name="Synthesis Exporter",
                htmlFileURL="web/dist/index.html",
                isVisible=True,
                showCloseButton=True,
                isResizable=True,
                width=1200,
                height=800,
                useNewWebBrowser=True,
            )
            onMessage = IncomingHTMLMessageHandler()
            exporterPalette.incomingFromHTML.add(onMessage)

            onClose = PaletteCloseHandler()
            exporterPalette.closed.add(onClose)

            exporterPalette.isVisible = True


class PaletteCloseHandler(PersistentEventHandler, adsk.core.UserInterfaceGeneralEventHandler):
    @logFailure
    def notify(self, e: Any) -> None:
        if exporterPalette:
            exporterPalette.deleteMe()


class IncomingHTMLMessageHandler(PersistentEventHandler, adsk.core.HTMLEventHandler):
    @logFailure(messageBox=True)
    def notify(self, html_args: adsk.core.HTMLEventArgs) -> None:
        data = json.loads(html_args.data)

        if html_args.action == "init":
            exporterOptions = (
                moduleExporterOptions.ExporterOptions().readFromDesign() or moduleExporterOptions.ExporterOptions()
            )

            design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
            gamepieceData = []
            for synGamepiece in exporterOptions.gamepieces:  # Copy this
                fusionOccurrence = design.findEntityByToken(synGamepiece.occurrenceToken)[0]
                gamepiece = adsk.fusion.Occurrence.cast(fusionOccurrence)
                gamepieceData.append(buildGamepiece(gamepiece))

            jointData = []
            if len(exporterOptions.joints):
                for synJoint in exporterOptions.joints:
                    fusionJoints = design.findEntityByToken(synJoint.jointToken)
                    if len(fusionJoints):
                        try:
                            joint = adsk.fusion.Joint.cast(fusionJoints[0])
                            jointData.append(buildJoint(joint))
                        except Exception as e:
                            logger.error(e)
            else:
                for joint in [*design.rootComponent.allJoints, *design.rootComponent.allAsBuiltJoints]:
                    if joint.jointMotion.jointType in SELECTABLE_JOINT_TYPES and not joint.isSuppressed:
                        try:
                            jointData.append(buildJoint(joint))
                        except Exception as e:
                            logger.error(e)

            tagData = []
            if len(exporterOptions.tags):
                for token, tag in exporterOptions.tags.items():
                    fusionBody = design.findEntityByToken(token)

                    if len(fusionBody):
                        body = adsk.fusion.BRepBody.cast(fusionBody[0])
                        try:
                            tagData.append(buildTaggedObject(body))
                        except Exception as e:
                            logger.error(e)
            html_args.returnData = json.dumps(
                {
                    "gamepieceData": gamepieceData,
                    "jointData": jointData,
                    "tagData": tagData,
                    "options": exporterOptions.writeToJson(),
                    "calculatedMass": convertMassUnitsTo(designMassCalculation()),
                }
            )
        elif html_args.action == "export":
            opts = moduleExporterOptions.ExporterOptions().readFromJSON(data)
            export(opts, html_args)
        elif html_args.action == "save":
            opts = moduleExporterOptions.ExporterOptions().readFromJSON(data)
            opts.writeToDesign()
            html_args.returnData = "{}"

        elif html_args.action == "selectJoint":
            try:
                selection = gm.app.userInterface.selectEntity("Select Joints", "Joints")
                joint = adsk.fusion.Joint.cast(selection.entity)
                html_args.returnData = json.dumps(buildJoint(joint))
            except Exception as e:
                html_args.returnData = json.dumps({"_err": e.__repr__()})
                logger.error(e)
            gm.ui.activeSelections.clear()
        elif html_args.action == "selectGamepiece":
            try:
                selection = gm.app.userInterface.selectEntity("Select Gamepieces", "Occurrences")
                rootComponent = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct).rootComponent
                entity = adsk.fusion.Occurrence.cast(selection.entity)
                occurrenceList: adsk.fusion.OccurrenceList = rootComponent.allOccurrencesByComponent(entity.component)
                gamepieces = []
                for i in range(occurrenceList.count):
                    occurrence = occurrenceList.item(i)
                    gamepieces.append(buildGamepiece(occurrence))
                html_args.returnData = json.dumps(gamepieces)
            except Exception as e:
                html_args.returnData = json.dumps({"_err": e.__repr__()})
                logger.error(e)
            gm.ui.activeSelections.clear()
        elif html_args.action == "selectBody":

            try:
                selection = gm.app.userInterface.selectEntity("Select Body", "SolidBodies,SurfaceBodies")
                logger.info(selection)
                body = adsk.fusion.BRepBody.cast(selection.entity)
                html_args.returnData = json.dumps(buildTaggedObject(body))
            except Exception as e:
                html_args.returnData = json.dumps({"_err": e.__repr__()})
                logger.error(e)

        elif html_args.action == "cancelSelection":
            gm.ui.terminateActiveCommand()
            html_args.returnData = "{}"
        else:
            gm.ui.messageBox(f"Event {html_args.action} arrived<span>{json.dumps(data, indent=2)}</span>")


def buildJoint(joint: adsk.fusion.Joint) -> dict[str, Any]:
    return {
        "name": joint.name,
        "entityToken": joint.entityToken,
        "jointType": joint.jointMotion.jointType,
    }


def buildTaggedObject(body: adsk.fusion.BRepBody) -> dict[str, Any]:
    key_body = body.nativeObject or body
    return {
        "name": key_body.name,
        "componentName": key_body.parentComponent.name,
        "entityToken": key_body.entityToken,
    }


def buildGamepiece(gamepiece: adsk.fusion.Occurrence) -> dict[str, Any]:
    physicalProps = gamepiece.component.getPhysicalProperties(adsk.fusion.CalculationAccuracy.LowCalculationAccuracy)
    response = {
        "name": gamepiece.name,
        "occurrenceToken": guid_occurrence(gamepiece),
        "mass": physicalProps.mass,
        "entityIDs": [gamepiece.entityToken],
    }

    def addChildOccurrences(childOccurrences: adsk.fusion.OccurrenceList) -> None:
        for i in range(childOccurrences.count):
            occ = childOccurrences.item(i)
            response["entityIDs"].append(occ.entityToken)

            if occ.childOccurrences:
                addChildOccurrences(occ.childOccurrences)

    if gamepiece.childOccurrences:
        addChildOccurrences(gamepiece.childOccurrences)
    return response


@logFailure(messageBox=True)
def export(exporterOptions: moduleExporterOptions.ExporterOptions, html_args: adsk.core.HTMLEventArgs) -> None:
    logger.info("NEWUI")
    logger.info(exporterOptions)
    design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
    fullName = design.rootComponent.name
    versionMatch = re.search(r"v\d+", fullName)
    docName = (fullName[: versionMatch.start()].strip() if versionMatch else fullName).replace(" ", "_")
    docVersion = versionMatch.group() if versionMatch else "v0"

    processedFileName = gm.app.activeDocument.name.replace(" ", "_")
    defaultFileName = f"{'_'.join([docName, docVersion])}.mira"
    if exporterOptions.exportLocation == ExportLocation.DOWNLOAD:
        savepath = FileDialogConfig.saveFileDialog(exporterOptions.fileLocation, defaultFileName)
    else:
        savepath = processedFileName

    if not savepath:  # User cancelled the save dialog
        return

    adsk.doEvents()
    exporterOptions.fileLocation = savepath
    exporterOptions.name = docName
    exporterOptions.version = docVersion
    exporterOptions.materials = 0

    try:
        Parser.Parser(exporterOptions).export()
    except RuntimeError as e:
        logger.error(f"Export failed with RuntimeError:\n{traceback.format_exc()}")
        html_args.returnData = json.dumps({"_err": str(e)})
        return
    exporterOptions.writeToDesign()
    html_args.returnData = "{}"

    if exporterOptions.openSynthesisUponExport:
        res = webbrowser.open(APP_WEBSITE_URL)
        if not res:
            gm.ui.messageBox("Failed to open Synthesis in your default browser.")


class ConfigureCommandExecuteHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when the `Export` button is clicked from the main configuration panel."""

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        exporterOptions = (
            moduleExporterOptions.ExporterOptions().readFromDesign() or moduleExporterOptions.ExporterOptions()
        )

        fullName: str = design.rootComponent.name
        versionMatch = re.search(r"v\d+", fullName)
        if versionMatch:
            strippedName: str = fullName[0 : versionMatch.start()].strip()
        else:
            strippedName = fullName
        docName = strippedName.replace(" ", "_")
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
        selectedTags = taggingConfigTab.getTags()

        exporterOptions = moduleExporterOptions.ExporterOptions(
            savepath,
            docName,
            docVersion,
            materials=0,
            joints=selectedJoints,
            wheels=selectedWheels,
            gamepieces=selectedGamepieces,
            tags=selectedTags,
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
        logger.info("OLDUI")
        logger.info(exporterOptions)

        try:
            Parser.Parser(exporterOptions).export()
        except Exception:
            logger.error(f"Export failed:\n{traceback.format_exc()}")
            jointConfigTab.reset()
            gamepieceConfigTab.reset()

            return
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

        if taggingConfigTab.isVisible and taggingConfigTab.isActive:
            taggingConfigTab.handleInputChanged(args, INPUTS_ROOT)


class MyCommandDestroyHandler(PersistentEventHandler, adsk.core.CommandEventHandler):
    """Called when the configuration panel is destroyed."""

    @logFailure(messageBox=True)
    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        jointConfigTab.reset()
        gamepieceConfigTab.reset()
        # exporterPalette.deleteMe()
        design = adsk.fusion.Design.cast(adsk.core.Application.get().activeProduct)
        for group in design.rootComponent.customGraphicsGroups:
            group.deleteMe()
