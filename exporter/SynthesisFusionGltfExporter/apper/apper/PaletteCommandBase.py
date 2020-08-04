"""
PaletteCommandBase.py
=========================================================
Python module for creating an HTML Palette based command

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
:copyright: (c) 2019 by Patrick Rainsberry.
:license: Apache 2.0, see LICENSE for more details.

"""
import traceback
from urllib.parse import urlparse

import adsk.core
import apper

import os
import sys


class PaletteCommandBase(apper.Fusion360CommandBase):
    """Class for creating a Fusion 360 Command that will show a web palette

    Args:
        name: Name of the Command
        options: Dictionary of options
    """

    def __init__(self, name: str, options: dict):
        super().__init__(name, options)

        ao = apper.AppObjects()

        self.palette_id = options.get('palette_id', 'Default Command Name')
        self.palette_name = options.get('palette_name', 'Palette Name')

        debug_path = options.get('palette_html_file_url_debug')
        palette_is_local = options.get('palette_is_local', True)

        if palette_is_local:
            rel_path = options.get('palette_html_file_url')

            if self.fusion_app.debug and (debug_path is not None):
                self.palette_html_file_url = debug_path

            elif rel_path is not None:

                local_path_from_root = os.path.dirname(
                    os.path.relpath(
                        sys.modules[self.__class__.__module__].__file__,
                        self.fusion_app.root_path
                    )
                )

                self.palette_html_file_url = os.path.join('./', local_path_from_root, rel_path)
            else:
                raise AttributeError(
                    "Resource Path not defined for Palette.  Set palette_html_file_url in command options")
        else:
            # TODO add some url validation
            self.palette_html_file_url = options.get('palette_html_file_url')

        if self.fusion_app.debug:
            ao.ui.messageBox(self.palette_html_file_url)

        self.palette_is_visible = options.get('palette_is_visible', True)
        self.palette_show_close_button = options.get('palette_show_close_button', True)
        self.palette_is_resizable = options.get('palette_is_resizable', True)
        self.palette_width = options.get('palette_width', 600)
        self.palette_height = options.get('palette_height', 600)
        self.palette_force_url_reload = options.get('palette_force_url_reload', True)

        self.palette = None
        self.args = None
        self.handlers = []
        self.html_handlers = []

    def _get_create_event(self):
        return _PaletteCreatedHandler(self)

    def on_html_event(self, html_args: adsk.core.HTMLEventArgs):
        """
        Args:
            html_args: the arguments sent with the command from the html page
        """
        pass

    def on_palette_close(self):
        """Run when the palette is closed

        """
        pass

    def on_palette_execute(self, palette: adsk.core.Palette):
        """Function is run when the palette is executed.  Useful to gather initial data and send to html page

        Args:
            palette: Reference to the palette
        """
        pass

    def on_stop(self):
        """Function is run when the addin stops.

        Clean up.  If overridden ensure to execute with super().on_stop()
        """
        app = adsk.core.Application.cast(adsk.core.Application.get())
        ui = app.userInterface
        palette = ui.palettes.itemById(self.palette_id)

        for handler in self.html_handlers:
            palette.incomingFromHTML.remove(handler)

        if palette:
            palette.deleteMe()

        super().on_stop()


class _PaletteCreatedHandler(adsk.core.CommandCreatedEventHandler):
    """Event handler for the palette created event.

    Args:
        cmd_object: the parent command object
    """

    def __init__(self, cmd_object):
        super().__init__()
        self.cmd_object_ = cmd_object

    def notify(self, args):
        """Method executed by Fusion.  DOn't rename

        Args:
            args: args for command
        """
        try:

            command_ = args.command
            inputs_ = command_.commandInputs

            on_execute_handler = _PaletteExecuteHandler(self.cmd_object_)
            command_.execute.add(on_execute_handler)
            self.cmd_object_.handlers.append(on_execute_handler)

            self.cmd_object_.on_create(command_, inputs_)

        except:
            app = adsk.core.Application.cast(adsk.core.Application.get())
            ui = app.userInterface
            ui.messageBox('Command created failed: {}'.format(traceback.format_exc()))


