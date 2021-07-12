# Should contain Physical and Apperance materials ?
import adsk

from .Utilities import *
from .. import ParseOptions
from proto.proto_out import assembly_pb2, types_pb2, material_pb2


def _MapAllAppearances(
    appearances: [],
    materials: material_pb2.Materials,
    options: ParseOptions,
    progressDialog,
) -> None:

    # in case there are no appearances on a body
    # this is just a color tho
    setDefaultAppearance(materials.materials["default"])

    fill_info(materials, None)

    for appearance in appearances:
        material = materials.materials[appearance.id]
        getMaterialAppearance(appearance, options, material)


def setDefaultAppearance(material: material_pb2.Material) -> types_pb2.Color:
    """Get a default color for the appearance

    Returns:
        types_pb2.Color: mira color
    """

    # add info
    construct_info("default", material)

    appearance = material.appearance
    appearance.roughness = 0.5
    appearance.metallic = 0.5
    appearance.specular = 0.5

    color = appearance.albedo
    color.R = 127
    color.G = 127
    color.B = 127
    color.A = 127

    return color


def getMaterialAppearance(
    fusionAppearance: adsk.core.Appearance,
    options: ParseOptions,
    material: material_pb2.Material,
) -> types_pb2.Color:
    """Takes in a Fusion 360 Mesh and converts it to a usable unity mesh

    Args:
        fusionAppearance (adsk.core.Appearance): Fusion 360 appearance material
    """
    construct_info("", material, fus_object=fusionAppearance)

    appearance = material.appearance
    appearance.roughness = 0.5
    appearance.metallic = 0.5
    appearance.specular = 0.5

    # set defaults just in case
    color = appearance.albedo
    color.R = 127
    color.G = 127
    color.B = 127
    color.A = 127

    properties = fusionAppearance.appearanceProperties

    # Thank Liam for this.
    modelItem = properties.itemById("interior_model")
    if modelItem:
        matModelType = modelItem.value
        baseColor = None

        if matModelType == 0:
            baseColor = properties.itemById("opaque_albedo").value
        elif matModelType == 1:
            baseColor = properties.itemById("metal_f0").value
        elif matModelType == 2:
            baseColor = properties.itemById("layered_diffuse").value

        if baseColor:
            color.R = baseColor.red
            color.G = baseColor.green
            color.B = baseColor.blue
            color.A = baseColor.opacity
        else:
            for prop in fusionAppearance.appearanceProperties:
                if (
                    (prop.name == "Color")
                    and (prop.value is not None)
                    and (prop.id != "surface_albedo")
                ):
                    baseColor = prop.value
                    color.R = baseColor.red
                    color.G = baseColor.green
                    color.B = baseColor.blue
                    color.A = baseColor.opacity
                    break

    return color
