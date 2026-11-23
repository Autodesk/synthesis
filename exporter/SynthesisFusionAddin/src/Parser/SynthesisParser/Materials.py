import adsk.core

from src.ErrorHandling import Err, ErrorSeverity, Ok, Result, handle_err_top
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.PDMessage import PDMessage
from src.Parser.SynthesisParser.Utilities import construct_info, fill_info
from src.Proto import material_pb2

OPACITY_RAMPING_CONSTANT = 14.0

# Appearances with this name are exported fully transparent
FULLY_TRANSPARENT_APPEARANCE_NAME = ["air", "polycarbonate (clear)", "glass (clear)"]

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


@handle_err_top
def mapAllPhysicalMaterials(
    physicalMaterials: list[material_pb2.PhysicalMaterial],
    materials: material_pb2.Materials,
    options: ExporterOptions,
    progressDialog: PDMessage,
) -> Result[None]:
    set_result = setDefaultMaterial(materials.physicalMaterials["default"], options)
    if set_result.is_fatal():
        return set_result

    for material in physicalMaterials:
        if material.name is None or material.id is None:
            return Err("Material missing id or name", ErrorSeverity.Fatal)

        progressDialog.addMaterial(material.name)
        if progressDialog.wasCancelled():
            return Err("User canceled export", ErrorSeverity.Fatal)

        newmaterial = materials.physicalMaterials[material.id]
        material_result = getPhysicalMaterialData(material, newmaterial, options)
        if material_result.is_fatal():
            return material_result

    return Ok(None)


def setDefaultMaterial(physicalMaterial: material_pb2.PhysicalMaterial, options: ExporterOptions) -> Result[None]:
    construct_info_result = construct_info("default", physicalMaterial)
    if construct_info_result.is_err():
        return construct_info_result

    physicalMaterial.description = "A default physical material"
    if options.frictionOverride:
        physicalMaterial.dynamic_friction = options.frictionOverrideCoeff
        physicalMaterial.static_friction = options.frictionOverrideCoeff
    else:
        physicalMaterial.dynamic_friction = 0.5
        physicalMaterial.static_friction = 0.5

    physicalMaterial.restitution = 0.5
    physicalMaterial.deformable = False
    physicalMaterial.matType = 0  # type: ignore[assignment]

    return Ok(None)


def getPhysicalMaterialData(
    fusionMaterial: adsk.core.Material, physicalMaterial: material_pb2.PhysicalMaterial, options: ExporterOptions
) -> Result[None]:
    """Gets the material data and adds it to protobuf

    Args:
        fusion_material (fusionmaterial): Fusion Material
        proto_material (protomaterial): proto material mirabuf
        options (parseoptions): parse options
    """
    construct_info_result = construct_info("", physicalMaterial, fus_object=fusionMaterial)
    if construct_info_result.is_err():
        return construct_info_result

    physicalMaterial.deformable = False
    physicalMaterial.matType = 0  # type: ignore[assignment]

    materialProperties = fusionMaterial.materialProperties

    thermalProperties = physicalMaterial.thermal
    mechanicalProperties = physicalMaterial.mechanical
    strengthProperties = physicalMaterial.strength

    if options.frictionOverride:
        physicalMaterial.dynamic_friction = options.frictionOverrideCoeff
        physicalMaterial.static_friction = options.frictionOverrideCoeff
    else:
        physicalMaterial.dynamic_friction = DYNAMIC_FRICTION_COEFFS.get(fusionMaterial.name, 0.5)
        physicalMaterial.static_friction = STATIC_FRICTION_COEFFS.get(fusionMaterial.name, 0.5)

    physicalMaterial.restitution = 0.5
    physicalMaterial.description = f"{fusionMaterial.name} exported from FUSION"

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

    missingProperties: list[str] = [
        k for k, v in mechanicalProperties.ListFields() if v is None and not k.startswith("__")
    ]  # ignore: type
    if missingProperties.__len__() > 0:
        _: Err[None] = Err(f"Missing Mechanical Properties {missingProperties}", ErrorSeverity.Warning)

    """
    Strength Properties
    """
    strengthProperties.yield_strength = materialProperties.itemById("structural_Minimum_yield_stress").value
    strengthProperties.tensile_strength = materialProperties.itemById("structural_Minimum_tensile_strength").value

    missingStrengthProperties: list[str] = [k for k, v in strengthProperties.ListFields() if v is None]  # ignore: type
    if missingStrengthProperties.__len__() > 0:
        __: Err[None] = Err(f"Missing Strength Properties {missingProperties}", ErrorSeverity.Warning)

    """
    strengthProperties.thermal_treatment = materialProperties.itemById(
        "structural_Thermally_treated"
    ).value
    """

    return Ok(None)


