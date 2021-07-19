from ..general_imports import *
from . import Helper

# from ..proto_out import Configuration_pb2
from ..Types.OString import OString

from typing import Union
import adsk.core, adsk.fusion, traceback


def SaveFileDialog(
    defaultPath="", defaultName="", ext="MiraBuf Package (*.mira)"
) -> Union[str, bool]:
    """Function to generate the Save File Dialog for the Hellion Data files

    Args:
        defaultPath (str): default path for the saving location
        defaultName (str): default name for the saving file

    Returns:
        bool: False if canceled
        str: full file path
    """

    ext="MiraBuf Package (*.mira)"
    
    fileDialog = gm.ui.createFileDialog()
    fileDialog.isMultiSelectEnabled = False

    fileDialog.title = "Save Export Result"
    fileDialog.filter = f"{ext}"

    if defaultPath == "":
        defaultPath = generateFilePath()

    fileDialog.initialDirectory = defaultPath

    # print(defaultPath)

    if defaultName == "":
        defaultName = generateFileName()

    fileDialog.initialFilename = defaultName

    fileDialog.filterIndex = 0
    dialogResult = fileDialog.showSave()

    if dialogResult == adsk.core.DialogResults.DialogOK:
        return fileDialog.filename
    else:
        return False


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
