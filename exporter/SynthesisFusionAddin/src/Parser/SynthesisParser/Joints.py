"""
This Module details how to derive the joint hierarchy for a given Assembly.

These steps make it possible:

Pre-Process
1. Find Grounded Occurrence (or choose one with minimal side-effects)
2. Get a List of all occurrence tokens effected by joints (given) (ignore all not given I suppose)

FORALL occurrences in list and grounded
3. Traverse each given occurrence and create tree (Node)
4. Add to children until you encounter an occurrence in the joints side-effect list
5. When encountered start a new Node and parent it to the root object (not grounded)
6. I guess maybe we don't need the parent child relationship per say

What we need:

A Graphcontainer the contains a root node, this root node has many children.
Each root child is a independent dynamic joint structure denoted by the parent occurrence.
This is to act independently from the forward kinematic nature of software like unity.
Each root child has a number of children that are all rigidly attached to the dynamic object.

"""

import traceback
import uuid
from typing import Any, Callable, Union

import adsk.core
import adsk.fusion

from proto.proto_out import assembly_pb2, joint_pb2, signal_pb2, types_pb2
from src.Logging import getLogger
from src.Parser.ExporterOptions import ExporterOptions
from src.Parser.SynthesisParser.PDMessage import PDMessage
from src.Parser.SynthesisParser.Utilities import (
    construct_info,
    fill_info,
    guid_occurrence,
)
from src.Types import Joint, JointParentType, SignalType, Wheel

logger = getLogger()


# Need to take in a graphcontainer
# Need to create a new base node for each Joint Instance
# Need to create at least one grounded Node

# JointHierarchy is truely a Joint Hierarchy maybe
# Then each Joint instance contains a tree for model part information

# Root
#   Grounded
#   Rev 1
#       Rev 2
#   Rev 3
#       Rev 5
#   Prism 1
#       Rev 4

# 1. create all Joints and Instances
# 2. add bodies to each of the Joint Instances
# 3. connect all instances with graphcontainer


def populateJoints(
    design: adsk.fusion.Design,
    joints: joint_pb2.Joints,
    signals: signal_pb2.Signals,
    progressDialog: PDMessage,
    options: ExporterOptions,
    assembly: assembly_pb2.Assembly,
) -> None:
    fill_info(joints, None)

    # This is for creating all of the Joint Definition objects
    # So we need to iterate through the joints and construct them and add them to the map
    if not options.joints:
        return

    # Add the grounded joints object - TODO: rename some of the protobuf stuff for the love of god
    joint_definition_ground = joints.joint_definitions["grounded"]
    construct_info("grounded", joint_definition_ground)

    joint_instance_ground = joints.joint_instances["grounded"]
    construct_info("grounded", joint_instance_ground)

    joint_instance_ground.joint_reference = joint_definition_ground.info.GUID

    # Add the rest of the dynamic objects

    for joint in list(design.rootComponent.allJoints) + list(design.rootComponent.allAsBuiltJoints):
        if joint.isSuppressed:
            continue

        # turn RigidJoints into RigidGroups
        if joint.jointMotion.jointType == 0:
            _addRigidGroup(joint, assembly)
            continue

        # for now if it's not a revolute or slider joint ignore it
        if joint.jointMotion.jointType != 1 and joint.jointMotion.jointType != 2:
            continue

        try:
            #  Fusion has no instances of joints but lets roll with it anyway

            # progressDialog.message = f"Exporting Joint configuration {joint.name}"
            progressDialog.addJoint(joint.name)

            # create the definition
            joint_definition = joints.joint_definitions[joint.entityToken]
            _addJoint(joint, joint_definition)

            # create the instance of the single definition
            joint_instance = joints.joint_instances[joint.entityToken]

            for parse_joints in options.joints:
                if parse_joints.jointToken == joint.entityToken:
                    guid = str(uuid.uuid4())
                    signal = signals.signal_map[guid]
                    construct_info(joint.name, signal, GUID=guid)
                    signal.io = signal_pb2.IOType.OUTPUT

                    # really could just map the enum to a friggin string
                    if parse_joints.signalType != SignalType.PASSIVE and assembly.dynamic:
                        if parse_joints.signalType == SignalType.CAN:
                            signal.device_type = signal_pb2.DeviceType.CANBUS
                        elif parse_joints.signalType == SignalType.PWM:
                            signal.device_type = signal_pb2.DeviceType.PWM

                        motor = joints.motor_definitions[joint.entityToken]
                        fill_info(motor, joint)
                        simple_motor = motor.simple_motor
                        simple_motor.stall_torque = parse_joints.force
                        simple_motor.max_velocity = parse_joints.speed
                        simple_motor.braking_constant = 0.8  # Default for now
                        joint_definition.motor_reference = joint.entityToken

                        joint_instance.signal_reference = signal.info.GUID
                    # else:
                    #     signals.signal_map.remove(guid)

            _addJointInstance(joint, joint_instance, joint_definition, signals, options)

            # adds information for joint motion and limits
            _motionFromJoint(joint.jointMotion, joint_definition)

        except:
            logger.error("Failed:\n{}".format(traceback.format_exc()))
            continue


