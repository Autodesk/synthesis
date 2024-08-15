import adsk

from proto.proto_out import material_pb2
from src.Logging import logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.PDMessage import PDMessage
from src.Parser.SynthesisParser.Utilities import construct_info, fill_info
from src.Proto import material_pb2

OPACITY_RAMPING_CONSTANT = 14.0

# Update tables as needed for UX and needed materials
STATIC_FRICTION_COEFFS = {
    "Aluminum": 1.1,
    "Steel, Cast": 0.75,
    "Steel, Mild": 0.75,
    "Rubber, Nitrile": 1.0,
    "ABS Plastic": 0.7,
}

DYNAMIC_FRICTION_COEFFS = {
    "Aluminum": 1.1,
    "Steel, Cast": 0.75,
    "Steel, Mild": 0.75,
    "Rubber, Nitrile": 1.0,
    "ABS Plastic": 0.7,
}


def _MapAllPhysicalMaterials(
    physicalMaterials: list,
    materials: material_pb2.Materials,
    options: ExporterOptions,
    progressDialog: PDMessage,
) -> None:
    setDefaultMaterial(materials.physicalMaterials["default"], options)

    for material in physicalMaterials:
        progressDialog.addMaterial(material.name)

        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        newmaterial = materials.physicalMaterials[material.id]
        getPhysicalMaterialData(material, newmaterial, options)


def setDefaultMaterial(physical_material: material_pb2.PhysicalMaterial, options: ExporterOptions):
    construct_info("default", physical_material)

    physical_material.description = "A default physical material"
    if options.frictionOverride:
        physical_material.dynamic_friction = options.frictionOverrideCoeff
        physical_material.static_friction = options.frictionOverrideCoeff
    else:
        physical_material.dynamic_friction = 0.5
        physical_material.static_friction = 0.5

    physical_material.restitution = 0.5
    physical_material.deformable = False
    physical_material.matType = 0


@logFailure
def getPhysicalMaterialData(fusion_material, proto_material, options):
    """Gets the material data and adds it to protobuf

    Args:
        fusion_material (fusionmaterial): Fusion Material
        proto_material (protomaterial): proto material mirabuf
        options (parseoptions): parse options
    """
    construct_info("", proto_material, fus_object=fusion_material)

    proto_material.deformable = False
    proto_material.matType = 0

    materialProperties = fusion_material.materialProperties

    thermalProperties = proto_material.thermal
    mechanicalProperties = proto_material.mechanical
    strengthProperties = proto_material.strength

    if options.frictionOverride:
        proto_material.dynamic_friction = options.frictionOverrideCoeff
        proto_material.static_friction = options.frictionOverrideCoeff
    else:
        proto_material.dynamic_friction = DYNAMIC_FRICTION_COEFFS.get(fusion_material.name, 0.5)
        proto_material.static_friction = STATIC_FRICTION_COEFFS.get(fusion_material.name, 0.5)

    proto_material.restitution = 0.5
    proto_material.description = f"{fusion_material.name} exported from FUSION"

    """
    Thermal Properties
    """

    """ # These are causing temporary failures when trying to find value. Better to not throw this many exceptions.
    if materialProperties.itemById(
            "thermal_Thermal_conductivity"
        ) is not None:
        thermalProperties.thermal_conductivity = materialProperties.itemById(
            "thermal_Thermal_conductivity"
        ).value
    if materialProperties.itemById(
            "structural_Specific_heat"
        ) is not None:
        thermalProperties.specific_heat = materialProperties.itemById(
            "structural_Specific_heat"
        ).value
    
    if materialProperties.itemById(
            "structural_Thermal_expansion_coefficient"
        ) is not None:
        thermalProperties.thermal_expansion_coefficient = materialProperties.itemById(
            "structural_Thermal_expansion_coefficient"
        ).value
    """

    """
    Mechanical Properties
    """
    mechanicalProperties.young_mod = materialProperties.itemById("structural_Young_modulus").value
    mechanicalProperties.poisson_ratio = materialProperties.itemById("structural_Poisson_ratio").value
    mechanicalProperties.shear_mod = materialProperties.itemById("structural_Shear_modulus").value
    mechanicalProperties.density = materialProperties.itemById("structural_Density").value
    mechanicalProperties.damping_coefficient = materialProperties.itemById("structural_Damping_coefficient").value

    """
    Strength Properties
    """
    strengthProperties.yield_strength = materialProperties.itemById("structural_Minimum_yield_stress").value
    strengthProperties.tensile_strength = materialProperties.itemById("structural_Minimum_tensile_strength").value
    """
    strengthProperties.thermal_treatment = materialProperties.itemById(
        "structural_Thermally_treated"
    ).value
    """


