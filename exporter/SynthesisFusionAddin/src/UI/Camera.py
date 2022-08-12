from ..general_imports import *

from . import Helper
import os

from ..Types.OString import OString

from adsk.core import SaveImageFileOptions


def captureThumbnail(size=250):
    """
    ## Captures Thumbnail and saves it to a temporary path - needs to be cleared after or on startup
    - Size: int (Default: 200) : (width & height)
    """
    app = adsk.core.Application.get()

    log = logging.getLogger("{INTERNAL_ID}.HUI.Camera")

    if Helper.check_solid_open():
        try:
            originalCamera = app.activeViewport.camera

            name = "Thumbnail_{0}.png".format(
                app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0].replace(
                    " ", ""
                )  # remove whitespace from just the filename
            )

            path = OString.ThumbnailPath(name)

            saveOptions = SaveImageFileOptions.create(str(path.getPath()))
            saveOptions.height = size
            saveOptions.width = size
            saveOptions.isAntiAliased = True
            saveOptions.isBackgroundTransparent = True

            newCamera = app.activeViewport.camera
            newCamera.isFitView = True

            app.activeViewport.camera = newCamera
            app.activeViewport.saveAsImageFileWithOptions(saveOptions)
            app.activeViewport.camera = originalCamera

            return str(path.getPath())

        except:
            if log:
                log.error("Failed\n{}".format(traceback.format_exc()))

            if A_EP:
                A_EP.send_exception()

    else:
        return None


def clearIconCache() -> None:
    """## Deletes all of the files in the ' src/Resources/Icons '

    This is useful for now but should be cached in the event the app is closed and re-opened.
    """
    path = OString.ThumbnailPath("Whatever.png").getDirectory()

    for _r, _d, f in os.walk(path):
        for file in f:
            if ".png" in file:
                fp = os.path.join(path, file)
                os.remove(fp)
