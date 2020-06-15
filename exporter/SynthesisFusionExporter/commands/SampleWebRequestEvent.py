import apper
from apper import AppObjects


class SampleWebRequestOpened(apper.Fusion360WebRequestEvent):

    def __init__(self, event_id: str, event_type):
        super().__init__(event_id, event_type)
        from multiprocessing.connection import Client
        address = ('localhost', 6000)
        self.conn = Client(address)

    def web_request_event_received(self, event_args, file, fusion_id, occurrence_or_document, private_info, properties):
        ao = AppObjects()

        # **********Do your stuff here**************
        ao.ui.messageBox("You just Opened: {} ".format(file))
        # Could close the file here also

        self.conn.send(['finished opening', file])

    def on_stop(self):
        super().on_stop()
        self.conn.close()