def _MapAllAppearances(
    appearances: list,
    materials: material_pb2.Materials,
    options: ExporterOptions,
    progressDialog: PDMessage,
) -> None:
    # in case there are no appearances on a body
    # this is just a color tho
    setDefaultAppearance(materials.appearances["default"])

    fill_info(materials, None)

    for appearance in appearances:
        progressDialog.addAppearance(appearance.name)

        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        material = materials.appearances["{}_{}".format(appearance.name, appearance.id)]
        getMaterialAppearance(appearance, options, material)


def setDefaultAppearance(appearance: material_pb2.Appearance) -> None:
    """Get a default color for the appearance

    Returns:
        types_pb2.Color: mira color
    """

    # add info
    construct_info("default", appearance)

    appearance.roughness = 0.5
    appearance.metallic = 0.5
    appearance.specular = 0.5

    color = appearance.albedo
    color.R = 127
    color.G = 127
    color.B = 127
    color.A = 255


def getMaterialAppearance(
    fusionAppearance: adsk.core.Appearance,
    options: ExporterOptions,
    appearance: material_pb2.Appearance,
) -> None:
    """Takes in a Fusion Mesh and converts it to a usable unity mesh

    Args:
        fusionAppearance (adsk.core.Appearance): Fusion appearance material
    """
    construct_info("", appearance, fus_object=fusionAppearance)

    appearance.roughness = 0.9
    appearance.metallic = 0.3
    appearance.specular = 0.5

    # set defaults just in case
    color = appearance.albedo
    color.R = 10
    color.G = 10
    color.B = 10
    color.A = 127

    properties = fusionAppearance.appearanceProperties

    # Thank Liam for this.
    modelItem = properties.itemById("interior_model")
    if modelItem:
        matModelType = modelItem.value
        baseColor = None

        if matModelType == 0:
            baseColor = properties.itemById("opaque_albedo").value
            if baseColor:
                baseColor.opacity = 255
        elif matModelType == 1:
            baseColor = properties.itemById("metal_f0").value
            if baseColor:
                baseColor.opacity = 255
        elif matModelType == 2:
            baseColor = properties.itemById("layered_diffuse").value
            if baseColor:
                baseColor.opacity = 255
        elif matModelType == 3:
            baseColor = properties.itemById("transparent_color").value
            transparent_distance = properties.itemById("transparent_distance").value

            opac = (255.0 * transparent_distance) / (transparent_distance + OPACITY_RAMPING_CONSTANT)
            if opac > 255:
                opac = 255
            elif opac < 0:
                opac = 0

            if baseColor:
                baseColor.opacity = int(round(opac))

        if baseColor:
            color.R = baseColor.red
            color.G = baseColor.green
            color.B = baseColor.blue
            color.A = baseColor.opacity
        else:
            for prop in fusionAppearance.appearanceProperties:
                if (prop.name == "Color") and (prop.value is not None) and (prop.id != "surface_albedo"):
                    baseColor = prop.value
                    color.R = baseColor.red
                    color.G = baseColor.green
                    color.B = baseColor.blue
                    color.A = baseColor.opacity
                    break
