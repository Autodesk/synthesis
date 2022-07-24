from enum import Enum
from ..general_imports import *
from ..configure import NOTIFIED, write_configuration
from ..Analytics.alert import showAnalyticsAlert
from . import Helper, FileDialogConfig, OsHelper, CustomGraphics, IconPaths
from ..Parser.ParseOptions import (
    Gamepiece,
    Mode,
    ParseOptions,
    _Joint,
    _Wheel,
    JointParentType,
)
from .Configuration.SerialCommand import SerialCommand

import adsk.core, adsk.fusion, traceback, logging, os
from types import SimpleNamespace

class ConfigureCommandExecuteHandler(adsk.core.CommandEventHandler):
    """### Called when Ok is pressed confirming the export to Unity.

    Process Steps:

        1. Check for process open in explorer

        1.5. Open file dialog to allow file location save
            - Not always optimal if sending over socket for parse

        2. Check Socket bind

        3. Check Socket recv
            - if true send data about file location in temp path

        4. Parse file and focus on unity window

    """

    def __init__(self, previous, fp):
        super().__init__()
        self.log = logging.getLogger(f"{INTERNAL_ID}.UI.{self.__class__.__name__}")
        self.previous = previous
        self.current = SerialCommand()
        self.fp = fp

    def notify(self, args):
        try:
            eventArgs = adsk.core.CommandEventArgs.cast(args)

            if eventArgs.executeFailed:
                self.log.error("Could not execute configuration due to failure")
                return

            mode_dropdown = eventArgs.command.commandInputs.itemById(
                "general_settings"
            ).children.itemById("mode")

            mode_dropdown = adsk.core.DropDownCommandInput.cast(mode_dropdown)
            mode = 5

            if mode_dropdown.selectedItem.name == "Synthesis Exporter":
                mode = 5

            # defaultPath = self.fp
            # defaultPath = os.getenv()

            if mode == 5:
                savepath = FileDialogConfig.SaveFileDialog(
                    defaultPath=self.fp, ext="Synthesis File (*.synth)"
                )
            else:
                savepath = FileDialogConfig.SaveFileDialog(defaultPath=self.fp)

            if savepath == False:
                # save was canceled
                return
            else:
                updatedPath = pathlib.Path(savepath).parent
                if updatedPath != self.current.filePath:
                    self.current.filePath = str(updatedPath)
                    Helper.writeConfigure(self.current.toJSON())

                adsk.doEvents()
                # get active document
                design = gm.app.activeDocument.design
                name = design.rootComponent.name.rsplit(" ", 1)[0]
                version = design.rootComponent.name.rsplit(" ", 1)[1]

                renderer = 0

                _exportWheels = []  # all selected wheels, formatted for parseOptions
                _exportJoints = []  # all selected joints, formatted for parseOptions
                _exportGamepieces = []  # TODO work on the code to populate Gamepiece
                _robotWeight = float
                _mode = Mode

                """
                Loops through all rows in the wheel table to extract all the input values
                """
                onSelect = gm.handlers[3]
                wheelTableInput = wheelTable()
                for row in range(wheelTableInput.rowCount):
                    if row == 0:
                        continue

                    wheelTypeIndex = wheelTableInput.getInputAtPosition(
                        row, 2
                    ).selectedItem.index  # This must be either 0 or 1 for standard or omni

                    signalTypeIndex = wheelTableInput.getInputAtPosition(
                        row, 3
                    ).selectedItem.index

                    _exportWheels.append(
                        _Wheel(
                            WheelListGlobal[row - 1].entityToken,
                            wheelTypeIndex,
                            signalTypeIndex,
                            # onSelect.wheelJointList[row-1][0] # GUID of wheel joint. if no joint found, default to None
                        )
                    )

                """
                Loops through all rows in the joint table to extract the input values
                """
                jointTableInput = jointTable()
                for row in range(jointTableInput.rowCount):
                    if row == 0:
                        continue

                    parentJointIndex = jointTableInput.getInputAtPosition(
                        row, 2
                    ).selectedItem.index  # parent joint index, int

                    signalTypeIndex = jointTableInput.getInputAtPosition(
                        row, 3
                    ).selectedItem.index  # signal type index, int

                    jointSpeed = jointTableInput.getInputAtPosition(
                        row, 4
                    ).value

                    jointForce = jointTableInput.getInputAtPosition(
                        row, 5
                    ).value

                    parentJointToken = ""

                    if parentJointIndex == 0:
                        _exportJoints.append(
                            _Joint(
                                JointListGlobal[row - 1].entityToken,
                                JointParentType.ROOT,
                                signalTypeIndex,  # index of selected signal in dropdown
                                jointSpeed,
                                jointForce / 100.0
                            )  # parent joint GUID
                        )
                        continue
                    elif parentJointIndex < row:
                        parentJointToken = JointListGlobal[
                            parentJointIndex - 1
                            ].entityToken  # parent joint GUID, str
                    else:
                        parentJointToken = JointListGlobal[
                            parentJointIndex + 1
                            ].entityToken  # parent joint GUID, str

                    # for wheel in _exportWheels:
                    # find some way to get joint
                    # 1. Compare Joint occurrence1 to wheel.occurrence_token
                    # 2. if true set the parent to Root

                    _exportJoints.append(
                        _Joint(
                            JointListGlobal[row - 1].entityToken, parentJointToken, signalTypeIndex, jointSpeed,
                            jointForce
                        )
                    )

                """
                Loops through all rows in the gamepiece table to extract the input values
                """
                gamepieceTableInput = gamepieceTable()
                weight_unit_f = INPUTS_ROOT.itemById("weight_unit_f")
                for row in range(gamepieceTableInput.rowCount):
                    if row == 0:
                        continue

                    weightValue = gamepieceTableInput.getInputAtPosition(
                        row, 2
                    ).value  # weight/mass input, float

                    if weight_unit_f.selectedItem.index == 0:
                        weightValue /= 2.2046226218

                    frictionValue = gamepieceTableInput.getInputAtPosition(
                        row, 3
                    ).valueOne  # friction value, float

                    _exportGamepieces.append(
                        Gamepiece(GamepieceListGlobal[row - 1].entityToken, weightValue, frictionValue)
                    )

                """
                Robot Weight
                """
                weight_input = INPUTS_ROOT.itemById("weight_input")
                weight_unit = INPUTS_ROOT.itemById("weight_unit")

                if weight_unit.selectedItem.index == 0:
                    _robotWeight = float(weight_input.value) / 2.2046226218
                else:
                    _robotWeight = float(weight_input.value)

                """
                Export Mode
                """
                dropdownExportMode = INPUTS_ROOT.itemById("mode")
                if dropdownExportMode.selectedItem.index == 0:
                    _mode = Mode.Synthesis
                elif dropdownExportMode.selectedItem.index == 1:
                    _mode = Mode.SynthesisField

                global compress

                options = ParseOptions(
                    savepath,
                    name,
                    version,
                    materials=renderer,
                    joints=_exportJoints,
                    wheels=_exportWheels,
                    gamepieces=_exportGamepieces,
                    weight=_robotWeight,
                    mode=_mode,
                    compress=compress
                )

                if options.parse(False):
                    # success
                    return
                else:
                    self.log.error(
                        f"Error: \n\t{name} could not be written to \n {savepath}"
                    )
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

                #    logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
        #    "Failed:\n{}".format(traceback.format_exc())
        # )


