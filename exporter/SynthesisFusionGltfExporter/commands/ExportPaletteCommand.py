import adsk.core
import adsk.fusion
import adsk.cam

import json
import time

import apper
from apper import AppObjects
from fusionutils.DebugHierarchy import *
from ..gltfutils.GLTFDesignExporter import GLTFDesignExporter

def exportDesign(showFileDialog=False, enableMaterials=True, enableMaterialOverrides=True, enableFaceMaterials=True, exportVisibleBodiesOnly=True):
    ao = AppObjects()

    if ao.document.dataFile is None:
        ao.ui.messageBox("Export error: You must save your Fusion design before exporting!")
        return

    start = time.perf_counter()
    startRealtime = time.time()

    exporter = GLTFDesignExporter(ao, enableMaterials, enableMaterialOverrides, enableFaceMaterials, exportVisibleBodiesOnly)
    if showFileDialog:
        dialog = ao.ui.createFileDialog() # type: adsk.core.FileDialog
        dialog.filter = "glTF Binary (*.glb)"
        dialog.isMultiSelectEnabled = False
        dialog.title = "Select glTF Export Location"
        dialog.initialFilename = f'{ao.document.name.replace(" ", "_")}.glb'
        results = dialog.showSave()
        if results != 0 and results != 2: # For some reason the generated python API enums were wrong, so we're just using the literals
            ao.ui.messageBox(f"The glTF export was cancelled.")
            return
        filePath = dialog.filename

    else:
        filePath = f'C:/temp/{ao.document.name.replace(" ", "_")}_{int(time.time())}.glb'
    exportResults = exporter.saveGLB(filePath)
    if exportResults is None:
        ao.ui.messageBox(f"The glTF export was cancelled.")
        return
    perfResults, bufferResults, warnings, modelStats, eventCounter = exportResults

    end = time.perf_counter()
    endRealtime = time.time()
    finishedMessage = (f"glTF export completed in {round(end - start, 4)} seconds ({round(endRealtime - startRealtime, 4)} realtime)\n"
                       f"File saved to {filePath}\n\n"
                       f"==== Export Performance Results ====\n"
                       f"{perfResults}\n"
                       f"==== Buffer Writing Results ====\n"
                       f"{bufferResults}\n"
                       f"==== Model Stats ====\n"
                       f"{modelStats}\n"
                       f"==== Events Counter ====\n"
                       f"{eventCounter}\n"
                       f"==== Warnings ====\n"
                       f"{warnings}\n"
                       )
    ao.ui.messageBox(finishedMessage)

# Class for a Fusion 360 Palette Command
class ExportPaletteShowCommand(apper.PaletteCommandBase):

    # Run when user executes command in UI, useful for handling extra tasks on palette like docking
    def on_palette_execute(self, palette: adsk.core.Palette):

        self.palette = palette

        # Dock the palette to the right side of Fusion window.
        if palette.dockingState == adsk.core.PaletteDockingStates.PaletteDockStateFloating:
            palette.dockingState = adsk.core.PaletteDockingStates.PaletteDockStateRight

        palette.setMaximumSize(250, 250)
        palette.setMinimumSize(250, 250)
        palette.setSize(250, 250)

    # Run when ever a fusion event is fired from the corresponding web page
    def on_html_event(self, html_args: adsk.core.HTMLEventArgs):
        data = json.loads(html_args.data)
        action = data['action']
        self.palette.isVisible = False

        if action == 'export':
            settings = data['settings']
            materials = settings['materials']
            faceMaterials = settings['faceMaterials']
            exportHidden = not settings['exportHidden']
            exportDesign(showFileDialog=True, enableMaterials=materials, enableFaceMaterials=faceMaterials, exportVisibleBodiesOnly=exportHidden)


    # Handle any extra cleanup when user closes palette here
    def on_palette_close(self):
        pass


# This is a standard Fusion Command that will send data to the palette
class ExportPaletteSendCommand(apper.Fusion360CommandBase):

    def __init__(self, name, options):
        super().__init__(name, options)

        # Pass in the palette_id as extra data in command definition
        # A generally useful technique to make commands more re-usable
        self.palette_id = options.get('palette_id', None)

    # When the command is clicked it will send this message to the HTML Palette
    def on_execute(self, command: adsk.core.Command, command_inputs: adsk.core.CommandInputs, args, input_values):

        # Get Reference to Palette
        ao = apper.AppObjects()
        palette = ao.ui.palettes.itemById(self.palette_id)

        # Get input value from string input
        message = input_values['palette_string']

        # Send message to the HTML Page
        if palette:
            palette.sendInfoToHTML('send', message)

    # Run when the user selects your command icon from the Fusion 360 UI
    def on_create(self, command: adsk.core.Command, command_inputs: adsk.core.CommandInputs):

        command_inputs.addStringValueInput('palette_string', 'Palette Message', 'Some text to send to Palette')
