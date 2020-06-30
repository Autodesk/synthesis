
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

    # Figure out a better way to install and import protobuf TODO: check for cross compatibility
    try:
        from .proto.synthesis_importbuf_pb2 import *
    except:
        try:
            from pathlib import Path
            p = Path(os.__file__).parents[1] # Assumes the location of the fusion python executable is two folders up from the os lib location
            with cd(p):
                os.system("python -m pip install protobuf") # Install protobuf with the fusion
            from .proto.synthesis_importbuf_pb2 import *
        except:
            app = adsk.core.Application.get()
            ui = app.userInterface
            if ui:
                ui.messageBox('Fatal Error: Unable to import protobuf {}'.format(traceback.format_exc()))

    # Basic Fusion 360 Command Base samples
    from .commands.ExportCommand import ExportCommand, exportRobot
    from .commands.SampleCommand2 import SampleCommand2

    # Palette Command Base samples
    from .commands.SamplePaletteCommand import SamplePaletteSendCommand, SamplePaletteShowCommand

    # Various Application event samples
    from .commands.SampleCustomEvent import SampleCustomEvent1
    from .commands.SampleDocumentEvents import SampleDocumentEvent1, SampleDocumentEvent2
    from .commands.SampleWorkspaceEvents import SampleWorkspaceEvent1
    from .commands.SampleWebRequestEvent import SampleWebRequestOpened
    from .commands.SampleCommandEvents import SampleCommandEvent

# Create our addin definition object
    my_addin = apper.FusionApp(config.app_name, config.company_name, False)

    # Creates a basic Hello World message box on execute
    my_addin.add_command(
        'Export Robot',
        ExportCommand,
        {
            'cmd_description': 'Export your robot to Synthesis.',
            'cmd_id': 'sample_cmd_1',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Commands',
            'cmd_resources': 'command_icons',
            'command_visible': True,
            'command_promoted': True,
        }
    )

    # General command showing inputs and user interaction
    my_addin.add_command(
        'Sample Command 2',
        SampleCommand2,
        {
            'cmd_description': 'A simple example of a Fusion 360 Command with various inputs',
            'cmd_id': 'sample_cmd_2',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Commands',
            'cmd_resources': 'command_icons',
            'command_visible': True,
            'command_promoted': False,
        }
    )

    # Create an html palette to as an alternative UI
    my_addin.add_command(
        'Sample Palette Command - Show',
        SamplePaletteShowCommand,
        {
            'cmd_description': 'Shows the Fusion 360 Demo Palette',
            'cmd_id': 'sample_palette_show',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Palette',
            'cmd_resources': 'palette_icons',
            'command_visible': True,
            'command_promoted': True,
            'palette_id': 'sample_palette',
            'palette_name': 'Sample Fusion 360 HTML Palette',
            'palette_html_file_url': 'palette_html/SynthesisFusionProtobufExporter.html',
            'palette_is_visible': True,
            'palette_show_close_button': True,
            'palette_is_resizable': True,
            'palette_width': 500,
            'palette_height': 600,
        }
    )

    # Send data from Fusion 360 to the palette
    my_addin.add_command(
        'Send Info to Palette',
        SamplePaletteSendCommand,
        {
            'cmd_description': 'Send data from a regular Fusion 360 command to a palette',
            'cmd_id': 'sample_palette_send',
            'workspace': 'FusionSolidEnvironment',
            'toolbar_panel_id': 'Palette',
            'cmd_resources': 'palette_icons',
            'command_visible': True,
            'command_promoted': False,
            'palette_id': 'sample_palette',
        }
    )

    app = adsk.core.Application.cast(adsk.core.Application.get())
    ui = app.userInterface

    # Uncomment as necessary.  Running all at once can be overwhelming :)
    # my_addin.add_custom_event("SynthesisFusionProtobufExporter_message_system", SampleCustomEvent1)

    # my_addin.add_document_event("SynthesisFusionProtobufExporter_open_event", app.documentActivated, SampleDocumentEvent1)
    # my_addin.add_document_event("SynthesisFusionProtobufExporter_close_event", app.documentClosed, SampleDocumentEvent2)

    # my_addin.add_workspace_event("SynthesisFusionProtobufExporter_workspace_event", ui.workspaceActivated, SampleWorkspaceEvent1)

    # my_addin.add_web_request_event("SynthesisFusionProtobufExporter_web_request_event", app.openedFromURL, SampleWebRequestOpened)

    # my_addin.add_command_event("SynthesisFusionProtobufExporter_command_event", app.userInterface.commandStarting, SampleCommandEvent)

except:
    app = adsk.core.Application.get()
    ui = app.userInterface
    if ui:
        ui.messageBox('Initialization: {}'.format(traceback.format_exc()))

# Set to True to display various useful messages when debugging your app
debug = False

def run(context):
    my_addin.run_app()

    exportRobot() # export on startup for debugging purposes TODO delete me


def stop(context):
    my_addin.stop_app()
    sys.path.pop(0)
    sys.path.pop(0)
