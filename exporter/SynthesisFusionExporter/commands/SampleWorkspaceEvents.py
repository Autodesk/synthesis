import adsk.core
import adsk.fusion
import traceback

import apper


class SampleWorkspaceEvent1(apper.Fusion360WorkspaceEvent):

    def workspace_event_received(self, event_args, workspace):
        app = adsk.core.Application.cast(adsk.core.Application.get())
        msg = "You just ACTIVATED a workspace called: {} ".format(workspace.name)
        app.userInterface.messageBox(msg)

