# Contains all of the logic for mapping the Components / Occurrences
import adsk.core, adsk.fusion, uuid, logging, traceback
from proto.proto_out import assembly_pb2, types_pb2, material_pb2, joint_pb2

from .Utilities import *
from .. import ParseOptions
from typing import *

from . import PhysicalProperties

from .PDMessage import PDMessage
from ...Analyzer.timer import TimeThis

# TODO: Impelement Material overrides


@TimeThis
def _MapAllComponents(
    design: adsk.fusion.Design,
    options: ParseOptions,
    progressDialog: PDMessage,
    partsData: assembly_pb2.Parts,
    materials: material_pb2.Materials,
) -> None:
    for component in design.allComponents:
        adsk.doEvents()
        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")
        progressDialog.addComponent(component.name)

        try:
            comp_ref = component.entityToken
        except RuntimeError:
            # backup in case encountered the bug
            comp_ref = component.name

        fill_info(partsData, None)

        partDefinition = partsData.part_definitions[comp_ref]

        fill_info(partDefinition, component)

        PhysicalProperties.GetPhysicalProperties(component, partDefinition.physical_data)

        if options.mode == 3:
            partDefinition.dynamic = False
        else:
            partDefinition.dynamic = True

        for body in component.bRepBodies:
            if progressDialog.wasCancelled():
                raise RuntimeError("User canceled export")
            if body.isLightBulbOn:
                part_body = partDefinition.bodies.add()
                fill_info(part_body, body)
                part_body.part = comp_ref
                _ParseBRep(body, options, part_body.triangle_mesh)

                appearance_key = "{}_{}".format(body.appearance.name, body.appearance.id)
                # this should be appearance
                if appearance_key in materials.appearances:
                    part_body.appearance_override = appearance_key
                else:
                    part_body.appearance_override = "default"


@TimeThis
def _ParseComponentRoot(
    component: adsk.fusion.Component,
    progressDialog: PDMessage,
    options: ParseOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict,
    node: types_pb2.Node,
) -> None:

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

    for occur in component.occurrences:

        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            child_node = types_pb2.Node()
            __parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, child_node
            )
            node.children.append(child_node)


def __parseChildOccurrence(
    occurrence: adsk.fusion.Occurrence,
    progressDialog: PDMessage,
    options: ParseOptions,
    partsData: assembly_pb2.Parts,
    material_map: dict,
    node: types_pb2.Node,
) -> None:

    if occurrence.isLightBulbOn is False:
        return

    progressDialog.addOccurrence(occurrence.name)

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

    if occurrence.appearance:
        part.appearance = "{}_{}".format(occurrence.appearance.name, occurrence.appearance.id)
        # TODO: Add phyical_material parser

    if occurrence.component.material:
        part.physical_material = occurrence.component.material.id

    def_map = partsData.part_definitions

    if compRef in def_map:
        part.part_definition_reference = compRef

    # TODO: Maybe make this a separate step where you dont go backwards and search for the gamepieces
    if options.mode == ParseOptions.Mode.SynthesisField:
        for x in options.gamepieces:
            if x.occurrence_token == mapConstant:
                partsData.part_definitions[part.part_definition_reference].dynamic = True
                break

    part.transform.spatial_matrix.extend(occurrence.transform.asArray())

    worldTransform = GetMatrixWorld(occurrence)

    if worldTransform:
        part.global_transform.spatial_matrix.extend(worldTransform.asArray())

    for occur in occurrence.childOccurrences:
        if progressDialog.wasCancelled():
            raise RuntimeError("User canceled export")

        if occur.isLightBulbOn:
            child_node = types_pb2.Node()
            __parseChildOccurrence(
                occur, progressDialog, options, partsData, material_map, child_node
            )
            node.children.append(child_node)


# saw online someone used this to get the correct context but oh boy does it look pricey
# I think if I can make all parts relative to a parent it should return that parents transform maybe
# TESTED AND VERIFIED - but unoptimized
def GetMatrixWorld(occurrence):
    matrix = occurrence.transform
    while occurrence.assemblyContext:
        matrix.transformBy(occurrence.assemblyContext.transform)
        occurrence = occurrence.assemblyContext
    return matrix


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
        plainmesh_out.uv.extend(mesh.textureCoordinatesAsFloat)
    except:
        logging.getLogger("{INTERNAL_ID}.Parser.BrepBody").error(
            "Failed:\n{}".format(traceback.format_exc())
        )

def _MapRigidGroups(
    rootComponent: adsk.fusion.Component,
    joints: joint_pb2.Joints
) -> None:
    groups = rootComponent.allRigidGroups
    for group in groups:
        mira_group = joint_pb2.RigidGroup()
        mira_group.name = group.entityToken
        for occ in group.occurrences:
            try:
                occRef = occ.entityToken
            except RuntimeError:
                occRef = occ.name
            mira_group.occurrences.append(occRef)
        if (len(mira_group.occurrences) > 1):
            joints.rigid_groups.append(mira_group)
    

