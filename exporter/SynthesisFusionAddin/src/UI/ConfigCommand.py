from ..general_imports import *
from ..configure import NOTIFIED, write_configuration
from ..Analytics.alert import showAnalyticsAlert
from . import Helper, FileDialogConfig, OsHelper

from ..Parser.ParseOptions import ParseOptions
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
tableCommandInput = adsk.core.TabCommandInput
selectedWheels = []

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

            global NOTIFIED, tableCommandInput
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

            onExecute = ConfigureCommandExecuteHandler(
                json.dumps(
                    previous, default=lambda o: o.__dict__, sort_keys=True, indent=1
                ),
                previous.filePath,
            )
            cmd.execute.add(onExecute)
            gm.handlers.append(onExecute)

            inputs_root = cmd.commandInputs

            inputs = inputs_root.addTabCommandInput("generalSettings", "General").children

            # This actually has the ability to entirely create a input from just a protobuf message which is really neat
            ## NOTE: actually is super neat but I don't have time at the moment
            # self.parseMessage(inputs)

            # This could be a button group
            dropdownExportMode = inputs.addDropDownCommandInput(
                "mode",
                "Export Mode",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            # dropdownExportMode.listItems.add("Unity", previous.general.exportMode == 0)
            dropdownExportMode.listItems.add("Dynamic Item", True)
            dropdownExportMode.listItems.add("Static Field Item", False)

            # dropdownExportMode.listItems.add(
            #    "Simulation", previous.general.exportMode == 1
            # )
            # dropdownExportMode.listItems.add("VR", previous.general.exportMode == 2)
            dropdownExportMode.tooltip = (
                "This will be future formats and or generic / advanced objects."
            )

            dropdownMaterialMode = inputs.addDropDownCommandInput(
                "renderMode",
                "Unity Project Type",
                dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
            )
            dropdownMaterialMode.listItems.add(
                "Standard", previous.general.RenderType == 0
            )
            dropdownMaterialMode.listItems.add("URP", previous.general.RenderType == 1)
            # dropdownMaterialMode.listItems.add("HDPR", previous.general.RenderType == 2)
            dropdownMaterialMode.tooltip = (
                "Change this to match the renderer Unity is currently using."
            )

            self.createBooleanInput(
                "materials",
                "Materials",
                inputs,
                checked=previous.general.material.checked,
                tooltip="Export Fusion 360 Materials into Unity.",
                tooltipadvanced="May be inconsistent dependening on type of material used.",
            )

            self.createBooleanInput(
                "joints",
                "Joints",
                inputs,
                checked=previous.general.joints.checked,
                tooltip="Export Fusion 360 Joints into Unity with accessibility scripts.",
                tooltipadvanced="May be inconsistent dependening on type of joint.",
                enabled=False,
            )

            self.createBooleanInput(
                "rigidGroups",
                "RigidGroups",
                inputs,
                checked=previous.general.rigidGroups.checked,
                tooltip="Export Rigidgroups into Unity",
                tooltipadvanced="This feature is still in development",
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

            # wheel configuration group
            wheelConfig = inputs.addGroupCommandInput("wheelconfig", "Wheel Configuration")
            wheelConfig.isExpanded = True
            wheelConfig.isEnabled = True
            wheelConfig.tooltip = "Specify drive-train wheel types in assembly"
            wheel_inputs = wheelConfig.children
            
            tableCommandInput = wheel_inputs.addTableCommandInput(
                "wheelselection", "", 2, "1:2"
            )

            # selection input
            wheelSelect = inputs.addSelectionInput("wheelselect", "Wheel Selection", "Select drive-train wheels in assembly.")
            wheelSelect.addSelectionFilter(adsk.fusion.BRepBodies) # limit selection to only bodies
            wheelSelect.setSelectionLimits(1) 

            # disable vr by default
            vr = inputs.addGroupCommandInput("vrsettings", "VR Settings")
            vr.isExpanded = False
            vr.isEnabled = False
            vr.tooltip = "Additional VR settings for VR mode projects, enables options for contact sets and additional data."
            vr_inputs = vr.children

            # selection = vr_inputs.addSelectionInput('contactsets', 'Contact Sets', 'Select any Body')
            # selection.addSelectionFilter("Bodies")

            tableInput = vr_inputs.addTableCommandInput(
                "contactsets", "Contact Sets", 3, "1:1:1"
            )
            # addRowToTable(tableInput)

            # Add inputs into the table.
            addButtonInput = vr_inputs.addBoolValueInput(
                "tableAdd", "Add Body", False, "", True
            )
            tableInput.addToolbarCommandInput(addButtonInput)
            deleteButtonInput = vr_inputs.addBoolValueInput(
                "tableDelete", "Remove Body", False, "", True
            )
            tableInput.addToolbarCommandInput(deleteButtonInput)

            self.createBooleanInput(
                "flatten",
                "Flatten Entire Hierarchy",
                vr_inputs,
                checked=True,
                tooltip="Flatten the model to just bodies with no nested transforms",
                tooltipadvanced="This may be easier to use in unity",
            )

            self.createBooleanInput(
                "condense",
                "Compress Model",
                vr_inputs,
                checked=False,
                tooltip="Compresses model into a single mesh.",
                tooltipadvanced="This may be easier to use in unity",
            )

            self.log.info("Created Configuration Input successfully")

            # so I don't need to re-write the above code
            # self.previous = previous

        except:
            self.log.error("Failed:\n{}".format(traceback.format_exc()))
            if A_EP:
                A_EP.send_exception()

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
                doc = gm.app.activeDocument
                design = doc.design
                name = design.rootComponent.name.rsplit(" ", 1)[0]
                version = design.rootComponent.name.rsplit(" ", 1)[1]

                render_dropdown = eventArgs.command.commandInputs.itemById(
                    "generalSettings"
                ).children.itemById("renderMode")
                renderer = 0
                dropdown = adsk.core.DropDownCommandInput.cast(render_dropdown)
                if dropdown.selectedItem.name == "Standard":
                    renderer = 0
                elif dropdown.selectedItem.name == "URP":
                    renderer = 1

                # now construct a ParseOptions and save the file
                # since self.current is already up to date might as well use it
                options = ParseOptions(
                    savepath,
                    name,
                    version,
                    materials=renderer,
                    joints=self.current.general.joints.checked,
                    mode=mode,
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

                _general.material.checked = generalSettingsInputs.itemById(
                    "materials"
                ).value
                _general.joints.checked = generalSettingsInputs.itemById("joints").value
                _general.rigidGroups.checked = generalSettingsInputs.itemById(
                    "rigidGroups"
                ).value

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
            selectedOccurrence = adsk.fusion.Occurrence.cast(args.selection.entity) 

            if selectedOccurrence:
                selectedWheels.append(selectedOccurrence)
                addWheelToTable(selectedOccurrence)
        except:
            if ui:
                ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


class MyUnSelectHandler(adsk.core.SelectionEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            selectedOccurrence = adsk.fusion.Occurrence.cast(args.selection.entity) 
            
            if selectedOccurrence:
                removeRowFromTable(selectedOccurrence)
                selectedWheels.remove(selectedOccurrence)
        except:
            if ui:
                ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


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
            eventArgs = adsk.core.InputChangedEventArgs.cast(args)
            cmdInput = eventArgs.input
            inputs = cmdInput.commandInputs

            if cmdInput.id == "mode":
                dropdown = adsk.core.DropDownCommandInput.cast(cmdInput)
                vr = inputs.itemById("vrsettings")
                if dropdown.selectedItem.name == "VR":
                    if vr:
                        vr.isEnabled = True
                        vr.isExpanded = True
                else:
                    if vr:
                        vr.isEnabled = False
                        vr.isExpanded = False

            if cmdInput.id == "joints":
                gm.ui.messageBox(
                    f"Joints are currently disabled, \n\nneeds updated mapping onto the new occurrence transforms."
                )
        except:
            self.log.error("Failed:\n{}".format(traceback.format_exc()))
            if A_EP:
                A_EP.send_exception()


def addWheelToTable(wheelOcc): # accepts a TableCommandInput
    global tableCommandInput
    
    try:
        # get the CommandInputs object associated with the parent command.
        cmdInputs = adsk.core.CommandInputs.cast(tableCommandInput.commandInputs)

        # Create three new command inputs.
        name =  cmdInputs.addTextBoxCommandInput("Occ_name", "Occ_name", wheelOcc.name, 3, True)

        dropdownWheelType = cmdInputs.addDropDownCommandInput(
            "wheelType",
            "Wheel Type",
            dropDownStyle=adsk.core.DropDownStyles.LabeledIconDropDownStyle,
        )
        dropdownWheelType.listItems.add(
            "Standard", previous.general.WheelType == 0
        )
        dropdownWheelType.listItems.add(
            "Omni", previous.general.WheelType == 1
            )

        dropdownWheelType.tooltip = (
            "Select your wheel type."
        )

        # Add the inputs to the table.
        row = tableCommandInput.rowCount
        
        tableCommandInput.addCommandInput(name, row, 0)
    except:
        if ui:
            ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

def removeRowFromTable(Occ):
    global tableCommandInput
    
    try:
        index = selectedWheels.index(Occ)
        tableCommandInput.deleteRow(index)
    except:
        if ui:
            ui.messageBox("Failed:\n{}".format(traceback.format_exc()))