@handle_err_top
def mapAllAppearances(
    appearances: list[material_pb2.Appearance],
    materials: material_pb2.Materials,
    options: ExporterOptions,
    progressDialog: PDMessage,
) -> Result[None]:
    # in case there are no appearances on a body
    # this is just a color tho
    set_default_result = setDefaultAppearance(materials.appearances["default"])
    if set_default_result.is_fatal():
        return set_default_result

    fill_info_result = fill_info(materials, None)
    if fill_info_result.is_err():
        return fill_info_result

    for appearance in appearances:
        progressDialog.addAppearance(appearance.name)

        # NOTE I'm not sure if this should be integrated with the error handling system or not, since it's fully intentional and immediantly aborts, which is the desired behavior
        # TODO Talk to Brandon about this
        if progressDialog.wasCancelled():
            return Err("User canceled export", ErrorSeverity.Fatal)

        material = materials.appearances["{}_{}".format(appearance.name, appearance.id)]
        material_result = getMaterialAppearance(appearance, options, material)
        if material_result.is_fatal():
            return material_result

    return Ok(None)


def setDefaultAppearance(appearance: material_pb2.Appearance) -> Result[None]:
    """Get a default color for the appearance

    Returns:
        types_pb2.Color: mira color
    """

    # add info
    # TODO: Check if appearance actually can be passed in here in place of an assembly or smth
    construct_info_result = construct_info("default", appearance)
    if construct_info_result.is_err():
        return construct_info_result

    appearance.roughness = 0.5
    appearance.metallic = 0.5
    appearance.specular = 0.5

    color = appearance.albedo
    color.R = 127
    color.G = 127
    color.B = 127
    color.A = 255

    return Ok(None)


def getMaterialAppearance(
    fusionAppearance: adsk.core.Appearance,
    options: ExporterOptions,
    appearance: material_pb2.Appearance,
) -> Result[None]:
    """Takes in a Fusion Mesh and converts it to a usable unity mesh

    Args:
        fusionAppearance (adsk.core.Appearance): Fusion appearance material
    """
    construct_info_result = construct_info("", appearance, fus_object=fusionAppearance)
    if construct_info_result.is_err():
        return construct_info_result

    # Make the transparent material actually transparent
    if fusionAppearance.name.lower() in FULLY_TRANSPARENT_APPEARANCE_NAME:
        appearance.roughness = 0.5
        appearance.metallic = 0.0
        appearance.specular = 0.0
        color = appearance.albedo
        color.R = 255
        color.G = 255
        color.B = 255
        color.A = 0
        return Ok(None)

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
    if properties is None:
        return Err("Apperarance Properties were None", ErrorSeverity.Fatal)

    roughnessProp = properties.itemById("surface_roughness")
    if roughnessProp:
        appearance.roughness = roughnessProp.value

    # Thank Liam for this.
    modelItem = properties.itemById("interior_model")
    if modelItem:
        matModelType = modelItem.value
        baseColor = None

        if matModelType == 0:
            reflectanceProp = properties.itemById("opaque_f0")
            if reflectanceProp:
                appearance.metallic = reflectanceProp.value
            baseColor = properties.itemById("opaque_albedo").value
            if baseColor:
                baseColor.opacity = 255
        elif matModelType == 1:
            baseColor = properties.itemById("metal_f0").value
            appearance.metallic = 0.8
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
    return Ok(None)
