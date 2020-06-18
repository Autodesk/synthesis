
import adsk, adsk.core, adsk.fusion, traceback
import apper
from apper import AppObjects


class ExportCommand(apper.Fusion360CommandBase):
    
    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        ao = AppObjects()
        app = adsk.core.Application.get()
        ui = app.userInterface

        title = 'Synthesis Exporter'

        design = app.activeDocument.design
        uuid = apper.Fusion360Utilities.get_a_uuid()
        user = app.currentUser.displayName

        currentDesignData = 'Current design data of: ' + design.parentDocument.name + '\n' + 'GUID: ' + uuid + '\n' + 'User: ' + user + '\n'
        ui.messageBox('Hello Synthesis')

        