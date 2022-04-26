from ..general_imports import *
from ..configure import CID, ANALYTICS, DEBUG

import urllib, sys, adsk.core

# Reference https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters

# Page Example
"""
    v=1              // Version.
    &tid=UA-XXXXX-Y  // Tracking ID / Property ID.
    &cid=555         // Anonymous Client ID.

    &t=pageview      // Pageview hit type.
    &dh=mydemo.com   // Document hostname.
    &dp=/home        // Page.
    &dt=homepage     // Title.
"""

# Event example
"""
    v=1              // Version.
    &tid=UA-XXXXX-Y  // Tracking ID / Property ID.
    &cid=555         // Anonymous Client ID.

    &t=event         // Event hit type
    &ec=video        // Event Category. Required.
    &ea=play         // Event Action. Required.
    &el=holiday      // Event label.
    &ev=300          // Event value.
"""

## Really should batch hits

# EVENT example final
# www.google-analytics.com/collect?v=1&tid=UA-188467590-1&cid=555&t=event&ec=action&et=export&el=robot&ev=1

# VIEW example final
# www.google-analytics.com/collect?v=1&tid=UA-188467590-1&cid=556&t=pageview&dp=exportView&dt=viewer

# FAILURES
# https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters#exception


class AnalyticsEndpoint:
    def __init__(self, tracking_id: str, version=1):
        """Creating a new Endpoint for analytics

        Args:
            tracking_id (str): tracking id UA-xxxxx
            client_id (str): Anon client ID - check guid
            version (int): Client version
        """
        self.logger = logging.getLogger(f"HellionFusion.HUI.{self.__class__.__name__}")
        self.url = "http://www.google-analytics.com"
        self.tracking = tracking_id
        self.a_id = CID
        self.version = version

    def __str__(self):
        return self.identity_string()

    def identity_string(self, cid=None) -> str:
        if cid is None:
            if self.a_id is None:
                cid = "anon"
            else:
                cid = self.a_id

        return (
            self.__form("tid", self.tracking)
            + self.__form("cid", CID)
            + self.__form("v", self.version)
            + self.__form("aip", 1)
        )

    def send_event(self, category: str, action: str, label="default", value=1) -> bool:
        """Send event happened

        Args:
            category (str): category of event (click , interaction , etc)
            action (str): name of the action (export, change setting, etc)
            label (str, optional): label for action for arbitrary data. Defaults to "".
            value (str, optional): value for the label. Defaults to "".
        """
        if ANALYTICS != True:
            return

        body_t = (self.identity_string()) + (
            self.__form("t", "event")
            + self.__form("ec", category)
            + self.__form("ea", action)
            + self.__form("el", label)
            + self.__form("ev", value)
        )
        # encode properly
        # params = json.dumps(body).encode("utf8")
        # print(body_t)
        return self.__send(body_t)

    def send_view(self, page: str, app="FusionUnity", title="") -> bool:
        if ANALYTICS != True:
            return

        body_view = (self.identity_string()) + (
            self.__form("t", "pageview")
            + self.__form("dh", f"{app}")
            + self.__form("dp", f"{page}")
            + self.__form("dt", title)
        )

        return self.__send(body_view)

    def send_exception(self) -> bool:
        """This sends the exception encountered to the developer through the analytics pageview section

        Args:
            e (Exception): exception to be formatted

        Returns:
            bool: success
        """

        err = f"{sys.exc_info()[1]}"

        ui = adsk.core.Application.get().userInterface

        res = ui.messageBox(
            f"The Unity Exporter Addin encountered an error during execution, would you like to send this information to the developer? \n - {err}",
            "Error",
            3,  # This is yes/no
            4,  # This is question icon
        )

        if res != 2:
            return

        body_t = (self.identity_string()) + (
            self.__form("t", "event")
            + self.__form("ec", "Exception")
            + self.__form("ea", "error")
            + self.__form("el", err)
            + self.__form("ev", 1)
        )

        self.__send(body_t)

        body_view = (self.identity_string()) + (
            self.__form("t", "pageview")
            + self.__form("dh", f"FusionUnity")
            + self.__form("dp", f"EXCEPTION")
            + self.__form("dt", err)
        )

        return self.__send(body_view)

    def __send(self, body) -> bool:
        try:
            # define user agent so this works
            headers = {}
            headers[
                "User-Agent"
            ] = "Mozilla/5.0 (X11; Linux i686) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.27 Safari/537.17"

            # print(f"{self.url}/collect?{body}")

            url = f"{self.url}/collect?{body}"

            if DEBUG:
                self.logger.debug(f"Sending request: \n {url}")

            req = urllib.request.Request(
                f"{self.url}/collect?{body}", data=b"", headers=headers
            )
            # makes the request
            response = urllib.request.urlopen(req)

            if response.code == 200:
                return True
            else:
                self.logger.error(f"Failed to log req : {body}")
                return False
        except:
            self.logger.error("Failed:\n{}".format(traceback.format_exc()))

    def __form(self, key: str, value: str) -> str:
        """This will format any string into a url encodable string

        Args:
            key (str): name
            value (str): encodable string

        Returns:
            str: attribute
        """
        value = urllib.parse.quote(str(value))
        return f"{key}={value}&"


# Old way I was testing, leave in for now in case I want to add exceptions etc.
# Maybe format into markdown etc
class URLString:
    def __init__(self, key: str, value: str):
        self.key = key
        self.value = value

    def build(self) -> str:
        value = urllib.parse.quote(str(value))
        return f"{key}={value}&"

    @staticmethod
    def build(key: str, value: str) -> str:
        value = urllib.parse.quote(str(value))
        return f"{key}={value}&"

    @classmethod
    def build(cls, key: str, value: str) -> object:
        return cls(key, value)
