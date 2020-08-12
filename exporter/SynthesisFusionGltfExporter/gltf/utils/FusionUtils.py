import adsk
import adsk.core
import adsk.fusion

from typing import *

def calculateMeshForBRep(fusionBRep: Union[adsk.fusion.BRepBody, adsk.fusion.BRepFace], meshQuality) -> Tuple[Optional[List[float]], Optional[List[float]]]:
    meshCalculator = fusionBRep.meshManager.createMeshCalculator()
    if meshCalculator is None:
        return None, None
    meshCalculator.setQuality(meshQuality)

    mesh = meshCalculator.calculate()

    if mesh is None:
        return None, None

    coords = mesh.nodeCoordinatesAsFloat
    indices = mesh.nodeIndices
    if len(indices) == 0 or len(coords) == 0:
        return None, None

    return coords, indices

def getPBRSettingsFromAppearance(fusionAppearance: adsk.core.Appearance, exportWarnings: List[str]) -> Optional[Tuple[List[float], List[float], float, float, bool]]:
    """Gets Physically Based Rendering settings from a fusion appearance.

    Args:
        fusionAppearance: The fusion appearance to read from.
        warnings: A list of warnings to append onto.

    Returns: None if the material is unrecognized, otherwise returns RGBA base color, RGB emissive factor, metallic factor, roughness factor, and whether the material is transparent.

    """
    props = fusionAppearance.appearanceProperties

    transparent = False
    emissiveColorFactor = None
    metallicFactor = 0.0
    roughnessProp = props.itemById("surface_roughness")
    roughnessFactor = roughnessProp.value if roughnessProp is not None else 0.1

    baseColor = None

    modelItem = props.itemById("interior_model")
    if modelItem is None:
        return None
    matModelType = modelItem.value

    if matModelType == 0:  # Opaque
        baseColor = props.itemById("opaque_albedo").value
        if props.itemById("opaque_emission").value:
            emissiveColorFactor = fusionColorToRGBAArray(props.itemById("opaque_luminance_modifier").value)[:3]
    elif matModelType == 1:  # Metal
        metallicFactor = 1.0
        baseColor = props.itemById("metal_f0").value
    elif matModelType == 2:  # Layered
        baseColor = props.itemById("layered_diffuse").value
    elif matModelType == 3:  # Transparent
        baseColor = props.itemById("transparent_color").value
        transparent = True
    elif matModelType == 5:  # Glazing
        baseColor = props.itemById("glazing_transmission_color").value
    else:  # ??? idk what type 4 material is
        exportWarnings.append(f"Unsupported material modeling type: {fusionAppearance.name}")

    if baseColor is None:
        exportWarnings.append(f"Ignoring material that does not have color: {fusionAppearance.name}")
        return None

    baseColorFactor = fusionColorToRGBAArray(baseColor)[:3] + [fusionAttenLengthToAlpha(props.itemById("transparent_distance"))]

    return baseColorFactor, emissiveColorFactor, metallicFactor, roughnessFactor, transparent

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
    if len(faces) == 0:
        return True
    materialName = faces[0].appearance.name
    for face in faces[1:]:
        if face.appearance.name != materialName:
            return False
    return True