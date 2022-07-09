import adsk.fusion, adsk.core, traceback, logging
from ..general_imports import *

def createTextGraphics(wheel: adsk.fusion.Occurrence, _wheels) -> None:
    try:
        design = gm.app.activeDocument.design

        boundingBox = wheel.boundingBox # occurrence bounding box
        
        min = boundingBox.minPoint.asArray() # [x, y, z] min coords
        max = boundingBox.maxPoint.asArray() # [x, y, z] max coords

        if design:
            graphics = gm.app.activeDocument.design.rootComponent.customGraphicsGroups.add()
            matrix = adsk.core.Matrix3D.create()
            matrix.translation = adsk.core.Vector3D.create(min[0], min[1]-5, min[2])

            billBoard = adsk.fusion.CustomGraphicsBillBoard.create(adsk.core.Point3D.create(0, 0, 0))
            billBoard.billBoardStyle = adsk.fusion.CustomGraphicsBillBoardStyles.ScreenBillBoardStyle

            text = str(_wheels.index(wheel)+1)
            graphicsText = graphics.addText(text, 'Arial Black', 6, matrix)
            graphicsText.billBoarding = billBoard # make the text follow the camera
            graphicsText.isSelectable = False # make it non-selectable
            graphicsText.cullMode = adsk.fusion.CustomGraphicsCullModes.CustomGraphicsCullBack
            graphicsText.color = adsk.fusion.CustomGraphicsShowThroughColorEffect.create(adsk.core.Color.create(230, 146, 18, 255), 1) # orange/synthesis theme
            graphicsText.depthPriority = 0
                    
            """
            create a bounding box around a wheel.
            """
            allIndices = [
                min[0],         min[1],         max[2],
                min[0],         min[1],         min[2],
                max[0],         min[1],         min[2],
                max[0],         min[1],         max[2],
                min[0],         min[1],         max[2],
            ]

            indexPairs = []
             
            for index in range(0, len(allIndices), 3):
                if index > len(allIndices)-5:
                    continue
                for i in allIndices[index:index+6]:
                    indexPairs.append(i)

            coords = adsk.fusion.CustomGraphicsCoordinates.create(
                indexPairs
            )
            line = graphics.addLines(
                coords,
                [],
                False,
            )
            line.color = adsk.fusion.CustomGraphicsShowThroughColorEffect.create(adsk.core.Color.create(0, 255, 0, 255), 0.2) # bright-green color
            line.weight = 1
            line.isScreenSpaceLineStyle = False
            line.isSelectable = False
            line.depthPriority = 1
    except:
        logging.getLogger("{INTERNAL_ID}.UI.CreateTextGraphics").error(
            "Failed:\n{}".format(traceback.format_exc())
        )