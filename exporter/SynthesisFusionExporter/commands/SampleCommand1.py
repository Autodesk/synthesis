
import adsk.core
import apper
import adsk.fusion
from apper import AppObjects


class SampleCommand1(apper.Fusion360CommandBase):
    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        ao = AppObjects()
        app = adsk.core.Application.get()

        active_design = ao.design
        print(active_design)
        
        if not active_design:
            ui.messageBox('No active Fusion 360 design', 'No Design')
            return
