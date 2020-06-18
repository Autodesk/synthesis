
import os
import sys
import adsk.core
import traceback

app_path = os.path.dirname(__file__)

sys.path.insert(0, app_path)
sys.path.insert(0, os.path.join(app_path, 'apper'))

try:
    import config
    import apper

    # Basic Fusion 360 Command Base samples
    from .commands.SampleCommand1 import SampleCommand1
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
        'Sample Command 1',
        SampleCommand1,
        {
            'cmd_description': 'Hello Synthesis!',
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
            'palette_html_file_url': 'palette_html/SynthesisFusionExporter.html',
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
    # my_addin.add_custom_event("SynthesisFusionExporter_message_system", SampleCustomEvent1)

    # my_addin.add_document_event("SynthesisFusionExporter_open_event", app.documentActivated, SampleDocumentEvent1)
    # my_addin.add_document_event("SynthesisFusionExporter_close_event", app.documentClosed, SampleDocumentEvent2)

    # my_addin.add_workspace_event("SynthesisFusionExporter_workspace_event", ui.workspaceActivated, SampleWorkspaceEvent1)

    # my_addin.add_web_request_event("SynthesisFusionExporter_web_request_event", app.openedFromURL, SampleWebRequestOpened)

    # my_addin.add_command_event("SynthesisFusionExporter_command_event", app.userInterface.commandStarting, SampleCommandEvent)

except:
    app = adsk.core.Application.get()
    ui = app.userInterface
    if ui:
        ui.messageBox('Initialization: {}'.format(traceback.format_exc()))

# Set to True to display various useful messages when debugging your app
debug = False

def getComponents(occurrences, level, input):
    for i in range(0, occurrences.count):
        occurence = occurrences.item(i)

        input += 'Name: ' + occurence.name + '\n'
        
        if occurence.childOccurrences:
            input = getComponents(occurence.childOccurrences, level + 1, input)

    return input


def run(context):
    ui = None
    try:
        app = adsk.core.Application.get()
        ui  = app.userInterface
        product = app.activeProduct

        design = adsk.fusion.Design.cast(product)
        if not design:
            ui.messageBox('No active Fusion design', 'No Design')
            return

        # get root
        rootComp = design.rootComponent

        # traverse assembly recursively + print in message box
        resultString = 'Assembly structure of ' + design.parentDocument.name + '\n'
        resultString = getComponents(rootComp.occurrences.asList, 1, resultString)
        ui.messageBox(resultString)
    except:
        if ui:
            ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

    my_addin.run_app()


def stop(context):
    my_addin.stop_app()
    sys.path.pop(0)
    sys.path.pop(0)
