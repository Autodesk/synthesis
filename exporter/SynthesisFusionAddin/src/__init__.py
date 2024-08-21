import os
import platform
from pathlib import Path

from src.GlobalManager import GlobalManager
from src.Util import makeDirectories

APP_NAME = "Synthesis"
APP_TITLE = "Synthesis Robot Exporter"
DESCRIPTION = "Exports files from Fusion into the Synthesis Format"
INTERNAL_ID = "Synthesis"
ADDIN_PATH = os.path.dirname(os.path.realpath(__file__))
IS_RELEASE = str(Path(os.path.abspath(__file__)).parent.parent.parent.parent).split(os.sep)[-1] == "ApplicationPlugins"

SYSTEM = platform.system()
if SYSTEM == "Windows":
    SUPPORT_PATH = makeDirectories(os.path.expandvars(r"%appdata%\Autodesk\Synthesis"))
else:
    assert SYSTEM == "Darwin"
    SUPPORT_PATH = makeDirectories(f"{os.path.expanduser('~')}/.config/Autodesk/Synthesis")

gm = GlobalManager()

__all__ = [
    "APP_NAME",
    "APP_TITLE",
    "DESCRIPTION",
    "INTERNAL_ID",
    "ADDIN_PATH",
    "IS_RELEASE",
    "SYSTEM",
    "SUPPORT_PATH",
    "gm",
]
