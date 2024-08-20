from typing import Callable

import adsk.core
import adsk.fusion

from src.Logging import getLogger, logFailure

# Ripped all the boiler plate from the example code: https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-c90ce6a2-c282-11e6-a365-3417ebc87622

# global mapping list of event handlers to keep them referenced for the duration of the command
# handlers = {}
handlers: list[adsk.core.CommandEventHandler] = []
cmdDefs: list[adsk.core.CommandDefinition] = []
entities: list[adsk.fusion.Occurrence] = []

logger = getLogger()


@logFailure(messageBox=True)
def setupMarkingMenu(ui: adsk.core.UserInterface) -> None:
    handlers.clear()

    @logFailure(messageBox=True)
    def setLinearMarkingMenu(args: adsk.core.MarkingMenuEventArgs) -> None:
        linearMenu = args.linearMarkingMenu
        linearMenu.controls.addSeparator("LinearSeparator")

        synthDropDown = linearMenu.controls.addDropDown("Synthesis", "", "synthesis")

        cmdSelectDisabled = ui.commandDefinitions.itemById("SelectDisabled")
        synthDropDown.controls.addCommand(cmdSelectDisabled)

        synthDropDown.controls.addSeparator()

        cmdEnableAll = ui.commandDefinitions.itemById("EnableAllCollision")
        synthDropDown.controls.addCommand(cmdEnableAll)
        synthDropDown.controls.addSeparator()

        if args.selectedEntities:
            sel0 = args.selectedEntities[0]
            occ = adsk.fusion.Occurrence.cast(sel0)

            if occ:
                if occ.attributes.itemByName("synthesis", "collision_off") == None:
                    cmdDisableCollision = ui.commandDefinitions.itemById("DisableCollision")
                    synthDropDown.controls.addCommand(cmdDisableCollision)
                else:
                    cmdEnableCollision = ui.commandDefinitions.itemById("EnableCollision")
                    synthDropDown.controls.addCommand(cmdEnableCollision)

    def setCollisionAttribute(occ: adsk.fusion.Occurrence, isEnabled: bool = True) -> None:
        attr = occ.attributes.itemByName("synthesis", "collision_off")
        if attr == None and not isEnabled:
            occ.attributes.add("synthesis", "collision_off", "true")
        elif attr != None and isEnabled:
            attr.deleteMe()

    def applyToSelfAndAllChildren(
        occ: adsk.fusion.Occurrence, modFunc: Callable[[adsk.fusion.Occurrence], None]
    ) -> None:
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
        @logFailure(messageBox=True)
        def notify(self, args: adsk.core.CommandCreatedEventArgs) -> None:
            command = args.command
            onCommandExcute = MyCommandExecuteHandler()
            handlers.append(onCommandExcute)
            command.execute.add(onCommandExcute)

    class MyCommandExecuteHandler(adsk.core.CommandEventHandler):
        @logFailure(messageBox=True)
        def notify(self, args: adsk.core.CommandEventArgs) -> None:
            command = args.firingEvent.sender
            cmdDef = command.parentCommandDefinition
            if cmdDef:
                if cmdDef.id == "EnableCollision":
                    # ui.messageBox('Enable')
                    if entities:
                        func = lambda occ: setCollisionAttribute(occ, True)
                        for e in entities:
                            occ = adsk.fusion.Occurrence.cast(e)
                            if occ:
                                applyToSelfAndAllChildren(occ, func)
                elif cmdDef.id == "DisableCollision":
                    # ui.messageBox('Disable')
                    if entities:
                        func = lambda occ: setCollisionAttribute(occ, False)
                        for e in entities:
                            occ = adsk.fusion.Occurrence.cast(e)
                            if occ:
                                applyToSelfAndAllChildren(occ, func)
                elif cmdDef.id == "SelectDisabled":
                    app = adsk.core.Application.get()
                    product = app.activeProduct
                    design = adsk.fusion.Design.cast(product)
                    ui.activeSelections.clear()
                    if design:
                        attrs = design.findAttributes("synthesis", "collision_off")
                        for attr in attrs:
                            for b in adsk.fusion.Occurrence.cast(attr.parent).bRepBodies:
                                ui.activeSelections.add(b)
                elif cmdDef.id == "EnableAllCollision":
                    app = adsk.core.Application.get()
                    product = app.activeProduct
                    design = adsk.fusion.Design.cast(product)
                    if design:
                        for attr in design.findAttributes("synthesis", "collision_off"):
                            attr.deleteMe()
                else:
                    ui.messageBox("command {} triggered.".format(cmdDef.id))
            else:
                ui.messageBox("No CommandDefinition")

    class MyMarkingMenuHandler(adsk.core.MarkingMenuEventHandler):
        @logFailure(messageBox=True)
        def notify(self, args: adsk.core.CommandEventArgs) -> None:
            setLinearMarkingMenu(args)

            # selected entities
            global entities
            entities.clear()
            entities = args.selectedEntities

    # Add customized handler for marking menu displaying
    onMarkingMenuDisplaying = MyMarkingMenuHandler()
    handlers.append(onMarkingMenuDisplaying)
    ui.markingMenuDisplaying.add(onMarkingMenuDisplaying)

    # Add customized handler for commands creating
    onCommandCreated = MyCommandCreatedEventHandler()
    handlers.append(onCommandCreated)

    cmdDisableCollision = ui.commandDefinitions.itemById("DisableCollision")
    if not cmdDisableCollision:
        cmdDisableCollision = ui.commandDefinitions.addButtonDefinition(
            "DisableCollision",
            "Disable Collisions",
            "Disable collisions with this occurrence inside Synthesis",
        )
        cmdDisableCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdDisableCollision)
    cmdEnableCollision = ui.commandDefinitions.itemById("EnableCollision")
    if not cmdEnableCollision:
        cmdEnableCollision = ui.commandDefinitions.addButtonDefinition(
            "EnableCollision",
            "Enable Collisions",
            "Enable collisions with this occurrence inside Synthesis",
        )
        cmdEnableCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdEnableCollision)
    cmdEnableAllCollision = ui.commandDefinitions.itemById("EnableAllCollision")
    if not cmdEnableAllCollision:
        cmdEnableAllCollision = ui.commandDefinitions.addButtonDefinition(
            "EnableAllCollision",
            "Enable All Collision",
            "Enable collisions for all occurrences in design",
        )
        cmdEnableAllCollision.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdEnableAllCollision)

    cmdSelectDisabled = ui.commandDefinitions.itemById("SelectDisabled")
    if not cmdSelectDisabled:
        cmdSelectDisabled = ui.commandDefinitions.addButtonDefinition(
            "SelectDisabled",
            "Selected Collision Disabled Occurrences",
            "Select all occurrences labeled for collision disabled",
        )
        cmdSelectDisabled.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdSelectDisabled)

    cmdDeleteComponent = ui.commandDefinitions.itemById("DeleteComponent")
    if not cmdDeleteComponent:
        cmdDeleteComponent = ui.commandDefinitions.addButtonDefinition(
            "DeleteComponent",
            "Delete All Occurrences",
            "Delete all occurrences with the same component",
        )
        cmdDeleteComponent.commandCreated.add(onCommandCreated)
        cmdDefs.append(cmdDeleteComponent)


@logFailure(messageBox=True)
def stopMarkingMenu(ui: adsk.core.UserInterface) -> None:
    for obj in cmdDefs:
        if obj.isValid:
            obj.deleteMe()
        else:
            logger.warn(f"{str(obj)} is not a valid object")

    cmdDefs.clear()
    handlers.clear()
