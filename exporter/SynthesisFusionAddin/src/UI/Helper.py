from ..general_imports import *
from inspect import getmembers, isfunction
from typing import Any, Union

from . import Events, HUI


def check_solid_open() -> bool:
    """### Checks to see if the current design open is Fusion Solid
    - Supplied as callback
    """
    return True

    app = adsk.core.Application.get()

    try:
        ui = app.userInterface
        doc = app.activeDocument

        if (doc is None) or (doc.dataFile is None):
            ui.messageBox("Please open a valid fusion 360 solid body document.")
        elif doc and (doc.isSaved == False) and (doc.dataFile.fileExtension == "f3d"):
            ui.messageBox("The current document must be saved first.")
        elif doc and (doc.dataFile.fileExtension != "f3d"):
            ui.messageBox("You must have a valid modeling document open.")
        elif doc and (doc.isSaved == True) and (doc.dataFile.fileExtension == "f3d"):
            return True
        else:
            ui.messageBox("Please open a valid fusion 360 solid body document.")

        return False

    except RuntimeError:
        return False


def previouslyConfigured() -> Union[str, None]:
    """Checks the Hellion File attribute stored in the document in order to get the saved configuration file

    - This is used in the (@ref ConfigCommand.py) in order to pre-populate the fields
    - Command check is  ` if (ret is None): return `

    Args:
        None ([type]): No Arguments

    Raises:
        RuntimeWarning: If failed attempt to handle in the parent since this is a helper (propogating up as a warning)

    Returns:
        (str | None): Will return serialized data if previously exported or None if never exported before.
    """

    app = adsk.core.Application.get()
    try:
        configured = app.activeDocument.attributes.itemByName(
            f"{INTERNAL_ID}", "Configuration"
        )
        if configured is not None:
            return configured.value
        return False
    except:
        # handle in above function - usually indicates some kind of error to do with improper file access
        raise RuntimeWarning(
            f"Could not access attributes of the file {app.activeDocument.name} in previouslyConfigured"
        )
        return False


def writeConfigure(serialized: str) -> bool:
    app = adsk.core.Application.get()
    #try:
        #app.activeDocument.attributes.add(
        #    f"{INTERNAL_ID}", "Configuration", f"{serialized}"
        #)
    return True
    #except:
    #    return False


def getDocName() -> str or None:
    """### Gets the active Document Name
    - If it can't find one then it will return None
    """
    app = adsk.core.Application.get()
    if check_solid_open():
        return app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0]
    else:
        return None


def checkAttribute() -> bool:
    """ ### Will process the file and look for a flag that unity is already using it. """
    app = adsk.core.Application.get()
    try:
        connected = app.activeDocument.attributes.itemByName("UnityFile", "Connected")
        if connected is not None:
            return connected.value
        return False
    except:
        app.userInterface.messageBox(
            f"Could not access the attributes of the file \n -- {traceback.format_exc()}."
        )
        return False


def addUnityAttribute() -> bool or None:
    """#### Adds an attribute to the Fusion File
    - Initially intended to be used to add a marker for in use untiy files
    - No longer necessary
    """
    app = adsk.core.Application.get()
    try:
        current = app.activeDocument.attributes.itemByName("UnityFile", "Connected")

        if check_solid_open and (current is None):
            val = app.activeDocument.attributes.add("UnityFile", "Connected", "True")
            return val
        elif current is not None:
            return current
        return None

    except:
        app.userInterface.messageBox(
            f"Could not access the attributes of the file \n -- {traceback.format_exc()}."
        )
        return False


def openPanel() -> None:
    """### Opens and creates the Panel
    notes:
        - this is here because of a bug on startup loading
        - isn't created if this isn't called
    """
    name = f"{APP_NAME}_panel"
    uid = name.replace(" ", "") + f"_p1_{INTERNAL_ID}"
    palette = gm.ui.palettes.itemById(uid)

    if palette:
        palette.isVisible = not palette.isVisible
        if palette.isVisible:
            # Hides the data panel
            gm.app.data.isDataPanelVisible = False
    else:
        func_list = [o for o in getmembers(Events, isfunction)]
        palette_new = HUI.HPalette(
            name, APP_TITLE, True, True, False, 400, 500, func_list
        )
        gm.elements.append(palette_new)

    return
