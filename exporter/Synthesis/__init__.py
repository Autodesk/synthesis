"""The Fusion Synthesis Robot Exporter.
"""

import os
import sys

from Synthesis.GlobalManager import GlobalManager

APP_NAME = "Synthesis"
APP_TITLE = "Synthesis Robot Exporter"
DESCRIPTION = "Exports files from Fusion into the Synthesis Format"
INTERNAL_ID = "synthesis"

PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
PATH_PROTO_FILES = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "proto", "proto_out"))

# TODO: Find out why this is needed
if not PATH in sys.path:
    sys.path.insert(1, PATH)

if not PATH_PROTO_FILES in sys.path:
    sys.path.insert(2, PATH_PROTO_FILES)

gm = GlobalManager()
