import importlib.machinery
import os
import subprocess
import sys
from pathlib import Path

import adsk.core
import adsk.fusion

from src import SYSTEM
from src.Logging import getLogger, logFailure

logger = getLogger()

# Since the Fusion python runtime is separate from the system python runtime we need to do some funky things
# in order to download and install python packages separate from the standard library.
PIP_DEPENDENCY_VERSION_MAP: dict[str, str] = {
    "protobuf": "5.27.2",
    "requests": "2.32.3",
}


@logFailure
def getInternalFusionPythonInstillationFolder() -> str:
    # Thank you Kris Kaplan
    # Find the folder location where the Autodesk python instillation keeps the 'os' standard library module.
    pythonStandardLibraryModulePath = importlib.machinery.PathFinder.find_spec("os", sys.path).origin

    # Depending on platform, adjust to folder to where the python executable binaries are stored.
    if SYSTEM == "Windows":
        folder = f"{Path(pythonStandardLibraryModulePath).parents[1]}"
    else:
        assert SYSTEM == "Darwin"
        folder = f"{Path(pythonStandardLibraryModulePath).parents[2]}/bin"

    return folder


def executeCommand(*args: str) -> subprocess.CompletedProcess:
    logger.debug(f"Running Command -> {' '.join(args)}")
    try:
        result: subprocess.CompletedProcess = subprocess.run(
            args, shell=False, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, check=True
        )
        logger.debug(f"Command Output:\n{result.stdout}")
        return result

    except subprocess.CalledProcessError as error:
        logger.error(f"Exit code: {error.returncode}")
        logger.error(f"Output:\n{error.stderr}")
        raise error


def getInstalledPipPackages(pythonExecutablePath: str) -> dict[str, str]:
    result: str = executeCommand(pythonExecutablePath, "-m", "pip", "freeze").stdout
    # We don't need to check against packages with a specific hash as those are not required by Synthesis.
    return {x.split("==")[0]: x.split("==")[1] for x in result.splitlines() if "==" in x}


def packagesOutOfDate(installedPackages: dict[str, str]) -> bool:
    for package, installedVersion in installedPackages.items():
        expectedVersion = PIP_DEPENDENCY_VERSION_MAP.get(package)
        if expectedVersion and expectedVersion != installedVersion:
            return True

    return False


@logFailure
def resolveDependencies() -> bool | None:
    app = adsk.core.Application.get()
    ui = app.userInterface
    if app.isOffLine:
        # If we have gotten this far that means that an import error was thrown for possible missing
        # dependencies... And we can't try to download them because we have no internet... ¯\_(ツ)_/¯
        ui.messageBox("Unable to resolve dependencies while not connected to the internet.")
        return False

    # This is important to reduce the chance of hang on startup.
    adsk.doEvents()

    pythonFolder = getInternalFusionPythonInstillationFolder()
    pythonExecutableFile = "python.exe" if SYSTEM == "Windows" else "python"  # Confirming 110% everything is fine.
    pythonExecutablePath = os.path.join(pythonFolder, pythonExecutableFile)

    progressBar = ui.createProgressDialog()
    progressBar.isCancelButtonShown = False
    progressBar.reset()
    progressBar.show("Synthesis", f"Installing dependencies...", 0, len(PIP_DEPENDENCY_VERSION_MAP) * 2 + 2, 0)

    # Install pip manually on macos as it is not included by default? Really?
    if SYSTEM == "Darwin" and not os.path.exists(os.path.join(pythonFolder, "pip")):
        pipInstallScriptPath = os.path.join(pythonFolder, "get-pip.py")
        if not os.path.exists(pipInstallScriptPath):
            executeCommand("curl", "https://bootstrap.pypa.io/get-pip.py", "-o", pipInstallScriptPath)
            progressBar.message = "Downloading PIP Installer..."

        progressBar.progressValue += 1
        progressBar.message = "Installing PIP..."
        executeCommand(pythonExecutablePath, pipInstallScriptPath)
        progressBar.progressValue += 1

    installedPackages = getInstalledPipPackages(pythonExecutablePath)
    if packagesOutOfDate(installedPackages):
        # Uninstall and reinstall everything to confirm updated versions.
        progressBar.message = "Uninstalling out-of-date Dependencies..."

        for dep in PIP_DEPENDENCY_VERSION_MAP.keys():
            progressBar.progressValue += 1
            executeCommand(pythonExecutablePath, "-m", "pip", "uninstall", "-y", dep)
    else:
        progressBar.progressValue += len(PIP_DEPENDENCY_VERSION_MAP)

    progressBar.message = "Installing Dependencies..."
    for dep, version in PIP_DEPENDENCY_VERSION_MAP.items():
        progressBar.progressValue += 1
        progressBar.message = f"Installing {dep}..."
        adsk.doEvents()

        result = executeCommand(pythonExecutablePath, "-m", "pip", "install", f"{dep}=={version}").returncode
        if result:
            logger.warn(f'Dep installation "{dep}" exited with code "{result}"')

    progressBar.hide()
    return True
