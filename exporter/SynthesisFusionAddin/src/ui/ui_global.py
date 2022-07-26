from ..Analytics.poster import AnalyticsEndpoint
from ..general_imports import *


class UiGlobal:
    """
    UiGlobal.INPUTS_ROOT (adsk.fusion.CommandInputs):
        - Provides access to the set of all commandInput ui elements in the panel
    """

    INPUTS_ROOT = None

    compress = False

    gamepiece_list_global = []
    wheel_list_global = []
    joint_list_global = []

    A_EP = AnalyticsEndpoint("UA-81892961-6", 1)

    @staticmethod
    def wheel_table():
        """### Returns the wheel table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("wheel_table")

    @staticmethod
    def joint_table():
        """### Returns the joint table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("joint_table")

    @staticmethod
    def gamepiece_table():
        """### Returns the gamepiece table command input

        Returns:
            adsk.fusion.TableCommandInput
        """
        return UiGlobal.INPUTS_ROOT.itemById("gamepiece_table")

    @staticmethod
    def send_event(category, action, label="default", value=1):
        if UiGlobal.A_EP:
            res = UiGlobal.A_EP.send_event(category, action, label, value)
            if not res:
                logging.getLogger(f"{INTERNAL_ID}.import_manager").error(
                    "failed to post analytics")
            else:
                logging.getLogger(f"{INTERNAL_ID}.import_manager").error(
                    "succeeded in posting analytics")
            return res
        else:
            logging.getLogger(f"{INTERNAL_ID}.import_manager").error(
                "failed to initialize analytics")
