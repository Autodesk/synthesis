from shutil import rmtree

import adsk.core

from .src.APS import APS
from .src.configure import setAnalytics, unload_config
from .src.general_imports import APP_NAME, DESCRIPTION, INTERNAL_ID, gm, root_logger
from .src.Types.OString import OString
from .src.UI import (
    HUI,
    Camera,
    ConfigCommand,
    Handlers,
    Helper,
    MarkingMenu,
    ShowAPSAuthCommand,
)
from .src.UI.Toolbar import Toolbar


def run(_):
    """## Entry point to application from Fusion.

    Arguments:
        **context** *context* -- Fusion context to derive app and UI.
    """

    try:
        # Remove all items prior to start just to make sure
        unregister_all()

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

        for file in gm.files:
            try:
                os.remove(file)
            except OSError:
                pass

        # removes path so that proto files don't get confused

        import sys

        path = os.path.abspath(os.path.dirname(__file__))

        path_proto_files = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "proto", "proto_out"))

        if path in sys.path:
            sys.path.remove(path)

        if path_proto_files in sys.path:
            sys.path.remove(path_proto_files)

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

    apsButton = HUI.HButton(
        "APS",
        work_panel,
        Helper.check_solid_open,
        ShowAPSAuthCommand.ShowAPSAuthCommandCreatedHandler,
        description=f"APS TEST",
        command=True,
    )

    gm.elements.append(apsButton)
