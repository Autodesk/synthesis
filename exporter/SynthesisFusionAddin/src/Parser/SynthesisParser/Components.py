# Contains all of the logic for mapping the Components / Occurrences
import adsk.core, adsk.fusion, uuid, logging, traceback
from proto.proto_out import assembly_pb2, types_pb2, material_pb2

from .Utilities import *
from .. import ParseOptions
from typing import *

from . import PhysicalProperties

# TODO: Impelement Material overrides

def _MapAllComponents(
    design: adsk.fusion.Design,
    options: ParseOptions,
    progressDialog,
    partsData: assembly_pb2.Parts,
    materials: material_pb2.Materials,
) -> None:
    for component in design.allComponents:
        adsk.doEvents()
        progressDialog.message = f"Exporting {component.name}"

        try:
            comp_ref = component.entityToken
        except RuntimeError:
            # backup in case encountered the bug
            comp_ref = component.name

        fill_info(partsData, None)

        partDefinition = partsData.part_definitions[comp_ref]

        fill_info(partDefinition, component)

        if options.physicalDepth >= 1:
            PhysicalProperties.GetPhysicalProperties(
                component, partDefinition.physical_data
            )

        for body in component.bRepBodies:
            if body.isLightBulbOn:
                progressDialog.progressValue = progressDialog.progressValue + 1

                part_body = partDefinition.bodies.add()
                fill_info(part_body, body)
                part_body.part = comp_ref
                _ParseBRep(body, options, part_body.triangle_mesh)

                if options.materials:
                    if body.appearance.id in materials:
                        part_body.material = body.appearance.id
                    else:
                        part_body.material = "default"


def _ParseComponentRoot(
    component: adsk.fusion.Component,
    progressDialog: adsk.core.ProgressDialog,
    options: ParseOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict,
    node: types_pb2.Node
) -> None:

    adsk.doEvents()

    try:
        mapConstant = component.entityToken
    except RuntimeError:
        mapConstant = component.name

    part = partsData.part_instances[mapConstant]

    node.value = mapConstant

    fill_info(part, component)

    def_map = partsData.part_definitions

    if mapConstant in def_map:
        part.part_definition_reference = mapConstant
        # part.transform.spatial_matrix = component.transform.asArray()

    for occur in component.occurrences:

        if progressDialog.wasCancelled:
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            part.children.append(f"{occur.entityToken}")
            edge = types_pb2.Edge()
            __parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, edge.node
            )
            node.children.append(edge)


def __parseChildOccurrence(
    occurrence: adsk.fusion.Occurrence,
    progressDialog: adsk.core.ProgressDialog,
    options: ParseOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict,
    node: types_pb2.Node
) -> None:

    if occurrence.isLightBulbOn is False:
        return

    adsk.doEvents()

    try:
        mapConstant = occurrence.entityToken
    except RuntimeError:
        mapConstant = occurrence.name

    try:
        compRef = occurrence.component.entityToken
    except RuntimeError:
        compRef = occurrence.component.name

    part = partsData.part_instances[mapConstant]

    node.value = mapConstant

    fill_info(part, occurrence)

    def_map = partsData.part_definitions

    if compRef in def_map:
        part.part_definition_reference = compRef

    part.transform.spatial_matrix.values.extend(occurrence.transform.asArray())

    for occur in occurrence.childOccurrences:
        if progressDialog.wasCancelled:
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            part.children.append(f"{occur.entityToken}")
            edge = types_pb2.Edge()
            __parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, edge.node
            )
            node.children.append(edge)

def _ParseBRep(
    body: adsk.fusion.BRepBody, options: ParseOptions, trimesh: assembly_pb2.TriangleMesh
) -> any:
    try:
        meshManager = body.meshManager
        calc = meshManager.createMeshCalculator()
        calc.setQuality(options.visual)
        mesh = calc.calculate()

        fill_info(trimesh, body)
        trimesh.has_volume = True

        plainmesh_out = trimesh.mesh

        plainmesh_out.verts.extend(mesh.nodeCoordinatesAsFloat)
        plainmesh_out.normals.extend(mesh.normalVectorsAsFloat)
        plainmesh_out.indices.extend(mesh.nodeIndices)
        plainmesh_out.uv.extend(mesh.textureCoordinates)
    except:
        logging.getLogger("{INTERNAL_ID}.Parser.BrepBody").error(
            "Failed:\n{}".format(traceback.format_exc())
        )
