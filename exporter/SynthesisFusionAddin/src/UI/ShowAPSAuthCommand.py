from src.general_imports import root_logger, gm, INTERNAL_ID, APP_NAME, DESCRIPTION, my_addin_path
from src.APS.APS import CLIENT_ID, convertAuthToken, getCodeChallenge
import time
import webbrowser
import logging
import urllib.parse
import urllib.request
import json
import adsk.core, traceback
import os
from src.APS.APS import auth_path

class ShowAPSAuthCommandExecuteHandler(adsk.core.CommandEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            logging.getLogger(f"{INTERNAL_ID}").info(auth_path)
            logging.getLogger(f"{INTERNAL_ID}").info(os.path.abspath(auth_path))
            logging.getLogger(f"{INTERNAL_ID}").info(my_addin_path)
            palette = gm.ui.palettes.itemById('authPalette')
            if not palette:
                callbackUrl = 'http://localhost:3003/api/aps/exporter/'
                challenge = getCodeChallenge()
                params = {
                    'response_type': 'code',
                    'client_id': CLIENT_ID,
                    'redirect_uri': urllib.parse.quote_plus(callbackUrl),
                    'scope': 'data:read',
                    'nonce': time.time(),
                    'prompt': 'login',
                    'code_challenge': challenge,
                    'code_challenge_method': 'S256'
                }
                query = "&".join(map(lambda pair: f"{pair[0]}={pair[1]}", params.items()))
                url = 'https://developer.api.autodesk.com/authentication/v2/authorize?' + query
                logging.getLogger(f"{INTERNAL_ID}").info(url)
                palette = gm.ui.palettes.add('authPalette', 'APS Authentication', url, True, True, True, 400, 400)
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
            gm.ui.messageBox('Command executed failed: {}'.format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error('Command executed failed: {}'.format(traceback.format_exc()))

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

class MyCloseEventHandler(adsk.core.UserInterfaceGeneralEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            logging.getLogger(f"{INTERNAL_ID}").info('Close button is clicked')
            # gm.ui.messageBox('Close button is clicked')
        except:
            gm.ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error('Failed:\n{}'.format(traceback.format_exc()))

class MyHTMLEventHandler(adsk.core.HTMLEventHandler):
    def __init__(self):
        super().__init__()
    def notify(self, args):
        try:
            htmlArgs = adsk.core.HTMLEventArgs.cast(args)
            data = json.loads(htmlArgs.data)
            msg = "An event has been fired from the html to Fusion with the following data:\n"
            msg += f"     Command: {htmlArgs.action}\n     data: {htmlArgs.data}\n     code: {data['code']}"
            logging.getLogger(f"{INTERNAL_ID}").info(msg)
            # gm.ui.messageBox(msg)
            convertAuthToken(data['code'])
        except:
            gm.ui.messageBox('Failed:\n'.format(traceback.format_exc()))
            logging.getLogger(f"{INTERNAL_ID}").error('Failed:\n'.format(traceback.format_exc()))
