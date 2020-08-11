import adsk
import adsk.core
import adsk.fusion

from typing import *


def checkIfAppearancesAreBugged(design: adsk.fusion.Design) -> bool:
    """Checks if the appearances of a fusion document are bugged.

    According to the Fusion 360 API documentation, the id property of a adsk.core.Appearance should be unique.
    For many models imported into Fusion 360 (as opposed to being designed in Fusion), the material ids are duplicated.
    This leads to a bug where the Fusion 360 API does not return the correct materials for a model, thus making it impossible to export the materials.

    Returns: True if the appearances are bugged.

    """
    usedIdMap = {}
    for appearance in design.appearances:
        if appearance.id in usedIdMap:
            return True
        usedIdMap[appearance.id] = True
    return False

def getDefaultAppearance(app: adsk.core.Application) -> Optional[adsk.core.Appearance]:
    fusionMatLib = app.materialLibraries.itemById("C1EEA57C-3F56-45FC-B8CB-A9EC46A9994C")  # Fusion 360 Material Library
    if fusionMatLib is None:
        return None
    aluminum = fusionMatLib.appearances.itemById("PrismMaterial-002_physmat_aspects:Prism-028")  # Aluminum - Satin
    return aluminum

def fusionColorToRGBAArray(color: adsk.core.Color) -> List[float]:
    return [
        color.red / 255,
        color.green / 255,
        color.blue / 255,
        color.opacity / 255,
    ]

def fusionAttenLengthToAlpha(attenLength: adsk.core.FloatProperty) -> float:
    if attenLength is None:
        return 1
    return max(min((464 - 7 * attenLength.value) / 1938, 1), 0.03)  # todo: this conversion is just made up, figure out an accurate one

def isSameMaterial(faces: List[adsk.fusion.BRepFace]):
    # if all faces use the same material
    materialName = faces[0].appearance.name
    for face in faces[1:]:
        if face.appearance.name != materialName:
            return False
    return True