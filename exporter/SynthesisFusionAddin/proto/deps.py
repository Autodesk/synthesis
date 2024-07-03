import logging
import os
import platform
from pathlib import Path

import adsk.core
import adsk.fusion

from src import INTERNAL_ID

system = platform.system()


def getPythonFolder() -> str:
    """Retreives the folder that contains the Autodesk python executable

    Raises:
        ImportError: Unrecognized Platform

    Returns:
        str: The path that contains the Autodesk python executable
    """

    # Thank you Kris Kaplan
    import importlib.machinery
    import sys

    osPath = importlib.machinery.PathFinder.find_spec("os", sys.path).origin

    # The location of the python executable is found relative to the location of the os module in each operating system
    if system == "Windows":
        pythonFolder = Path(osPath).parents[1]
    elif system == "Darwin":
        pythonFolder = f"{Path(osPath).parents[2]}/bin"
    else:
        raise ImportError("Unsupported platform! This add-in only supports windows and macos")

    logging.getLogger(f"{INTERNAL_ID}").debug(f"Python Folder -> {pythonFolder}")
    return pythonFolder


def executeCommand(command: tuple) -> int:
    """Abstracts the execution of commands to account for platform differences

    Args:
        command (tuple): Tuple starting with command, and each indice having the arguments for said command

    Returns:
        int: Exit code of the process
    """

    joinedCommand = str.join(" ", command)
    logging.getLogger(f"{INTERNAL_ID}").debug(f"Command -> {joinedCommand}")
    executionResult = os.system(joinedCommand)

    return executionResult


def installCross(pipDeps: list) -> bool:
    """Attempts to fetch pip script and resolve dependencies with less user interaction

    Args:
        pipDeps (list): List of all string imports

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
    progressBar.show("Synthesis", f"Installing dependencies...", 0, len(pipDeps), 0)

    # this is important to reduce the chance of hang on startup
    adsk.doEvents()

    try:
        pythonFolder = getPythonFolder()
    except ImportError as e:
        logging.getLogger(f"{INTERNAL_ID}").error(f"Failed to download dependencies: {e.msg}")
        return False

    if system == "Darwin":  # macos
        # if nothing has previously fetched it
        if not os.path.exists(f"{pythonFolder}/get-pip.py"):
            executeCommand(
                [
                    "curl",
                    "https://bootstrap.pypa.io/get-pip.py",
                    "-o",
                    f'"{pythonFolder}/get-pip.py"',
                ]
            )

        executeCommand([f'"{pythonFolder}/python"', f'"{pythonFolder}/get-pip.py"'])

    pythonExecutable = "python"
    if system == "Windows":
        pythonExecutable = "python.exe"

    for depName in pipDeps:
        progressBar.progressValue += 1
        progressBar.message = f"Installing {depName}..."
        adsk.doEvents()

        # os.path.join needed for varying system path separators
        installResult = executeCommand(
            [
                f'"{os.path.join(pythonFolder, pythonExecutable)}"',
                "-m",
                "pip",
                "install",
                depName,
            ]
        )
        if installResult != 0:
            logging.getLogger(f"{INTERNAL_ID}").warn(f'Dep installation "{depName}" exited with code "{installResult}"')

    if system == "Darwin":
        pipAntiDeps = ["dataclasses", "typing"]
        progressBar.progressValue = 0
        progressBar.maximumValue = len(pipAntiDeps)
        for depName in pipAntiDeps:
            progressBar.message = f"Uninstalling {depName}..."
            progressBar.progressValue += 1
            adsk.doEvents()
            uninstallResult = executeCommand(
                [
                    f'"{os.path.join(pythonFolder, pythonExecutable)}"',
                    "-m",
                    "pip",
                    "uninstall",
                    f"{depName}",
                    "-y",
                ]
            )
            if uninstallResult != 0:
                logging.getLogger(f"{INTERNAL_ID}").warn(
                    f'AntiDep uninstallation "{depName}" exited with code "{uninstallResult}"'
                )

    progressBar.hide()

    if _checkDeps():
        return True
    else:
        ui.messageBox("Failed to install dependencies needed")


def _checkDeps() -> bool:
    try:
        from .proto_out import assembly_pb2, joint_pb2, material_pb2, types_pb2

        return True
    except ImportError:
        return False


try:
    import logging.handlers

    import google.protobuf
    import pkg_resources

    from .proto_out import assembly_pb2, joint_pb2, material_pb2, types_pb2
except ImportError or ModuleNotFoundError:
    installCross(["protobuf==4.23.3"])
    from .proto_out import assembly_pb2, joint_pb2, material_pb2, types_pb2
