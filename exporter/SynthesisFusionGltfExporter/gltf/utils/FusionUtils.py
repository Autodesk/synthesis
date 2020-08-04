import adsk
import adsk.core
import adsk.fusion

from typing import List


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