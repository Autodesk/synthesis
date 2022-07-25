from src.general_imports import root_logger, gm, INTERNAL_ID, APP_NAME, DESCRIPTION

from src.ui import hui, handlers, camera, helper, config_command
from src.ui.toolbar import Toolbar
from src.types.ostring import OString
from src.configure import setAnalytics, unload_config

from shutil import rmtree
import logging.handlers, traceback, importlib.util, os

from src.ui import marking_menu
import adsk.core


def run(_):
    """## Entry point to application from Fusion 360.

    Arguments:
        **context** *context* -- Fusion 360 context to derive app and ui.
    """

    try:
        # Remove all items prior to start just to make sure
        unregister_all()

        # creates the ui elements
        register_ui()

        app = adsk.core.Application.get()
        ui  = app.userInterface

        marking_menu.setupMarkingMenu(ui)

    except:
        logging.getLogger(f"{INTERNAL_ID}").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def stop(_):
    """## Fusion 360 exit point - deconstructs buttons and handlers

    Arguments:
        **context** *context* -- Fusion 360 Data.
    """
    try:
        unregister_all()

        app = adsk.core.Application.get()
        ui  = app.userInterface

        marking_menu.stopMarkingMenu(ui)

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

        path_proto_files = os.path.abspath(
            os.path.join(os.path.dirname(__file__), "..", "proto", "proto_out")
        )

        if path in sys.path:
            sys.path.remove(path)

        if path_proto_files in sys.path:
            sys.path.remove(path_proto_files)

    except:
        logging.getLogger(f"{INTERNAL_ID}").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def unregister_all() -> None:
    """Unregisters all ui elements in case any still exist.

    - Good place to unregister custom app events that may repeat.
    """
    try:
        camera.clearIconCache()

        for element in gm.elements:
            element.deleteMe()

        for tab in gm.tabs:
            tab.deleteMe()

    except:
        logging.getLogger(f"{INTERNAL_ID}").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def register_ui() -> None:
    """ #### Generic Function to add all ui objects in a simple non destructive way."""

    # if A_EP:
    #     A_EP.send_view("open")

    # toolbar = Toolbar('SketchFab')
    work_panel = Toolbar.getNewPanel(f"{APP_NAME}", f"{INTERNAL_ID}", "ToolsTab")

    command_button = hui.HButton(
        "Synthesis Exporter",
        work_panel,
        helper.check_solid_open,
        config_command.ConfigureCommandCreatedHandler,
        description=f"{DESCRIPTION}",
        command=True,
    )

    gm.elements.append(command_button)
