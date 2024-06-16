from adsk.core import *
from adsk.fusion import *

def BuildJointEditPanel(joint: Joint, args: CommandCreatedEventArgs):
    inputs = args.command.commandInputs

    cmdTest = inputs.itemById('test')
    if cmdTest:
        cmdTest.deleteMe()
    
    cmdTest = inputs.addBoolValueInput('test', 'Test Boolean Input', False)

    print('Built joint edit panel')