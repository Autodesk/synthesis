import adsk.core
from typing import Tuple, Optional


def exportAppearance(appearance: adsk.core.Appearance) -> Optional[Tuple[adsk.core.Color, float, float]]:
    props = appearance.appearanceProperties

    metallic = 0.0

    roughnessProp = props.itemById("surface_roughness")
    if roughnessProp is None:
        return None
    smoothness = 1.0 - roughnessProp.value

    modelProp = props.itemById("interior_model")
    if modelProp is None:
        return None
    matModelType = modelProp.value

    if matModelType == 0:  # Opaque
        albedo = props.itemById("opaque_albedo").value
    elif matModelType == 1:  # Metal
        metallic = 1.0
        albedo = props.itemById("metal_f0").value
    elif matModelType == 2:  # Layered
        albedo = props.itemById("layered_diffuse").value
    elif matModelType == 3:  # Transparent
        albedo = props.itemById("transparent_color").value
    elif matModelType == 5:  # Glazing
        albedo = props.itemById("glazing_transmission_color").value
    else:  # ??? idk what type 4 material is or any other index
        print(f"Unsupported appearance modeling type: {appearance.name}")
        return None

    return albedo, metallic, smoothness