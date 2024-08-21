import os
import tempfile
from pathlib import Path

import adsk.core
import adsk.fusion

from src import gm
from src.Types import OString


def saveFileDialog(defaultPath: str | None = None, defaultName: str | None = None) -> str | os.PathLike[str] | None:
    """Function to generate the Save File Dialog for the Hellion Data files

    Args:
        defaultPath (str): default path for the saving location
        defaultName (str): default name for the saving file

    Returns:
        None: if canceled
        str: full file path
    """

    fileDialog: adsk.core.FileDialog = gm.ui.createFileDialog()
    fileDialog.isMultiSelectEnabled = False

    fileDialog.title = "Save Export Result"
    fileDialog.filter = "MiraBuf Package (*.mira)"

    if not defaultPath:
        defaultPath = generateFilePath()

    fileDialog.initialDirectory = defaultPath

    if not defaultName:
        defaultName = generateFileName()

    fileDialog.initialFilename = defaultName

    fileDialog.filterIndex = 0
    dialogResult = fileDialog.showSave()

    if dialogResult != adsk.core.DialogResults.DialogOK:
        return None

    canWrite = isWriteableDirectory(Path(fileDialog.filename).parent)
    if not canWrite:
        gm.ui.messageBox("Synthesis does not have the required permissions to write to this directory.")
        return saveFileDialog(defaultPath, defaultName)

    return fileDialog.filename


def isWriteableDirectory(path: str | os.PathLike[str]) -> bool:
    if not os.access(path, os.W_OK):
        return False

    try:
        with tempfile.NamedTemporaryFile(dir=path, delete=True) as f:
            f.write(b"test")
    except OSError:
        return False

    return True


def generateFilePath() -> str:
    """Generates a temporary file path that can be used to save the file for exporting

    Example:
     - "%appdata%/Roaming/Temp/HellionFiles"

    Returns:
        str: file path
    """
    tempPath = OString.TempPath("").getPath()
    return str(tempPath)


def generateFileName() -> str:
    """Generates a file name that can be used to uniquely save the file for exporting

    Example:
    - "Test_v1.unitypackage"

    Returns:
        str: fileName and extension
    """
    design = gm.app.activeDocument.design
    name = design.rootComponent.name.rsplit(" ", 1)[0]
    version = design.rootComponent.name.rsplit(" ", 1)[1]

    # in case there are any spaces in the name replace them with friendlier characters
    name.replace(" ", "_")

    return "{0}_{1}.mira".format(name, version)


def OpenFileDialog():
    pass
