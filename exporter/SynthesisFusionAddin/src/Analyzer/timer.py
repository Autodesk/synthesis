""" Takes in a given function call and times and tests the memory allocations to get data
"""
from ..general_imports import *
from time import time
import os, inspect


class Timer:
    def __init__(self):
        self.logger = logging.getLogger(f"{INTERNAL_ID}.Analyzer.Timer")

        (
            self.filename,
            self.line_number,
            self.funcName,
            self.lines,
            _,
        ) = inspect.getframeinfo(
            inspect.currentframe().f_back.f_back
        )  # func name doesn't always work here

        self.stopped = False

    def start(self):
        self.stopped = False
        self.t0 = time()

    def stop(self):
        self.stopped = True
        self.t1 = time()

    def _str(self) -> str:
        if not self.stopped:
            self.stop()

        # should really just use format and limit the number
        return f"Timer \n Location: {os.path.basename(self.filename)} : {self.line_number} -> {str(self.funcName)}  \n \t - Time taken: {self.t1-self.t0} seconds \n"

    def log(self):
        self.logger.debug(self._str())

    def print(self):
        print(self._str())


class TimeThis:
    def __init__(self, function):
        """Function decorater to analyze calls to function

        Args:
            function (function): function to be analyzed
        """
        self.logger = logging.getLogger(f"{INTERNAL_ID}.Analyzer.TimeThis")
        self.function = function
        self.timer = Timer()

    def __call__(self, *args, **kwargs):
        if not DEBUG:
            # Basically just executing the same function
            self.function(*args, **kwargs)

        if DEBUG:
            # self.logger.debug(f"Timing: {self.function.__name__}")
            self.timer.start()
            self.function(*args, **kwargs)
            self.timer.stop()
            self.timer.log()
