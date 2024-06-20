"""General helper functions for operating system specific tasks.
"""

import platform


def getOSPath(*args: str) -> str:
    path = ""
    for arg in args:
        if platform.system() == "win32":
            path += rf"{arg}\\"
        else:
            path += rf"{arg}/"

    return path
