from ..general_imports import *


class HButtonCommandCreatedEvent(adsk.core.CommandCreatedEventHandler):
    """## Abstraction of CreatedEvent as its mostly useless in this context


    **adsk.core.CommandCreatedEventHandler** -- Parent abstract created event class
    """

    def __init__(self, button):
        super().__init__()
        self.button = button

    def notify(self, args):
        """## Called when parent button object is created and links the execute function pointer.

        Arguments:
            **args** *args* -- List of arbitrary info given to fusion event handlers.
        """
        cmd = adsk.core.CommandCreatedEventArgs.cast(args).command

        if self.button.check_func():
            onExecute = HButtonCommandExecuteHandler(self.button)
            cmd.execute.add(onExecute)
            self.button.handlers.append(onExecute)


class HButtonCommandExecuteHandler(adsk.core.CommandEventHandler):
    """## Abstraction of the CommandExecute Handler which will make a simple call to the function supplied by the button class.

    Parent Class:
    **adsk.core.CommandEventHandler** -- Fusion CommandEventHandler Abstract parent to link notify to ui.
    """

    def __init__(self, button):
        super().__init__()
        self.button = button

    def notify(self, _):
        self.button.exec_func()


""" OLD PALETTE COMMANDS
class HPaletteHTMLEventHandler(adsk.core.HTMLEventHandler):
    def __init__(self, palette):
        super().__init__()
        self.palette = palette

    def notify(self, args) -> None:
        ui = adsk.core.Application.get().userInterface
        try:
            htmlArgs = adsk.core.HTMLEventArgs.cast(args)

            for event in self.palette.events[0]:
                if event[0] == htmlArgs.action:
                    # if Helper.check_solid_open() :
                    val = event[1](htmlArgs.data)
                    if val is not None:
                        # logging.getLogger("HellionFusion.HUI.Handlers").debug(
                        #    f"{htmlArgs.action}: response: {val}"
                        # )
                        htmlArgs.returnData = val
                        return
                else:
                    htmlArgs.returnData = ""
        except:
            ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            
"""

""" These are old functions that mapped the palette commands

class CustomDocumentSavedHandler(adsk.core.DocumentEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args) -> bool:
        eventArgs = adsk.core.DocumentEventArgs.cast(args)
        name = Helper.getDocName()

        if name in gm.queue:
            connected = Helper.checkAttribute()
            if connected and (connected == "True"):
                try:
                    # req = DesignModificationLookup(gm.app.activeDocument.design)
                    # if req is not None:
                    #    sent = nm.send(req)
                    #    logging.getLogger('HellionFusion.HUI.Handlers.DocumentSaved').debug(f'Sending update with data: {req}')

                    design = gm.app.activeDocument.design
                    name = design.rootComponent.name.rsplit(" ", 1)[0]
                    version = design.rootComponent.name.rsplit(" ", 1)[1]

                    # version comes back the same - this is terrible but it will do
                    version = int(version[1:])
                    version += 1
                    version = f"v{version}"

                    Helper.addUnityAttribute()
                    req = parser(parseOptions=ParseOptions()).parseUpdated(version)
                    if req is not None:
                        sent = nm.send(req)
                    else:
                        logging.getLogger(
                            "HellionFusion.HUI.Handlers.DocumentSave"
                        ).error(
                            f"Failed to Parse Update or generate request ----- \n {req}"
                        )
                    return True

                except:
                    gm.ui.messageBox()
                    logging.getLogger("HellionFusion.HUI.Handlers.DocumentSave").error(
                        "Failed:\n{}".format(traceback.format_exc())
                    )

                # TODO: add item to queue here
                # let the Network manager send them and control how they are sent
                # if len(gm.palettes) >= 1:
                #    palette = gm.ui.palettes.itemById(gm.palettes[0].uid)
                #    if palette:
                #        name = Helper.getDocName()
        else:
            return False


class ConnectionPaletteHandler(adsk.core.CustomEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        ui = adsk.core.Application.get().userInterface
        try:
            if ui.activeCommand != "SelectCommand":
                ui.commandDefinitions.itemById("SelectCommand").execute()

            if len(gm.palettes) >= 1:
                palette = ui.palettes.itemById(gm.palettes[0].uid)
                if palette:
                    res = palette.sendInfoToHTML(
                        "updateConnection", nm.NetCommand.connected()
                    )
        except:
            if ui:
                ui.messageBox("Failed:\n{}".format(traceback.format_exc()))

"""