class CommandExecutePreviewHandler(adsk.core.CommandEventHandler):
    """### Gets an event that is fired when the command has completed gathering the required input and now needs to perform a preview.

    Args:
        adsk (CommandEventHandler): Command event handler that a client derives from to handle events triggered by a CommandEvent.
    """

    def __init__(self, cmd) -> None:
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        """Notify member called when a command event is triggered

        Args:
            args (CommandEventArgs): command event argument
        """
        try:
            eventArgs = adsk.core.CommandEventArgs.cast(args)
            # inputs = eventArgs.command.commandInputs # equivalent to INPUTS_ROOT global

            auto_calc_weight_f = INPUTS_ROOT.itemById("auto_calc_weight_f")

            removeWheelInput = INPUTS_ROOT.itemById("wheel_delete")
            removeJointInput = INPUTS_ROOT.itemById("joint_delete")
            removeFieldInput = INPUTS_ROOT.itemById("field_delete")

            addWheelInput = INPUTS_ROOT.itemById("wheel_add")
            addJointInput = INPUTS_ROOT.itemById("joint_add")
            addFieldInput = INPUTS_ROOT.itemById("field_add")

            wheelTableInput = wheelTable()
            jointTableInput = jointTable()
            gamepieceTableInput = gamepieceTable()

            if wheelTableInput.rowCount <= 1:
                removeWheelInput.isEnabled = False
            else:
                removeWheelInput.isEnabled = True

            if jointTableInput.rowCount <= 1:
                removeJointInput.isEnabled = False
            else:
                removeJointInput.isEnabled = True

            if gamepieceTableInput.rowCount <= 1:
                removeFieldInput.isEnabled = \
                    auto_calc_weight_f.isEnabled = False
            else:
                removeFieldInput.isEnabled = \
                    auto_calc_weight_f.isEnabled = True

            if not addWheelInput.isEnabled or not removeWheelInput:
                # for wheel in WheelListGlobal:
                #     wheel.component.opacity = 0.25
                #     CustomGraphics.createTextGraphics(wheel, WheelListGlobal)

                gm.app.activeViewport.refresh()
            else:
                gm.app.activeDocument.design.rootComponent.opacity = 1
                for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
                    group.deleteMe()

            if not addJointInput.isEnabled or not removeJointInput:  # TODO: improve joint highlighting
                # for joint in JointListGlobal:
                #    CustomGraphics.highlightJointedOccurrences(joint)

                # gm.app.activeViewport.refresh()
                gm.app.activeDocument.design.rootComponent.opacity = 0.15
            # else:
            # for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
            #    group.deleteMe()
            # gm.app.activeDocument.design.rootComponent.opacity = 1

            if not addFieldInput.isEnabled or not removeFieldInput:
                for gamepiece in GamepieceListGlobal:
                    gamepiece.component.opacity = 0.25
                    CustomGraphics.createTextGraphics(gamepiece, GamepieceListGlobal)
            else:
                gm.app.activeDocument.design.rootComponent.opacity = 1
        except AttributeError:
            pass
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class MySelectHandler(adsk.core.SelectionEventHandler):
    """### Event fires when the user selects an entity.
    ##### This is different from a preselection where an entity is shown as being available for selection as the mouse passes over the entity. This is the actual selection where the user has clicked the mouse on the entity.

    Args: SelectionEventHandler
    """
    lastInputCmd = None

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

        self.allWheelPreselections = []  # all child occurrences of selections
        self.allGamepiecePreselections = []  # all child gamepiece occurrences of selections

        self.selectedOcc = None  # selected occurrence (if there is one)
        self.selectedJoint = None  # selected joint (if there is one)

        self.wheelJointList = []
        self.algorithmicSelection = True

    def traverseAssembly(self, child_occurrences: adsk.fusion.OccurrenceList,
                         jointedOcc: dict):  # recursive traversal to check if children are jointed
        """### Traverses the entire occurrence hierarchy to find a match (jointed occurrence) in self.occurrence

        Args:
            child_occurrences (adsk.fusion.OccurrenceList): a list of child occurrences

        Returns:
            occ (Occurrence): if a match is found, return the jointed occurrence
            None: if no match is found
        """
        try:
            for occ in child_occurrences:
                for joint, value in jointedOcc.items():
                    if occ in value:
                        return [joint, occ]  # occurrence that is jointed

                if occ.childOccurrences:  # if occurrence has children, traverse sub-tree
                    self.traverseAssembly(occ.childOccurrences, jointedOcc)
            return None  # no jointed occurrence found
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.traverseAssembly()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    def wheelParent(self, occ: adsk.fusion.Occurrence):
        """### Identify an occurrence that encompasses the entire wheel component.

        Process:

            1. if the selection has no parent, return the selection.

            2. if the selection is directly jointed, return the selection.

            3. else keep climbing the occurrence tree until no parent is found.

            - if a jointed occurrence is in the tree, return the selection parent.

            - if no jointed occurrence was found, return the selection.

        Args:
            occ (Occurrence): The selected child occurrence

        Returns:
            occ (Occurrence): Wheel parent
        """
        try:
            parent = occ.assemblyContext
            jointedOcc = {}  # dictionary with all jointed occurrences

            try:
                for joint in occ.joints:
                    if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]
            except:
                for joint in occ.component.joints:
                    if joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
                        # gm.ui.messageBox("Selection is directly jointed.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> " + joint.name)
                        return [joint.entityToken, occ]

            if parent == None:  # no parent occurrence
                # gm.ui.messageBox("Selection has no parent occurrence.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
                return [None, occ]  # return what is selected

            for joint in gm.app.activeDocument.design.rootComponent.allJoints:
                if joint.jointMotion.jointType != adsk.fusion.JointTypes.RevoluteJointType:
                    continue
                jointedOcc[joint.entityToken] = [joint.occurrenceOne, joint.occurrenceTwo]

            parentLevel = 1  # the number of nodes above the one selected
            returned = None  # the returned value of traverseAssembly()
            parentOccurrence = occ  # the parent occurrence that will be returned
            treeParent = parent  # each parent that will traverse up in algorithm.

            while treeParent != None:  # loops until reaches top-level component
                returned = self.traverseAssembly(treeParent.childOccurrences, jointedOcc)

                if returned != None:
                    for i in range(parentLevel):
                        parentOccurrence = parentOccurrence.assemblyContext

                    # gm.ui.messageBox("Joint found.\nReturning parent occurrence.\n\n" + "Selected occurrence:\n--> " + occ.name + "\nParent:\n--> " + parentOccurrence.name + "\nJoint:\n--> " + returned[0] + "\nNodes above selection:\n--> " + str(parentLevel))
                    return [returned[0], parentOccurrence]

                parentLevel += 1
                treeParent = treeParent.assemblyContext
            # gm.ui.messageBox("No jointed occurrence found.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
            return [None, occ]  # no jointed occurrence found, return what is selected
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.wheelParent()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )
            # gm.ui.messageBox("Selection's component has no referenced joints.\nReturning selection.\n\n" + "Occurrence:\n--> " + occ.name + "\nJoint:\n--> NONE")
            return [None, occ]

    def notify(self, args: adsk.core.SelectionEventArgs):
        """### Notify member is called when a selection event is triggered.

        Args:
            args (SelectionEventArgs): A selection event argument
        """
        try:
            # eventArgs = adsk.core.SelectionEventArgs.cast(args)

            self.selectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)
            self.selectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            duplicateSelection = INPUTS_ROOT.itemById("duplicate_selection")
            # indicator = INPUTS_ROOT.itemById("algorithmic_indicator")

            if self.selectedOcc:
                if dropdownExportMode.selectedItem.index == 1:
                    occurrenceList = (
                        gm.app.activeDocument.design.rootComponent.allOccurrencesByComponent(
                            self.selectedOcc.component
                        )
                    )
                    for occ in occurrenceList:
                        if occ not in GamepieceListGlobal:
                            addGamepieceToTable(occ)
                        else:
                            removeGamePieceFromTable(GamepieceListGlobal.index(occ))

            elif self.selectedJoint:
                jointType = self.selectedJoint.jointMotion.jointType
                if (
                        jointType == JointMotions.REVOLUTE.value
                        or jointType == JointMotions.SLIDER.value
                ):
                    if (jointType == JointMotions.REVOLUTE.value and MySelectHandler.lastInputCmd.id == "wheel_add"):
                        addWheelToTable(self.selectedJoint)
                    elif (
                            jointType == JointMotions.REVOLUTE.value and MySelectHandler.lastInputCmd.id == "wheel_remove"):
                        if self.selectedJoint in WheelListGlobal:
                            removeWheelFromTable(WheelListGlobal.index(self.selectedJoint))
                    else:
                        if self.selectedJoint not in JointListGlobal:
                            addJointToTable(self.selectedJoint)
                        else:
                            removeJointFromTable(self.selectedJoint)
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class MyPreSelectHandler(adsk.core.SelectionEventHandler):
    """### Event fires when a entity preselection is made (mouse hovering).
    ##### When a user is selecting geometry, they move the mouse over the model and if the entity the mouse is currently over is valid for selection it will highlight indicating that it can be selected. This process of determining what is available for selection and highlighting it is referred to as the "preselect" behavior.

    Args: SelectionEventHandler
    """

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        try:
            design = adsk.fusion.Design.cast(gm.app.activeProduct)
            preSelectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)

            onSelect = gm.handlers[3]  # select handler

            if not preSelectedOcc or not design:
                self.cmd.setCursor("", 0, 0)
                return

            dropdownExportMode = INPUTS_ROOT.itemById("mode")
            if preSelectedOcc and design:
                if dropdownExportMode.selectedItem.index == 0:
                    if preSelectedOcc.entityToken in onSelect.allWheelPreselections:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["remove"],
                            0,
                            0,
                        )
                    else:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["add"],
                            0,
                            0,
                        )

                elif dropdownExportMode.selectedItem.index == 1:
                    if preSelectedOcc.entityToken in onSelect.allGamepiecePreselections:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["remove"],
                            0,
                            0,
                        )
                    else:
                        self.cmd.setCursor(
                            IconPaths.mouseIcons["add"],
                            0,
                            0,
                        )
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class MyPreselectEndHandler(adsk.core.SelectionEventHandler):
    """### Event fires when the mouse is moved away from an entity that was in a preselect state.

    Args: SelectionEventArgs
    """

    def __init__(self, cmd):
        super().__init__()
        self.cmd = cmd

    def notify(self, args):
        try:
            design = adsk.fusion.Design.cast(gm.app.activeProduct)
            preSelectedOcc = adsk.fusion.Occurrence.cast(args.selection.entity)

            if preSelectedOcc and design:
                self.cmd.setCursor("", 0,
                                   0)  # if preselection ends (mouse off of design), reset the mouse icon to default
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class ConfigureCommandInputChanged(adsk.core.InputChangedEventHandler):
    """### Gets an event that is fired whenever an input value is changed.
        - Button pressed, selection made, switching tabs, etc...

    Args: InputChangedEventHandler
    """

    def __init__(self, cmd):
        super().__init__()
        self.log = logging.getLogger(
            f"{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
        )
        self.cmd = cmd
        self.allWeights = [None, None]  # [lbs, kg]
        self.isLbs = True
        self.isLbs_f = True

    def reset(self):
        """### Process:
            - Reset the mouse icon to default
            - Clear active selections
        """
        try:
            self.cmd.setCursor("", 0, 0)
            gm.ui.activeSelections.clear()
        except:
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.reset()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    def weight(self, isLbs=True):  # maybe add a progress dialog??
        """### Get the total design weight using the predetermined units.

        Args:
            isLbs (bool, optional): Is selected mass unit pounds? Defaults to True.

        Returns:
            value (float): weight value in specified unit
        """
        try:
            if gm.app.activeDocument.design:

                massCalculation = FullMassCalculuation()
                totalMass = massCalculation.get_total_mass()

                value = float

                self.allWeights[0] = round(
                    totalMass * 2.2046226218, 2
                )

                self.allWeights[1] = round(
                    totalMass, 2
                )

                if isLbs:
                    value = self.allWeights[0]
                else:
                    value = self.allWeights[1]

                value = round(  # round weight to 2 decimals places
                    value, 2
                )
                return value
        except:
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}.weight()").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

    def notify(self, args):
        try:
            eventArgs = adsk.core.InputChangedEventArgs.cast(args)
            cmdInput = eventArgs.input
            MySelectHandler.lastInputCmd = cmdInput
            inputs = cmdInput.commandInputs
            onSelect = gm.handlers[3]

            frictionCoeff = INPUTS_ROOT.itemById("friction_coeff_override")

            wheelSelect = inputs.itemById("wheel_select")
            jointSelect = inputs.itemById("joint_select")
            gamepieceSelect = inputs.itemById("gamepiece_select")

            wheelTableInput = wheelTable()
            jointTableInput = jointTable()
            gamepieceTableInput = gamepieceTable()
            weightTableInput = inputs.itemById("weight_table")

            weight_input = INPUTS_ROOT.itemById("weight_input")

            wheelConfig = inputs.itemById("wheel_config")
            jointConfig = inputs.itemById("joint_config")
            gamepieceConfig = inputs.itemById("gamepiece_config")

            addWheelInput = INPUTS_ROOT.itemById("wheel_add")
            addJointInput = INPUTS_ROOT.itemById("joint_add")
            addFieldInput = INPUTS_ROOT.itemById("field_add")

            indicator = INPUTS_ROOT.itemById("algorithmic_indicator")

            # gm.ui.messageBox(cmdInput.id) # DEBUG statement, displays CommandInput user-defined id

            position = int

            if cmdInput.id == "mode":
                modeDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

                if modeDropdown.selectedItem.index == 0:
                    if gamepieceConfig:
                        gm.ui.activeSelections.clear()
                        gm.app.activeDocument.design.rootComponent.opacity = 1

                        gamepieceConfig.isVisible = False
                        weightTableInput.isVisible = True

                        addFieldInput.isEnabled = \
                            wheelConfig.isVisible = \
                            jointConfig.isVisible = True

                elif modeDropdown.selectedItem.index == 1:
                    if gamepieceConfig:
                        gm.ui.activeSelections.clear()
                        gm.app.activeDocument.design.rootComponent.opacity = 1

                        addWheelInput.isEnabled = \
                            addJointInput.isEnabled = \
                            gamepieceConfig.isVisible = True

                        jointConfig.isVisible = \
                            wheelConfig.isVisible = \
                            weightTableInput.isVisible = False

            elif cmdInput.id == "joint_config":
                gm.app.activeDocument.design.rootComponent.opacity = 1

            elif (cmdInput.id == "placeholder_w"
                  or cmdInput.id == "name_w"
                  or cmdInput.id == "signal_type_w"
            ):
                self.reset()

                wheelSelect.isEnabled = False
                addWheelInput.isEnabled = True

                cmdInput_str = cmdInput.id

                if cmdInput_str == "placeholder_w":
                    position = (
                            wheelTableInput.getPosition(
                                adsk.core.ImageCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )
                elif cmdInput_str == "name_w":
                    position = (
                            wheelTableInput.getPosition(
                                adsk.core.TextBoxCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )
                elif cmdInput_str == "signal_type_w":
                    position = (
                            wheelTableInput.getPosition(
                                adsk.core.DropDownCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )

                gm.ui.activeSelections.add(WheelListGlobal[position])

            elif (cmdInput.id == "placeholder"
                  or cmdInput.id == "name_j"
                  or cmdInput.id == "joint_parent"
                  or cmdInput.id == "signal_type"
            ):
                self.reset()
                jointSelect.isEnabled = False
                addJointInput.isEnabled = True

            elif (cmdInput.id == "blank_gp"
                  or cmdInput.id == "name_gp"
                  or cmdInput.id == "weight_gp"
            ):
                self.reset()

                gamepieceSelect.isEnabled = False
                addFieldInput.isEnabled = True

                cmdInput_str = cmdInput.id

                if cmdInput_str == "name_gp":
                    position = (
                            gamepieceTableInput.getPosition(
                                adsk.core.TextBoxCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )
                elif cmdInput_str == "weight_gp":
                    position = (
                            gamepieceTableInput.getPosition(
                                adsk.core.ValueCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )
                elif cmdInput_str == "blank_gp":
                    position = (
                            gamepieceTableInput.getPosition(
                                adsk.core.ImageCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )
                else:
                    position = (
                            gamepieceTableInput.getPosition(
                                adsk.core.FloatSliderCommandInput.cast(cmdInput)
                            )[1]
                            - 1
                    )

                gm.ui.activeSelections.add(GamepieceListGlobal[position])

            elif cmdInput.id == "wheel_type_w":
                self.reset()

                wheelSelect.isEnabled = False
                addWheelInput.isEnabled = True

                cmdInput_str = cmdInput.id
                position = (
                        wheelTableInput.getPosition(
                            adsk.core.DropDownCommandInput.cast(cmdInput)
                        )[1]
                        - 1
                )
                wheelDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

                if wheelDropdown.selectedItem.index == 0:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["standard"]
                    iconInput.tooltip = "Standard wheel"

                elif wheelDropdown.selectedItem.index == 1:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["omni"]
                    iconInput.tooltip = "Omni wheel"

                elif wheelDropdown.selectedItem.index == 2:
                    getPosition = wheelTableInput.getPosition(
                        adsk.core.DropDownCommandInput.cast(cmdInput)
                    )
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = IconPaths.wheelIcons["mecanum"]
                    iconInput.tooltip = "Mecanum wheel"

                gm.ui.activeSelections.add(WheelListGlobal[position])

            elif cmdInput.id == "wheel_add":
                self.reset()

                wheelSelect.isEnabled = addJointInput.isEnabled = True
                addWheelInput.isEnabled = False

            elif cmdInput.id == "joint_add":
                self.reset()

                addWheelInput.isEnabled = \
                    jointSelect.isEnabled = True
                addJointInput.isEnabled = False

            elif cmdInput.id == "field_add":
                self.reset()

                gamepieceSelect.isEnabled = True
                addFieldInput.isEnabled = False

            elif cmdInput.id == "wheel_delete":
                gm.ui.activeSelections.clear()

                addWheelInput.isEnabled = True
                if wheelTableInput.selectedRow == -1 or wheelTableInput.selectedRow == 0:
                    wheelTableInput.selectedRow = wheelTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    index = wheelTableInput.selectedRow - 1
                    removeWheelFromTable(index)

            elif cmdInput.id == "joint_delete":
                gm.ui.activeSelections.clear()

                addJointInput.isEnabled = True
                addWheelInput.isEnabled = True

                if jointTableInput.selectedRow == -1 or jointTableInput.selectedRow == 0:
                    jointTableInput.selectedRow = jointTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    joint = JointListGlobal[jointTableInput.selectedRow - 1]
                    removeJointFromTable(joint)

            elif cmdInput.id == "field_delete":
                gm.ui.activeSelections.clear()

                addFieldInput.isEnabled = True

                if gamepieceTableInput.selectedRow == -1 or gamepieceTableInput.selectedRow == 0:
                    gamepieceTableInput.selectedRow = gamepieceTableInput.rowCount - 1
                    gm.ui.messageBox("Select a row to delete.")
                else:
                    index = gamepieceTableInput.selectedRow - 1
                    removeGamePieceFromTable(index)

            elif cmdInput.id == "wheel_select":
                self.reset()

                wheelSelect.isEnabled = False
                addWheelInput.isEnabled = True

            elif cmdInput.id == "joint_select":
                self.reset()

                jointSelect.isEnabled = False
                addJointInput.isEnabled = True

            elif cmdInput.id == "gamepiece_select":
                self.reset()

                gamepieceSelect.isEnabled = False
                addFieldInput.isEnabled = True

            elif cmdInput.id == "friction_override":
                boolValue = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if boolValue.value == True:
                    frictionCoeff.isVisible = True
                else:
                    frictionCoeff.isVisible = False

            elif cmdInput.id == "weight_unit":
                unitDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
                weightInput = weightTableInput.getInputAtPosition(0, 2)
                if unitDropdown.selectedItem.index == 0:
                    self.isLbs = True

                    weightInput.tooltipDescription = """<tt>(in pounds)</tt><hr>This is the weight of the entire robot assembly."""
                elif unitDropdown.selectedItem.index == 1:
                    self.isLbs = False

                    weightInput.tooltipDescription = """<tt>(in kilograms)</tt><hr>This is the weight of the entire robot assembly."""

            elif cmdInput.id == "weight_unit_f":
                unitDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
                if unitDropdown.selectedItem.index == 0:
                    self.isLbs_f = True

                    for row in range(gamepieceTableInput.rowCount):
                        if row == 0:
                            continue
                        weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                        weightInput.tooltipDescription = "<tt>(in pounds)</tt>"
                elif unitDropdown.selectedItem.index == 1:
                    self.isLbs_f = False

                    for row in range(gamepieceTableInput.rowCount):
                        if row == 0:
                            continue
                        weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                        weightInput.tooltipDescription = "<tt>(in kilograms)</tt>"

            elif cmdInput.id == "auto_calc_weight":
                button = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if button.value == True:  # CALCULATE button pressed
                    if self.allWeights.count(None) == 2:  # if button is pressed for the first time
                        if self.isLbs:  # if pounds unit selected
                            self.allWeights[0] = self.weight()
                            weight_input.value = self.allWeights[0]
                        else:  # if kg unit selected
                            self.allWeights[1] = self.weight(False)
                            weight_input.value = self.allWeights[1]
                    else:  # if a mass value has already been configured
                        if weight_input.value != self.allWeights[0] or weight_input.value != self.allWeights[
                            1] or not weight_input.isValidExpression:
                            if self.isLbs:
                                weight_input.value = self.allWeights[0]
                            else:
                                weight_input.value = self.allWeights[1]

            elif cmdInput.id == "auto_calc_weight_f":
                button = adsk.core.BoolValueCommandInput.cast(cmdInput)

                if button.value == True:  # CALCULATE button pressed
                    if self.isLbs_f:
                        for row in range(gamepieceTableInput.rowCount):
                            if row == 0:
                                continue
                            weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                            physical = GamepieceListGlobal[row - 1].component.getPhysicalProperties(
                                adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                            )
                            value = round(
                                physical.mass * 2.2046226218, 2
                            )
                            weightInput.value = value

                    else:
                        for row in range(gamepieceTableInput.rowCount):
                            if row == 0:
                                continue
                            weightInput = gamepieceTableInput.getInputAtPosition(row, 2)
                            physical = GamepieceListGlobal[row - 1].component.getPhysicalProperties(
                                adsk.fusion.CalculationAccuracy.LowCalculationAccuracy
                            )
                            value = round(
                                physical.mass, 2
                            )
                            weightInput.value = value
            elif cmdInput.id == "compress":
                checkBox = adsk.core.BoolValueCommandInput.cast(cmdInput)
                if checkBox.value:
                    global compress
                    compress = checkBox.value
            elif cmdInput.id == "algorithmic_selection":
                checkBox = adsk.core.BoolValueCommandInput.cast(cmdInput)
                onSelect.algorithmicSelection = checkBox.value
                if checkBox.value:
                    indicator.formattedText = "🟢"
                    indicator.tooltipDescription = (
                            "<tt>(enabled)</tt>" +
                            "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                            "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                            "<ul>" +
                            "<tt>" +
                            "<li>Jointing the wheel differently, or</li><br>" +
                            "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                            "<li>Disabling Algorithmic Selection.</li>" +
                            "</tt>" +
                            "</ul>"
                    )

                else:
                    indicator.formattedText = "🔴"
                    indicator.tooltipDescription = (
                            "<tt>(disabled)</tt>" +
                            "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                            "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                            "<ul>" +
                            "<tt>" +
                            "<li>Jointing the wheel differently, or</li><br>" +
                            "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                            "<li>Disabling Algorithmic Selection.</li>" +
                            "</tt>" +
                            "</ul>"
                    )
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )


class MyKeyDownHandler(adsk.core.KeyboardEventHandler):
    def __init__(self) -> None:
        super().__init__()

    def notify(self, args):
        eventArgs = adsk.core.KeyboardEventArgs.cast(args)
        keyCode = eventArgs.keyCode
        onSelect = gm.handlers[3]
        algorithmicSelection = INPUTS_ROOT.itemById("algorithmic_selection")
        indicator = INPUTS_ROOT.itemById("algorithmic_indicator")
        # wheelAddButton = INPUTS_ROOT.itemById("wheel_add")

        # if wheelAddButton.isEnabled:
        #    return

        if keyCode == 16777249:  # CTRL key pressed
            # gm.ui.messageBox("KEY DOWN")
            onSelect.algorithmicSelection = not algorithmicSelection.value
            if algorithmicSelection.value:
                indicator.formattedText = "🔴"
                indicator.tooltipDescription = (
                        "<tt>(disabled)</tt>" +
                        "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                        "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                        "<ul>" +
                        "<tt>" +
                        "<li>Jointing the wheel differently, or</li><br>" +
                        "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                        "<li>Disabling Algorithmic Selection.</li>" +
                        "</tt>" +
                        "</ul>"
                )
            else:
                indicator.formattedText = "🟢"
                indicator.tooltipDescription = (
                        "<tt>(enabled)</tt>" +
                        "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                        "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                        "<ul>" +
                        "<tt>" +
                        "<li>Jointing the wheel differently, or</li><br>" +
                        "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                        "<li>Disabling Algorithmic Selection.</li>" +
                        "</tt>" +
                        "</ul>"
                )


class MyKeyUpHandler(adsk.core.KeyboardEventHandler):
    def __init__(self) -> None:
        super().__init__()

    def notify(self, args):
        eventArgs = adsk.core.KeyboardEventArgs.cast(args)
        keyCode = eventArgs.keyCode

        onSelect = gm.handlers[3]
        algorithmicSelection = INPUTS_ROOT.itemById("algorithmic_selection")
        indicator = INPUTS_ROOT.itemById("algorithmic_indicator")
        # wheelAddButton = INPUTS_ROOT.itemById("wheel_add")

        # if wheelAddButton.isEnabled:
        #    return

        if keyCode == 16777249:  # CTRL key released
            # gm.ui.messageBox("KEY UP")
            onSelect.algorithmicSelection = algorithmicSelection.value
            if algorithmicSelection.value:
                indicator.formattedText = "🟢"
                indicator.tooltipDescription = (
                        "<tt>(enabled)</tt>" +
                        "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                        "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                        "<ul>" +
                        "<tt>" +
                        "<li>Jointing the wheel differently, or</li><br>" +
                        "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                        "<li>Disabling Algorithmic Selection.</li>" +
                        "</tt>" +
                        "</ul>"
                )
            else:
                indicator.formattedText = "🔴"
                indicator.tooltipDescription = (
                        "<tt>(disabled)</tt>" +
                        "<hr>If a sub-part of a wheel is selected (eg. a roller of an omni wheel), an algorithm will traverse the assembly to best determine the entire wheel component.<br>" +
                        "<br>This traversal operates on how the wheel is jointed and where the joint is placed. If the automatic selection fails, try:" +
                        "<ul>" +
                        "<tt>" +
                        "<li>Jointing the wheel differently, or</li><br>" +
                        "<li>Selecting the wheel from the browser while holding down <span style='text-decoration:overline;text-decoration:underline;background-color: #c27b10'>&nbsp;CTRL&nbsp;</span></span>, or</li><br>" +
                        "<li>Disabling Algorithmic Selection.</li>" +
                        "</tt>" +
                        "</ul>"
                )


class MyCommandDestroyHandler(adsk.core.CommandEventHandler):
    """### Gets the event that is fired when the command is destroyed. Globals lists are released and active selections are cleared (when exiting the panel).
        - In other words, when the OK or Cancel button is pressed...

    Args: CommandEventHandler
    """

    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            onSelect = gm.handlers[3]

            WheelListGlobal.clear()
            JointListGlobal.clear()
            GamepieceListGlobal.clear()
            onSelect.allWheelPreselections.clear()
            onSelect.wheelJointList.clear()

            for group in gm.app.activeDocument.design.rootComponent.customGraphicsGroups:
                group.deleteMe()

            gm.ui.activeSelections.clear()
            gm.app.activeDocument.design.rootComponent.opacity = 1
        except:
            if gm.ui:
                gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger("{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}").error(
                "Failed:\n{}".format(traceback.format_exc())
            )

