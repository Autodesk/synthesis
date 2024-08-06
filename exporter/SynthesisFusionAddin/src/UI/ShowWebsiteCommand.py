import webbrowser

import adsk.core

from ..general_imports import *


class ShowWebsiteCommandExecuteHandler(adsk.core.CommandEventHandler):
    def __init__(self) -> None:
        super().__init__()

    def notify(self, args):
        try:
            url = "https://synthesis.autodesk.com/tutorials.html"
            res = webbrowser.open(url, new=2)
            if not res:
                gm.ui.messageBox("Failed\n{}".format(traceback.format_exc()))
        except:
            gm.ui.messageBox("Failed\n{}".format(traceback.format_exc()))


class ShowWebsiteCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    def __init__(self, configure) -> None:
        super().__init__()

    def notify(self, args):
        try:
            command = args.command
            onExecute = ShowWebsiteCommandExecuteHandler()
            command.execute.add(onExecute)
            gm.handlers.append(onExecute)
        except:
            gm.ui.messageBox("Failed\n{}".format(traceback.format_exc()))
