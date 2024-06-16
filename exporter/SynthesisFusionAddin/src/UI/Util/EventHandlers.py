from typing import List
from adsk.core import *
from adsk.core import MarkingMenuEventArgs

from ...general_imports import INTERNAL_ID, gm
import logging, traceback

def ErrorHandle(handler, eventArgs):
    try:
        handler(eventArgs)
    except:
        gm.ui.messageBox(f'command executed failed: {traceback.format_exc()}')
        logging.getLogger(f"{INTERNAL_ID}").error(
            "Failed:\n{}".format(traceback.format_exc())
        )

class _CommandCreatedHandlerShort(CommandCreatedEventHandler):
    def __init__(self, handler):
        super().__init__()
        self.handler = handler

    def notify(self, eventArgs: CommandCreatedEventArgs) -> None:
        ErrorHandle(self.handler, eventArgs)

class _CommandExecuteHandlerShort(CommandEventHandler):
    def __init__(self, handler):
        super().__init__()
        self.handler = handler

    def notify(self, eventArgs: CommandEventArgs) -> None:
        ErrorHandle(self.handler, eventArgs)

class _SelectionEventHandlerShort(SelectionEventHandler):
    def __init__(self, handler):
        super().__init__()
        self.handler = handler

    def notify(self, eventArgs: SelectionEventArgs) -> None:
        ErrorHandle(self.handler, eventArgs)

class _MarkingMenuEventHandlerShort(MarkingMenuEventHandler):
    def __init__(self, handler):
        super().__init__()
        self.handler = handler

    def notify(self, eventArgs: MarkingMenuEventArgs) -> None:
        # print('AHHH')
        ErrorHandle(self.handler, eventArgs)

def MakeCommandCreatedHandler(notify, handlerCollection: List | None = None) -> _CommandCreatedHandlerShort:
    handler = _CommandCreatedHandlerShort(notify)
    if handlerCollection is not None:
        handlerCollection.append(handler)
    return handler

def MakeCommandExecuteHandler(notify, handlerCollection: List | None = None) -> _CommandExecuteHandlerShort:
    handler = _CommandExecuteHandlerShort(notify)
    if handlerCollection is not None:
        handlerCollection.append(handler)
    return handler

def MakeMarkingMenuEventHandler(notify, handlerCollection: List | None = None) -> _MarkingMenuEventHandlerShort:
    handler = _MarkingMenuEventHandlerShort(notify)
    if handlerCollection is not None:
        handlerCollection.append(handler)
    return handler