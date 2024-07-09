import logging.handlers
import os
import sys
import traceback

import adsk.core

# Required for absolute imports
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from src import APP_NAME, DESCRIPTION, INTERNAL_ID, gm
from src.configure import unload_config
from src.logging import setupLogger
from src.UI import HUI, Camera, ConfigCommand, Helper, MarkingMenu
from src.UI.Toolbar import Toolbar

root_logger: logging.Logger  # TODO: Will need to be updated after GH-1010


def run(_):
    """## Entry point to application from Fusion.

    Arguments:
        **context** *context* -- Fusion context to derive app and UI.
    """

    try:
        global root_logger
        root_logger, _ = setupLogger()  # TODO: Will need to be updated after GH-1010

        # creates the UI elements
        register_ui()

        app = adsk.core.Application.get()
        ui = app.userInterface

        MarkingMenu.setupMarkingMenu(ui)

    except:
        logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))


def stop(_):
    """## Fusion exit point - deconstructs buttons and handlers

    Arguments:
        **context** *context* -- Fusion Data.
    """
    try:
        unregister_all()

        app = adsk.core.Application.get()
        ui = app.userInterface

        MarkingMenu.stopMarkingMenu(ui)

        # nm.deleteMe()

        # should make a logger class
        handlers = root_logger.handlers[:]
        for handler in handlers:
            handler.close()
            # I think this will clear the file completely
            # root_logger.removeHandler(handler)

        unload_config()

        gm.clear()
    except:
        logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))


def unregister_all() -> None:
    """Unregisters all UI elements in case any still exist.

    - Good place to unregister custom app events that may repeat.
    """
    try:
        Camera.clearIconCache()

        for element in gm.elements:
            element.deleteMe()

        for tab in gm.tabs:
            tab.deleteMe()

    except:
        logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))


def register_ui() -> None:
    """#### Generic Function to add all UI objects in a simple non destructive way."""

    # if A_EP:
    #     A_EP.send_view("open")

    # toolbar = Toolbar('SketchFab')
    work_panel = Toolbar.getNewPanel(f"{APP_NAME}", f"{INTERNAL_ID}", "ToolsTab")

    commandButton = HUI.HButton(
        "Synthesis Exporter",
        work_panel,
        Helper.check_solid_open,
        ConfigCommand.ConfigureCommandCreatedHandler,
        description=f"{DESCRIPTION}",
        command=True,
    )

    gm.elements.append(commandButton)
