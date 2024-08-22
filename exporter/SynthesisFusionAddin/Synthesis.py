import os
import sys
from typing import Any

import adsk.core

# Required for absolute imports.
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from src.Dependencies import resolveDependencies
from src.Logging import logFailure, setupLogger

logger = setupLogger()

try:
    # Attempt to import required pip dependencies to verify their installation.
    import requests

    from src.Proto import (
        assembly_pb2,
        joint_pb2,
        material_pb2,
        motor_pb2,
        signal_pb2,
        types_pb2,
    )
except (ImportError, ModuleNotFoundError, BaseException) as error:  # BaseException required to catch proto.VersionError
    logger.warn(f"Running resolve dependencies with error of:\n{error}")
    result = resolveDependencies()
    if result:
        adsk.core.Application.get().userInterface.messageBox("Installed required dependencies.\nPlease restart Fusion.")


from src import APP_NAME, DESCRIPTION, INTERNAL_ID, gm
from src.UI import (
    HUI,
    Camera,
    ConfigCommand,
    MarkingMenu,
    ShowAPSAuthCommand,
    ShowWebsiteCommand,
)
from src.UI.Toolbar import Toolbar


@logFailure
def run(_context: dict[str, Any]) -> None:
    """## Entry point to application from Fusion.

    Arguments:
        **context** *context* -- Fusion context to derive app and UI.
    """

    # Remove all items prior to start just to make sure
    unregister_all()

    # creates the UI elements
    register_ui()

    app = adsk.core.Application.get()
    ui = app.userInterface

    MarkingMenu.setupMarkingMenu(ui)


@logFailure
def stop(_context: dict[str, Any]) -> None:
    """## Fusion exit point - deconstructs buttons and handlers

    Arguments:
        **context** *context* -- Fusion Data.
    """
    unregister_all()

    app = adsk.core.Application.get()
    ui = app.userInterface

    MarkingMenu.stopMarkingMenu(ui)

    # nm.deleteMe()

    logger.cleanupHandlers()
    gm.clear()


@logFailure
def unregister_all() -> None:
    """Unregisters all UI elements in case any still exist.

    - Good place to unregister custom app events that may repeat.
    """
    Camera.clearIconCache()

    for element in gm.elements:
        element.deleteMe()

    for tab in gm.tabs:
        tab.deleteMe()


@logFailure
def register_ui() -> None:
    """#### Generic Function to add all UI objects in a simple non destructive way."""

    # toolbar = Toolbar('SketchFab')
    work_panel = Toolbar.getNewPanel(f"{APP_NAME}", f"{INTERNAL_ID}", "ToolsTab")

    commandButton = HUI.HButton(
        "Synthesis Exporter",
        work_panel,
        lambda *_: True,  # TODO: Should be redone with various refactors.
        ConfigCommand.ConfigureCommandCreatedHandler,
        description=f"{DESCRIPTION}",
        command=True,
    )

    gm.elements.append(commandButton)

    apsButton = HUI.HButton(
        "APS",
        work_panel,
        lambda *_: True,  # TODO: Should be redone with various refactors.
        ShowAPSAuthCommand.ShowAPSAuthCommandCreatedHandler,
        description=f"APS",
        command=True,
    )

    gm.elements.append(apsButton)

    websiteButton = HUI.HButton(
        "Synthesis Website",
        work_panel,
        lambda *_: True,
        ShowWebsiteCommand.ShowWebsiteCommandCreatedHandler,
        description=f"Website Test",
        command=True,
    )
    gm.elements.append(websiteButton)
