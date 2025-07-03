# Contains all of the logic for mapping the Components / Occurrences
from platform import python_build

import adsk.core
import adsk.fusion
from google.protobuf.message import Error
from requests.models import parse_header_links

from src.ErrorHandling import Err, ErrorSeverity, Ok, Result
from src.Logging import getLogger, logFailure
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser import PhysicalProperties
from src.Parser.SynthesisParser.PDMessage import PDMessage
from src.Parser.SynthesisParser.Utilities import (
    fill_info,
    guid_component,
    guid_occurrence,
)
from src.Proto import assembly_pb2, joint_pb2, material_pb2, types_pb2
from src.Types import ExportMode


# TODO: Impelement Material overrides
def mapAllComponents(
    design: adsk.fusion.Design,
    options: ExporterOptions,
    progressDialog: PDMessage,
    partsData: assembly_pb2.Parts,
    materials: material_pb2.Materials,
) -> Result[None]:

    for component in design.allComponents:
        adsk.doEvents()
        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")
        progressDialog.addComponent(component.name)

        comp_ref = guid_component(component)

        fill_info_result = fill_info(partsData, None)
        if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
            return fill_info_result

        partDefinition = partsData.part_definitions[comp_ref]

        fill_info_result = fill_info(partDefinition, component, comp_ref)
        if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
            return fill_info_result

        physical_properties_result = PhysicalProperties.getPhysicalProperties(component, partDefinition.physical_data)
        if physical_properties_result.is_err() and physical_properties_result.unwrap_err()[1] == ErrorSeverity.Fatal:
            return physical_properties_result

        partDefinition.dynamic = options.exportMode != ExportMode.FIELD

        def processBody(body: adsk.fusion.BRepBody | adsk.fusion.MeshBody) -> Result[None]:
            if progressDialog.wasCancelled():
                raise RuntimeError("User canceled export")
            if body.isLightBulbOn:
                part_body = partDefinition.bodies.add()

                fill_info_result = fill_info(part_body, body)
                if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
                    return fill_info_result

                part_body.part = comp_ref

                fill_info_result = fill_info(part_body, body)
                if fill_info_result.is_err():
                    return fill_info_result

                if isinstance(body, adsk.fusion.BRepBody):
                    parse_result = parseBRep(body, options, part_body.triangle_mesh)
                    if parse_result.is_err() and parse_result.unwrap_err()[1] == ErrorSeverity.Fatal:
                        return parse_result
                else:
                    parse_result = parseMesh(body, options, part_body.triangle_mesh)
                    if parse_result.is_err() and parse_result.unwrap_err()[1] == ErrorSeverity.Fatal:
                        return parse_result

                appearance_key = "{}_{}".format(body.appearance.name, body.appearance.id)
                # this should be appearance
                if appearance_key in materials.appearances:
                    part_body.appearance_override = appearance_key
                else:
                    part_body.appearance_override = "default"

            return Ok(None)

        for body in component.bRepBodies:
            process_result = processBody(body)
            if process_result.is_err() and process_result.unwrap_err()[1] == ErrorSeverity.Fatal:
                return process_result
        for body in component.meshBodies:
            process_result = processBody(body)
            if process_result.is_err() and process_result.unwrap_err()[1] == ErrorSeverity.Fatal:
                return process_result

    return Ok(None)


def parseComponentRoot(
    component: adsk.fusion.Component,
    progressDialog: PDMessage,
    options: ExporterOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict[str, material_pb2.Appearance],
    node: types_pb2.Node,
) -> Result[None]:
    mapConstant = guid_component(component)

    part = partsData.part_instances[mapConstant]

    node.value = mapConstant

    fill_info_result = fill_info(part, component, mapConstant)
    if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
        return fill_info_result

    def_map = partsData.part_definitions

    if mapConstant in def_map:
        part.part_definition_reference = mapConstant

    for occur in component.occurrences:
        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            child_node = types_pb2.Node()

            parse_child_result = parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, child_node
            )
            if parse_child_result.is_err():
                return parse_child_result

            node.children.append(child_node)
    return Ok(None)


