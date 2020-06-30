"""
FusionApp.py
=========================================================
Python module for creating a Fusion 360 Addin

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
:copyright: (c) 2019 by Patrick Rainsberry.
:license: Apache 2.0, see LICENSE for more details.

"""
import traceback

import adsk.core
import json

import os
from os.path import expanduser
from pathlib import Path

from typing import Optional, List, Union, Any, Iterable


class FusionApp:
    """Base class for creating a Fusion 360 Add-in

    Args:
        name: The name of the addin
        company: the name of your company or organization
        debug: set this flag as True to enable more interactive feedback when developing.
    """

    def __init__(self, name: str, company: str, debug: bool):
        self.name = name
        self.company = company
        self.debug = debug
        self.commands = []
        self.events = []
        self.tabs = []
        self.default_dir = self._get_default_dir()
        self.preferences = self.get_all_preferences()
        self.root_path = Path(__file__).parent.parent.parent
        self.command_dict = {}

    def add_command(
            self,
            name: str,
            command_class: Any,
            options: dict
    ):
        """Adds a command to the application

        Args:
            name: The name of the command
            command_class: This should be your subclass of apper.Fusion360CommandBase or apper.PaletteCommandBase
            options: Set of options for the command see the full set of `options <usage/options>`_
        """
        app = adsk.core.Application.cast(adsk.core.Application.get())
        ui = app.userInterface

        try:
            base_cmd_id = options.get('cmd_id', 'default_id')
            new_id = self.company + "_" + self.name + "_" + base_cmd_id
            options['cmd_id'] = new_id

            options['app_name'] = self.name
            options['fusion_app'] = self

            tab_id = options.get('toolbar_tab_id')
            tab_name = options.get('toolbar_tab_name')

            if tab_name is None:
                options['toolbar_tab_name'] = self.name

            if tab_id is None:
                options['toolbar_tab_id'] = self.name

            _workspace = options.get('workspace', 'FusionSolidEnvironment')

            if isinstance(_workspace, str):
                _this_tab_id = options['toolbar_tab_id'] + '_' + _workspace
                options['toolbar_tab_id'] = _this_tab_id

                command = command_class(name, options)

                self.commands.append(command)
                self.command_dict[base_cmd_id] = new_id

            elif isinstance(_workspace, Iterable):
                if all(isinstance(item, str) for item in _workspace):
                    for workspace in _workspace:
                        options['workspace'] = workspace

                        _this_id = new_id + '_' + workspace
                        options['cmd_ctrl_id'] = _this_id

                        _this_tab_id = options['toolbar_tab_id'] + '_' + workspace
                        options['toolbar_tab_id'] = _this_tab_id

                        command = command_class(name, options)
                        self.commands.append(command)
                        self.command_dict[base_cmd_id] = new_id
            else:
                raise TypeError  # TODO or something along that line

        except:
            if ui:
                ui.messageBox('Apper Add Command failed: {}'.format(traceback.format_exc()))

    def command_id_from_name(self, name: str) -> Optional[str]:
        """Returns the full cmd_id defined by apper

        Args:
            name: this is the value set in options for cmd_id

        Returns:
            The full cmd_id (i.e. CompanyName_AppName_cmd_id)
        """
        cmd_id = self.command_dict.get(name)
        return cmd_id

    def add_document_event(self, event_id: str, event_type: adsk.core.DocumentEvent, event_class: Any):
        """Register a document event that can respond to various document actions

        Args:
            event_id: A unique identifier for the event
            event_type: Any document event in the current application
            event_class: Your subclass of apper.Fusion360DocumentEvent
        """
        doc_event = event_class(event_id, event_type)
        doc_event.fusion_app = self
        self.events.append(doc_event)

    def add_custom_event(self, event_id: str, event_class: Any):
        """Register a custom event to respond to a function running in a new thread

        Args:
            event_id: A unique identifier for the event
            event_class: Your subclass of apper.Fusion360CustomThread
        """

        custom_event = event_class(event_id)
        custom_event.fusion_app = self
        self.events.append(custom_event)

    def add_custom_event_no_thread(self, event_id: str, event_class: Any):
        """Register a custom event

        Args:
            event_id: A unique identifier for the event
            event_class: Your subclass of apper.Fusion360CustomThread
        """

        custom_event = event_class(event_id)
        custom_event.fusion_app = self
        self.events.append(custom_event)

    def add_workspace_event(self, event_id: str, workspace_name: str, event_class: Any):
        """Register a workspace event that can respond to various workspace actions

        Args:
            event_id: A unique identifier for the event
            workspace_name: name of the workspace (i.e.
            event_class: Your subclass of apper.Fusion360WorkspaceEvent
        """
        workspace_event = event_class(event_id, workspace_name)
        workspace_event.fusion_app = self
        self.events.append(workspace_event)

    def add_command_event(self, event_id: str, event_type: Any, event_class: Any):
        """Register a workspace event that can respond to various workspace actions

        Args:
            event_id: A unique identifier for the event
            event_type: One of [UserInterface.commandCreated, UserInterface.commandStarting, UserInterface.commandTerminated]
            event_class: Your subclass of apper.Fusion360CommandEvent class
        """
        command_event = event_class(event_id, event_type)
        command_event.fusion_app = self
        self.events.append(command_event)

    def add_web_request_event(self, event_id: str, event_type: adsk.core.WebRequestEvent, event_class: Any):
        """Register a workspace event that can respond to various workspace actions

        Args:
            event_id: A unique identifier for the event
            event_class: Your subclass of apper.Fusion360WebRequestEvent
            event_type: Opened or Inserting from URL event type such as (app.openedFromURL)
        """
        web_request_event = event_class(event_id, event_type)
        web_request_event.fusion_app = self
        self.events.append(web_request_event)

    def check_for_updates(self):
        """Not Implemented"""
        pass

    def run_app(self):
        """Runs the Addin"""

        app = adsk.core.Application.cast(adsk.core.Application.get())
        ui = app.userInterface
        try:
            for run_command in self.commands:
                run_command.on_run()
        except:
            if ui:
                ui.messageBox('Running App failed: {}'.format(traceback.format_exc()))

    def stop_app(self):
        """Stops the Addin and cleans up all of the created UI elements"""

        app = adsk.core.Application.cast(adsk.core.Application.get())
        ui = app.userInterface

        try:
            for stop_command in self.commands:
                stop_command.on_stop()

            for toolbar_tab in self.tabs:
                if toolbar_tab.isValid:
                    toolbar_tab.deleteMe()

            for event in self.events:
                event.on_stop()

        except:
            if ui:
                ui.messageBox('Input changed event failed: {}'.format(traceback.format_exc()))

    # Get default directory
    def _get_default_dir(self):

        # Get user's home directory
        default_dir = expanduser("~")

        # Create a subdirectory for this application settings
        default_dir = os.path.join(default_dir, self.name, "")

        # Create the folder if it does not exist
        if not os.path.exists(default_dir):
            os.makedirs(default_dir)

        return default_dir

    def get_all_preferences(self) -> dict:
        """Gets all preferences stored for this application

        Returns:
            All preferences as a dictionary
        """

        file_name = os.path.join(self.default_dir, ".preferences.json")

        if os.path.exists(file_name):
            with open(file_name) as f:
                try:
                    all_preferences = json.load(f)
                except:
                    all_preferences = {}
        else:
            all_preferences = {}

        return all_preferences

    def get_group_preferences(self, group_name: str) -> dict:
        """Gets preferences for a particular group (typically a given command)

        Args:
            group_name: name of parent group in which to store preferences

        Returns:
            A dictionary of just the options associated to this particular group
        """

        all_preferences = self.get_all_preferences()

        group_preferences = all_preferences.get(group_name, {})

        return group_preferences

    def save_preferences(self, group_name: str, new_group_preferences: dict, merge: bool):
        """Saves preferences for the application

        Args:
            group_name: name of parent group in which to store preferences
            new_group_preferences: Dictionary of preferences to save
            merge: If True then the new preferences in the group will be merged, if False all old values are deleted
        """

        all_preferences = self.get_all_preferences()

        old_group_preferences = all_preferences.get(group_name, None)

        if old_group_preferences is not None:
            result = "Updated"
        else:
            result = "Created"

        if merge:
            old_group_preferences.update(new_group_preferences)
            all_preferences[group_name] = old_group_preferences
        else:
            all_preferences[group_name] = new_group_preferences

        preferences_text = json.dumps(all_preferences)

        file_name = os.path.join(self.default_dir, ".preferences.json")
        with open(file_name, "w") as f:
            f.write(preferences_text)

        return result
