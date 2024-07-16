import json
import logging
import os
import time
import traceback
import urllib.parse
import urllib.request
import webbrowser

import adsk.core

from src.APS.APS import CLIENT_ID, auth_path, convertAuthToken, getCodeChallenge
from src.general_imports import (
    APP_NAME,
    DESCRIPTION,
    INTERNAL_ID,
    gm,
    my_addin_path,
    root_logger,
)

palette = None


class ShowAPSAuthCommandExecuteHandler(adsk.core.CommandEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            global palette
            palette = gm.ui.palettes.itemById("authPalette")
            if not palette:
                callbackUrl = "http://localhost:80/api/aps/exporter/"
                challenge = getCodeChallenge()
                if challenge is None:
                    logging.getLogger(f"{INTERNAL_ID}").error(
                        "Code challenge is None when attempting to authorize for APS."
                    )
                    return
                params = {
                    "response_type": "code",
                    "client_id": CLIENT_ID,
                    "redirect_uri": urllib.parse.quote_plus(callbackUrl),
                    "scope": "data:create data:write data:search",
                    "nonce": time.time(),
                    "prompt": "login",
                    "code_challenge": challenge,
                    "code_challenge_method": "S256",
                }
                query = "&".join(map(lambda pair: f"{pair[0]}={pair[1]}", params.items()))
                url = "https://developer.api.autodesk.com/authentication/v2/authorize?" + query
                palette = gm.ui.palettes.add("authPalette", "APS Authentication", url, True, True, True, 400, 400)
                palette.dockingState = adsk.core.PaletteDockingStates.PaletteDockStateRight
                # register events
                onHTMLEvent = MyHTMLEventHandler()
                palette.incomingFromHTML.add(onHTMLEvent)
                gm.handlers.append(onHTMLEvent)

                onClosed = MyCloseEventHandler()
                palette.closed.add(onClosed)
                gm.handlers.append(onClosed)
            else:
                palette.isVisible = True
        except:
            gm.ui.messageBox("Command executed failed: {}".format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error("Command executed failed: {}".format(traceback.format_exc()))
            if palette:
                palette.deleteMe()


class ShowAPSAuthCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    def __init__(self, configure):
        super().__init__()

    def notify(self, args):
        try:
            command = args.command
            onExecute = ShowAPSAuthCommandExecuteHandler()
            command.execute.add(onExecute)
            gm.handlers.append(onExecute)
        except:
            gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))
            if palette:
                palette.deleteMe()


class SendInfoCommandExecuteHandler(adsk.core.CommandEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        pass


class SendInfoCommandCreatedHandler(adsk.core.CommandCreatedEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            command = args.command
            onExecute = SendInfoCommandExecuteHandler()
            command.execute.add(onExecute)
            gm.handlers.append(onExecute)
        except:
            gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))
            if palette:
                palette.deleteMe()


class MyCloseEventHandler(adsk.core.UserInterfaceGeneralEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            if palette:
                palette.deleteMe()
            # gm.ui.messageBox('Close button is clicked')
        except:
            gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error("Failed:\n{}".format(traceback.format_exc()))
            if palette:
                palette.deleteMe()


class MyHTMLEventHandler(adsk.core.HTMLEventHandler):
    def __init__(self):
        super().__init__()

    def notify(self, args):
        try:
            htmlArgs = adsk.core.HTMLEventArgs.cast(args)
            data = json.loads(htmlArgs.data)
            # gm.ui.messageBox(msg)

            convertAuthToken(data["code"])
        except:
            gm.ui.messageBox("Failed:{}\n".format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error("Failed:{}\n".format(traceback.format_exc()))
        if palette:
            palette.deleteMe()
