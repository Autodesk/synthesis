
import os
import sys
import adsk, adsk.core, adsk.fusion, traceback
import traceback

app_path = os.path.dirname(__file__)

sys.path.insert(0, app_path)
sys.path.insert(0, os.path.join(app_path, 'apper'))

my_addin = None

try:
    import config
    # Figure out a better way to install python deps
    try:
        import apper
        import pygltflib
        import numpy
        import google.protobuf

    except:
        app = adsk.core.Application.get()
        ui = app.userInterface
        progressBar = ui.createProgressDialog()
        progressBar.isCancelButtonShown = False
        progressBar.reset()
        progressBar.show("Synthesis glTF Exporter", f"Installing dependencies...", 0, 3, 0)
        adsk.doEvents()

        try:
            from pathlib import Path
            import platform
            import subprocess

            system = platform.system()
            if system == "Windows":
                pythonFolder = Path(os.__file__).parents[1]  # Assumes the location of the fusion python executable is two folders up from the os lib location
            elif system == "Darwin":   # macos
                pythonFolder = Path(os.__file__).parents[2] / "bin"
                progressBar.message = f"Installing pip..."
                adsk.doEvents()
                os.system(f"curl https://bootstrap.pypa.io/get-pip.py -o \"{pythonFolder / 'get-pip.py'}\"")
                os.system(f"\"{pythonFolder / 'python'}\" \"{pythonFolder / 'get-pip.py'}\"")
            else:
                raise ImportError(f"Unsupported platform! This add-in only supports windows and macos")

            pipDeps = ["pygltflib", "numpy", "protobuf"]
            for depName in pipDeps:
                progressBar.progressValue += 1
                progressBar.message = f"Installing {depName}..."
                adsk.doEvents()
                os.system(f"\"{pythonFolder / 'python'}\" -m pip install {depName}")

            if system == "Darwin":  # macos # TODO: High priority: This method of fixing the python deps on macos can potentially break other fusion add-ins and potentially parts of fusion itself. Find a better method to use deps on both windows and macos.
                pipAntiDeps = ["dataclasses", "typing"]
                for depName in pipAntiDeps:
                    progressBar.message = f"Uninstalling {depName}..."
                    adsk.doEvents()
                    os.system(f"\"{pythonFolder / 'python'}\" -m pip uninstall {depName} -y")

            progressBar.hide()

            import apper
            import pygltflib
            import numpy
            import google.protobuf

        except:
            print(traceback.format_exc())
            app = adsk.core.Application.get()
            ui = app.userInterface
            if ui:
                ui.messageBox(f'Unable to install python dependencies for the glTF Exporter for Synthesis addin! \n\n{traceback.format_exc()}')

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
        ui.messageBox(f'Unable to start glTF Exporter for Synthesis!\nPlease contact frc@autodesk.com to report this bug.\n\n{traceback.format_exc()}')
        # ui.messageBox(f'Initialization: {traceback.format_exc()}')

# Set to True to display various useful messages when debugging your app
debug = False

def run(context):
    if my_addin is None:
        return
    try:
        my_addin.run_app()
    except:
        app = adsk.core.Application.get()
        ui = app.userInterface
        if ui:
            ui.messageBox(f'glTF Exporter for Synthesis has encountered an error!\nPlease contact frc@autodesk.com to report this bug.\n\n{traceback.format_exc()}')

def stop(context):
    try:
        my_addin.stop_app()
        sys.path.pop(0)
        sys.path.pop(0)
    except:
        pass
