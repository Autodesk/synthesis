import logging
import traceback

import adsk.core
import adsk.fusion

from ..Parser.ExporterOptions import (
    Joint,
    JointParentType,
    SignalType,
    Wheel,
    WheelType,
)
from . import IconPaths
from .CreateCommandInputsHelper import (
    createBooleanInput,
    createTableInput,
    createTextBoxInput,
)

# Wish we did not need this. Could look into storing everything within the design every time - Brandon
selectedJointList: list[adsk.fusion.Joint] = []
previousWheelCheckboxState: list[bool] = []
jointWheelIndexMap: dict[str, int] = {}
jointConfigTable: adsk.core.TableCommandInput
wheelConfigTable: adsk.core.TableCommandInput


def createJointConfigTab(args: adsk.core.CommandCreatedEventArgs) -> None:
    try:
        inputs = args.command.commandInputs
        jointConfigTab = inputs.addTabCommandInput("jointSettings", "Joint Settings")
        jointConfigTab.tooltip = "Select and configure robot joints."
        jointConfigTabInputs = jointConfigTab.children

        # TODO: Change background colors and such - Brandon
        global jointConfigTable
        jointConfigTable = createTableInput(
            "jointTable",
            "Joint Table",
            jointConfigTabInputs,
            7,
            "1:2:2:2:2:2:2",
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "jointMotionHeader",
                "Motion",
                jointConfigTabInputs,
                "Motion",
                bold=False,
            ),
            0,
            0,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput("nameHeader", "Name", jointConfigTabInputs, "Joint name", bold=False),
            0,
            1,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "parentHeader",
                "Parent",
                jointConfigTabInputs,
                "Parent joint",
                background="#d9d9d9",
            ),
            0,
            2,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "signalHeader",
                "Signal",
                jointConfigTabInputs,
                "Signal type",
                background="#d9d9d9",
            ),
            0,
            3,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "speedHeader",
                "Speed",
                jointConfigTabInputs,
                "Joint Speed",
                background="#d9d9d9",
            ),
            0,
            4,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "forceHeader",
                "Force",
                jointConfigTabInputs,
                "Joint Force",
                background="#d9d9d9",
            ),
            0,
            5,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "wheelHeader",
                "Is Wheel",
                jointConfigTabInputs,
                "Is Wheel",
                background="#d9d9d9",
            ),
            0,
            6,
        )

        jointSelect = jointConfigTabInputs.addSelectionInput(
            "jointSelection", "Selection", "Select a joint in your assembly to add."
        )
        jointSelect.addSelectionFilter("Joints")
        jointSelect.setSelectionLimits(0)

        # Visibility is triggered by `addJointInputButton`
        jointSelect.isEnabled = jointSelect.isVisible = False

        jointConfigTabInputs.addTextBoxCommandInput("jointTabBlankSpacer", "", "", 1, True)

        global wheelConfigTable
        wheelConfigTable = createTableInput(
            "wheelTable",
            "Wheel Table",
            jointConfigTabInputs,
            4,
            "1:2:2:2",
        )

        wheelConfigTable.addCommandInput(
            createTextBoxInput(
                "wheelMotionHeader",
                "Motion",
                jointConfigTabInputs,
                "Motion",
                bold=False,
            ),
            0,
            0,
        )

        wheelConfigTable.addCommandInput(
            createTextBoxInput("name_header", "Name", jointConfigTabInputs, "Joint name", bold=False),
            0,
            1,
        )

        wheelConfigTable.addCommandInput(
            createTextBoxInput(
                "wheelTypeHeader",
                "WheelType",
                jointConfigTabInputs,
                "Wheel type",
                background="#d9d9d9",
            ),
            0,
            2,
        )

        wheelConfigTable.addCommandInput(
            createTextBoxInput(
                "signalTypeHeader",
                "SignalType",
                jointConfigTabInputs,
                "Signal type",
                background="#d9d9d9",
            ),
            0,
            3,
        )

        jointSelectCancelButton = jointConfigTabInputs.addBoolValueInput("jointSelectCancelButton", "Cancel", False)
        jointSelectCancelButton.isEnabled = jointSelectCancelButton.isVisible = False

        addJointInputButton = jointConfigTabInputs.addBoolValueInput("jointAddButton", "Add", False)
        removeJointInputButton = jointConfigTabInputs.addBoolValueInput("jointRemoveButton", "Remove", False)
        addJointInputButton.isEnabled = removeJointInputButton.isEnabled = True

        jointConfigTable.addToolbarCommandInput(addJointInputButton)
        jointConfigTable.addToolbarCommandInput(removeJointInputButton)
        jointConfigTable.addToolbarCommandInput(jointSelectCancelButton)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.createJointConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def addJointToConfigTab(fusionJoint: adsk.fusion.Joint, synJoint: Joint | None = None) -> bool:
    try:
        if fusionJoint in selectedJointList:
            return False

        selectedJointList.append(fusionJoint)
        commandInputs = jointConfigTable.commandInputs

        if fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.RigidJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Rigid", IconPaths.jointIcons["rigid"])
            icon.tooltip = "Rigid joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Revolute", IconPaths.jointIcons["revolute"])
            icon.tooltip = "Revolute joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.SliderJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Slider", IconPaths.jointIcons["slider"])
            icon.tooltip = "Slider joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.PlanarJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Planar", IconPaths.jointIcons["planar"])
            icon.tooltip = "Planar joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.PinSlotJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Pin Slot", IconPaths.jointIcons["pin_slot"])
            icon.tooltip = "Pin slot joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.CylindricalJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Cylindrical", IconPaths.jointIcons["cylindrical"])
            icon.tooltip = "Cylindrical joint"

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.BallJointType:
            icon = commandInputs.addImageCommandInput("placeholder", "Ball", IconPaths.jointIcons["ball"])
            icon.tooltip = "Ball joint"

        name = commandInputs.addTextBoxCommandInput("name_j", "Occurrence name", "", 1, True)
        name.tooltip = fusionJoint.name
        name.formattedText = f"<p style='font-size:11px'>{fusionJoint.name}</p>"

        jointType = commandInputs.addDropDownCommandInput(
            "jointParent",
            "Joint Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )

        jointType.isFullWidth = True

        # Transition: AARD-1685
        # Implementation of joint parent system needs to be revisited.
        jointType.listItems.add("Root", True)

        for row in range(1, jointConfigTable.rowCount):  # Row is 1 indexed
            dropDown = jointConfigTable.getInputAtPosition(row, 2)
            dropDown.listItems.add(selectedJointList[-1].name, False)

        for fusionJoint in selectedJointList:
            jointType.listItems.add(fusionJoint.name, False)

        jointType.tooltip = "Possible parent joints"
        jointType.tooltipDescription = "<hr>The root component is usually the parent.</hr>"

        signalType = commandInputs.addDropDownCommandInput(
            "signalTypeJoint",
            "Signal Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )

        # TODO: Make this better, this is bad bad bad - Brandon
        if synJoint:
            signalType.listItems.add("‎", synJoint.signalType is SignalType.PWM, IconPaths.signalIcons["PWM"])
            signalType.listItems.add("‎", synJoint.signalType is SignalType.CAN, IconPaths.signalIcons["CAN"])
            signalType.listItems.add("‎", synJoint.signalType is SignalType.PASSIVE, IconPaths.signalIcons["PASSIVE"])
        else:
            signalType.listItems.add("‎", True, IconPaths.signalIcons["PWM"])
            signalType.listItems.add("‎", False, IconPaths.signalIcons["CAN"])
            signalType.listItems.add("‎", False, IconPaths.signalIcons["PASSIVE"])

        signalType.tooltip = "Signal type"

        row = jointConfigTable.rowCount
        jointConfigTable.addCommandInput(icon, row, 0)
        jointConfigTable.addCommandInput(name, row, 1)
        jointConfigTable.addCommandInput(jointType, row, 2)
        jointConfigTable.addCommandInput(signalType, row, 3)

        # Comparison by `==` over `is` because the Autodesk API does not use `Enum` for their enum classes
        if fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
            if synJoint:
                jointSpeedValue = synJoint.speed
            else:
                jointSpeedValue = 3.1415926

            jointSpeed = commandInputs.addValueInput(
                "jointSpeed",
                "Speed",
                "deg",
                adsk.core.ValueInput.createByReal(jointSpeedValue),
            )
            jointSpeed.tooltip = "Degrees per second"
            jointConfigTable.addCommandInput(jointSpeed, row, 4)

        elif fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.SliderJointType:
            if synJoint:
                jointSpeedValue = synJoint.speed
            else:
                jointSpeedValue = 100

            jointSpeed = commandInputs.addValueInput(
                "jointSpeed",
                "Speed",
                "m",
                adsk.core.ValueInput.createByReal(jointSpeedValue),
            )
            jointSpeed.tooltip = "Meters per second"
            jointConfigTable.addCommandInput(jointSpeed, row, 4)

        if synJoint:
            jointForceValue = synJoint.force * 100  # Currently a factor of 100 - Should be investigated
        else:
            jointForceValue = 5

        jointForce = commandInputs.addValueInput(
            "jointForce", "Force", "N", adsk.core.ValueInput.createByReal(jointForceValue)
        )
        jointForce.tooltip = "Newtons"
        jointConfigTable.addCommandInput(jointForce, row, 5)

        if fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
            wheelCheckboxEnabled = True
            wheelCheckboxTooltip = "Determines if this joint should be counted as a wheel."
        else:
            wheelCheckboxEnabled = False
            wheelCheckboxTooltip = "Only Revolute joints can be treated as wheels."

        isWheel = synJoint.isWheel if synJoint else False

        # Transition: AARD-1685
        # All command inputs should be created using the helpers.
        jointConfigTable.addCommandInput(
            createBooleanInput(
                "isWheel",
                "Is Wheel",
                commandInputs,
                wheelCheckboxTooltip,
                checked=isWheel,
                enabled=wheelCheckboxEnabled,
            ),
            row,
            6,
        )

        previousWheelCheckboxState.append(isWheel)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.addJointToConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )

    return True


def addWheelToConfigTab(joint: adsk.fusion.Joint, wheel: Wheel | None = None) -> None:
    jointWheelIndexMap[joint.entityToken] = wheelConfigTable.rowCount

    commandInputs = wheelConfigTable.commandInputs
    wheelIcon = commandInputs.addImageCommandInput("wheelPlaceholder", "Placeholder", IconPaths.wheelIcons["standard"])
    wheelName = commandInputs.addTextBoxCommandInput("wheelName", "Joint Name", joint.name, 1, True)
    wheelName.tooltip = joint.name  # TODO: Should this be the same?
    wheelType = commandInputs.addDropDownCommandInput(
        "wheelType", "Wheel Type", dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
    )

    selectedWheelType = wheel.wheelType if wheel else WheelType.STANDARD
    wheelType.listItems.add("Standard", selectedWheelType is WheelType.STANDARD, "")
    wheelType.listItems.add("OMNI", selectedWheelType is WheelType.OMNI, "")
    wheelType.listItems.add("Mecanum", selectedWheelType is WheelType.MECANUM, "")
    wheelType.tooltip = "Wheel type"
    wheelType.tooltipDescription = "".join(
        [
            "<Br>Omni-directional wheels can be used just like regular drive wheels",
            "but they have the advantage of being able to roll freely perpendicular to",
            "the drive direction.</Br>",
        ]
    )

    signalType = commandInputs.addDropDownCommandInput(
        "wheelSignalType", "Signal Type", dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle
    )
    signalType.isFullWidth = True
    signalType.isEnabled = False
    signalType.tooltip = "Wheel signal type is linked with the respective joint signal type."
    i = selectedJointList.index(joint)
    jointSignalType = SignalType(jointConfigTable.getInputAtPosition(i + 1, 3).selectedItem.index + 1)
    signalType.listItems.add("‎", jointSignalType is SignalType.PWM, IconPaths.signalIcons["PWM"])
    signalType.listItems.add("‎", jointSignalType is SignalType.CAN, IconPaths.signalIcons["CAN"])
    signalType.listItems.add("‎", jointSignalType is SignalType.PASSIVE, IconPaths.signalIcons["PASSIVE"])

    row = wheelConfigTable.rowCount
    wheelConfigTable.addCommandInput(wheelIcon, row, 0)
    wheelConfigTable.addCommandInput(wheelName, row, 1)
    wheelConfigTable.addCommandInput(wheelType, row, 2)
    wheelConfigTable.addCommandInput(signalType, row, 3)


def removeIndexedJointFromConfigTab(index: int) -> None:
    try:
        removeJointFromConfigTab(selectedJointList[index])
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.removeIndexedJointFromConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeJointFromConfigTab(joint: adsk.fusion.Joint) -> None:
    try:
        if jointWheelIndexMap.get(joint.entityToken):
            removeWheelFromConfigTab(joint)

        i = selectedJointList.index(joint)
        selectedJointList.remove(joint)
        previousWheelCheckboxState.pop(i)
        jointConfigTable.deleteRow(i + 1)
        for row in range(1, jointConfigTable.rowCount):  # Row is 1 indexed
            # TODO: Step through this in the debugger and figure out if this is all necessary.
            listItems = jointConfigTable.getInputAtPosition(row, 2).listItems
            if row > i:
                if listItems.item(i + 1).isSelected:
                    listItems.item(i).isSelected = True
                    listItems.item(i + 1).deleteMe()
                else:
                    listItems.item(i + 1).deleteMe()
            else:
                if listItems.item(i).isSelected:
                    listItems.item(i - 1).isSelected = True
                    listItems.item(i).deleteMe()
                else:
                    listItems.item(i).deleteMe()
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.removeJointFromConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeWheelFromConfigTab(joint: adsk.fusion.Joint) -> None:
    try:
        row = jointWheelIndexMap[joint.entityToken]
        wheelConfigTable.deleteRow(row)
        del jointWheelIndexMap[joint.entityToken]
        for key, value in jointWheelIndexMap.items():
            if value > row - 1:
                jointWheelIndexMap[key] -= 1
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.removeJointFromConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def getSelectedJointsAndWheels() -> tuple[list[Joint], list[Wheel]]:
    joints: list[Joint] = []
    wheels: list[Wheel] = []
    for row in range(1, jointConfigTable.rowCount):  # Row is 1 indexed
        jointEntityToken = selectedJointList[row - 1].entityToken
        signalTypeIndex = jointConfigTable.getInputAtPosition(row, 3).selectedItem.index
        signalType = SignalType(signalTypeIndex + 1)
        jointSpeed = jointConfigTable.getInputAtPosition(row, 4).value
        jointForce = jointConfigTable.getInputAtPosition(row, 5).value
        isWheel = jointConfigTable.getInputAtPosition(row, 6).value

        joints.append(
            Joint(
                jointEntityToken,
                JointParentType.ROOT,
                signalType,
                jointSpeed,
                jointForce / 100.0,
                isWheel,
            )
        )

        if isWheel:
            wheelRow = jointWheelIndexMap[jointEntityToken]
            wheelTypeIndex = wheelConfigTable.getInputAtPosition(wheelRow, 2).selectedItem.index
            wheels.append(
                Wheel(
                    jointEntityToken,
                    WheelType(wheelTypeIndex + 1),
                    signalType,
                )
            )

    return (joints, wheels)


def resetSelectedJoints() -> None:
    selectedJointList.clear()
    previousWheelCheckboxState.clear()
    jointWheelIndexMap.clear()


# Transition: AARD-1685
# Find a way to not pass the global commandInputs into this function
# Perhaps get the joint tab from the args then get what we want?
def handleJointConfigTabInputChanged(
    args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
) -> None:
    commandInput = args.input

    # TODO: Reorder
    if commandInput.id == "wheelType":
        wheelTypeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
        position = wheelConfigTable.getPosition(wheelTypeDropdown)[1]
        iconInput = wheelConfigTable.getInputAtPosition(position, 0)

        if wheelTypeDropdown.selectedItem.index == 0:
            iconInput.imageFile = IconPaths.wheelIcons["standard"]
            iconInput.tooltip = "Standard wheel"
        elif wheelTypeDropdown.selectedItem.index == 1:
            iconInput.imageFile = IconPaths.wheelIcons["omni"]
            iconInput.tooltip = "Omni wheel"
        elif wheelTypeDropdown.selectedItem.index == 2:
            iconInput.imageFile = IconPaths.wheelIcons["mecanum"]
            iconInput.tooltip = "Mecanum wheel"

    elif commandInput.id == "isWheel":
        isWheelCheckbox = adsk.core.BoolValueCommandInput.cast(commandInput)
        position = jointConfigTable.getPosition(isWheelCheckbox)[1] - 1
        isAlreadyWheel = bool(jointWheelIndexMap.get(selectedJointList[position].entityToken))

        if isWheelCheckbox.value != previousWheelCheckboxState[position]:
            if not isAlreadyWheel:
                addWheelToConfigTab(selectedJointList[position])
            else:
                removeWheelFromConfigTab(selectedJointList[position])

            previousWheelCheckboxState[position] = isWheelCheckbox.value

    elif commandInput.id == "signalTypeJoint":
        signalTypeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
        position = jointConfigTable.getPosition(signalTypeDropdown)[1]  # 1 indexed
        wheelTabPosition = jointWheelIndexMap.get(selectedJointList[position - 1].entityToken)

        if wheelTabPosition:
            wheelSignalItems = wheelConfigTable.getInputAtPosition(position, 3).listItems
            wheelSignalItems.item(signalTypeDropdown.selectedItem.index).isSelected = True

    elif commandInput.id == "jointAddButton":
        jointAddButton = globalCommandInputs.itemById("jointAddButton")
        jointRemoveButton = globalCommandInputs.itemById("jointRemoveButton")
        jointSelectCancelButton = globalCommandInputs.itemById("jointSelectCancelButton")
        jointSelection = globalCommandInputs.itemById("jointSelection")

        jointSelection.isVisible = jointSelection.isEnabled = True
        jointSelection.clearSelection()
        jointAddButton.isEnabled = jointRemoveButton.isEnabled = False
        jointSelectCancelButton.isVisible = jointSelectCancelButton.isEnabled = True

    elif commandInput.id == "jointRemoveButton":
        jointAddButton = globalCommandInputs.itemById("jointAddButton")
        jointTable = args.inputs.itemById("jointTable")

        jointAddButton.isEnabled = True

        if jointTable.selectedRow == -1 or jointTable.selectedRow == 0:
            ui = adsk.core.Application.get().userInterface
            ui.messageBox("Select a row to delete.")
        else:
            # Select Row is 1 indexed
            removeIndexedJointFromConfigTab(jointTable.selectedRow - 1)

    elif commandInput.id == "jointSelectCancelButton":
        jointAddButton = globalCommandInputs.itemById("jointAddButton")
        jointRemoveButton = globalCommandInputs.itemById("jointRemoveButton")
        jointSelectCancelButton = globalCommandInputs.itemById("jointSelectCancelButton")
        jointSelection = globalCommandInputs.itemById("jointSelection")
        jointSelection.isEnabled = jointSelection.isVisible = False
        jointSelectCancelButton.isEnabled = jointSelectCancelButton.isVisible = False
        jointAddButton.isEnabled = jointRemoveButton.isEnabled = True


def handleJointConfigTabSelectionEvent(args: adsk.core.SelectionEventArgs, selectedJoint: adsk.fusion.Joint) -> None:
    selectionInput = args.activeInput
    jointType = selectedJoint.jointMotion.jointType
    if jointType == adsk.fusion.JointTypes.RevoluteJointType or jointType == adsk.fusion.JointTypes.SliderJointType:
        if not addJointToConfigTab(selectedJoint):
            ui = adsk.core.Application.get().userInterface
            result = ui.messageBox(
                "You have already selected this joint.\n" "Would you like to remove it?",
                "Synthesis: Remove Joint Confirmation",
                adsk.core.MessageBoxButtonTypes.YesNoButtonType,
                adsk.core.MessageBoxIconTypes.QuestionIconType,
            )

            if result == adsk.core.DialogResults.DialogYes:
                removeJointFromConfigTab(selectedJoint)

        selectionInput.isEnabled = selectionInput.isVisible = False


def handleJointConfigTabPreviewEvent(args: adsk.core.CommandEventArgs) -> None:
    jointAddButton = args.command.commandInputs.itemById("jointAddButton")
    jointRemoveButton = args.command.commandInputs.itemById("jointRemoveButton")
    jointSelectCancelButton = args.command.commandInputs.itemById("jointSelectCancelButton")
    jointSelection = args.command.commandInputs.itemById("jointSelection")

    if jointConfigTable.rowCount <= 1:
        jointRemoveButton.isEnabled = False

    if not jointSelection.isEnabled:
        jointAddButton.isEnabled = jointRemoveButton.isEnabled = True
        jointSelectCancelButton.isVisible = jointSelectCancelButton.isEnabled = False
