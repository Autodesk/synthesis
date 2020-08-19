
import os
import platform
import subprocess
import sys
from pathlib import Path

import adsk
import adsk.core
import adsk.fusion

from .config import *
from .gltf.utils.FusionUtils import reportErrorToUser

app_path = os.path.dirname(__file__)

sys.path.insert(0, app_path)
sys.path.insert(0, os.path.join(app_path, 'apper'))

my_addin = None

def areDepsInstalled():
    # noinspection PyBroadException
    try:
        import apper
        import pygltflib
        import numpy
        import google.protobuf
        return True
    except:
        return False

def installDeps():
    # TODO: Figure out a better way to install python deps

    if areDepsInstalled():
        return True

    progressBar = adsk.core.Application.get().userInterface.createProgressDialog()
    progressBar.isCancelButtonShown = False
    progressBar.reset()
    progressBar.show("Synthesis glTF Exporter", f"Installing dependencies...", 0, 4, 0)
    adsk.doEvents()

    system = platform.system()

    # TODO: This method of finding the python folder is highly susceptible to breaking from changes to the fusion360 python installer. Figure out a better method to get deps.
    if system == "Windows":
        pythonFolder = Path(os.__file__).parents[1]  # Assumes the location of the fusion python executable is two folders up from the os lib location
    elif system == "Darwin":  # macos
        pythonFolder = Path(os.__file__).parents[2] / "bin"
        progressBar.message = f"Installing pip..."
        adsk.doEvents()
        subprocess.run(f"curl https://bootstrap.pypa.io/get-pip.py -o \"{pythonFolder / 'get-pip.py'}\"", shell=True)
        subprocess.run(f"\"{pythonFolder / 'python'}\" \"{pythonFolder / 'get-pip.py'}\"", shell=True)
    else:
        raise ImportError(f"Unsupported platform! This add-in only supports windows and macos")

    pipDeps = ["pygltflib", "numpy", "protobuf"]
    for depName in pipDeps:
        progressBar.progressValue += 1
        progressBar.message = f"Installing {depName}..."
        adsk.doEvents()
        subprocess.run(f"\"{pythonFolder / 'python'}\" -m pip install {depName}", shell=True)

    if system == "Darwin":  # macos # TODO: High priority: This method of fixing the python deps on macos can potentially break other fusion add-ins and potentially parts of fusion itself. Find a better method to use deps on both windows and macos.
        pipAntiDeps = ["dataclasses", "typing"]
        for depName in pipAntiDeps:
            progressBar.message = f"Uninstalling {depName}..."
            adsk.doEvents()
            subprocess.run(f"\"{pythonFolder / 'python'}\" -m pip uninstall {depName} -y", shell=True)

    progressBar.hide()

    if areDepsInstalled():
        return True

    reportErrorToUser("Unable to install python dependencies for the glTF Exporter for Synthesis addin!")
    return False


def initializeAddin():
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


# noinspection PyBroadException
try:
    if installDeps():
        my_addin = initializeAddin()
except:
    reportErrorToUser("Unable to start glTF Exporter for Synthesis!")

def run(_):
    if my_addin is None:
        return
    # noinspection PyBroadException
    try:
        my_addin.run_app()
    except:
        reportErrorToUser("glTF Exporter for Synthesis has encountered an error!")

def stop(_):
    # noinspection PyBroadException
    try:
        my_addin.stop_app()
        sys.path.pop(0)
        sys.path.pop(0)
    except:
        pass
