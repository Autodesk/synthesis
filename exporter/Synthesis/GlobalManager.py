import adsk.core
import adsk.fusion

class GlobalManager:
    class __GlobalManager:
        def __init__(self):
            self.app = adsk.core.Application.get()

            if self.app:
                self.ui = self.app.userInterface

            """ Is unity currently connected """
            self.connected = False

            """ Collection of unique ID values to not overlap """
            self.uniqueIds = []

            """ Unique constructed buttons to delete """
            self.elements = []

            """ Unique constructed palettes to delete """
            self.palettes = []

            """ Object to store all event handlers to custom events like saving. """
            self.handlers = []

            """ Set of Tab objects to keep track of. """
            self.tabs = []

            """ This will eventually implement the Python SimpleQueue synchronized workflow
                - this is the list of objects being sent
            """
            self.queue = []

            self.files = []

        def __str__(self):
            return "GlobalManager"

    instance = None

    def __new__(cls):
        if not GlobalManager.instance:
            GlobalManager.instance = GlobalManager.__GlobalManager()
        return GlobalManager.instance

    def __getattr__(self, name):
        return getattr(self.instance, name)

    def __setattr__(self, name):
        return setattr(self.instance, name)
