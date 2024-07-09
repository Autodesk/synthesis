import inspect
import functools
import logging.handlers
import os
import pathlib
import sys
import time
import traceback
from datetime import date, datetime
from typing import cast

import adsk.core

from .strings import INTERNAL_ID
from .UI.OsHelper import getOSPath

MAX_LOG_FILES_TO_KEEP = 10
TIMING_LEVEL = 25


class SynthesisLogger(logging.Logger):
    def timing(self, msg: str, *args: any, **kwargs: any) -> None:
        return self.log(TIMING_LEVEL, msg, *args, **kwargs)

    def cleanupHandlers(self) -> None:
        for handler in self.handlers:
            handler.close()


def setupLogger() -> None:
    now = datetime.now().strftime("%H-%M-%S")
    today = date.today()
    logFileFolder = getOSPath(f"{pathlib.Path(__file__).parent.parent}", "logs")
    logFiles = [os.path.join(logFileFolder, file) for file in os.listdir(logFileFolder) if file.endswith(".log")]
    logFiles.sort()
    if len(logFiles) >= MAX_LOG_FILES_TO_KEEP:
        for file in logFiles[: len(logFiles) - MAX_LOG_FILES_TO_KEEP]:
            os.remove(file)

    logFileName = f"{logFileFolder}{getOSPath(f'{INTERNAL_ID}-{today}-{now}.log')}"
    logHandler = logging.handlers.WatchedFileHandler(logFileName, mode="w")
    logHandler.setFormatter(logging.Formatter("%(name)s - %(levelname)s - %(message)s"))

    logging.setLoggerClass(SynthesisLogger)
    logging.addLevelName(TIMING_LEVEL, "TIMING")
    logger = getLogger(INTERNAL_ID)
    logger.setLevel(10)  # Debug
    logger.addHandler(logHandler)


def getLogger(name: str | None = None) -> SynthesisLogger:
    if not name:
        # Inspect the caller stack to automatically get the module from which the function is being called from.
        name = f"{INTERNAL_ID}.{'.'.join(inspect.getmodule(inspect.stack()[1][0]).__name__.split('.')[1:])}"

    return cast(SynthesisLogger, logging.getLogger(name))


# Log function failure decorator.
def logFailure(func: callable = None, /, *, messageBox: bool = False) -> callable:
    def wrap(func: callable) -> callable:
        @functools.wraps(func)
        def wrapper(*args: any, **kwargs: any) -> any:
            try:
                return func(*args, **kwargs)
            except BaseException:
                excType, excValue, excTrace = sys.exc_info()
                tb = traceback.TracebackException(excType, excValue, excTrace)
                formattedTb = "".join(list(tb.format())[2:])  # Remove the wrapper func from the traceback.
                clsName = ""
                if args and hasattr(args[0], "__class__"):
                    clsName = args[0].__class__.__name__ + "."

                getLogger(f"{INTERNAL_ID}.{clsName}{func.__name__}").error(f"Failed:\n{formattedTb}")
                if messageBox:
                    ui = adsk.core.Application.get().userInterface
                    ui.messageBox(f"Internal Failure: {formattedTb}", "Synthesis: Error")

        return wrapper

    if func is None:
        # Called with parens.
        return wrap

    # Called without parens.
    return wrap(func)


# Time function decorator.
def timed(func: callable) -> callable:
    def wrapper(*args: any, **kwargs: any) -> any:
        startTime = time.perf_counter()
        result = func(*args, **kwargs)
        endTime = time.perf_counter()
        runTime = f"{endTime - startTime:5f}s"

        clsName = ""
        if args and hasattr(args[0], "__class__"):
            clsName = args[0].__class__.__name__ + "."

        logger = getLogger(f"{INTERNAL_ID}.{clsName}{func.__name__}")
        logger.timing(f"Runtime of '{func.__name__}' took '{runTime}'.")
        return result

    return wrapper