def _addJoint(joint: adsk.fusion.Joint, joint_definition: joint_pb2.Joint) -> None:
    fill_info(joint_definition, joint)

    jointPivotTranslation = _jointOrigin(joint)

    if jointPivotTranslation:
        joint_definition.origin.x = jointPivotTranslation.x
        joint_definition.origin.y = jointPivotTranslation.y
        joint_definition.origin.z = jointPivotTranslation.z
    else:
        joint_definition.origin.x = 0.0
        joint_definition.origin.y = 0.0
        joint_definition.origin.z = 0.0

        logger.error(f"Cannot find joint origin on joint {joint.name}")

    joint_definition.break_magnitude = 0.0


def _addJointInstance(
    joint: adsk.fusion.Joint,
    joint_instance: joint_pb2.JointInstance,
    joint_definition: joint_pb2.Joint,
    signals: signal_pb2.Signals,
    options: ExporterOptions,
) -> None:
    fill_info(joint_instance, joint)
    # because there is only one and we are using the token - should be the same
    joint_instance.joint_reference = joint_instance.info.GUID

    # Need to check if it is in a rigidgroup first, if yes then make the parent the actual parent

    # assign part id values - bug with entity tokens
    joint_instance.parent_part = guid_occurrence(joint.occurrenceOne)

    joint_instance.child_part = guid_occurrence(joint.occurrenceTwo)

    # FIX FOR ISSUE WHERE CHILD PART IS ACTUAL PART OF A LARGER GROUP THAT IS RIGID
    # MAY ALSO BE A FIX FR THE HIERARCHY DETECTION

    # THIS IS THE SAME RIGIDGROUP
    # rigid_groups_1 = joint.occurrenceOne.rigidGroups
    # rigid_groups_2 = joint.occurrenceTwo.rigidGroups

    # There should almost never be multiple - unless this is a parent of something else... hmmmm TBD
    # for rigid_group in rigid_groups_1:
    #     joint_instance.parent_part = rigid_group.assemblyContext.entityToken

    # for rigid_group in rigid_groups_2:
    #     joint_instance.child_part = rigid_group.assemblyContext.entityToken

    # ENDFIX

    if options.wheels:
        for wheel in options.wheels:
            if wheel.jointToken == joint.entityToken:
                joint_definition.user_data.data["wheel"] = "true"

                # Must convert type 'enum' to int to store wheelType in mirabuf
                joint_definition.user_data.data["wheelType"] = str(wheel.wheelType.value - 1)

                # if it exists get it and overwrite the signal type
                if joint_instance.signal_reference:
                    signal = signals.signal_map[joint_instance.signal_reference]
                else:  # if not then create it and add the signal type
                    guid = str(uuid.uuid4())
                    signal = signals.signal_map[guid]
                    construct_info("joint_signal", signal, GUID=guid)
                    signal.io = signal_pb2.IOType.OUTPUT
                    joint_instance.signal_reference = signal.info.GUID

                if wheel.signalType != SignalType.PASSIVE:
                    if wheel.signalType == SignalType.CAN:
                        signal.device_type = signal_pb2.DeviceType.CANBUS
                    elif wheel.signalType == SignalType.PWM:
                        signal.device_type = signal_pb2.DeviceType.PWM
                else:
                    joint_instance.signal_reference = ""


def _addRigidGroup(joint: adsk.fusion.Joint, assembly: assembly_pb2.Assembly) -> None:
    if joint.jointMotion.jointType != 0 or not (
        joint.occurrenceOne.isLightBulbOn and joint.occurrenceTwo.isLightBulbOn
    ):
        return

    mira_group = joint_pb2.RigidGroup()
    mira_group.name = f"group_{joint.occurrenceOne.name}_{joint.occurrenceTwo.name}"
    mira_group.occurrences.append(guid_occurrence(joint.occurrenceOne))
    mira_group.occurrences.append(guid_occurrence(joint.occurrenceTwo))
    assembly.data.joints.rigid_groups.append(mira_group)


