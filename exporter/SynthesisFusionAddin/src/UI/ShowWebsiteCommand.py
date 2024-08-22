import webbrowser
from typing import Any

import adsk.core

from src import gm
from src.Logging import logFailure


class ShowWebsiteCommandExecuteHandler(adsk.core.CommandEventHandler):
    def __init__(self) -> None:
        super().__init__()

    @logFailure
    def notify(self, args: adsk.core.CommandEventArgs) -> None:
        url = "https://synthesis.autodesk.com/tutorials.html"
        res = webbrowser.open(url, new=2)
        if not res:
            raise BaseException("Could not open webbrowser")


class ShowWebsiteCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    def __init__(self, configure: Any) -> None:
        super().__init__()

    @logFailure
    def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        command = args.command
        onExecute = ShowWebsiteCommandExecuteHandler()
        command.execute.add(onExecute)
        gm.handlers.append(onExecute)
