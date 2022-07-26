import platform, subprocess, shlex
from pathlib import Path, PurePath
from os import system, chdir, __file__, getcwd, path

import adsk.core, adsk.fusion, traceback

def installCross(pipDeps: list) -> bool:
    """Attempts to fetch pip script and resolve dependencies with less user interaction

    Args:
        pipDeps (list): List of all string imports

    Raises:
        ImportError: Unrecognized Platform

    Returns:
        bool: Success

    Notes:
        Liam originally came up with this style after realizing accessing the python dir was too unreliable.
    """
    app = adsk.core.Application.get()
    ui = app.userInterface

    if app.isOffLine:
        ui.messageBox(
            "Unable to install dependencies when Fusion is offline. Please connect to the internet and try again!"
        )
        return False

    progressBar = ui.createProgressDialog()
    progressBar.isCancelButtonShown = False
    progressBar.reset()
    progressBar.show("Unity Addin", f"Installing dependencies...", 0, 4, 0)

    # this is important to reduce the chance of hang on startup
    adsk.doEvents()

    system = platform.system()

    if system == "Windows":
        pythonFolder = Path(__file__).parents[
            1
        ]  # Assumes the location of the fusion python executable is two folders up from the os lib location
    elif system == "Darwin":  # macos
        pythonFolder = Path(__file__).parents[2] / "bin"
        progressBar.message = f"Fetching pip..."
        adsk.doEvents()
        subprocess.call(
            f"curl https://bootstrap.pypa.io/get-pip.py -o \"{pythonFolder / 'get-pip.py'}\"",
            creationflags=0x08000000,
            shell=False,
        )
        subprocess.call(
            f"\"{pythonFolder / 'python'}\" \"{pythonFolder / 'get-pip.py'}\"",
            creationflags=0x08000000,
            shell=False,
        )
    else:
        raise ImportError(
            f"Unsupported platform! This add-in only supports windows and macos"
        )

    for depName in pipDeps:
        progressBar.progressValue += 1
        progressBar.message = f"Installing {depName}..."
        adsk.doEvents()
        subprocess.call(
            f"\"{pythonFolder / 'python'}\" -m pip install {depName}",
            creationflags=0x08000000,
            shell=False
        )

    if system == "Darwin":
        pipAntiDeps = ["dataclasses", "typing"]
        for depName in pipAntiDeps:
            progressBar.message = f"Uninstalling {depName}..."
            adsk.doEvents()
            subprocess.call(
                f"\"{pythonFolder / 'python'}\" -m pip uninstall {depName} -y",
                creationflags=0x08000000,
                shell=False,
            )

    progressBar.hide()

    if _checkDeps():
        return True
    else:
        ui.messageBox("Failed to install dependencies needed")


def _checkDeps() -> bool:
    try:
        from .proto_out import joint_pb2, assembly_pb2, types_pb2, material_pb2

        return True
    except ImportError:
        return False


try:
    import logging.handlers
    import google.protobuf
    import pkg_resources

    #logging.getLogger("deps").error()

    from .proto_out import joint_pb2, assembly_pb2, types_pb2, material_pb2
except ImportError or ModuleNotFoundError:
    # Version 1 with built in Pip - cannot check if it works on OSX right now.
    # Works with fusion debug builds
    installCross(["protobuf==3.19.4"])
    from .proto_out import joint_pb2, assembly_pb2, types_pb2, material_pb2

    # Version 2 with no shell and fetched pip
    # NOTE: This does NOT work on dev streamer since it looks for installed fusion