def _motionFromJoint(fusionMotionDefinition: adsk.fusion.JointMotion, proto_joint: joint_pb2.Joint) -> None:
    # if fusionJoint.geometryOrOriginOne.objectType == "adsk::fusion::JointGeometry"
    # create the DOF depending on what kind of information the joint has

    fillJointMotionFuncSwitcher: dict[int, Callable[..., None]] = {
        0: notImplementedPlaceholder,  # this should be ignored
        1: fillRevoluteJointMotion,
        2: fillSliderJointMotion,
        3: notImplementedPlaceholder,  # TODO: Implement - Ball Joint at least
        4: notImplementedPlaceholder,  # TODO: Implement
        5: notImplementedPlaceholder,  # TODO: Implement
        6: notImplementedPlaceholder,  # TODO: Implement
    }

    fillJointMotionFunc = fillJointMotionFuncSwitcher.get(fusionMotionDefinition.jointType, notImplementedPlaceholder)

    fillJointMotionFunc(fusionMotionDefinition, proto_joint)


def fillRevoluteJointMotion(revoluteMotion: adsk.fusion.RevoluteJointMotion, proto_joint: joint_pb2.Joint) -> None:
    """#### Fill Protobuf revolute joint motion data

    Args:
        revoluteMotion (adsk.fusion.RevoluteJointMotion): Fusion Revolute Joint Data
        protoJoint (joint_pb2.Joint): Protobuf joint that is being modified
    """

    proto_joint.joint_motion_type = joint_pb2.JointMotion.REVOLUTE

    dof = proto_joint.rotational.rotational_freedom

    # name
    # axis
    # pivot
    # dynamics
    # limits
    # current value

    dof.name = "Rotational Joint"

    dof.value = revoluteMotion.rotationValue

    if revoluteMotion.rotationLimits:
        dof.limits.lower = revoluteMotion.rotationLimits.minimumValue
        dof.limits.upper = revoluteMotion.rotationLimits.maximumValue

    rotationAxisVector = revoluteMotion.rotationAxisVector
    if rotationAxisVector:
        dof.axis.x = rotationAxisVector.x
        dof.axis.y = rotationAxisVector.y
        dof.axis.z = rotationAxisVector.z
    else:
        rotationAxis = revoluteMotion.rotationAxis
        # don't handle 4 for now
        # There is a bug here https://jira.autodesk.com/browse/FUS-80533
        # I have 0 memory of why this is necessary
        dof.axis.x = int(rotationAxis == 0)
        dof.axis.y = int(rotationAxis == 2)
        dof.axis.z = int(rotationAxis == 1)


def fillSliderJointMotion(sliderMotion: adsk.fusion.SliderJointMotion, proto_joint: joint_pb2.Joint) -> None:
    """#### Fill Protobuf slider joint motion data

    Args:
        sliderMotion (adsk.fusion.SliderJointMotion): Fusion Slider Joint Data
        protoJoint (joint_pb2.Joint): Protobuf joint that is being modified

    """

    proto_joint.joint_motion_type = joint_pb2.JointMotion.SLIDER

    dof = proto_joint.prismatic.prismatic_freedom

    # dof.axis = sliderMotion.slideDirectionVector
    dof.axis.x = -sliderMotion.slideDirectionVector.x
    dof.axis.y = sliderMotion.slideDirectionVector.y
    dof.axis.z = sliderMotion.slideDirectionVector.z

    if sliderMotion.slideDirection is adsk.fusion.JointDirections.XAxisJointDirection:
        dof.pivotDirection = types_pb2.Axis.X
    elif sliderMotion.slideDirection is adsk.fusion.JointDirections.YAxisJointDirection:
        dof.pivotDirection = types_pb2.Axis.Y
    elif sliderMotion.slideDirection is adsk.fusion.JointDirections.ZAxisJointDirection:
        dof.pivotDirection = types_pb2.Axis.Z

    if sliderMotion.slideLimits:
        dof.limits.lower = sliderMotion.slideLimits.minimumValue
        dof.limits.upper = sliderMotion.slideLimits.maximumValue

    dof.value = sliderMotion.slideValue


def notImplementedPlaceholder(*argv: Any) -> None: ...


def _searchForGrounded(
    occ: adsk.fusion.Occurrence,
) -> Union[adsk.fusion.Occurrence, None]:
    """Search for a grounded component or occurrence in the assembly

    Args:
        occ (adsk.fusion.Occurrence): start point

    Returns:
        Union(adsk.fusion.Occurrence, None): Either a grounded part or nothing
    """

    # Why did I not just supply all occurrences for some reason?
    if occ.objectType == "adsk::fusion::Component":
        # this makes it possible to search an object twice (unoptimized)
        collection = occ.allOccurrences

        # components cannot be grounded technically

    else:  # Object is an occurrence
        if occ.isGrounded:
            return occ

        collection = occ.childOccurrences

    for occ in collection:
        searched = _searchForGrounded(occ)

        if searched != None:
            return searched

    return None


