"""The Fusion Synthesis Robot Exporter.
"""

import adsk.core

from Synthesis import gm, APP_TITLE, INTERNAL_ID, DESCRIPTION, APP_NAME
from Synthesis.UI import Toolbar, HButton
from Synthesis.Logging import setupLogger, logFailure


# TODO: Type 'context' and replace with '_'
@logFailure
def run(context) -> None:
    setupLogger()


# TODO: Type 'context' and replace with '_'
@logFailure
def stop(context) -> None:
    pass


def register_ui() -> None:
    work_panel = Toolbar.getNewPanel(f"{APP_NAME}", f"{INTERNAL_ID}", "ToolsTab")

    commandButton = HButton(
        APP_TITLE,
        work_panel,
        lambda: True, # TODO: Figure out what this was used for
        ConfigCommand.ConfigureCommandCreatedHandler,
        description=f"{DESCRIPTION}",
        command=True,
    )

    gm.elements.append(commandButton)


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
