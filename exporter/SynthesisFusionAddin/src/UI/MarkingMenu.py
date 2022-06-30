import adsk.core, adsk.fusion, traceback
import logging.handlers

# Ripped all the boiler plate from the example code: https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-c90ce6a2-c282-11e6-a365-3417ebc87622

# global mapping list of event handlers to keep them referenced for the duration of the command
#handlers = {}
handlers = []
cmdDefs = []
entities = []
occurrencesOfComponents = {}

def setupMarkingMenu(ui: adsk.core.UserInterface):
    handlers.clear()
    try:
        
        def setLinearMarkingMenu(args):
            try:
                menuArgs = adsk.core.MarkingMenuEventArgs.cast(args)
                    
                linearMenu = menuArgs.linearMarkingMenu
                linearMenu.controls.addSeparator('LinearSeparator')

                synthDropDown = linearMenu.controls.addDropDown('Synthesis', '', 'synthesis')

                cmdSelectDisabled = ui.commandDefinitions.itemById('SelectDisabled')
                synthDropDown.controls.addCommand(cmdSelectDisabled)

                synthDropDown.controls.addSeparator()
                
                cmdEnableAll = ui.commandDefinitions.itemById('EnableAllCollision')
                synthDropDown.controls.addCommand(cmdEnableAll)
                synthDropDown.controls.addSeparator()
                
                if args.selectedEntities:
                    sel0 = args.selectedEntities[0]
                    occ = adsk.fusion.Occurrence.cast(sel0)

                    if occ:
                        if occ.attributes.itemByName('synthesis', 'collision_off') == None:
                            cmdDisableCollision = ui.commandDefinitions.itemById('DisableCollision')
                            synthDropDown.controls.addCommand(cmdDisableCollision)
                        else:
                            cmdEnableCollision = ui.commandDefinitions.itemById('EnableCollision')
                            synthDropDown.controls.addCommand(cmdEnableCollision)     
            except:
                if ui:
                    ui.messageBox('setting linear menu failed: {}').format(traceback.format_exc())

        def setCollisionAttribute(occ: adsk.fusion.Occurrence, isEnabled: bool = True):
            attr = occ.attributes.itemByName('synthesis', 'collision_off')
            if attr == None and not isEnabled:
                occ.attributes.add('synthesis', 'collision_off', 'true')
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

        class MyCommandCreatedEventHandler(adsk.core.CommandCreatedEventHandler):
            def __init__(self):
                super().__init__()
            def notify(self, args):
                try:
                    command = args.command                                     
                    onCommandExcute = MyCommandExecuteHandler()
                    handlers.append(onCommandExcute)
                    command.execute.add(onCommandExcute)
                except:
                    ui.messageBox('command created failed: {}').format(traceback.format_exc())
        
        class MyCommandExecuteHandler(adsk.core.CommandEventHandler):
            def __init__(self):
                super().__init__()
            def notify(self, args):
                try:
                    command = args.firingEvent.sender
                    cmdDef = command.parentCommandDefinition
                    if cmdDef:
                        if cmdDef.id == 'EnableCollision':
                            # ui.messageBox('Enable')
                            if entities:
                                func = lambda occ: setCollisionAttribute(occ, True)
                                for e in entities:
                                    occ = adsk.fusion.Occurrence.cast(e)
                                    if occ:
                                        applyToSelfAndAllChildren(occ, func)
                        elif cmdDef.id == 'DisableCollision':
                            # ui.messageBox('Disable')
                            if entities:
                                func = lambda occ: setCollisionAttribute(occ, False)
                                for e in entities:
                                    occ = adsk.fusion.Occurrence.cast(e)
                                    if occ:
                                        applyToSelfAndAllChildren(occ, func)
                        elif cmdDef.id == 'SelectDisabled':
                            app = adsk.core.Application.get()
                            product = app.activeProduct
                            design = adsk.fusion.Design.cast(product)
                            ui.activeSelections.clear()
                            if design:
                                attrs = design.findAttributes('synthesis', 'collision_off')
                                for attr in attrs:
                                    for b in adsk.fusion.Occurrence.cast(attr.parent).bRepBodies:
                                        ui.activeSelections.add(b)
                        elif cmdDef.id == 'EnableAllCollision':
                            app = adsk.core.Application.get()
                            product = app.activeProduct
                            design = adsk.fusion.Design.cast(product)
                            if design:
                                for attr in design.findAttributes('synthesis', 'collision_off'):
                                    attr.deleteMe()
                        else:
                            ui.messageBox('command {} triggered.'.format(cmdDef.id))
                    else:
                        ui.messageBox('No CommandDefinition')
                except:
                    ui.messageBox('command executed failed: {}').format(traceback.format_exc())
                    logging.getLogger(f"{INTERNAL_ID}").error(
                        "Failed:\n{}".format(traceback.format_exc())
                    )

        class MyMarkingMenuHandler(adsk.core.MarkingMenuEventHandler):
            def __init__(self):
                super().__init__()
            def notify(self, args):
                try:
                    setLinearMarkingMenu(args)

                    global occurrencesOfComponents

                    # selected entities
                    global entities
                    entities.clear()
                    entities = args.selectedEntities
                except:
                    if ui:
                        ui.messageBox('Marking Menu Displaying event failed: {}'.format(traceback.format_exc()))
        
        # Add customized handler for marking menu displaying
        onMarkingMenuDisplaying = MyMarkingMenuHandler()                   
        handlers.append(onMarkingMenuDisplaying)                     
        ui.markingMenuDisplaying.add(onMarkingMenuDisplaying)
        
        # Add customized handler for commands creating
        onCommandCreated = MyCommandCreatedEventHandler()
        handlers.append(onCommandCreated)

        cmdDisableCollision = ui.commandDefinitions.itemById('DisableCollision')
        if not cmdDisableCollision:
            cmdDisableCollision = ui.commandDefinitions.addButtonDefinition('DisableCollision', 'Disable Collisions', 'Disable collisions with this occurrence inside Synthesis')
            cmdDisableCollision.commandCreated.add(onCommandCreated)
            cmdDefs.append(cmdDisableCollision)
        cmdEnableCollision = ui.commandDefinitions.itemById('EnableCollision')
        if not cmdEnableCollision:
            cmdEnableCollision = ui.commandDefinitions.addButtonDefinition('EnableCollision', 'Enable Collisions', 'Enable collisions with this occurrence inside Synthesis')
            cmdEnableCollision.commandCreated.add(onCommandCreated)
            cmdDefs.append(cmdEnableCollision)
        cmdEnableAllCollision = ui.commandDefinitions.itemById('EnableAllCollision')
        if not cmdEnableAllCollision:
            cmdEnableAllCollision = ui.commandDefinitions.addButtonDefinition('EnableAllCollision', 'Enable All Collision', 'Enable collisions for all occurrences in design')
            cmdEnableAllCollision.commandCreated.add(onCommandCreated)
            cmdDefs.append(cmdEnableAllCollision)
        
        cmdSelectDisabled = ui.commandDefinitions.itemById('SelectDisabled')
        if not cmdSelectDisabled:
            cmdSelectDisabled = ui.commandDefinitions.addButtonDefinition('SelectDisabled', 'Selected Collision Disabled Occurrences', 'Select all occurrences labeled for collision disabled')
            cmdSelectDisabled.commandCreated.add(onCommandCreated)
            cmdDefs.append(cmdSelectDisabled)

        cmdDeleteComponent = ui.commandDefinitions.itemById('DeleteComponent')
        if not cmdDeleteComponent:
            cmdDeleteComponent = ui.commandDefinitions.addButtonDefinition('DeleteComponent', 'Delete All Occurrences', 'Delete all occurrences with the same component')
            cmdDeleteComponent.commandCreated.add(onCommandCreated)
            cmdDefs.append(cmdDeleteComponent)      
        
    except:
        ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

def stopMarkingMenu(ui: adsk.core.UserInterface):
    try:
        for obj in cmdDefs:
            if obj.isValid:
                obj.deleteMe()
            else:
                ui.messageBox(str(obj) + ' is not a valid object')

        handlers.clear()
    except:
        ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))