def _jointOrigin(fusionJoint: Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]) -> adsk.core.Point3D:
    """#### Joint Origin Internal Finder that was orignally created for Synthesis by Liam Wang

    Args:
        joint (Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]): A Fusion Joint that is either a Proper or As Build Joint

    Returns:
        Point3D by Autodesk Fusion
    """
    geometryOrOrigin = (
        (
            fusionJoint.geometryOrOriginOne
            if fusionJoint.geometryOrOriginOne.objectType == "adsk::fusion::JointGeometry"
            else fusionJoint.geometryOrOriginTwo
        )
        if fusionJoint.objectType == "adsk::fusion::Joint"
        else fusionJoint.geometry
    )

    # This can apparently happen
    # I suppose an AsBuilt with Rigid doesn't need a Origin perhaps?
    if geometryOrOrigin is None:
        return None

    if geometryOrOrigin.objectType == "adsk::fusion::JointGeometry":
        ent = geometryOrOrigin.entityOne
        if ent.objectType == "adsk::fusion::BRepEdge":
            if ent.assemblyContext is None:
                newEnt = ent.createForAssemblyContext(fusionJoint.occurrenceOne)
                min = newEnt.boundingBox.minPoint
                max = newEnt.boundingBox.maxPoint
                org = adsk.core.Point3D.create((max.x + min.x) / 2.0, (max.y + min.y) / 2.0, (max.z + min.z) / 2.0)
                return org  # ent.startVertex.geometry
            else:
                return geometryOrOrigin.origin
        if ent.objectType == "adsk::fusion::BRepFace":
            if ent.assemblyContext is None:
                newEnt = ent.createForAssemblyContext(fusionJoint.occurrenceOne)
                return newEnt.centroid
            else:
                return geometryOrOrigin.origin
        else:
            return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
        offsetX = 0 if geometryOrOrigin.offsetX is None else geometryOrOrigin.offsetX.value
        offsetY = 0 if geometryOrOrigin.offsetY is None else geometryOrOrigin.offsetY.value
        offsetZ = 0 if geometryOrOrigin.offsetZ is None else geometryOrOrigin.offsetZ.value
        # noinspection PyArgumentList
        return adsk.core.Point3D.create(origin.x + offsetX, origin.y + offsetY, origin.z + offsetZ)


def createJointGraph(
    suppliedJoints: list[Joint],
    _wheels: list[Wheel],
    jointTree: types_pb2.GraphContainer,
    progressDialog: PDMessage,
) -> None:
    # progressDialog.message = f"Building Joint Graph Map from given joints"

    progressDialog.currentMessage = f"Building Joint Graph Map from given joints"
    progressDialog.update()

    # keep track of current nodes to link them
    nodeMap = dict()

    # contains all of the static ground objects
    groundNode = types_pb2.Node()
    groundNode.value = "ground"

    nodeMap[groundNode.value] = groundNode

    # addWheelsToGraph(wheels, groundNode, joint_tree)

    # first iterate through to create the nodes
    for suppliedJoint in suppliedJoints:
        newNode = types_pb2.Node()
        newNode.value = suppliedJoint.jointToken
        nodeMap[newNode.value] = newNode

    # second sort them
    for suppliedJoint in suppliedJoints:
        if suppliedJoint.parent == JointParentType.ROOT:
            nodeMap["ground"].children.append(nodeMap[suppliedJoint.jointToken])
        elif nodeMap[suppliedJoint.parent.value] is not None and nodeMap[suppliedJoint.jointToken] is not None:
            nodeMap[str(suppliedJoint.parent)].children.append(nodeMap[suppliedJoint.jointToken])
        else:
            logger.error(f"Cannot construct hierarhcy because of detached tree at : {suppliedJoint.jointToken}")

    for node in nodeMap.values():
        # append everything at top level to isolate kinematics
        jointTree.nodes.append(node)


def addWheelsToGraph(wheels: list[Wheel], rootNode: types_pb2.Node, jointTree: types_pb2.GraphContainer) -> None:
    for wheel in wheels:
        # wheel name
        # wheel signal
        # wheel occ id
        # these don't have children
        wheelNode = types_pb2.Node()
        wheelNode.value = wheel.jointToken
        rootNode.children.append(wheelNode)
        jointTree.nodes.append(wheelNode)
