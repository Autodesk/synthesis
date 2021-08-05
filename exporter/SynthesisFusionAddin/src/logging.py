""" Module to create and setup the logger so that logging is accessible for other internal modules
"""

import logging.handlers
from datetime import datetime
import os, pathlib

from .UI.OsHelper import getOSPath
from .strings import *


def setupLogger():
    """setupLogger() will setup the file-watcher and writer to write to the log file

    Sub modules can access their own specific logger if they wish using the logging.getLogger(HellionFusion.submodule)
    """
    _now = datetime.now().strftime("-%H%M%S")

    loc = pathlib.Path(__file__).parent.parent
    path = getOSPath(f"{loc}", "logs")

    _log_handler = logging.handlers.WatchedFileHandler(
        os.environ.get("LOGFILE", f"{path}{INTERNAL_ID}.log"), mode="w"
    )

    # This will make it so I can see the auxiliary logging levels of each of the subclasses
    _log_handler.setFormatter(
        logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s")
    )

    log = logging.getLogger(f"{INTERNAL_ID}")
    log.setLevel(os.environ.get("LOGLEVEL", "DEBUG"))
    log.addHandler(_log_handler)

    # returns the root level logger to the global namespace
    return log, _log_handler