class _PaletteExecuteHandler(adsk.core.CommandEventHandler):
    """Event handler for the palette execution event.

    Args:
        cmd_object: the parent command object
    """

    def __init__(self, cmd_object):
        super().__init__()
        self.cmd_object_ = cmd_object

    def notify(self, args):
        """Method executed by Fusion.  Don't rename

        Args:
            args: args for command
        """
        app = adsk.core.Application.cast(adsk.core.Application.get())
        ui = app.userInterface
        try:

            # Create and display the palette.
            palette = ui.palettes.itemById(self.cmd_object_.palette_id)

            if not palette:
                palette = ui.palettes.add(
                    self.cmd_object_.palette_id,
                    self.cmd_object_.palette_name,
                    self.cmd_object_.palette_html_file_url,
                    self.cmd_object_.palette_is_visible,
                    self.cmd_object_.palette_show_close_button,
                    self.cmd_object_.palette_is_resizable,
                    self.cmd_object_.palette_width,
                    self.cmd_object_.palette_height,
                    True
                )

                # Add handler to HTMLEvent of the palette.
                on_html_event_handler = _HTMLEventHandler(self.cmd_object_)
                palette.incomingFromHTML.add(on_html_event_handler)
                self.cmd_object_.handlers.append(on_html_event_handler)
                self.cmd_object_.html_handlers.append(on_html_event_handler)

                # Add handler to CloseEvent of the palette.
                on_closed_handler = _PaletteCloseHandler(self.cmd_object_)
                palette.closed.add(on_closed_handler)
                self.cmd_object_.handlers.append(on_closed_handler)

            else:
                main_url = urlparse(self.cmd_object_.palette_html_file_url)
                current_url = urlparse(palette.htmlFileURL)

                if not (
                        (not self.cmd_object_.palette_force_url_reload) &
                        (main_url.netloc == current_url.netloc) &
                        (main_url.path == current_url.path)
                ):
                    # ui.messageBox(current_url.netloc + "  vs.  " + main_url.netloc)
                    # ui.messageBox(current_url.path + "  vs.  " + main_url.path)
                    # ui.messageBox(str(self.cmd_object_.palette_force_url_reload))
                    palette.htmlFileURL = self.cmd_object_.palette_html_file_url

                palette.isVisible = True

            self.cmd_object_.on_palette_execute(palette)

        except:
            ui.messageBox('Palette ({}) Execution Failed: {}'.format(
                self.cmd_object_.palette_html_file_url,
                traceback.format_exc())
            )


class _HTMLEventHandler(adsk.core.HTMLEventHandler):
    """Event handler for the palette HTML event.

    Args:
        cmd_object: the parent command object
    """

    def __init__(self, cmd_object):
        super().__init__()
        self.cmd_object_ = cmd_object

    def notify(self, args):
        """Method executed by Fusion.  Don't rename

        Args:
            args: args for command
        """
        try:
            html_args = adsk.core.HTMLEventArgs.cast(args)
            self.cmd_object_.on_html_event(html_args)

        except:
            app = adsk.core.Application.cast(adsk.core.Application.get())
            ui = app.userInterface
            ui.messageBox('Failed Handling HTML Event:\n{}'.format(traceback.format_exc()))


class _PaletteCloseHandler(adsk.core.UserInterfaceGeneralEventHandler):
    """Event handler for the palette close event.

    Args:
        cmd_object: the parent command object
    """

    def __init__(self, cmd_object):
        super().__init__()
        self.cmd_object_ = cmd_object

    def notify(self, args):
        """Method executed by Fusion.  Don't rename

        Args:
            args: args for command
        """
        try:
            self.cmd_object_.on_palette_close()

        except:
            app = adsk.core.Application.cast(adsk.core.Application.get())
            ui = app.userInterface
            ui.messageBox('Failed During Palette Close:\n{}'.format(traceback.format_exc()))
