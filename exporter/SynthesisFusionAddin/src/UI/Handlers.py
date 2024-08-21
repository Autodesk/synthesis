from typing import Any

import adsk.core


class HButtonCommandCreatedEvent(adsk.core.CommandCreatedEventHandler):
    """## Abstraction of CreatedEvent as its mostly useless in this context


    **adsk.core.CommandCreatedEventHandler** -- Parent abstract created event class
    """

    def __init__(self, button: Any) -> None:
        super().__init__()
        self.button = button

    def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        """## Called when parent button object is created and links the execute function pointer.

        Arguments:
            **args** *args* -- List of arbitrary info given to fusion event handlers.
        """
        cmd = args.command

        if self.button.check_func():
            onExecute = HButtonCommandExecuteHandler(self.button)
            cmd.execute.add(onExecute)
            self.button.handlers.append(onExecute)


class HButtonCommandExecuteHandler(adsk.core.CommandEventHandler):
    """## Abstraction of the CommandExecute Handler which will make a simple call to the function supplied by the button class.

    Parent Class:
    **adsk.core.CommandEventHandler** -- Fusion CommandEventHandler Abstract parent to link notify to ui.
    """

    def __init__(self, button: Any) -> None:
        super().__init__()
        self.button = button

    def notify(self, _: adsk.core.CommandEventArgs) -> None:
        self.button.exec_func()
