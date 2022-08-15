'''
from curses.textpad import Textbox
import adsk.fusion, adsk.core, traceback
from ..general_imports import *
from . import IconPaths, OsHelper
from . ConfigCommand import *

def addWheelToTable(wheel: adsk.fusion.Joint) -> None:
    """### Adds a wheel joint to its global list and wheel table.

    Args:
        wheel (adsk.fusion.Joint): wheel Joint object to be added.
    """
    try:
        onSelect = gm.handlers[3]
        wheelTableInput = INPUTS_ROOT.itemById("wheel_table")
        
        # def addPreselections(child_occurrences):
        #     for occ in child_occurrences:
        #         onSelect.allWheelPreselections.append(occ.entityToken)

        #         if occ.childOccurrences:
        #             addPreselections(occ.childOccurrences)

        # if wheel.childOccurrences:    
        #     addPreselections(wheel.childOccurrences)
        # else:
        #     onSelect.allWheelPreselections.append(wheel.entityToken)
        onSelect.allWheelPreselections.append(wheel.entityToken)

        WheelListGlobal.append(wheel)
        cmdInputs = adsk.core.CommandInputs.cast(wheelTableInput.commandInputs)

        icon = cmdInputs.addImageCommandInput(
            "placeholder_w", "Placeholder", IconPaths.wheelIcons["standard"]
        )

        name = cmdInputs.addTextBoxCommandInput(
            "name_w", "Joint name", wheel.name, 1, True
        )
        name.tooltip = wheel.name

        wheelType = cmdInputs.addDropDownCommandInput(
            "wheel_type_w",
            "Wheel Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        wheelType.listItems.add("Standard", True, "")
        wheelType.listItems.add("Omni", False, "")
        wheelType.listItems.add("Mecanum", False, "")
        wheelType.tooltip = "Wheel type"
        wheelType.tooltipDescription = "<Br>Omni-directional wheels can be used just like regular drive wheels but they have the advantage of being able to roll freely perpendicular to the drive direction.</Br>"
        wheelType.toolClipFilename = OsHelper.getOSPath(".", "src", "Resources") + os.path.join("WheelIcons", "omni-wheel-preview.png")

        signalType = cmdInputs.addDropDownCommandInput(
            "signal_type_w",
            "Signal Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        signalType.isFullWidth = True
        signalType.listItems.add("‎", True, IconPaths.signalIcons["PWM"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["CAN"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["PASSIVE"])
        signalType.tooltip = "Signal type"

        row = wheelTableInput.rowCount

        wheelTableInput.addCommandInput(icon, row, 0)
        wheelTableInput.addCommandInput(name, row, 1)
        wheelTableInput.addCommandInput(wheelType, row, 2)
        wheelTableInput.addCommandInput(signalType, row, 3)

    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addWheelToTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )

def addJointToTable(joint: adsk.fusion.Joint) -> None:
    """### Adds a Joint object to its global list and joint table.

    Args:
        joint (adsk.fusion.Joint): Joint object to be added
    """
    try:
        JointListGlobal.append(joint)
        jointTableInput = INPUTS_ROOT.itemById("joint_table")
        cmdInputs = adsk.core.CommandInputs.cast(jointTableInput.commandInputs)

        # joint type icons
        if joint.jointMotion.jointType == adsk.fusion.JointTypes.RigidJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Rigid", IconPaths.jointIcons["rigid"]
            )
            icon.tooltip = "Rigid joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Revolute", IconPaths.jointIcons["revolute"]
            )
            icon.tooltip = "Revolute joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.SliderJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Slider", IconPaths.jointIcons["slider"]
            )
            icon.tooltip = "Slider joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.PlanarJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Planar", IconPaths.jointIcons["planar"]
            )
            icon.tooltip = "Planar joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.PinSlotJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Pin Slot", IconPaths.jointIcons["pin_slot"]
            )
            icon.tooltip = "Pin slot joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.CylindricalJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Cylindrical", IconPaths.jointIcons["cylindrical"]
            )
            icon.tooltip = "Cylindrical joint"

        elif joint.jointMotion.jointType == adsk.fusion.JointTypes.BallJointType:
            icon = cmdInputs.addImageCommandInput(
                "placeholder", "Ball", IconPaths.jointIcons["ball"]
            )
            icon.tooltip = "Ball joint"

        # joint name
        name = cmdInputs.addTextBoxCommandInput(
            "name_j", "Occurrence name", "", 1, True
        )
        name.tooltip = joint.name
        name.formattedText = "<p style='font-size:11px'>{}</p>".format(joint.name)

        jointType = cmdInputs.addDropDownCommandInput(
            "joint_parent",
            "Joint Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        jointType.isFullWidth = True
        jointType.listItems.add("Root", True)

        # after each additional joint added, add joint to the dropdown of all preview rows/joints
        for row in range(jointTableInput.rowCount):
            if row != 0:
                dropDown = jointTableInput.getInputAtPosition(row, 2)
                dropDown.listItems.add(JointListGlobal[-1].name, False)

        # add all parent joint options to added joint dropdown
        for j in range(len(JointListGlobal) - 1):
            jointType.listItems.add(JointListGlobal[j].name, False)

        jointType.tooltip = "Possible parent joints"
        jointType.tooltipDescription = "<hr>The root component is usually the parent."

        signalType = cmdInputs.addDropDownCommandInput(
            "signal_type",
            "Signal Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        signalType.listItems.add("‎", True, IconPaths.signalIcons["PWM"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["CAN"])
        signalType.listItems.add("‎", False, IconPaths.signalIcons["PASSIVE"])
        signalType.tooltip = "Signal type"

        defaultMotorSpeed = adsk.core.ValueInput()
        defaultMotorSpeed.realValue = 90
        testMotorSpeed = cmdInputs.addTextBoxCommandInput(
            "test_j", "Omfg", "", 1, True
        )
        testMotorSpeed.formattedText = 'j'

        row = jointTableInput.rowCount

        jointTableInput.addCommandInput(icon, row, 0)
        jointTableInput.addCommandInput(name, row, 1)
        jointTableInput.addCommandInput(jointType, row, 2)
        jointTableInput.addCommandInput(signalType, row, 3)
        jointTableInput.addCommandInput(testMotorSpeed, row, 4)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addJointToTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )

def addGamepieceToTable(gamepiece: adsk.fusion.Occurrence) -> None:
    """### Adds a gamepiece occurrence to its global list and gamepiece table.

    Args:
        gamepiece (adsk.fusion.Occurrence): Gamepiece occurrence to be added
    """
    try:
        onSelect = gm.handlers[3]
        gamepieceTableInput = INPUTS_ROOT.itemById("gamepiece_table")
        def addPreselections(child_occurrences):
            for occ in child_occurrences:
                onSelect.allGamepiecePreselections.append(occ.entityToken)
                
                if occ.childOccurrences:
                    addPreselections(occ.childOccurrences)

        if gamepiece.childOccurrences:
            addPreselections(gamepiece.childOccurrences)
        else:
            onSelect.allGamepiecePreselections.append(gamepiece.entityToken)

        GamepieceListGlobal.append(gamepiece)
        cmdInputs = adsk.core.CommandInputs.cast(gamepieceTableInput.commandInputs)
        blankIcon = cmdInputs.addImageCommandInput(
            "blank_gp", "Blank", IconPaths.gamepieceIcons["blank"]
        )

        type = cmdInputs.addTextBoxCommandInput(
            "name_gp", "Occurrence name", gamepiece.name, 1, True
        )

        value = 0.0
        physical = gamepiece.component.getPhysicalProperties(
            adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
        )
        value = physical.mass

        # check if dropdown unit is kg or lbs. bool value taken from ConfigureCommandInputChanged
        massUnitInString = ""
        onInputChanged = gm.handlers[1]
        if onInputChanged.isLbs_f:
            value = round(
                value * 2.2046226218, 2 # lbs
            )
            massUnitInString = "<tt>(in pounds)</tt>"
        else:
            value = round(
                value, 2 # kg
            )
            massUnitInString = "<tt>(in kilograms)</tt>"

        weight = cmdInputs.addValueInput(
            "weight_gp", "Weight Input", "", adsk.core.ValueInput.createByString(str(value))
        )

        valueList = [1]
        for i in range(20):
            valueList.append(i / 20)

        friction_coeff = cmdInputs.addFloatSliderListCommandInput(
            "friction_coeff", "", "", valueList
        )
        friction_coeff.valueOne = 0.5
        
        type.tooltip = gamepiece.name

        weight.tooltip = "Weight of field element"
        weight.tooltipDescription = massUnitInString

        friction_coeff.tooltip = "Friction coefficient of field element"
        friction_coeff.tooltipDescription = (
            "<i>Friction coefficients range from 0 (ice) to 1 (rubber).</i>"
        )
        row = gamepieceTableInput.rowCount

        gamepieceTableInput.addCommandInput(blankIcon, row, 0)
        gamepieceTableInput.addCommandInput(type, row, 1)
        gamepieceTableInput.addCommandInput(weight, row, 2)
        gamepieceTableInput.addCommandInput(friction_coeff, row, 3)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.addGamepieceToTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )

def removeWheelFromTable(index: int) -> None:
    """### Removes a wheel occurrence from its global list and wheel table.

    Args:
        index (int): index of wheel item in its global list
    """
    try:
        onSelect = gm.handlers[3]
        wheelTableInput = INPUTS_ROOT.itemById("wheel_table")
        wheel = WheelListGlobal[index]

        def removePreselections(child_occurrences):
            for occ in child_occurrences:
                onSelect.allWheelPreselections.remove(occ.entityToken)

                if occ.childOccurrences:
                    removePreselections(occ.childOccurrences)

        if wheel.childOccurrences:
            removePreselections(wheel.childOccurrences)
        else:
            onSelect.allWheelPreselections.remove(wheel.entityToken)

        del WheelListGlobal[index]
        wheelTableInput.deleteRow(index + 1)

        #updateJointTable(wheel)
    except IndexError:
        pass
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeWheelFromTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )

def removeJointFromTable(joint: adsk.fusion.Joint) -> None:
    """### Removes a joint occurrence from its global list and joint table.

    Args:
        joint (adsk.fusion.Joint): Joint object to be removed
    """
    try:
        index = JointListGlobal.index(joint)
        jointTableInput = INPUTS_ROOT.itemById("joint_table")
        JointListGlobal.remove(joint)

        jointTableInput.deleteRow(index + 1)

        for row in range(jointTableInput.rowCount):
            if row == 0:
                continue

            dropDown = jointTableInput.getInputAtPosition(row, 2)
            listItems = dropDown.listItems

            if row > index:
                if listItems.item(index + 1).isSelected:
                    listItems.item(index).isSelected = True
                    listItems.item(index + 1).deleteMe()
                else:
                    listItems.item(index + 1).deleteMe()
            else:
                if listItems.item(index).isSelected:
                    listItems.item(index - 1).isSelected = True
                    listItems.item(index).deleteMe()
                else:
                    listItems.item(index).deleteMe()
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeJointFromTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )

def removeGamePieceFromTable(index: int) -> None:
    """### Removes a gamepiece occurrence from its global list and gamepiece table.

    Args:
        index (int): index of gamepiece item in its global list.
    """
    onSelect = gm.handlers[3]
    gamepieceTableInput = INPUTS_ROOT.itemById("gamepiece_table")
    gamepiece = GamepieceListGlobal[index]

    def removePreselections(child_occurrences):
        for occ in child_occurrences:
            onSelect.allGamepiecePreselections.remove(occ.entityToken)
            
            if occ.childOccurrences:
                removePreselections(occ.childOccurrences)
    try:
        if gamepiece.childOccurrences:
            removePreselections(GamepieceListGlobal[index].childOccurrences)
        else:
            onSelect.allGamepiecePreselections.remove(gamepiece.entityToken)

        del GamepieceListGlobal[index]
        gamepieceTableInput.deleteRow(index + 1)
    except IndexError:
        pass
    except:
        logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.removeGamePieceFromTable()").error(
        "Failed:\n{}".format(traceback.format_exc())
    )
'''
