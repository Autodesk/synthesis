import adsk.core
import adsk.fusion
import traceback

import apper


class SampleDocumentEvent1(apper.Fusion360DocumentEvent):

    def document_event_received(self, event_args, document):
        app = adsk.core.Application.cast(adsk.core.Application.get())
        msg = "You just ACTIVATED a document called: {} ".format(document.name)
        app.userInterface.messageBox(msg)


class SampleDocumentEvent2(apper.Fusion360DocumentEvent):

    def document_event_received(self, event_args, document):
        app = adsk.core.Application.cast(adsk.core.Application.get())
        msg = "You just CLOSED a document called: {}".format(document.name)
        app.userInterface.messageBox(msg)
