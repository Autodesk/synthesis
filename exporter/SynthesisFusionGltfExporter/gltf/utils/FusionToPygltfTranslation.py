from pygltflib import *
from .FusionUtils import *

def fusionMatToGltf(fusionAppearance: adsk.core.Appearance, exportWarnings: List[str]) -> Optional[Material]:
    appearancePBR = getPBRSettingsFromAppearance(fusionAppearance, exportWarnings)
    if appearancePBR is None:
        return
    baseColorFactor, emissiveColorFactor, metallicFactor, roughnessFactor, transparent = appearancePBR

    material = Material()
    material.name = fusionAppearance.name
    material.alphaCutoff = None  # this is a bug with the gltf python lib
    material.emissiveFactor = emissiveColorFactor

    pbr = PbrMetallicRoughness()
    pbr.baseColorFactor = baseColorFactor
    pbr.metallicFactor = metallicFactor
    pbr.roughnessFactor = roughnessFactor

    material.pbrMetallicRoughness = pbr

    return material