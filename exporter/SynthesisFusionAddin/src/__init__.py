import os
import platform

from src.GlobalManager import GlobalManager

APP_NAME = "Synthesis"
APP_TITLE = "Synthesis Robot Exporter"
APP_WEBSITE_URL = "https://synthesis.autodesk.com/fission/"
DESCRIPTION = "Exports files from Fusion into the Synthesis Format"
INTERNAL_ID = "Synthesis"
ADDIN_PATH = os.path.dirname(os.path.realpath(__file__))

SYSTEM = platform.system()
assert SYSTEM != "Linux"

gm = GlobalManager()

__all__ = ["APP_NAME", "APP_TITLE", "DESCRIPTION", "INTERNAL_ID", "ADDIN_PATH", "SYSTEM", "gm"]
