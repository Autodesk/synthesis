from ..general_imports import *

from . import Helper
import os

from ..Types.OString import OString


def captureThumbnail() -> str:
    """## Captures a thumbnail and store it at src/Resources/Icons
    - Returns: str (path of the icon) (currently formatted for windows only)
    TODO: create a better solution than this - or replace it with a static image
    """
    app = adsk.core.Application.get()

    log = logging.getLogger("{INTERNAL_ID}.HUI.Camera")

    if Helper.check_solid_open():
        try:
            originalCamera = app.activeViewport.camera

            # TODO: Update this to work with OSX - return a PATH object
            # Ill create a HStr
            # path = "{}\\..\\Resources\\Icons\\Thumbnail_{}.jpg".format(
            #    os.path.dirname(os.path.abspath(__file__)),
            #    app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0],
            # )

            name = "Thumbnail_{0}.png".format(
                app.activeDocument.design.rootComponent.name.rsplit(" ", 1)[0]
            )

            # log.info(name)

            path = OString.ThumbnailPath(name)

            # path = getOSPath("src", "Resources", "Icons", name)

            newCamera = app.activeViewport.camera
            newCamera.isFitView = True

            app.activeViewport.camera = newCamera
            app.activeViewport.saveAsImageFile(str(path.getPath()), 500, 500)
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

    for r, d, f in os.walk(path):
        for file in f:
            if ".png" in file:
                fp = os.path.join(path, file)
                os.remove(fp)
