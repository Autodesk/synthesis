"""File logging setup for the Synthesis Exporter.
"""

import logging.handlers
import os
import pathlib

from Synthesis import INTERNAL_ID
from Synthesis.OsHelper import getOSPath


def setupLogger():
    logLocation = pathlib.Path(__file__).parent
    path = getOSPath(f"{logLocation}", "logs")

    logHandler = logging.handlers.WatchedFileHandler(os.environ.get("LOGFILE", f"{path}{INTERNAL_ID}.log"), mode="w")
    logHandler.setFormatter(logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s"))

    log = logging.getLogger(f"{INTERNAL_ID}")
    log.setLevel(os.environ.get("LOGLEVEL", "DEBUG"))
    log.addHandler(logHandler)
