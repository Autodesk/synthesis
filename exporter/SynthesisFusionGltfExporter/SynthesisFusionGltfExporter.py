import os
import platform
import subprocess
import sys
from pathlib import Path
import traceback

import adsk
import adsk.core
import adsk.fusion

from .config import *

app_path = os.path.dirname(__file__)

sys.path.insert(0, app_path)
sys.path.insert(0, os.path.join(app_path, 'apper'))

my_addin = None


def reportErrorToUser(message):
    # noinspection PyArgumentList
    app = adsk.core.Application.get()
    ui = app.userInterface
    if ui:  # TODO: Automatic error reporting
        ui.messageBox(f'{message}\nPlease screenshot and send this error report to frc@autodesk.com.\n\nReport:\n{traceback.format_exc()}')


def areDepsInstalled():
    # noinspection PyBroadException
    try:
        import apper
        import pygltflib
        import numpy
        import pyquaternion
        import google.protobuf
        return True
    except:
        return False


def tryInstallDeps():
    # TODO: Figure out a better way to install python deps

    # USE THIS FOR TESTING NUMPY ISSUE
    # subprocess.run(f"\"{Path(os.__file__).parents[1] / 'python'}\" -m pip uninstall numpy -y", shell=True)

    if areDepsInstalled():
        return True

    # noinspection PyArgumentList
    app = adsk.core.Application.get()
    ui = app.userInterface

    if app.isOffLine:
        ui.messageBox("Unable to install dependencies for Synthesis glTF Exporter when Fusion is offline. Please connect to the internet and try again!")
        return False

    progressBar = ui.createProgressDialog()
    progressBar.isCancelButtonShown = False
    progressBar.reset()
    progressBar.show("Synthesis glTF Exporter", f"Installing dependencies...", 0, 4, 0)
    # noinspection PyUnresolvedReferences
    adsk.doEvents()

    system = platform.system()

    # TODO: This method of finding the python folder is highly susceptible to breaking from changes to the fusion360 python installer. Figure out a better method to get deps.
    if system == "Windows":
        pythonFolder = Path(os.__file__).parents[1]  # Assumes the location of the fusion python executable is two folders up from the os lib location
    elif system == "Darwin":  # macos
        pythonFolder = Path(os.__file__).parents[2] / "bin"
        progressBar.message = f"Installing pip..."
        # noinspection PyUnresolvedReferences
        adsk.doEvents()
        subprocess.run(f"curl https://bootstrap.pypa.io/get-pip.py -o \"{pythonFolder / 'get-pip.py'}\"", shell=True)
        subprocess.run(f"\"{pythonFolder / 'python'}\" \"{pythonFolder / 'get-pip.py'}\"", shell=True)
    else:
        raise ImportError(f"Unsupported platform! This add-in only supports windows and macos")

    pipDeps = ["pygltflib", "numpy=1.18.5", "protobuf", "pyquaternion"]
    for depName in pipDeps:
        progressBar.progressValue += 1
        progressBar.message = f"Installing {depName}..."
        # noinspection PyUnresolvedReferences
        adsk.doEvents()
        subprocess.run(f"\"{pythonFolder / 'python'}\" -m pip install {depName}", shell=True)

    if system == "Darwin":  # macos # TODO: High priority: This method of fixing the python deps on macos can potentially break other fusion add-ins and potentially parts of fusion itself. Find a better method to use deps on both windows and macos.
        pipAntiDeps = ["dataclasses", "typing"]
        for depName in pipAntiDeps:
            progressBar.message = f"Uninstalling {depName}..."
            # noinspection PyUnresolvedReferences
            adsk.doEvents()
            subprocess.run(f"\"{pythonFolder / 'python'}\" -m pip uninstall {depName} -y", shell=True)

    progressBar.hide()

    if areDepsInstalled():
        return True

    reportErrorToUser("Unable to install python dependencies for the glTF Exporter for Synthesis addin!")
    return False


def createExporterAddin():
    import apper
    # from .commands.ExportCommand import ExportCommand
    from .commands.ExportPaletteCommand import ExportPaletteShowCommand
    fusionGltfExporterApp = apper.FusionApp(APP_NAME, COMPANY_NAME, False)
    # fusionGltfExporterApp.add_command(
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
    fusionGltfExporterApp.add_command(
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

    return fusionGltfExporterApp


handlers = []


# noinspection PyUnresolvedReferences
class DocumentActivatedHandler(adsk.core.DocumentEventHandler):
    def __init__(self):
        super().__init__()

    # noinspection PyMethodMayBeStatic,PyUnusedLocal
    def notify(self, args):
        createAndStartAddin()


def createAndStartAddin():
    # noinspection PyBroadException
    try:
        global my_addin
        if my_addin is None and tryInstallDeps():
            my_addin = createExporterAddin()
            my_addin.run_app()
    except:
        reportErrorToUser("glTF Exporter for Synthesis has encountered an error!")


def run(context):  # Called by Fusion API
    if context.get("IsApplicationStartup", False):
        # noinspection PyArgumentList
        app = adsk.core.Application.get()
        onDocumentActivated = DocumentActivatedHandler()
        app.documentActivated.add(onDocumentActivated)
        handlers.append(onDocumentActivated)
    else:
        createAndStartAddin()


# noinspection PyUnusedLocal
def stop(context):  # Called by Fusion API
    global my_addin
    if my_addin is not None:
        # noinspection PyUnresolvedReferences
        my_addin.stop_app()
        my_addin = None
    sys.path.pop(0)
    sys.path.pop(0)