def parseChildOccurrence(
    occurrence: adsk.fusion.Occurrence,
    progressDialog: PDMessage,
    options: ExporterOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict[str, material_pb2.Appearance],
    node: types_pb2.Node,
) -> Result[None]:
    if occurrence.isLightBulbOn is False:
        return Ok(None)

    progressDialog.addOccurrence(occurrence.name)

    mapConstant = guid_occurrence(occurrence)

    compRef = guid_component(occurrence.component)

    part = partsData.part_instances[mapConstant]

    node.value = mapConstant

    fill_info_result = fill_info(part, occurrence, mapConstant)
    if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
        return fill_info_result

    collision_attr = occurrence.attributes.itemByName("synthesis", "collision_off")
    if collision_attr != None:
        part.skip_collider = True

    if occurrence.appearance:
        try:
            part.appearance = "{}_{}".format(occurrence.appearance.name, occurrence.appearance.id)
        except:
            _: Err[None] = Err("Failed to format part appearance", ErrorSeverity.Warning)
            part.appearance = "default"
        # TODO: Add phyical_material parser

    # TODO: I'm fairly sure that this should be a fatal error
    if occurrence.component.material:
        part.physical_material = occurrence.component.material.id
    else:
        return Err(f"Component Material is None", ErrorSeverity.Fatal)

    def_map = partsData.part_definitions

    if compRef in def_map:
        part.part_definition_reference = compRef

    # TODO: Maybe make this a separate step where you dont go backwards and search for the gamepieces
    if options.exportMode == ExportMode.FIELD:
        for x in options.gamepieces:
            if x.occurrenceToken == mapConstant:
                partsData.part_definitions[part.part_definition_reference].dynamic = True
                break

    part.transform.spatial_matrix.extend(occurrence.transform.asArray())

    worldTransform = getMatrixWorld(occurrence)

    if worldTransform:
        part.global_transform.spatial_matrix.extend(worldTransform.asArray())

    for occur in occurrence.childOccurrences:
        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            child_node = types_pb2.Node()

            parse_child_result = parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, child_node
            )
            if parse_child_result.is_err():
                return parse_child_result

            node.children.append(child_node)
    return Ok(None)


# saw online someone used this to get the correct context but oh boy does it look pricey
# I think if I can make all parts relative to a parent it should return that parents transform maybe
# TESTED AND VERIFIED - but unoptimized
def getMatrixWorld(occurrence: adsk.fusion.Occurrence) -> adsk.core.Matrix3D:
    matrix = occurrence.transform2
    while occurrence.assemblyContext:
        matrix.transformBy(occurrence.assemblyContext.transform2)
        occurrence = occurrence.assemblyContext
    return matrix


def parseBRep(
    body: adsk.fusion.BRepBody,
    options: ExporterOptions,
    trimesh: assembly_pb2.TriangleMesh,
) -> Result[None]:

    calc = body.meshManager.createMeshCalculator()
    # Disabling for now. We need the user to be able to adjust this, otherwise it gets locked
    # into whatever the default was at the time it first creates the export options.
    # calc.setQuality(options.visualQuality)
    _ = calc.setQuality(adsk.fusion.TriangleMeshQualityOptions.LowQualityTriangleMesh)
    # calc.maxNormalDeviation = 3.14159 * (1.0 / 6.0)
    # calc.surfaceTolerance = 0.5
    try:
        mesh = calc.calculate()
    except:
        return Err(f"Failed to calculate mesh for {body.name}", ErrorSeverity.Error)

    fill_info_result = fill_info(trimesh, body)
    if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
        return fill_info_result

    trimesh.has_volume = True

    plainmesh_out = trimesh.mesh
    plainmesh_out.verts.extend(mesh.nodeCoordinatesAsFloat)
    plainmesh_out.normals.extend(mesh.normalVectorsAsFloat)
    plainmesh_out.indices.extend(mesh.nodeIndices)
    plainmesh_out.uv.extend(mesh.textureCoordinatesAsFloat)

    return Ok(None)


def parseMesh(
    meshBody: adsk.fusion.MeshBody,
    options: ExporterOptions,
    trimesh: assembly_pb2.TriangleMesh,
) -> Result[None]:
    mesh = meshBody.displayMesh
    if mesh is None:
        return Err("Component Mesh was None", ErrorSeverity.Fatal)

    fill_info_result = fill_info(trimesh, meshBody)
    if fill_info_result.is_err() and fill_info_result.unwrap_err()[1] == ErrorSeverity.Fatal:
        return fill_info_result

    trimesh.has_volume = True

    plainmesh_out = trimesh.mesh

    plainmesh_out.verts.extend(mesh.nodeCoordinatesAsFloat)
    plainmesh_out.normals.extend(mesh.normalVectorsAsFloat)
    plainmesh_out.indices.extend(mesh.nodeIndices)
    plainmesh_out.uv.extend(mesh.textureCoordinatesAsFloat)

    return Ok(None)


def mapRigidGroups(rootComponent: adsk.fusion.Component, joints: joint_pb2.Joints) -> None:
    groups = rootComponent.allRigidGroups
    for group in groups:
        mira_group = joint_pb2.RigidGroup()
        mira_group.name = group.entityToken
        for occ in group.occurrences:
            if occ == None:
                a = 1
                continue

            if not occ.isLightBulbOn:
                continue

            occRef = guid_occurrence(occ)
            mira_group.occurrences.append(occRef)
        if len(mira_group.occurrences) > 1:
            joints.rigid_groups.append(mira_group)
