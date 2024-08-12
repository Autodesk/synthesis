import adsk.core
import adsk.fusion

from src.Logging import logFailure
from src.Types import Joint, JointParentType, SignalType, Wheel, WheelType
from src.UI import IconPaths
from src.UI.CreateCommandInputsHelper import (
    createBooleanInput,
    createTableInput,
    createTextBoxInput,
)


class JointConfigTab:
    selectedJointList: list[adsk.fusion.Joint] = []
    previousWheelCheckboxState: list[bool] = []
    jointWheelIndexMap: dict[str, int] = {}
    jointConfigTab: adsk.core.TabCommandInput
    jointConfigTable: adsk.core.TableCommandInput
    wheelConfigTable: adsk.core.TableCommandInput

    @logFailure
    def __init__(self, args: adsk.core.CommandCreatedEventArgs) -> None:
        inputs = args.command.commandInputs
        self.jointConfigTab = inputs.addTabCommandInput("jointSettings", "Joint Settings")
        self.jointConfigTab.tooltip = "Select and configure robot joints."
        jointConfigTabInputs = self.jointConfigTab.children
        self.jointConfigTable = createTableInput("jointTable", "Joint Table", jointConfigTabInputs, 7, "1:2:2:2:2:2:2")
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("jointMotionHeader", "Motion", jointConfigTabInputs, "Motion", bold=False), 0, 0
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("nameHeader", "Name", jointConfigTabInputs, "Joint name", bold=False), 0, 1
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("parentHeader", "Parent", jointConfigTabInputs, "Parent joint", background="#d9d9d9"),
            0,
            2,
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("signalHeader", "Signal", jointConfigTabInputs, "Signal type", background="#d9d9d9"),
            0,
            3,
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("speedHeader", "Speed", jointConfigTabInputs, "Joint Speed", background="#d9d9d9"),
            0,
            4,
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("forceHeader", "Force", jointConfigTabInputs, "Joint Force", background="#d9d9d9"),
            0,
            5,
        )
        self.jointConfigTable.addCommandInput(
            createTextBoxInput("wheelHeader", "Is Wheel", jointConfigTabInputs, "Is Wheel", background="#d9d9d9"),
            0,
            6,
        )

        jointSelect = jointConfigTabInputs.addSelectionInput(
            "jointSelection", "Selection", "Select a joint in your assembly to add."
        )
        jointSelect.addSelectionFilter("Joints")
        jointSelect.setSelectionLimits(0)
        jointSelect.isEnabled = jointSelect.isVisible = False  # Visibility is triggered by `addJointInputButton`

        jointConfigTabInputs.addTextBoxCommandInput("jointTabBlankSpacer", "", "", 1, True)

        self.wheelConfigTable = createTableInput("wheelTable", "Wheel Table", jointConfigTabInputs, 4, "1:2:2:2")
        self.wheelConfigTable.addCommandInput(
            createTextBoxInput("wheelMotionHeader", "Motion", jointConfigTabInputs, "Motion", bold=False), 0, 0
        )
        self.wheelConfigTable.addCommandInput(
            createTextBoxInput("name_header", "Name", jointConfigTabInputs, "Joint name", bold=False), 0, 1
        )
        self.wheelConfigTable.addCommandInput(
            createTextBoxInput(
                "wheelTypeHeader", "WheelType", jointConfigTabInputs, "Wheel type", background="#d9d9d9"
            ),
            0,
            2,
        )
        self.wheelConfigTable.addCommandInput(
            createTextBoxInput(
                "signalTypeHeader", "SignalType", jointConfigTabInputs, "Signal type", background="#d9d9d9"
            ),
            0,
            3,
        )

        jointSelectCancelButton = jointConfigTabInputs.addBoolValueInput("jointSelectCancelButton", "Cancel", False)
        jointSelectCancelButton.isEnabled = jointSelectCancelButton.isVisible = False

        addJointInputButton = jointConfigTabInputs.addBoolValueInput("jointAddButton", "Add", False)
        removeJointInputButton = jointConfigTabInputs.addBoolValueInput("jointRemoveButton", "Remove", False)
        addJointInputButton.isEnabled = removeJointInputButton.isEnabled = True

        self.jointConfigTable.addToolbarCommandInput(addJointInputButton)
        self.jointConfigTable.addToolbarCommandInput(removeJointInputButton)
        self.jointConfigTable.addToolbarCommandInput(jointSelectCancelButton)

        self.reset()

    @property
    def isVisible(self) -> bool:
        return self.jointConfigTab.isVisible

    @isVisible.setter
    def isVisible(self, value: bool) -> None:
        self.jointConfigTab.isVisible = value

    @logFailure
    def addJoint(self, fusionJoint: adsk.fusion.Joint, synJoint: Joint | None = None) -> bool:
        if fusionJoint in self.selectedJointList:
            return False

        self.selectedJointList.append(fusionJoint)
        commandInputs = self.jointConfigTable.commandInputs

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

        for row in range(1, self.jointConfigTable.rowCount):  # Row is 1 indexed
            dropDown = self.jointConfigTable.getInputAtPosition(row, 2)
            dropDown.listItems.add(self.selectedJointList[-1].name, False)

        for fusionJoint in self.selectedJointList:
            jointType.listItems.add(fusionJoint.name, False)

        jointType.tooltip = "Possible parent joints"
        jointType.tooltipDescription = "<hr>The root component is usually the parent.</hr>"

        signalType = commandInputs.addDropDownCommandInput(
            "signalTypeJoint",
            "Signal Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )

        # Invisible white space characters are required in the list item name field to make this work.
        # I have no idea why, Fusion API needs some special education help - Brandon
        if synJoint:
            signalType.listItems.add("‎", synJoint.signalType is SignalType.PWM, IconPaths.signalIcons["PWM"])
            signalType.listItems.add("‎", synJoint.signalType is SignalType.CAN, IconPaths.signalIcons["CAN"])
            signalType.listItems.add("‎", synJoint.signalType is SignalType.PASSIVE, IconPaths.signalIcons["PASSIVE"])
        else:
            signalType.listItems.add("‎", True, IconPaths.signalIcons["PWM"])
            signalType.listItems.add("‎", False, IconPaths.signalIcons["CAN"])
            signalType.listItems.add("‎", False, IconPaths.signalIcons["PASSIVE"])

        signalType.tooltip = "Signal type"

        row = self.jointConfigTable.rowCount
        self.jointConfigTable.addCommandInput(icon, row, 0)
        self.jointConfigTable.addCommandInput(name, row, 1)
        self.jointConfigTable.addCommandInput(jointType, row, 2)
        self.jointConfigTable.addCommandInput(signalType, row, 3)

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
            self.jointConfigTable.addCommandInput(jointSpeed, row, 4)

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
            self.jointConfigTable.addCommandInput(jointSpeed, row, 4)

        if synJoint:
            jointForceValue = synJoint.force * 100  # Currently a factor of 100 - Should be investigated
        else:
            jointForceValue = 5

        jointForce = commandInputs.addValueInput(
            "jointForce", "Force", "N", adsk.core.ValueInput.createByReal(jointForceValue)
        )
        jointForce.tooltip = "Newtons"
        self.jointConfigTable.addCommandInput(jointForce, row, 5)

        if fusionJoint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
            wheelCheckboxEnabled = True
            wheelCheckboxTooltip = "Determines if this joint should be counted as a wheel."
        else:
            wheelCheckboxEnabled = False
            wheelCheckboxTooltip = "Only Revolute joints can be treated as wheels."

        isWheel = synJoint.isWheel if synJoint else False

        # Transition: AARD-1685
        # All command inputs should be created using the helpers.
        self.jointConfigTable.addCommandInput(
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

        self.previousWheelCheckboxState.append(isWheel)

    @logFailure
    def addWheel(self, joint: adsk.fusion.Joint, wheel: Wheel | None = None) -> None:
        self.jointWheelIndexMap[joint.entityToken] = self.wheelConfigTable.rowCount

        commandInputs = self.wheelConfigTable.commandInputs
        wheelIcon = commandInputs.addImageCommandInput(
            "wheelPlaceholder", "Placeholder", IconPaths.wheelIcons["standard"]
        )
        wheelIcon.tooltip = "Standard wheel"
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
        i = self.selectedJointList.index(joint)
        jointSignalType = SignalType(self.jointConfigTable.getInputAtPosition(i + 1, 3).selectedItem.index + 1)

        # Invisible white space characters are required in the list item name field to make this work.
        # I have no idea why, Fusion API needs some special education help - Brandon
        signalType.listItems.add("‎", jointSignalType is SignalType.PWM, IconPaths.signalIcons["PWM"])
        signalType.listItems.add("‎", jointSignalType is SignalType.CAN, IconPaths.signalIcons["CAN"])
        signalType.listItems.add("‎", jointSignalType is SignalType.PASSIVE, IconPaths.signalIcons["PASSIVE"])

        row = self.wheelConfigTable.rowCount
        self.wheelConfigTable.addCommandInput(wheelIcon, row, 0)
        self.wheelConfigTable.addCommandInput(wheelName, row, 1)
        self.wheelConfigTable.addCommandInput(wheelType, row, 2)
        self.wheelConfigTable.addCommandInput(signalType, row, 3)

    @logFailure
    def removeIndexedJoint(self, index: int) -> None:
        self.removeJoint(self.selectedJointList[index])

    @logFailure
    def removeJoint(self, joint: adsk.fusion.Joint) -> None:
        if self.jointWheelIndexMap.get(joint.entityToken):
            self.removeWheel(joint)

        i = self.selectedJointList.index(joint)
        self.selectedJointList.remove(joint)
        self.previousWheelCheckboxState.pop(i)
        self.jointConfigTable.deleteRow(i + 1)
        for row in range(1, self.jointConfigTable.rowCount):  # Row is 1 indexed
            # TODO: Step through this in the debugger and figure out if this is all necessary.
            listItems = self.jointConfigTable.getInputAtPosition(row, 2).listItems
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

    @logFailure
    def removeWheel(self, joint: adsk.fusion.Joint) -> None:
        row = self.jointWheelIndexMap[joint.entityToken]
        self.wheelConfigTable.deleteRow(row)
        del self.jointWheelIndexMap[joint.entityToken]
        for key, value in self.jointWheelIndexMap.items():
            if value > row - 1:
                self.jointWheelIndexMap[key] -= 1

    @logFailure
    def getSelectedJointsAndWheels(self) -> tuple[list[Joint], list[Wheel]]:
        joints: list[Joint] = []
        wheels: list[Wheel] = []
        for row in range(1, self.jointConfigTable.rowCount):  # Row is 1 indexed
            jointEntityToken = self.selectedJointList[row - 1].entityToken
            signalTypeIndex = self.jointConfigTable.getInputAtPosition(row, 3).selectedItem.index
            signalType = SignalType(signalTypeIndex + 1)
            jointSpeed: float = self.jointConfigTable.getInputAtPosition(row, 4).value
            jointForce: float = self.jointConfigTable.getInputAtPosition(row, 5).value
            isWheel: bool = self.jointConfigTable.getInputAtPosition(row, 6).value

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
                wheelRow = self.jointWheelIndexMap[jointEntityToken]
                wheelTypeIndex = self.wheelConfigTable.getInputAtPosition(wheelRow, 2).selectedItem.index
                wheels.append(
                    Wheel(
                        jointEntityToken,
                        WheelType(wheelTypeIndex + 1),
                        signalType,
                    )
                )

        return (joints, wheels)

    def reset(self) -> None:
        self.selectedJointList.clear()
        self.previousWheelCheckboxState.clear()
        self.jointWheelIndexMap.clear()

    # Transition: AARD-1685
    # Find a way to not pass the global commandInputs into this function
    # Perhaps get the joint tab from the args then get what we want?
    # Idk the Fusion API seems to think that you would never need to change anything other than the effected
    # commandInput in a input changed handle for some reason.
    @logFailure
    def handleInputChanged(
        self, args: adsk.core.InputChangedEventArgs, globalCommandInputs: adsk.core.CommandInputs
    ) -> None:
        commandInput = args.input
        if commandInput.id == "wheelType":
            wheelTypeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
            position = self.wheelConfigTable.getPosition(wheelTypeDropdown)[1]
            iconInput: adsk.core.ImageCommandInput = self.wheelConfigTable.getInputAtPosition(position, 0)

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
            position = self.jointConfigTable.getPosition(isWheelCheckbox)[1] - 1
            isAlreadyWheel = bool(self.jointWheelIndexMap.get(self.selectedJointList[position].entityToken))

            if isWheelCheckbox.value != self.previousWheelCheckboxState[position]:
                if not isAlreadyWheel:
                    self.addWheel(self.selectedJointList[position])
                else:
                    self.removeWheel(self.selectedJointList[position])

                self.previousWheelCheckboxState[position] = isWheelCheckbox.value

        elif commandInput.id == "signalTypeJoint":
            signalTypeDropdown = adsk.core.DropDownCommandInput.cast(commandInput)
            jointTabPosition = self.jointConfigTable.getPosition(signalTypeDropdown)[1]  # 1 indexed
            wheelTabPosition = self.jointWheelIndexMap.get(self.selectedJointList[jointTabPosition - 1].entityToken)

            if wheelTabPosition:
                wheelSignalItems: adsk.core.DropDownCommandInput = self.wheelConfigTable.getInputAtPosition(
                    wheelTabPosition, 3
                )
                wheelSignalItems.listItems.item(signalTypeDropdown.selectedItem.index).isSelected = True

        elif commandInput.id == "jointAddButton":
            jointAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("jointAddButton")
            jointRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("jointRemoveButton")
            jointSelectCancelButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "jointSelectCancelButton"
            )
            jointSelection: adsk.core.SelectionCommandInput = globalCommandInputs.itemById("jointSelection")

            jointSelection.isVisible = jointSelection.isEnabled = True
            jointSelection.clearSelection()
            jointAddButton.isEnabled = jointRemoveButton.isEnabled = False
            jointSelectCancelButton.isVisible = jointSelectCancelButton.isEnabled = True

        elif commandInput.id == "jointRemoveButton":
            jointAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("jointAddButton")
            jointTable: adsk.core.TableCommandInput = args.inputs.itemById("jointTable")

            jointAddButton.isEnabled = True

            if jointTable.selectedRow == -1 or jointTable.selectedRow == 0:
                ui = adsk.core.Application.get().userInterface
                ui.messageBox("Select a row to delete.")
            else:
                self.removeIndexedJoint(jointTable.selectedRow - 1)  # selectedRow is 1 indexed

        elif commandInput.id == "jointSelectCancelButton":
            jointAddButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("jointAddButton")
            jointRemoveButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById("jointRemoveButton")
            jointSelectCancelButton: adsk.core.BoolValueCommandInput = globalCommandInputs.itemById(
                "jointSelectCancelButton"
            )
            jointSelection: adsk.core.SelectionCommandInput = globalCommandInputs.itemById("jointSelection")
            jointSelection.isEnabled = jointSelection.isVisible = False
            jointSelectCancelButton.isEnabled = jointSelectCancelButton.isVisible = False
            jointAddButton.isEnabled = jointRemoveButton.isEnabled = True

    @logFailure
    def handleSelectionEvent(self, args: adsk.core.SelectionEventArgs, selectedJoint: adsk.fusion.Joint) -> None:
        selectionInput = args.activeInput
        jointType = selectedJoint.jointMotion.jointType
        if jointType == adsk.fusion.JointTypes.RevoluteJointType or jointType == adsk.fusion.JointTypes.SliderJointType:
            if not self.addJoint(selectedJoint):
                ui = adsk.core.Application.get().userInterface
                result = ui.messageBox(
                    "You have already selected this joint.\n" "Would you like to remove it?",
                    "Synthesis: Remove Joint Confirmation",
                    adsk.core.MessageBoxButtonTypes.YesNoButtonType,
                    adsk.core.MessageBoxIconTypes.QuestionIconType,
                )

                if result == adsk.core.DialogResults.DialogYes:
                    self.removeJoint(selectedJoint)

            selectionInput.isEnabled = selectionInput.isVisible = False

    @logFailure
    def handlePreviewEvent(self, args: adsk.core.CommandEventArgs) -> None:
        commandInputs = args.command.commandInputs
        jointAddButton: adsk.core.BoolValueCommandInput = commandInputs.itemById("jointAddButton")
        jointRemoveButton: adsk.core.BoolValueCommandInput = commandInputs.itemById("jointRemoveButton")
        jointSelectCancelButton: adsk.core.BoolValueCommandInput = commandInputs.itemById("jointSelectCancelButton")
        jointSelection: adsk.core.SelectionCommandInput = commandInputs.itemById("jointSelection")

        jointRemoveButton.isEnabled = self.jointConfigTable.rowCount > 1
        if not jointSelection.isEnabled:
            jointAddButton.isEnabled = True
            jointSelectCancelButton.isVisible = jointSelectCancelButton.isEnabled = False
