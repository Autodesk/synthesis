from . import OsHelper
from . import Handlers

from ..general_imports import *

# no longer used
class HPalette:
    handlers = []
    events = []

    def __init__(
        self,
        name: str,
        title: str,
        visible: bool,
        closeable: bool,
        resizeable: bool,
        width: int,
        height: int,
        *argv,
    ):
        """#### Creates a HPalette Object with a number of function pointers that correspond to a action on the js side.

        Arguments:
            **name** *str* -- name of the palette that links the folder in Resources/palette
            **title** *str* -- title displayed on the palette
            **visible** *bool* -- is the palette visible?
            **closeable** *bool* -- is the palette closable?
            **resizeable** *bool* -- is the palette resizeable?
            **argv** *ptr* -- ptr to a list of function pointers that correspond to actions for the palette.

        Keyword Arguments:
            **width** *int* -- width of the palette in pixels (default: {400})
            **height** *int* -- height of the palette in pixels (default: {400})

        Raises:
            ValueError: If the unique ID is used and not be me
            ValueError: If the unique ID is used and it is by me
        """
        self.logger = logging.getLogger(f"{INTERNAL_ID}.HUI.{self.__class__.__name__}")

        self.uid = name.replace(" ", "") + f"_p2_{INTERNAL_ID}"
        self.name = name

        for arg in argv:
            self.events.append(arg)

        if self.uid in gm.uniqueIds:
            raise ValueError(
                f"Cannot create two UI Elements with the same ID {self.uid}\n"
            )

        if gm.ui.palettes is None:
            raise RuntimeError(f"No Palette object exists yet")

        self.palette = gm.ui.palettes.itemById(self.uid)

        if self.palette is None:
            path = OsHelper.getOSPathPalette(
                "src", "Resources", "Palette", f'{self.name.replace(" ", "")}'
            )

            self.palette = gm.ui.palettes.add(
                self.uid,
                title,
                f'{path}{self.name.replace(" ", "")}.html',
                visible,
                closeable,
                resizeable,
                width,
                height,
            )

            self.palette.dockingState = (
                adsk.core.PaletteDockingStates.PaletteDockStateLeft
            )

            onHTML = Handlers.HPaletteHTMLEventHandler(self)
            self.palette.incomingFromHTML.add(onHTML)
            self.handlers.append(onHTML)

        self.logger.info(f"Created Palette {self.uid}")

        self.palette.isVisible = True

    def deleteMe(self) -> None:
        """## Removes the palette"""
        palette = gm.app.userInterface.palettes.itemById(self.uid)
        if palette:
            # I hope you like problems because this is nothing but problems
            # self.events.clear()
            # self.handlers.clear()
            self.logger.debug(f"Deleted Palette {self.uid}")
            palette.deleteMe()


class HButton:
    handlers = []
    """ Keeps all handler classes alive which is essential apparently. - used in command events """

    def __init__(
        self,
        name: str,
        location: str,
        check_func: object,
        exec_func: object,
        description: str = "No Description",
        command: bool = False,
    ):
        """# Creates a new HButton Class.

        Arguments:
            **name** *str* -- name of button for display and ID - must be unique.\n
            **location** *str* -- location for button to be attached.\n
            **exec_func** *FunctionType* -- Function pointer to button execution logic.\n
            **description** *str* -- Helper text for onhover (default: *'No Description'*).\n
            **command** *bool* -- Is this a internal Fusion 360 command created event or a pass through

        Raises:
            **ValueError**: if *location* does not exist in the current context
        """
        self.logger = logging.getLogger(f"{INTERNAL_ID}.HUI.{self.__class__.__name__}")
        self.uid = name.replace(" ", "") + f"_{INTERNAL_ID}"

        if self.uid in gm.uniqueIds:
            raise ValueError(
                f"Cannot create two UI Elements with the same ID {self.uid}\n"
            )

        self.name = name

        if gm.ui.allToolbarPanels.itemById(location) is not None:
            self.location = location
        else:
            raise ValueError(f"{location} is not an id in the toolbar")

        cmdDef = gm.ui.commandDefinitions.itemById(self.uid)
        if cmdDef:
            # gm.ui.messageBox("Looks like you have experienced a crash we will do cleanup.")
            self.logger.debug("Looks like there was a crash, doing cleanup in button id")
            self.scrub()

        # needs to updated with new OString data
        self.button = gm.ui.commandDefinitions.addButtonDefinition(
            self.uid,
            f"{name}",
            f"{description}",
            OsHelper.getOSPath(".", "src", "Resources", f'{self.name.replace(" ", "")}'),
        )
        """ Button Command Definition stored as a member """

        gm.uniqueIds.append(self.uid)
        self.exec_func = exec_func
        self.check_func = check_func

        if command:
            # This will be a seperate command created event handler
            ccEventHandler = exec_func(self)
        else:
            ccEventHandler = Handlers.HButtonCommandCreatedEvent(self)

        self.button.commandCreated.add(ccEventHandler)
        self.handlers.append(ccEventHandler)

        panel = gm.ui.allToolbarPanels.itemById(f"{location}")

        if panel:
            self.buttonControl = panel.controls.addCommand(self.button)
            self.logger.info(f"Created Button {self.uid} in Panel {location}")
        else:
            self.logger.error(f"Cannot Create Button {self.uid} in Panel {location}")

        self.promote(True)
        """ Promote determines whether or not buttons are displayed on toolbar """

    def promote(self, flag: bool) -> None:
        """## Adds button to toolbar

        TODO: Add a promote to specific toolbar section

        Arguments:
            **flag** *bool* -- Is added to toolbar?

        Raises:
            **ValueError**: Given type of not bool
        """
        if self.buttonControl is not None:
            self.buttonControl.isPromotedByDefault = flag
            self.buttonControl.isPromoted = flag
        else:
            raise RuntimeError("ButtonControl was not defined for {}".format(self.uid))

    def deleteMe(self):
        """## Custom deleteMe method to easily deconstruct button data.

        This somehow doesn't work if I keep local references to all of these definitions.

        Arguments:
            **ui** *adsk.core.userinterface* -- user interface instance so I can find the references easily.
        """
        cmdDef = gm.ui.commandDefinitions.itemById(self.uid)
        if cmdDef:
            self.logger.debug(f"Removing Button {self.uid}")
            cmdDef.deleteMe()

        ctrl = gm.ui.allToolbarPanels.itemById(self.location).controls.itemById(self.uid)
        if ctrl:
            self.logger.debug(f"Removing Button Control {self.location}:{self.uid}")
            ctrl.deleteMe()

    def scrub(self):
        """### In-case I make a mistake or a crash happens early it can scrub the command.

        It can only be called if the ID is not currently in the buttons list.

        This is different than **deleteMe** because it would be good to have metrics on.
        """
        self.deleteMe()

    def __str__(self):
        """### Retrieves the button unique ID and treats it as a string.
        Returns:
            *str* -- button unique ID.
        """
        return self.uid
