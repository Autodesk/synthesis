import logging
import traceback

import adsk.core
import adsk.fusion

from . import IconPaths
from .CreateCommandInputsHelper import createTableInput, createTextBoxInput

from ..Parser.ExporterOptions import JointParentType, Joint, SignalType

# Wish we did not need this. Could look into storing everything within the design every time - Brandon
selectedJointList: list[adsk.fusion.Joint] = []
jointConfigTable: adsk.core.TableCommandInput


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
            6,
            "1:2:2:2:2:2",
            50,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "motionHeader",
                "Motion",
                jointConfigTabInputs,
                "Motion",
                bold=False,
            ),
            0,
            0,
        )

        jointConfigTable.addCommandInput(
            createTextBoxInput(
                "nameHeader", "Name", jointConfigTabInputs, "Joint name", bold=False
            ),
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

        jointSelect = jointConfigTabInputs.addSelectionInput(
            "jointSelection", "Selection", "Select a joint in your assembly to add."
        )
        jointSelect.addSelectionFilter("Joints")
        jointSelect.setSelectionLimits(0)

        # Visibility is triggered by `addJointInputButton`
        jointSelect.isEnabled = jointSelect.isVisible = False

        addJointInputButton = jointConfigTabInputs.addBoolValueInput("jointAddButton", "Add", False)
        removeJointInputButton = jointConfigTabInputs.addBoolValueInput("jointRemoveButton", "Remove", False)
        addJointInputButton.isEnabled = removeJointInputButton.isEnabled = True

        jointConfigTable.addToolbarCommandInput(addJointInputButton)
        jointConfigTable.addToolbarCommandInput(removeJointInputButton)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.createJointConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def addJointToConfigTab(fusionJoint: adsk.fusion.Joint, synJoint: Joint = None) -> None:
    try:
        if fusionJoint in selectedJointList:
            removeJointFromConfigTab(fusionJoint)
            return

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

        for row in range(1, jointConfigTable.rowCount): # Row is 1 indexed
            dropDown = jointConfigTable.getInputAtPosition(row, 2)
            dropDown.listItems.add(selectedJointList[-1].name, False)

        for fusionJoint in selectedJointList:
            jointType.listItems.add(fusionJoint.name, False)

        jointType.tooltip = "Possible parent joints"
        jointType.tooltipDescription = "<hr>The root component is usually the parent.</hr>"

        signalType = commandInputs.addDropDownCommandInput(
            "signalType",
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

        # Joint speed must be added within an `if` because there is variance between different joint types
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
            jointForceValue = synJoint.force * 100 # Currently a factor of 100 - Should be investigated
        else:
            jointForceValue = 5

        jointForce = commandInputs.addValueInput("jointForce", "Force", "N", adsk.core.ValueInput.createByReal(jointForceValue))
        jointForce.tooltip = "Newtons"
        jointConfigTable.addCommandInput(jointForce, row, 5)
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.addJointToConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeIndexedJointFromConfigTab(index: int) -> None:
    try:
        removeJointFromConfigTab(selectedJointList[index])
    except:
        logging.getLogger("{INTERNAL_ID}.UI.JointConfigTab.removeIndexedJointFromConfigTab()").error(
            "Failed:\n{}".format(traceback.format_exc())
        )


def removeJointFromConfigTab(joint: adsk.fusion.Joint) -> None:
    try:
        i = selectedJointList.index(joint)
        selectedJointList.remove(joint)

        jointConfigTable.deleteRow(i + 1)
        for row in range(jointConfigTable.rowCount): # Row is 1 indexed
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


# Converts the current list of selected adsk.fusion.joints into list[Synthesis.Joint]
def getSelectedJoints() -> list[Joint]:
    joints: list[Joint] = []
    for row in range(1, jointConfigTable.rowCount): # Row is 1 indexed
        signalTypeIndex = jointConfigTable.getInputAtPosition(row, 3).selectedItem.index
        jointSpeed = jointConfigTable.getInputAtPosition(row, 4).value
        jointForce = jointConfigTable.getInputAtPosition(row, 5).value

        joints.append(
            Joint(
                selectedJointList[row - 1].entityToken, # Row is 1 indexed
                JointParentType.ROOT,
                SignalType(signalTypeIndex + 1),
                jointSpeed,
                jointForce / 100.0,
            )
        )

    return joints


def resetSelectedJoints() -> None:
    selectedJointList.clear()
