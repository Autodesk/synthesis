from logging import PlaceHolder
from os import remove
from proto.proto_out.joint_pb2 import Joint
from ..general_imports import *
from ..configure import NOTIFIED, write_configuration
from ..Analytics.alert import showAnalyticsAlert
from . import Helper, FileDialogConfig, OsHelper

from ..Parser.ParseOptions import ParseOptions, _Joint, _Wheel
from .Configuration.SerialCommand import (
    Struct,
    SerialCommand,
    General,
    Advanced,
    BooleanInput,
    ExportMode,
)

import adsk.core, adsk.fusion, traceback
from types import SimpleNamespace

"""
File to generate and link the Configuration Command seen when pressing the button from the Addins Panel
"""
previous = None

ui = adsk.core.UserInterface.cast(None)
wheelTableInput = adsk.core.TableCommandInput
jointTableInput = adsk.core.TableCommandInput

# store selected wheels & joints
_occ = {
    "wheel": [],
    "joint": []
}

_wheels = []
_joints = []

_select = {
    "wheel": [],
    "joint": []
}

# easy-access image paths for icons
iconPaths = {
    "omni": "src/Resources/omni-wheel-preview16x16.png",
    "standard": "src/Resources/standard-wheel-preview16x16.png",
    "rigid": "src/Resources/rigid-preview16x16.png",
    "revolute": "src/Resources/revolute-preview16x16.png",
    "slider": "src/Resources/slider-preview16x16.png"
}


class ConfigureCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    """Start the Command Input Object and define all of the input groups to create our ParserOptions object.

    Notes:
        - linked and called from (@ref HButton) and linked
        - will be called from (@ref Events.py)
    """

    def __init__(self, configure):
        super().__init__()
        self.log = logging.getLogger(f"{INTERNAL_ID}.UI.{self.__class__.__name__}")

    def notify(self, args):
        try:
            if not Helper.check_solid_open():
                return

            global NOTIFIED, wheelTableInput, jointTableInput
            if not NOTIFIED:
                showAnalyticsAlert()
                NOTIFIED = True
                write_configuration("analytics", "notified", "yes")

            global previous

            saved = Helper.previouslyConfigured()
            if type(saved) == str:
                try:
                    # probably need some way to validate for each usage below
                    previous = json.loads(
                        saved, object_hook=lambda d: SimpleNamespace(**d)
                    )
                    # self.log(f"Found previous {previous}")
                except:
                    self.log.error("Failed:\n{}".format(traceback.format_exc()))
                    gm.ui.messageBox(
                        "Failed to read previous Unity Configuration\n  - Using default configuration"
                    )
                    previous = SerialCommand()
            else:
                # new file configuration
                previous = SerialCommand()

            if A_EP:
                A_EP.send_view("export_panel")

            eventArgs = adsk.core.CommandCreatedEventArgs.cast(args)
            cmd = eventArgs.command

            # Set to false so won't automatically export on switch context
            cmd.isAutoExecute = False
            cmd.isExecutedWhenPreEmpted = False
            cmd.okButtonText = "Export"

            # First check if the object has previosuly had saved information
            # self._parsePrevious()

            onInputChanged = ConfigureCommandInputChanged()
            cmd.inputChanged.add(onInputChanged)
            gm.handlers.append(onInputChanged)

            onSelect = MySelectHandler()
            cmd.select.add(onSelect)
            gm.handlers.append(onSelect) 
            
            onUnSelect = MyUnSelectHandler()
            cmd.unselect.add(onUnSelect)            
            gm.handlers.append(onUnSelect)

            onDestroy = MyCommandDestroyHandler()
            cmd.destroy.add(onDestroy)
            gm.handlers.append(onDestroy)

            onExecute = ConfigureCommandExecuteHandler(
                json.dumps(
                    previous, default=lambda o: o.__dict__, sort_keys=True, indent=1
                ),
                previous.filePath,
            )
            cmd.execute.add(onExecute)
            gm.handlers.append(onExecute)

            inputs_root = cmd.commandInputs

            """
            General Tab
            """
            inputs = inputs_root.addTabCommandInput("generalSettings", "General").children

            # This actually has the ability to entirely create a input from just a protobuf message which is really neat
            ## NOTE: actually is super neat but I don't have time at the moment
            # self.parseMessage(inputs)

            """
            Export Mode
            """
            # This could be a button group
            dropdownExportMode = inputs.addDropDownCommandInput(
                "mode",
                "Export Mode",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            dropdownExportMode.listItems.add("Dynamic Item", True)
            dropdownExportMode.listItems.add("Static Field Item", False)

            dropdownExportMode.tooltip = (
                "This will be future formats and or generic / advanced objects."
            )

            """
            Weight Configuration
            """
            weightTableInput = self.createTableInput(
                "weighttable",
                "Weight Table",
                inputs,
                3,
                "5:3:2",
                1
            )
            weightTableInput.tablePresentationStyle = 2
            weightTableInput.columnSpacing = 0

            weight_name = inputs.addStringValueInput("weightname", "Weight")
            weight_name.value = "Weight"
            weight_name.isReadOnly = True

            weight_input = inputs.addValueInput("weightinput", "Weight Input", "", adsk.core.ValueInput.createByString("0.0"))
            weight_input.tooltip = "Weight"

            weight_unit = inputs.addDropDownCommandInput("weightunit", "Weight Unit", adsk.core.DropDownStyles.LabeledIconDropDownStyle)
            weight_unit.listItems.add("lbs", True)
            weight_unit.listItems.add("kg", False)
            weight_unit.tooltip = "Mass units"

            weightTableInput.addCommandInput(weight_name, 0, 0)
            weightTableInput.addCommandInput(weight_input, 0, 1)
            weightTableInput.addCommandInput(weight_unit, 0, 2)

            self.createBooleanInput(
                "exportjoints",
                "Export Additional Joints",
                inputs,
                #checked=previous.general.joints.checked,
                tooltip="Export additional Fusion 360 Joints into Unity.",
                #tooltipadvanced="May be inconsistent dependening on type of joint.",
                enabled=True,
            )

            # if previous is defined it will go through and assign the booleans in a hard coded way
            # self._generateGeneral(inputs)

            # advanced settings minimized by default
            advancedSettings = inputs_root.addTabCommandInput(
                "advancedsettings", "Advanced"
            )
            # advancedSettings.isExpanded = False
            # advancedSettings.isActive = False
            advancedSettings.tooltip = "Additional Advanced Settings to change how your model will be translated into Unity."
            
            a_input = advancedSettings.children

            physics_advanced_group = a_input.addGroupCommandInput("advanced_physics_group", "Physics Properties").children

            self.createBooleanInput(
                "density",
                "Density",
                physics_advanced_group,
                checked=True,
                tooltipadvanced="Export Density for each physical body in kg/m^3",
                enabled=True,
            )

            joints_advanced_group = a_input.addGroupCommandInput("advanced_joints_group", "Joints Properties").children

            self.createBooleanInput(
                "kinematic_joints",
                "Kinematic Joints",
                joints_advanced_group,
                checked=True,
                tooltipadvanced="Export Joints as Kinematic Bodies.",
                enabled=True,
            )

            """
            Wheel Conifguration
            """
            wheelConfig = inputs.addGroupCommandInput("wheelconfig", "Wheel Configuration")
            wheelConfig.isExpanded = True # later, change back to false
            wheelConfig.isEnabled = True
            wheelConfig.tooltip = "Select and define the drive-train wheels in your assembly."
            wheel_inputs = wheelConfig.children

            wheelTableInput = self.createTableInput(
                "wheeltable",
                "Wheel Table",
                wheel_inputs,
                3,
                "1:3:2",
                10,
            )

            addWheelInput = wheel_inputs.addBoolValueInput(
                "wheeladd", "Add", False
            )

            deleteWheelInput = wheel_inputs.addBoolValueInput(
                "wheeldelete", "Remove", False
            )

            addWheelInput.tooltip = "Add a wheel component"
            deleteWheelInput.tooltip = "Remove a wheel component"

            wheelSelectInput = wheel_inputs.addSelectionInput("wheelselect", "Selection", "Select the wheels in your drive-train assembly.")
            wheelSelectInput.addSelectionFilter("Occurrences") # limit selection to only bodies
            wheelSelectInput.setSelectionLimits(0)
            wheelSelectInput.isEnabled = False
            wheelSelectInput.isVisible = False

            wheelTableInput.addToolbarCommandInput(addWheelInput)
            wheelTableInput.addToolbarCommandInput(deleteWheelInput)

            self.log.info("Created Configuration Input successfully")

            """
            Simple wheel export button
            """
            self.createBooleanInput(
                "simplewheelexport",
                "Simple Wheel Export",
                inputs,
                #checked=previous.general.simpleWheelExport.checked,
                tooltipadvanced="Export Center of Mass Vector for each body?",
                enabled=True,
            )

            """
            Joint Configuration
            """
            jointConfig = inputs.addGroupCommandInput("jointconfig", "Joint Configuration")
            jointConfig.isExpanded = True # later, change back to false
            jointConfig.isEnabled = True
            jointConfig.tooltip = "Select and define joint occurrences in your assembly."
            joint_inputs = jointConfig.children

            jointTableInput = self.createTableInput(
                "jointtable",
                "Joint Table",
                joint_inputs,
                3,
                "1:3:2",
                50,
            )

            addJointInput = joint_inputs.addBoolValueInput(
                "jointadd", "Add", False
            )

            deleteJointInput = joint_inputs.addBoolValueInput(
                "jointdelete", "Remove", False
            )

            addJointInput.tooltip = "Add a joint selection"
            deleteJointInput.tooltip = "Remove a joint selection"

            jointSelectInput = joint_inputs.addSelectionInput("jointselect", "Selection", "Select a joint in your drive-train assembly.")
            jointSelectInput.addSelectionFilter("Joints") # limit selection to only joints
            jointSelectInput.setSelectionLimits(0)
            jointSelectInput.isEnabled = False
            jointSelectInput.isVisible = False

            jointTableInput.addToolbarCommandInput(addJointInput)
            jointTableInput.addToolbarCommandInput(deleteJointInput)

            self.log.info("Created Configuration Input successfully")
            
            """
            Advanced Tab
            """
            # advanced settings minimized by default
            advancedSettings = inputs_root.addTabCommandInput(
                "advancedsettings", "Advanced"
            )
            # advancedSettings.isExpanded = False
            # advancedSettings.isActive = False
            advancedSettings.tooltip = "Additional Advanced Settings to change how your model will be translated into Unity."
            a_input = advancedSettings.children

            self.createBooleanInput(
                "friction",
                "Friction",
                a_input,
                checked=previous.advanced.friction.checked,
                tooltipadvanced="Export Friction value for each body?",
                enabled=False,
            )

            self.createBooleanInput(
                "density",
                "Density",
                a_input,
                checked=previous.advanced.density.checked,
                tooltipadvanced="Export Density value for each body?",
                enabled=False,
            )

            self.createBooleanInput(
                "mass",
                "Mass",
                a_input,
                checked=previous.advanced.mass.checked,
                tooltipadvanced="Export Mass value for each body in kg?",
                enabled=False,
            )

            self.createBooleanInput(
                "volume",
                "Volume",
                a_input,
                checked=previous.advanced.volume.checked,
                tooltipadvanced="Export volume value for each body?",
                enabled=False,
            )

            self.createBooleanInput(
                "surfacearea",
                "Surface Area",
                a_input,
                checked=previous.advanced.surfaceArea.checked,
                tooltipadvanced="Export Surface Area value for each body?",
                enabled=False,
            )

            self.createBooleanInput(
                "com",
                "Center of Mass",
                a_input,
                checked=previous.advanced.com.checked,
                tooltipadvanced="Export Center of Mass Vector for each body?",
                enabled=False,
            )
        except:
            self.log.error("Failed:\n{}".format(traceback.format_exc()))
            if A_EP:
                A_EP.send_exception()
            elif gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

    def createBooleanInput(
        self,
        _id: str,
        name: str,
        inputs: adsk.core.CommandInputs,
        tooltip="",
        tooltipadvanced="",
        checked=True,
        enabled=True,
    ) -> adsk.core.BoolValueCommandInput:
        """Simple helper to generate all of the options for me to create a boolean command input

        Args:
            _id (str): id value of the object - pretty much lowercase name
            name (str): name as displayed by the command prompt
            inputs (adsk.core.CommandInputs): parent command input container
            tooltip (str, optional): Description on hover of the checkbox. Defaults to "".
            tooltipadvanced (str, optional): Long hover description. Defaults to "".
            checked (bool, optional): Is checked by default?. Defaults to True.

        Returns:
            adsk.core.BoolValueCommandInput: Recently created command input
        """
        _input = inputs.addBoolValueInput(_id, name, True)
        _input.value = checked
        _input.isEnabled = enabled
        _input.tooltip = tooltip
        _input.tooltipDescription = tooltipadvanced
        return _input

    def createTableInput(
        self,
        _id: str,
        name: str,
        inputs: adsk.core.CommandInputs,
        columns: int,
        ratio: str,
        maxRows: int,
        minRows=1
        ) -> adsk.core.TableCommandInput: # accepts an occurrence (wheel)
            _input = inputs.addTableCommandInput(_id, name, columns, ratio)
            _input.minimumVisibleRows = minRows
            _input.maximumVisibleRows = maxRows
            return _input


class ConfigureCommandExecuteHandler(adsk.core.CommandEventHandler):
    """Called when Ok is pressed confirming the export to Unity.

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
                "generalSettings"
            ).children.itemById("mode")

            mode_dropdown = adsk.core.DropDownCommandInput.cast(mode_dropdown)
            mode = 5

            if mode_dropdown.selectedItem.name == "Synthesis Exporter":
                mode = 5

            # Get the values from the command inputs.
            try:
                self.writeConfiguration(eventArgs.command.commandInputs)
                self.log.info("Wrote Configuration")

                # if it's different
                if self.current.toJSON() != self.previous:
                    Helper.writeConfigure(self.current.toJSON())
            except:
                self.log.error("Failed:\n{}".format(traceback.format_exc()))
                gm.ui.messageBox("Failed to read previous File Export Configuration")

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

                render_dropdown = eventArgs.command.commandInputs.itemById(
                    "generalSettings"
                ).children.itemById("renderMode")
                renderer = 0
                dropdown = adsk.core.DropDownCommandInput.cast(render_dropdown)
                if (dropdown):
                    if dropdown.selectedItem.name == "Standard":
                        renderer = 0
                    elif dropdown.selectedItem.name == "URP":
                        renderer = 1

                """_exportWheels = []
                _exportJoints = []

                for row in range(wheelTableInput.rowCount):
                    index = wheelTableInput.getInputAtPosition(row, 2).selectedItem.index
                    
                    if index == 0:
                            _exportWheels.append(_Wheel(_wheels[row], WheelType.STANDARD))
                    elif index == 1:
                            _exportWheels.append(_Wheel(_wheels[row], WheelType.OMNI))

                for row in range(jointTableInput.rowCount):
                    item = jointTableInput.getInputAtPosition(row, 2).selectedItem

                    if item.name == "Root":
                        _exportJoints.append(_Joint(_joints[row], JointParentType.ROOT))
                    else:
                        for occ in _joints:
                            if item.name == occ.name:
                                _exportJoints.append(_Joint(_joints[row], occ))

                # now construct a ParseOptions and save the file
                # since self.current is already up to date might as well use it
                options = ParseOptions(
                    savepath,
                    name,
                    version,
                    materials=renderer,
                    #joints=_exportJoints,
                    #wheels=_exportWheels,
                    mode=mode,
                )"""
                
                # now construct a ParseOptions and save the file
                # since self.current is already up to date might as well use it
                options = ParseOptions(
                    savepath,
                    name,
                    version,
                    mode=mode,
                    wheel=[],
                    joints=[]
                )
                
                if options.parse(False):
                    # success
                    pass
                else:
                    gm.ui.messageBox(
                        f"Error: \n\t{name} could not be written to \n {savepath}"
                    )
                    self.log.error(
                        f"Error: \n\t{name} could not be written to \n {savepath}"
                    )

            # gm.ui.messageBox(f"general materials is set to: {inputs.itemById('generalSettings').children.itemById('materials').value}")
        except:
            self.log.error("Failed:\n{}".format(traceback.format_exc()))
            if A_EP:
                A_EP.send_exception()
            elif gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

    def writeConfiguration(self, rootCommandInput: adsk.core.CommandInputs):
        """Simple hard coded function to save the parameters of the export to the file

        - This is horribly written but im in a rush
        - Should have most likely been a map that could be serialized easily

        Args:
            rootCommandInput (adsk.core.CommandInputs): Base Command Inputs
        """
        if self.current:
            self.current.filePath = self.fp
            _general = self.current.general
            _advanced = self.current.advanced
            # _vr = self.previous.vrSettings

            generalSettingsInputs = rootCommandInput.itemById("generalSettings").children
            advancedSettingsInputs = rootCommandInput.itemById(
                "advancedsettings"
            ).children

            if generalSettingsInputs and _general:
                # vrSettingsInputs = generalSettingsInputs.itemById("vrSettings")
                try:
                    _general.material.checked = generalSettingsInputs.itemById(
                        "materials"
                    ).value

                    _general.joints.checked = generalSettingsInputs.itemById("joints").value
                    _general.rigidGroups.checked = generalSettingsInputs.itemById(
                        "rigidGroups"
                    ).value
                except:
                    # this will force an error - ignore for now
                    pass

            if advancedSettingsInputs and _advanced:

                _advanced.friction.checked = advancedSettingsInputs.itemById(
                    "friction"
                ).value
                _advanced.density.checked = advancedSettingsInputs.itemById(
                    "density"
                ).value
                _advanced.mass.checked = advancedSettingsInputs.itemById("mass").value
                _advanced.volume.checked = advancedSettingsInputs.itemById("volume").value
                _advanced.surfaceArea.checked = advancedSettingsInputs.itemById(
                    "surfacearea"
                ).value
                _advanced.com.checked = advancedSettingsInputs.itemById("com").value


class MySelectHandler(adsk.core.SelectionEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            selectedWheel = adsk.fusion.Occurrence.cast(args.selection.entity)
            selectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

            if selectedWheel: # selection kinda breaks when you select root comp. if statement here maybe?
                _wheels.append(selectedWheel)
                addWheelToTable(selectedWheel)
            
            elif selectedJoint:
                if selectedJoint.jointMotion.jointType == 0:
                    gm.ui.activeSelections.removeByIndex(gm.ui.activeSelections.count - 1)
                else:
                    _joints.append(selectedJoint)
                    addJointToTable(selectedJoint)
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


class MyUnSelectHandler(adsk.core.SelectionEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            selectedWheel = adsk.fusion.Occurrence.cast(args.selection.entity) 
            selectedJoint = adsk.fusion.Joint.cast(args.selection.entity)

            if selectedWheel:
                removeWheelFromTable(selectedWheel)

            elif selectedJoint:
                removeJointFromTable(selectedJoint)
        except ValueError: # bad solution to value error. replace with somethin better
            pass
        except:
            if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


class ConfigureCommandInputChanged(adsk.core.InputChangedEventHandler):
    """Gets instantiated from Fusion whenever there is a valid input change.

    Process:
        - Enable Advanced Features
        - Enable VR Features
            - Give optional hand contact set placement
            - Check for additional params on exit
            - serialize additional data
    """

    def __init__(self):
        super().__init__()
        self.log = logging.getLogger(
            f"{INTERNAL_ID}.UI.ConfigCommand.{self.__class__.__name__}"
        )

    def notify(self, args):
        try:
            global wheelTableInput
            
            eventArgs = adsk.core.InputChangedEventArgs.cast(args)
            cmdInput = eventArgs.input
            inputs = cmdInput.commandInputs

            #gm.ui.messageBox(str(cmdInput.id))

            wheelSelect = inputs.itemById("wheelselect")
            jointSelect = inputs.itemById("jointselect")

            if cmdInput.id == "mode":
                dropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

            elif cmdInput.id == "exportjoints":
                boolValue = adsk.core.BoolValueCommandInput.cast(cmdInput)
                jointConfig = inputs.itemById("jointconfig")

                if boolValue.value == True:
                    if jointConfig:
                        jointConfig.isEnabled = True
                        jointConfig.isExpanded = True
                else:
                    if jointConfig:
                        jointConfig.isEnabled = False
                        jointConfig.isExpanded = False

            elif cmdInput.id == "wheelconfig":
                group1 = adsk.core.GroupCommandInput.cast(cmdInput)

                if not group1.isExpanded:
                    gm.ui.activeSelections.clear()
                else:
                    for i in range(len(_occ["wheel"])):
                        #wheel_select.append(_occ["wheel"][i])
                        gm.ui.activeSelections.add(_occ["wheel"][i])
            elif cmdInput.id == "jointconfig":
                group2 = adsk.core.GroupCommandInput.cast(cmdInput)

                if not group2.isExpanded:
                    gm.ui.activeSelections.clear()
                else:
                    for i in range(len(_joints)):
                        gm.ui.activeSelections.add(_joints[i])

            elif cmdInput.id == "wheeltype":
                wheelDropdown = adsk.core.DropDownCommandInput.cast(cmdInput)

                if wheelDropdown.selectedItem.name == "Standard":
                    getPosition = wheelTableInput.getPosition(adsk.core.DropDownCommandInput.cast(cmdInput))
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = iconPaths["standard"]
                    iconInput.tooltip = "Standard wheel"

                elif wheelDropdown.selectedItem.name == "Omni":
                    getPosition = wheelTableInput.getPosition(adsk.core.DropDownCommandInput.cast(cmdInput))
                    iconInput = wheelTableInput.getInputAtPosition(getPosition[1], 0)
                    iconInput.imageFile = iconPaths["omni"]
                    iconInput.tooltip = "Omni wheel"

            elif cmdInput.id == "wheeladd":
                wheelSelect.isEnabled = True

            elif cmdInput.id == "wheeldelete":
                table = inputs.itemById("wheeltable")
                
                if table.selectedRow == -1:
                    gm.ui.messageBox(
                        "Select one row to delete.")
                else:
                    selectedRow = table.selectedRow
                    wheel = _wheels[table.selectedRow]
                    removeWheelFromTable(wheel)

                    gm.ui.activeSelections.removeByIndex(selectedRow)
                    #gm.ui.messageBox(str(gm.ui.activeSelections.count))

            elif cmdInput.id == "wheelselect":
                wheelSelect.isEnabled = False

            elif cmdInput.id == "jointadd":
                jointSelect.isEnabled = True

            elif cmdInput.id == "jointdelete":
                table = inputs.itemById("jointtable")
                
                if table.selectedRow == -1:
                    gm.ui.messageBox(
                        "Select one row to delete.")
                else:
                    selectedRow = table.selectedRow
                    joint = _joints[table.selectedRow]
                    removeJointFromTable(joint)

                    gm.ui.activeSelections.removeByIndex(selectedRow)
                    #gm.ui.messageBox(str(gm.ui.activeSelections.count))
            
            elif cmdInput.id == "jointselect":
                jointSelect.isEnabled = False

        except:
            self.log.error("Failed:\n{}".format(traceback.format_exc()))
            if A_EP:
                A_EP.send_exception()
            elif gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


# terminates the script.            
class MyCommandDestroyHandler(adsk.core.CommandEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            _wheels.clear()
            _joints.clear()

            _select["wheel"].clear()
            _select["wheel"].clear()
        except:
            gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))


def createMeshGraphics():
        design = adsk.fusion.Design.cast(gm.app.activeProduct)
        
        for occ in _wheels:
            for body in occ.bRepBodies:
                graphics = design.rootComponent.customGraphicsGroups.add()

                bodyMesh = body.meshManager.displayMeshes.bestMesh
                coords = adsk.fusion.CustomGraphicsCoordinates.create(bodyMesh.nodeCoordinatesAsDouble)
                mesh = graphics.addMesh(
                    coords, bodyMesh.nodeIndices, bodyMesh.normalVectorsAsDouble, bodyMesh.nodeIndices
                )
                #redColor = adsk.core.Color.create(255,0,0,255)
                #solidColor = adsk.fusion.CustomGraphicsSolidColorEffect.create(redColor)
                #mesh.color = solidColor

                showThrough = adsk.fusion.CustomGraphicsShowThroughColorEffect.create(adsk.core.Color.create(255, 0, 0, 255), 0.2)
                mesh.color = showThrough

        #gm.app.activeViewport.refresh()

def addJointToTable(joint):
    # get the CommandInputs object associated with the parent command.
    cmdInputs = adsk.core.CommandInputs.cast(jointTableInput.commandInputs)

    if joint.jointMotion.jointType == adsk.fusion.JointTypes.RigidJointType:
        icon = cmdInputs.addImageCommandInput("placeholder", "Rigid", iconPaths["rigid"])
        icon.tooltip = "Rigid joint"
    
    elif joint.jointMotion.jointType == adsk.fusion.JointTypes.RevoluteJointType:
        icon = cmdInputs.addImageCommandInput("placeholder", "Revolute", iconPaths["revolute"])
        icon.tooltip = "Revolute joint"

    elif joint.jointMotion.jointType == adsk.fusion.JointTypes.SliderJointType:
        icon = cmdInputs.addImageCommandInput("placeholder", "Slider", iconPaths["slider"])
        icon.tooltip = "Slider joint"
  
    name = cmdInputs.addTextBoxCommandInput("occ_name", "Occurrence name", joint.name, 1, True)
    name.tooltip = (
        "Selection set"
    )
    jointType = cmdInputs.addDropDownCommandInput(
        "jointtype",
        "Joint Type",
        dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
    )
    jointType.listItems.add(
        "Root", True, ""
    )

    # after each additional joint added, add joint to the dropdown of all preview rows/joints
    for i in range(jointTableInput.rowCount):
        dropDown = jointTableInput.getInputAtPosition(i, 2)
        dropDown.listItems.add(
            _joints[-1].name, False, ""
        )
    
    # add all parent joint options to added joint dropdown
    for j in range(len(_joints) - 1):
        jointType.listItems.add(
            _joints[j].name, True, ""
        )

    jointType.tooltip = (
        "Select the parent joint"
    )

    row = jointTableInput.rowCount

    jointTableInput.addCommandInput(icon, row, 0)
    jointTableInput.addCommandInput(name, row, 1)
    jointTableInput.addCommandInput(jointType, row, 2)

def addWheelToTable(wheel):
    # get the CommandInputs object associated with the parent command.
    cmdInputs = adsk.core.CommandInputs.cast(wheelTableInput.commandInputs)
    icon = cmdInputs.addImageCommandInput("placeholder", "Placeholder", iconPaths["standard"])
    name = cmdInputs.addTextBoxCommandInput("occ_name", "Occurrence name", wheel.name, 1, True)
    name.tooltip = (
        "Selection set"
    )
    wheelType = cmdInputs.addDropDownCommandInput(
        "wheeltype",
        "Wheel Type",
        dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
    )
    wheelType.listItems.add(
        "Standard", True, ""
    )
    wheelType.listItems.add(
        "Omni", False, ""
    )
    wheelType.tooltip = (
        "Wheel type"
    )
    wheelType.tooltipDescription = (
        "<Br>Omni-directional wheels can be used just like regular drive wheels but they have the advantage of being able to roll freely perpendicular to the drive direction.</Br>"
    )
    wheelType.toolClipFilename = (
        "src\Resources\omni-wheel-preview.png"
    )
    row = wheelTableInput.rowCount

    wheelTableInput.addCommandInput(icon, row, 0)
    wheelTableInput.addCommandInput(name, row, 1)
    wheelTableInput.addCommandInput(wheelType, row, 2)

def removeJointFromTable(joint):
    index = _joints.index(joint)
    _joints.remove(joint)
    
    for i in range(jointTableInput.rowCount):
        dropDown = jointTableInput.getInputAtPosition(i, 2)
        dropDown.listItems.item(index).deleteMe()
    
    jointTableInput.deleteRow(index)

def removeWheelFromTable(wheel):
    try:
        index = _wheels.index(wheel)
        _wheels.remove(wheel)
        wheelTableInput.deleteRow(index)

    except ValueError:
        pass
    except:
        if gm.ui:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
