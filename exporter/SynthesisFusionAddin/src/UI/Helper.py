import traceback
from inspect import getmembers, isfunction

import adsk.core

from src import APP_NAME, APP_TITLE, INTERNAL_ID, gm
from src.UI import HUI, Events


def check_solid_open() -> bool:
    """### Checks to see if the current design open is Fusion Solid
    - Supplied as callback
    WARN - THIS NO LONGER FUNCTIONS
    """
    return True


def getDocName() -> str | None:
    """### Gets the active Document Name
    - If it can't find one then it will return None
    """
    app = adsk.core.Application.get()
    if check_solid_open():
        return app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0]
    else:
        return None


def checkAttribute() -> bool:
    """### Will process the file and look for a flag that unity is already using it."""
    app = adsk.core.Application.get()
    try:
        connected = app.activeDocument.attributes.itemByName("UnityFile", "Connected")
        if connected is not None:
            return connected.value
        return False
    except:
        app.userInterface.messageBox(f"Could not access the attributes of the file \n -- {traceback.format_exc()}.")
        return False


def addUnityAttribute() -> bool | None:
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
        app.userInterface.messageBox(f"Could not access the attributes of the file \n -- {traceback.format_exc()}.")
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
        palette_new = HUI.HPalette(name, APP_TITLE, True, True, False, 400, 500, func_list)
        gm.elements.append(palette_new)

    return
