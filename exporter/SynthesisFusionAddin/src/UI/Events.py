from ..general_imports import *
from . import Camera, Helper

from typing import Sequence, Optional, Tuple
import logging.handlers, traceback

""" # This file is Special
    It links all function names to command requests that palletes can make automatically
    If you create a function you can automatically call it from a javascript request or the palette js code.
"""


def updateDocument(*argv: Sequence[str]):
    pass


def updateConnection(_) -> str:
    """Updates the JS side connection with the Network Manager connected()

    Args:
        _ (Any): Any

    Returns:
        str: Json formatted connected: true | false
    """
    return json.dumps("")


def focusJoint(json_data: str) -> str:
    """Called from JS to focus on the joint clicked

    Args:
        json_data (str): Joint identifiable data

    Returns:
        str: Empty Response
    """
    data = json.loads(json_data)
    data = data["arguments"]
    gm.ui.messageBox(f"Attempting to focus on a joint: {data}\n TODO: Implement")
    return ""


def openDocument(json_data: str) -> str:
    """Opens a specific document in from the current active doc list

    Args:
        json_data (str): Identifiable Document Design data

    Returns:
        str: Empty Response
    """
    data = json.loads(json_data)
    data = data["arguments"]
    gm.ui.messageBox(
        f"Attempting to open and focus on a given document: {data}\n TODO: Implement"
    )
    logging.getLogger(f"{INTERNAL_ID}.Events.openDocument").info(
        f"Attempting to open and focus on a given document: {data}\n TODO: Implement"
    )
    return ""


def example(palette):
    app = adsk.core.Application.get()
    app.userInterface(f"{Helper.getDocName()}")
