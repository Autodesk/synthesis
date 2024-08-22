""" Initializes the global variables that are set in the run method to reduce hanging commands. """

from typing import Any

import adsk.core
import adsk.fusion


class GlobalManager:
    def __init__(self) -> None:
        self.app = adsk.core.Application.get()

        if self.app:
            self.ui = self.app.userInterface

        self.connected = False
        """ Is unity currently connected """

        self.uniqueIds: list[str] = []  # type of HButton
        """ Collection of unique ID values to not overlap """

        self.elements: list[Any] = []
        """ Unique constructed buttons to delete """

        # Transition: AARD-1765
        # Will likely be removed later as this is no longer used. Avoiding adding typing for now.
        self.palettes = []  # type: ignore
        """ Unique constructed palettes to delete """

        self.handlers: list[adsk.core.EventHandler] = []
        """ Object to store all event handlers to custom events like saving. """

        self.tabs: list[adsk.core.ToolbarPanel] = []
        """ Set of Tab objects to keep track of. """

        # Transition: AARD-1765
        # Will likely be removed later as this is no longer used. Avoiding adding typing for now.
        self.queue = []  # type: ignore
        """ This will eventually implement the Python SimpleQueue synchronized workflow
            - this is the list of objects being sent
        """

        # Transition: AARD-1765
        # Will likely be removed later as this is no longer used. Avoiding adding typing for now.
        self.files = []  # type: ignore

    def __str__(self) -> str:
        return "GlobalManager"

    def clear(self) -> None:
        for attr, value in self.__dict__.items():
            if isinstance(value, list):
                setattr(self, attr, [])
