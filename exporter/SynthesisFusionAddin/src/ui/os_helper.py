import os, platform


def getOSPath(*argv) -> str:
    """Takes n strings and constructs a OS specific path

    Returns:
        *str* -- OS specific local path
    """
    path = ""
    for arg in argv:
        if getOS() == "win32":
            path += arg + r"\\"
        else:
            path += arg + r"/"
    return path


def getOSPathPalette(*argv) -> str:
    """## This is a different delimeter than the resources path."""
    path = ""
    for arg in argv:
        path += arg + r"/"
    return path


def getDesktop():
    """Gets the Desktop Path.

    Returns:
        *str* -- Absolute Path to Desktop.
    """
    if getOS() == "Windows":
        return os.path.join(os.path.join(os.environ["USERPROFILE"]), "Desktop\\")
    else:
        return os.path.join(os.path.join(os.environ["USERPROFILE"]), "Desktop/")


def getOS():
    """## Returns platform as a string

    - Darwin
    - Linux
    - Windows or win32 ?
    """
    return platform.system()

