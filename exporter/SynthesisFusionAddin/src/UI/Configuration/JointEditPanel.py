from adsk.core import *
from adsk.fusion import *
from ..Util.EventHandlers import *

COMM_JOINT_SPEED = 'JointSpeed'

handlers = []

def remove(id: str, inputs: CommandInputs):
    cmd = inputs.itemById(id)
    if cmd:
        cmd.deleteMe()

def buildJointEditPanel(joint: Joint, args: CommandCreatedEventArgs):
    inputs = args.command.commandInputs

    remove(COMM_JOINT_SPEED, inputs)
    cmdJointSpeed = inputs.addValueInput(
        COMM_JOINT_SPEED,
        'Joint Speed',
        'deg',
        ValueInput.createByReal(3.1415926)
    )

    args.command.execute.add(MakeCommandExecuteHandler(updateJointInfo, handlers))

    print('Built joint edit panel')

def updateJointInfo(args: CommandEventArgs):
    print(f'Executing')