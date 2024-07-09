""" Initializes the global variables that are set in the run method to reduce hanging commands. """

import adsk.core
import adsk.fusion


class GlobalManager(object):
    """Global Manager instance"""

    class __GlobalManager:
        def __init__(self):
            self.app = adsk.core.Application.get()

            if self.app:
                self.ui = self.app.userInterface

            self.uniqueIds = []
            """ Collection of unique ID values to not overlap """

            self.elements = []
            """ Unique constructed buttons to delete """

            self.palettes = []
            """ Unique constructed palettes to delete """

            self.handlers = []
            """ Object to store all event handlers to custom events like saving. """

            self.tabs = []
            """ Set of Tab objects to keep track of. """

            self.queue = []
            """ This will eventually implement the Python SimpleQueue synchronized workflow
                - this is the list of objects being sent
            """

        def __str__(self):
            return "GlobalManager"

        def clear(self):
            for attr, value in self.__dict__.items():
                if isinstance(value, list):
                    setattr(self, attr, [])

    instance = None

    def __new__(cls):
        if not GlobalManager.instance:
            """
            (filename, line_number, function_name, lines, index) = inspect.getframeinfo(
                inspect.currentframe().f_back
            )
            logging.getLogger(f"HellionFusion.Runtime").debug(
                f"\n Called from {filename}\n \t - {lines} : {line_number} \n"
            )
            """
            GlobalManager.instance = GlobalManager.__GlobalManager()
        return GlobalManager.instance

    def __getattr__(self, name):
        return getattr(self.instance, name)

    def __setattr__(self, name):
        return setattr(self.instance, name)
