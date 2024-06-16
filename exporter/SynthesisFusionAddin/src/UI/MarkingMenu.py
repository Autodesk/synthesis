from .Util.EventHandlers import *
from .Configuration.JointEditPanel import *
import adsk.core, adsk.fusion, traceback
import logging.handlers

# Ripped all the boiler plate from the example code: https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-c90ce6a2-c282-11e6-a365-3417ebc87622

# global mapping list of event handlers to keep them referenced for the duration of the command
# handlers = {}
handlers = []
cmdDefs = []
entities = []
occurrencesOfComponents = {}

DROPDOWN_ID = "SynthesisMain"
DROPDOWN_COLLISION_ID = "synthesis"
DROPDOWN_CONFIG_ID = "SynthesisConfig"

SEPARATOR = "LinearSeparator"

COMM_SELECT_DISABLED = "SelectDisabled"
COMM_ENABLE_ALL = "EnableAllCollision"
COMM_DISABLE_COLLISION = "DisableCollision"
COMM_ENABLE_COLLISION = "EnableCollision"
COMM_DELETE = "DeleteComponent"

COMM_EDIT_JOINT = "EditJoint"

def setupMarkingMenu(ui: adsk.core.UserInterface):
    print('Setting up mark up menu')
    handlers.clear()
    try:
        def setCollisionAttribute(occ: adsk.fusion.Occurrence, isEnabled: bool = True):
            attr = occ.attributes.itemByName("synthesis", "collision_off")
            if attr == None and not isEnabled:
                occ.attributes.add("synthesis", "collision_off", "true")
            elif attr != None and isEnabled:
                attr.deleteMe()

        def applyToSelfAndAllChildren(occ: adsk.fusion.Occurrence, modFunc):
            modFunc(occ)
            childLists = []
            childLists.append(occ.childOccurrences)
            counter = 1
            while len(childLists) > 0:
                childList = childLists.pop(0)
                for o in childList:
                    counter += 1
                    modFunc(o)
                    if o.childOccurrences.count > 0:
                        childLists.append(o.childOccurrences)

        def handleCommandExecute(args: adsk.core.CommandEventArgs):
            try:
                command = args.firingEvent.sender
                cmdDef = CommandDefinition.cast(command.parentCommandDefinition)
                if cmdDef:
                    if cmdDef.id == COMM_ENABLE_COLLISION:
                        # ui.messageBox('Enable')
                        if entities:
                            func = lambda occ: setCollisionAttribute(occ, True)
                            for e in entities:
                                occ = adsk.fusion.Occurrence.cast(e)
                                if occ:
                                    applyToSelfAndAllChildren(occ, func)
                    elif cmdDef.id == COMM_DISABLE_COLLISION:
                        # ui.messageBox('Disable')
                        if entities:
                            func = lambda occ: setCollisionAttribute(occ, False)
                            for e in entities:
                                occ = adsk.fusion.Occurrence.cast(e)
                                if occ:
                                    applyToSelfAndAllChildren(occ, func)
                    elif cmdDef.id == COMM_SELECT_DISABLED:
                        app = adsk.core.Application.get()
                        product = app.activeProduct
                        design = adsk.fusion.Design.cast(product)
                        ui.activeSelections.clear()
                        if design:
                            attrs = design.findAttributes(
                                "synthesis", "collision_off"
                            )
                            for attr in attrs:
                                for b in adsk.fusion.Occurrence.cast(
                                    attr.parent
                                ).bRepBodies:
                                    ui.activeSelections.add(b)
                    elif cmdDef.id == COMM_ENABLE_ALL:
                        app = adsk.core.Application.get()
                        product = app.activeProduct
                        design = adsk.fusion.Design.cast(product)
                        if design:
                            for attr in design.findAttributes(
                                "synthesis", "collision_off"
                            ):
                                attr.deleteMe()
                    # elif cmdDef.id == COMM_EDIT_JOINT:
                    #     if entities:
                    #         joint = adsk.fusion.Joint.case(entities[0])
                    #         if joint:
                    #             print('Joint edit')
                    else:
                        ui.messageBox("command {} triggered.".format(cmdDef.id))
                else:
                    ui.messageBox("No CommandDefinition")
            except:
                print('Error')
                ui.messageBox("command executed failed: {}").format(
                    traceback.format_exc()
                )
                logging.getLogger(f"{INTERNAL_ID}").error(
                    "Failed:\n{}".format(traceback.format_exc())
                )

        def handleMarkingMenu(args: adsk.core.MarkingMenuEventArgs):
            linearMenu = args.linearMarkingMenu
            linearMenu.controls.addSeparator(SEPARATOR)

            synthesisDropDown = linearMenu.controls.addDropDown(
                "Synthesis", "", DROPDOWN_ID
            )

            '''
                COLLISION
            '''
            synthesisCollisionDropDown = synthesisDropDown.controls.addDropDown(
                "Collision", "", DROPDOWN_COLLISION_ID
            )

            cmdSelectDisabled = ui.commandDefinitions.itemById(COMM_SELECT_DISABLED)
            synthesisCollisionDropDown.controls.addCommand(cmdSelectDisabled)

            synthesisCollisionDropDown.controls.addSeparator()

            cmdEnableAll = ui.commandDefinitions.itemById(COMM_ENABLE_ALL)
            synthesisCollisionDropDown.controls.addCommand(cmdEnableAll)
            synthesisCollisionDropDown.controls.addSeparator()

            if args.selectedEntities:
                sel0 = args.selectedEntities[0]
                occ = adsk.fusion.Occurrence.cast(sel0)

                if occ:
                    if not occ.attributes.itemByName("synthesis", "collision_off"):
                        cmdDisableCollision = ui.commandDefinitions.itemById(COMM_DISABLE_COLLISION)
                        synthesisCollisionDropDown.controls.addCommand(cmdDisableCollision)
                    else:
                        cmdEnableCollision = ui.commandDefinitions.itemById(COMM_ENABLE_COLLISION)
                        synthesisCollisionDropDown.controls.addCommand(cmdEnableCollision)

            '''
                CONFIGURATION
            '''
            synthesisConfigDropDown = synthesisDropDown.controls.itemById(DROPDOWN_CONFIG_ID)
            if synthesisConfigDropDown:
                synthesisConfigDropDown.deleteMe()
            
            synthesisConfigDropDown = synthesisDropDown.controls.addDropDown(
                "Config", "", DROPDOWN_CONFIG_ID
            )

            configEmpty = True

            if args.selectedEntities and len(args.selectedEntities) == 1:
                selectedJoint = adsk.fusion.Joint.cast(args.selectedEntities[0])
                
                if selectedJoint:
                    cmdEditJoint = ui.commandDefinitions.itemById(COMM_EDIT_JOINT)
                    synthesisConfigDropDown.controls.addCommand(cmdEditJoint)
                    configEmpty = False

            if configEmpty:
                synthesisConfigDropDown.deleteMe()

            '''
                Globals
            '''
            global occurrencesOfComponents

            # selected entities
            global entities
            entities.clear()
            entities = args.selectedEntities

        # Add customized handler for marking menu displaying
        ui.markingMenuDisplaying.add(MakeMarkingMenuEventHandler(handleMarkingMenu, handlers))

        # Add customized handler for commands creating

        def handleCommandCreated(args: adsk.core.CommandCreatedEventArgs):
            args.command.execute.add(MakeCommandExecuteHandler(handleCommandExecute, handlers))

        onCommandCreated = MakeCommandCreatedHandler(handleCommandCreated, handlers)

        # Disable Collision Button
        cmdDisableCollision = ui.commandDefinitions.itemById(COMM_DISABLE_COLLISION)
        if cmdDisableCollision:
            cmdDisableCollision.deleteMe()
        
        cmdDisableCollision = ui.commandDefinitions.addButtonDefinition(
            COMM_DISABLE_COLLISION,
            "Disable Collisions",
            "Disable collisions with this occurrence inside Synthesis",
        )
        cmdDisableCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdDisableCollision)

        # Enable Collision Button
        cmdEnableCollision = ui.commandDefinitions.itemById(COMM_ENABLE_COLLISION)
        if cmdEnableCollision:
            cmdEnableCollision.deleteMe()

        cmdEnableCollision = ui.commandDefinitions.addButtonDefinition(
            COMM_ENABLE_COLLISION,
            "Enable Collisions",
            "Enable collisions with this occurrence inside Synthesis",
        )
        cmdEnableCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdEnableCollision)

        # Enable All Collision Button
        cmdEnableAllCollision = ui.commandDefinitions.itemById(COMM_ENABLE_ALL)
        if cmdEnableAllCollision:
            cmdEnableAllCollision.deleteMe()

        cmdEnableAllCollision = ui.commandDefinitions.addButtonDefinition(
            COMM_ENABLE_ALL,
            "Enable All Collision",
            "Enable collisions for all occurrences in design",
        )
        cmdEnableAllCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdEnableAllCollision)

        # Select Disabled Button
        cmdSelectDisabled = ui.commandDefinitions.itemById(COMM_SELECT_DISABLED)
        if cmdSelectDisabled:
            cmdSelectDisabled.deleteMe()

        cmdSelectDisabled = ui.commandDefinitions.addButtonDefinition(
            COMM_SELECT_DISABLED,
            "Selected Collision Disabled Occurrences",
            "Select all occurrences labeled for collision disabled",
        )
        cmdSelectDisabled.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdSelectDisabled)

        # Delete Component Button
        cmdDeleteComponent = ui.commandDefinitions.itemById(COMM_DELETE)
        if cmdDeleteComponent:
            cmdDeleteComponent.deleteMe()

        cmdDeleteComponent = ui.commandDefinitions.addButtonDefinition(
            COMM_DELETE,
            "Delete All Occurrences",
            "Delete all occurrences with the same component",
        )
        cmdDeleteComponent.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdDeleteComponent)

        '''
            CONFIG COMMANDS
        '''
        def handleEditJoint(args: CommandCreatedEventArgs):
            if entities:
                joint = adsk.fusion.Joint.cast(entities[0])
                if joint:
                    BuildJointEditPanel(joint, args)

        onJointEditCreated = MakeCommandCreatedHandler(
            handleEditJoint,
            handlers
        )

        cmdEditJoint = ui.commandDefinitions.itemById(COMM_EDIT_JOINT)
        if cmdEditJoint:
            cmdEditJoint.deleteMe()

        cmdEditJoint = ui.commandDefinitions.addButtonDefinition(
            COMM_EDIT_JOINT, "Edit Joint", "Edit joint details for Synthesis"
        )
        cmdEditJoint.commandCreated.add(onJointEditCreated)
        cmdDefs.append(cmdEditJoint)

    except:
        ui.messageBox("Failed:\n{}".format(traceback.format_exc()))


def stopMarkingMenu(ui: adsk.core.UserInterface):
    try:
        for obj in cmdDefs:
            if obj.isValid:
                obj.deleteMe()
            else:
                ui.messageBox(str(obj) + " is not a valid object")

        handlers.clear()
    except:
        ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
