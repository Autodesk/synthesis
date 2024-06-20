"""File logging setup for the Synthesis Exporter.
"""

import logging
import logging.handlers
import os
import pathlib
import traceback

from typing import cast

from Synthesis import INTERNAL_ID
from Synthesis.OsHelper import getOSPath


class SynthesisLogger(logging.Logger):
    def logFailure(self) -> None:
        self.error(f"Failed:\n{traceback.format_exc()}")


def setupLogger() -> None:
    logging.setLoggerClass(SynthesisLogger)

    logLocation = pathlib.Path(__file__).parent
    path = getOSPath(f"{logLocation}", "logs")

    logHandler = logging.handlers.WatchedFileHandler(os.environ.get("LOGFILE", f"{path}{INTERNAL_ID}.log"), mode="w")
    logHandler.setFormatter(logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s"))

    log = logging.getLogger(f"{INTERNAL_ID}")
    log.setLevel(os.environ.get("LOGLEVEL", "DEBUG"))
    log.addHandler(logHandler)


def getLogger(name: str) -> SynthesisLogger:
    return cast(SynthesisLogger, logging.getLogger(name))


def logFailure(function: callable) -> callable:
    def wrapper(*args, **kwargs) -> any | None:
        try:
            return function(*args, **kwargs)
        except:
            logger = getLogger(INTERNAL_ID)
            logger.logFailure()
            return None

    return wrapper
