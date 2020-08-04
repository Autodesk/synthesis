
import os
import sys
import adsk, adsk.core, adsk.fusion, traceback
import traceback

app_path = os.path.dirname(__file__)

sys.path.insert(0, app_path)
sys.path.insert(0, os.path.join(app_path, 'apper'))

try:
    import config
    import apper

    class cd:
        def __init__(self, newPath):
            self.newPath = os.path.expanduser(newPath)

        def __enter__(self):
            self.savedPath = os.getcwd()
            os.chdir(self.newPath)

        def __exit__(self, etype, value, traceback):
            os.chdir(self.savedPath)

    # Figure out a better way to install python deps TODO: check for cross compatibility
    try:
        import pygltflib
        import numpy
        import google.protobuf
    except:
        try:
            from pathlib import Path
            p = Path(os.__file__).parents[1] # Assumes the location of the fusion python executable is two folders up from the os lib location
            with cd(p):
                os.system("python -m pip install pygltflib")
                os.system("python -m pip install numpy")
                os.system("python -m pip install protobuf")
            from pygltflib import *
            import numpy as np

        except:
            app = adsk.core.Application.get()
            ui = app.userInterface
            if ui:
                ui.messageBox('Fatal Error: Unable to import libraries {}'.format(traceback.format_exc()))

    # from .commands.ExportCommand import ExportCommand
    from .commands.ExportPaletteCommand import ExportPaletteSendCommand, ExportPaletteShowCommand

    my_addin = apper.FusionApp(config.app_name, config.company_name, False)

    # my_addin.add_command(
    #     'Quick export to glTF',
    #     ExportCommand,
    #     {
    #         'cmd_description': 'Exports the open design to a glTF file with default settings.',
    #         'cmd_id': 'quick_export_gltf',
    #         'workspace': 'FusionSolidEnvironment',
    #         'toolbar_panel_id': 'Export to glTF',
    #         'toolbar_tab_id': 'export_gltf_tab',
    #         'toolbar_tab_name': 'Synthesis glTF Exporter',
    #         'cmd_resources': 'command_icons',
    #         'command_visible': True,
    #         'command_promoted': True,
    #     }
    # )

    my_addin.add_command(
        'Export to glTF',
        ExportPaletteShowCommand,
        {
            'cmd_description': 'Exports the open design to a glTF file.',
            'cmd_id': 'export_gltf_palette_show',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Export to glTF',
            'toolbar_tab_id': 'export_gltf_tab',
            'toolbar_tab_name': 'Synthesis glTF Exporter',
            'cmd_resources': 'command_icons',
            'command_visible': True,
            'command_promoted': True,
            'palette_id': 'export_gltf_palette',
            'palette_name': 'Export Design to glTF',
            'palette_html_file_url': 'palette_html/SynthesisFusionGltfExporter.html',
            'palette_is_visible': True,
            'palette_show_close_button': False,
            'palette_is_resizable': False,
        }
    )

    app = adsk.core.Application.cast(adsk.core.Application.get())
    ui = app.userInterface

except:
    app = adsk.core.Application.get()
    ui = app.userInterface
    if ui:
        ui.messageBox(f'Initialization: {traceback.format_exc()}')

# Set to True to display various useful messages when debugging your app
debug = False

def run(context):
    my_addin.run_app()

def stop(context):
    my_addin.stop_app()
    sys.path.pop(0)
    sys.path.pop(0)
