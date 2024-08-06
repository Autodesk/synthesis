import os
import sys

from src.GlobalManager import GlobalManager

APP_NAME = "Synthesis"
APP_TITLE = "Synthesis Robot Exporter"
DESCRIPTION = "Exports files from Fusion into the Synthesis Format"
INTERNAL_ID = "Synthesis"
ADDIN_PATH = os.path.dirname(os.path.realpath(__file__))

A_EP = None  # TODO: Will be removed by GH-1010
DEBUG = True  # TODO: Will be removed by GH-1010

gm = GlobalManager()

__all__ = ["APP_NAME", "APP_TITLE", "DESCRIPTION", "INTERNAL_ID", "ADDIN_PATH", "gm"]

# Transition: AARD-1737
# This method of running the resolve dependencies module will be revisited in AARD-1734
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "proto", "proto_out")))
# from proto import deps
