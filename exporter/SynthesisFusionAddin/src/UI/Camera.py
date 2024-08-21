import os

import adsk.core

from src import SUPPORT_PATH
from src.Logging import logFailure
from src.Types import OString
from src.Util import makeDirectories


@logFailure
def captureThumbnail(size: int = 250) -> str:
    """
    ## Captures Thumbnail and saves it to a temporary path - needs to be cleared after or on startup
    - Size: int (Default: 200) : (width & height)
    """
    app = adsk.core.Application.get()
    originalCamera = app.activeViewport.camera

    name = "Thumbnail_{0}.png".format(
        app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0].replace(
            " ", ""
        )  # remove whitespace from just the filename
    )

    path = makeDirectories(f"{SUPPORT_PATH}/Resources/Icons/")
    path += name

    saveOptions = adsk.core.SaveImageFileOptions.create(path)
    saveOptions.height = size
    saveOptions.width = size
    saveOptions.isAntiAliased = True
    saveOptions.isBackgroundTransparent = True

    newCamera = app.activeViewport.camera
    newCamera.isFitView = True

    app.activeViewport.camera = newCamera
    app.activeViewport.saveAsImageFileWithOptions(saveOptions)
    app.activeViewport.camera = originalCamera

    return path


def clearIconCache() -> None:
    """## Deletes all of the files in the ' src/Resources/Icons '

    This is useful for now but should be cached in the event the app is closed and re-opened.
    """
    path = OString.ThumbnailPath("Whatever.png").getDirectory()  # type: ignore[attr-defined]

    for _r, _d, f in os.walk(path):
        for file in f:
            if ".png" in file:
                fp = os.path.join(path, file)
                os.remove(fp)
