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


""" Old code I believe 
def openFileLocation(fileLoc: str) -> bool:
    osName = getOS()
    if osName == "Windows" or osName == "win32":
        # explorer is kinda sorta a symbolic link that can be broken if not careful so use this apparently
        # thank you stack overflow
        path = os.path.normpath(fileLoc)
        FILEBROWSER_PATH = os.path.join(os.getenv("WINDIR"), "explorer.exe")
        # this could use pOpen and call a background task if the wait causes a problem=
        subprocess.run([FILEBROWSER_PATH, "/select,", path])
        return True
    elif osName == "Darwin":
        # from .Camera import captureThumbnail
        # icon_name = captureThumbnail()
        # path = os.path.normpath(fileLoc)
        # subprocess.call(f"sips -i {icon_name}")
        subprocess.call(f"open -R {fileLoc}", shell=True)
        return True
    elif osName == "Linux":
        return False
    else:
        return False
"""
