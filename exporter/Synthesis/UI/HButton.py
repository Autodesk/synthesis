import adsk.core

from Synthesis import gm, INTERNAL_ID
from Synthesis.Logging import getLogger
from Synthesis.OsHelper import getOSPath

logger = getLogger(INTERNAL_ID)


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
            logger.debug("Looks like there was a crash, doing cleanup in button id")
            self.scrub()

        # needs to updated with new OString data
        self.button = gm.ui.commandDefinitions.addButtonDefinition(
            self.uid,
            f"{name}",
            f"{description}",
            getOSPath(".", "src", "Resources", f'{self.name.replace(" ", "")}'),
        )
        """ Button Command Definition stored as a member """

        gm.uniqueIds.append(self.uid)
        self.exec_func = exec_func
        self.check_func = check_func

        if command:
            # This will be a seperate command created event handler
            ccEventHandler = exec_func(self)
        else:
            ccEventHandler = HButtonCommandCreatedEvent(self)

        self.button.commandCreated.add(ccEventHandler)
        self.handlers.append(ccEventHandler)

        panel = gm.ui.allToolbarPanels.itemById(f"{location}")

        if panel:
            self.buttonControl = panel.controls.addCommand(self.button)
            logger.info(f"Created Button {self.uid} in Panel {location}")
        else:
            logger.error(f"Cannot Create Button {self.uid} in Panel {location}")

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
            logger.debug(f"Removing Button {self.uid}")
            cmdDef.deleteMe()

        ctrl = gm.ui.allToolbarPanels.itemById(self.location).controls.itemById(self.uid)
        if ctrl:
            logger.debug(f"Removing Button Control {self.location}:{self.uid}")
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


class HButtonCommandCreatedEvent(adsk.core.CommandCreatedEventHandler):
    """## Abstraction of CreatedEvent as its mostly useless in this context


    **adsk.core.CommandCreatedEventHandler** -- Parent abstract created event class
    """

    def __init__(self, button):
        super().__init__()
        self.button = button

    def notify(self, args):
        """## Called when parent button object is created and links the execute function pointer.

        Arguments:
            **args** *args* -- List of arbitrary info given to fusion event handlers.
        """
        cmd = adsk.core.CommandCreatedEventArgs.cast(args).command

        if self.button.check_func():
            onExecute = HButtonCommandExecuteHandler(self.button)
            cmd.execute.add(onExecute)
            self.button.handlers.append(onExecute)


class HButtonCommandExecuteHandler(adsk.core.CommandEventHandler):
    """## Abstraction of the CommandExecute Handler which will make a simple call to the function supplied by the button class.

    Parent Class:
    **adsk.core.CommandEventHandler** -- Fusion CommandEventHandler Abstract parent to link notify to ui.
    """

    def __init__(self, button):
        super().__init__()
        self.button = button

    def notify(self, _):
        self.button.exec_func()
