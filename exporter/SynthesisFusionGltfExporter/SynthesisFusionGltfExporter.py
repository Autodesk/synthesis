
import os
import sys
import adsk, adsk.core, adsk.fusion, traceback
import traceback
#
# import pydevd_pycharm
# pydevd_pycharm.settrace('localhost', port=12343, stdoutToServer=True, stderrToServer=True)

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

    # Figure out a better way to install and import protobuf TODO: check for cross compatibility
    try:
        from pygltflib import *
        import numpy as np
    except:
        try:
            from pathlib import Path
            p = Path(os.__file__).parents[1] # Assumes the location of the fusion python executable is two folders up from the os lib location
            with cd(p):
                os.system("python -m pip install pygltflib") # Install protobuf with the fusion
                os.system("python -m pip install numpy") # Install protobuf with the fusion
            from pygltflib import *
            import numpy as np

        except:
            app = adsk.core.Application.get()
            ui = app.userInterface
            if ui:
                ui.messageBox('Fatal Error: Unable to import libraries {}'.format(traceback.format_exc()))

    # Basic Fusion 360 Command Base samples
    from .commands.ExportCommand import ExportCommand, exportDesign

    # Palette Command Base samples
    from .commands.ExportPaletteCommand import ExportPaletteSendCommand, ExportPaletteShowCommand


# Create our addin definition object
    my_addin = apper.FusionApp(config.app_name, config.company_name, False)

    # # Creates a basic Hello World message box on execute
    # my_addin.add_command(
    #     'Export Assembly',
    #     ExportCommand,
    #     {
    #         'cmd_description': 'Export your assembly to Synthesis.',
    #         'cmd_id': 'sample_cmd_1',
    #         'workspace': 'FusionSolidEnvironment',
    #         'toolbar_panel_id': 'Commands',
    #         'cmd_resources': 'command_icons',
    #         'command_visible': True,
    #         'command_promoted': True,
    #     }
    # )

    # Create an html palette to as an alternative UI
    my_addin.add_command(
        'Export to glTF',
        ExportPaletteShowCommand,
        {
            'cmd_description': 'Exports the open design to a glTF file.',
            'cmd_id': 'export_gltf_palette_show',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Export to glTF',
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

    # Uncomment as necessary.  Running all at once can be overwhelming :)
    # my_addin.add_custom_event("SynthesisFusionGltfExporter_message_system", SampleCustomEvent1)

    # my_addin.add_document_event("SynthesisFusionGltfExporter_open_event", app.documentActivated, SampleDocumentEvent1)
    # my_addin.add_document_event("SynthesisFusionGltfExporter_close_event", app.documentClosed, SampleDocumentEvent2)

    # my_addin.add_workspace_event("SynthesisFusionGltfExporter_workspace_event", ui.workspaceActivated, SampleWorkspaceEvent1)

    # my_addin.add_web_request_event("SynthesisFusionGltfExporter_web_request_event", app.openedFromURL, SampleWebRequestOpened)

    # my_addin.add_command_event("SynthesisFusionGltfExporter_command_event", app.userInterface.commandStarting, SampleCommandEvent)

except:
    app = adsk.core.Application.get()
    ui = app.userInterface
    if ui:
        ui.messageBox('Initialization: {}'.format(traceback.format_exc()))

# Set to True to display various useful messages when debugging your app
debug = False

def run(context):
    my_addin.run_app()
    #
    # try:
    #     exportDesign()  # export on startup for debugging purposes TODO delete me
    # except:
    #     app = adsk.core.Application.get()
    #     ui = app.userInterface
    #     if ui:
    #         ui.messageBox('Initialization: {}'.format(traceback.format_exc()))


def stop(context):
    my_addin.stop_app()
    sys.path.pop(0)
    sys.path.pop(0)
