""" Takes in a given function call and times and tests the memory allocations to get data
"""
from ..general_imports import *
from time import time
import tracemalloc, time, linecache, os, inspect


class Sniffer:
    def __init__(self):
        self.logger = logging.getLogger(f"{INTERNAL_ID}.Analyzer.Sniffer")

        (self.filename, self.line_number, _, self.lines, _) = inspect.getframeinfo(
            inspect.currentframe().f_back
        )

        self.stopped = False

    def start(self):
        self.stopped = False
        self.t0 = time.time()
        tracemalloc.start(5)

    def stop(self):
        self.stopped = True
        self.mem_size, self.mem_peak = tracemalloc.get_traced_memory()
        self.snapshot = tracemalloc.take_snapshot()
        self.t1 = time.time()
        tracemalloc.stop()

    def _str(self) -> str:
        if not self.stopped:
            self.stop()

        return f"Analyzer \n Location: {os.path.basename(self.filename)} : {self.line_number}  \n \t - Time taken: {self.t1-self.t0} seconds \n \t - Memory Allocated: {self.mem_size / 1024} Kb \n"

    def log(self):
        """Logs the memory usage and time taken"""
        self.logger.debug(self._str())

    def print(self):
        """Prints the memory usage and time taken"""
        print(self._str())

    def print_top(self):
        """Prints out the info on the top 10 memory blocks"""
        if not self.stopped:
            self.stop()
        print(display_top(self.snapshot))

    def log_top(self):
        """Logs out the info on the top 10 memory blocks"""
        if not self.stopped:
            self.stop()
        out = self._str() + "\n"
        self.logger.debug(out + display_top(self.snapshot))


# this prints the top 10 memory usages in snapchat for the process
# found on : https://docs.python.org/3/library/tracemalloc.html#functions
def display_top(snapshot, key_type="lineno", limit=10):
    snapshot = snapshot.filter_traces(
        (
            tracemalloc.Filter(False, "<frozen importlib._bootstrap>"),
            tracemalloc.Filter(False, "<unknown>"),
        )
    )
    top_stats = snapshot.statistics(key_type)

    out = ""

    out += "Top %s lines \n" % limit
    for index, stat in enumerate(top_stats[:limit], 1):
        frame = stat.traceback[0]
        out += "#%s: %s:\n\t%s: %.1f KiB" % (
            index,
            frame.filename,
            frame.lineno,
            stat.size / 1024,
        )
        line = linecache.getline(frame.filename, frame.lineno).strip()
        if line:
            out += "    %s \n" % line

    other = top_stats[limit:]
    if other:
        size = sum(stat.size for stat in other)
        out += "%s other: %.1f KiB \n" % (len(other), size / 1024)
    total = sum(stat.size for stat in top_stats)
    out += "Total allocated size: %.1f KiB \n" % (total / 1024)
    return out


def sniff(func, positional_arguments, keyword_arguments):
    """sniffs the current func with the positional and keyword arguments

    ex.
        method2(method1, ['spam'], {'ham': 'ham'})

    Args:
        func (function reference): Called function
        positional_arguments (positional args): args that are position dependent
        keyword_arguments (keyword args): args that are defined by keywords

    Returns:
        Any: result of func
    """
    t0 = time.time()
    tracemalloc.start()

    ret = func(*positional_arguments, **keyword_arguments)

    mem_size, mem_peak = tracemalloc.get_traced_memory()
    t1 = time.time()
    tracemalloc.stop()

    logging.getLogger(f"{INTERNAL_ID}.Analyzer").debug(
        f"Analyzer result on : {func.__name__} \n \t - Time taken: {t1-t0} \n \t - Memory Allocated: {mem_size} \n"
    )

    return ret


class Analyze:
    def __init__(self, function):
        """Function decorater to analyze calls to function

        Args:
            function (function): function to be analyzed
        """
        self.logger = logging.getLogger(f"{INTERNAL_ID}.Analyzer.Analyze")
        self.function = function
        self.sniffer = Sniffer()

    def __call__(self, *args, **kwargs):
        if not DEBUG:
            # Basically just executing the same function
            self.function(*args, **kwargs)

        if DEBUG:
            self.logger.debug(f"Analyzing: {self.function.__name__}")
            self.sniffer.start()
            self.function(*args, **kwargs)
            self.sniffer.stop()
            self.sniffer.log_top()


def timer_decorator(func):
    def timer_wrapper(*args, **kwargs):
        import datetime

        before = datetime.datetime.now()
        result = func(*args, **kwargs)
        after = datetime.datetime.now()
        return result
