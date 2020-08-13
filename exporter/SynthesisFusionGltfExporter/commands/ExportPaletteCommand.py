import adsk.core
import adsk.fusion
import adsk.cam

import json

import apper
from apper import AppObjects
from ..gltf.FusionGltfExporter import FusionGltfExporter
from ..gltf.utils.GltfConstants import FileType


# Class for a Fusion 360 Palette Command
class ExportPaletteShowCommand(apper.PaletteCommandBase):

    def __init__(self, name: str, options: dict):
        super().__init__(name, options)
        self.ao = AppObjects()
        self.exporter = FusionGltfExporter(self.ao)

    # Run when user executes command in UI, useful for handling extra tasks on palette like docking
    def on_palette_execute(self, palette: adsk.core.Palette):

        self.palette = palette

        # Dock the palette to the right side of Fusion window.
        if palette.dockingState == adsk.core.PaletteDockingStates.PaletteDockStateFloating:
            palette.dockingState = adsk.core.PaletteDockingStates.PaletteDockStateRight

        palette.setMaximumSize(300, 300)
        palette.setMinimumSize(300, 300)
        palette.setSize(300, 300)

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
            quality = int(settings['quality'])
            includeSynthesis = settings['includeSynthesis']
            useGlb = FileType.fromString(settings['useGlb'])
            self.exporter.exportDesignUI(self.ao.app.activeDocument, showFileDialog=True, enableMaterials=materials, enableFaceMaterials=faceMaterials, exportVisibleBodiesOnly=exportHidden, fileType=useGlb, quality=quality, includeSynthesisData=includeSynthesis)


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
