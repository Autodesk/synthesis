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

import adsk.fusion, adsk.core, traceback

from proto.proto_out import types_pb2, joint_pb2
from typing import List, Union

from ...general_imports import logging, INTERNAL_ID, DEBUG
from .Utilities import fill_info


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

def populateJoints (
    design: adsk.fusion.Design,
    joints: joint_pb2.Joints,
    progressDialog: adsk.core.ProgressDialog,
    options
):
    fill_info(joints, None)

    # This is for creating all of the Joint Definition objects
    # So we need to iterate through the joints and construct them and add them to the map
    if options.joints or DEBUG:
        for joint in list(design.rootComponent.allJoints) + list(
            design.rootComponent.allAsBuiltJoints
        ):
            if joint.isSuppressed:
                continue

            # for now if it's not a revolute or slider joint ignore it
            if joint.jointMotion.jointType != 1 and joint.jointMotion.jointType != 2:
                continue

            try:
                #  Fusion has no instances of joints but lets roll with it anyway

                progressDialog.message = f"Exporting Joint configuration {joint.name}"

                # create the definition
                joint_definition = joints.joint_definitions[joint.entityToken]
                _addJoint(joint, joint_definition)

                # create the instance of the single definition
                joint_instance = joints.joint_instances[joint.entityToken]
                _addJointInstance(joint, joint_instance)

                # adds information for joint motion and limits
                _motionFromJoint(joint.jointMotion, joint_definition)

            except:
                logging.getLogger(f"{INTERNAL_ID}.JointParser").error("Failed:\n{}".format(traceback.format_exc()))
                continue


def _addJoint(joint: adsk.fusion.Joint, joint_definition: joint_pb2.Joint):
    fill_info(joint_definition, joint)
    
    jointPivotTranslation = _jointOrigin(joint)

    if jointPivotTranslation :
        joint_definition.origin.x = jointPivotTranslation.x
        joint_definition.origin.y = jointPivotTranslation.y
        joint_definition.origin.z = jointPivotTranslation.z
    else:
        joint_definition.origin.x = 0.0
        joint_definition.origin.y = 0.0
        joint_definition.origin.z = 0.0
        if DEBUG:
            logging.getLogger(f"{INTERNAL_ID}.JointParser._addJoint").error(f"Cannot find joint origin on joint {joint.name}")

    joint_definition.break_magnitude = 0.0

def _addJointInstance(joint: adsk.fusion.Joint, joint_instance: joint_pb2.JointInstance):
    fill_info(joint_instance, joint)
    # because there is only one and we are using the token - should be the same
    joint_instance.joint_reference = joint_instance.info.GUID

    # assign part id values - bug with entity tokens
    try:
        joint_instance.parent = joint.occurrenceOne.entityToken
    except:
        joint_instance.parent = joint.occurrenceOne.name

    try:
        joint_instance.child = joint.occurrenceTwo.entityToken
    except:
        joint_instance.child = joint.occurrenceTwo.name

    # fill info for what parts are contained within this joint

def _motionFromJoint(fusionMotionDefinition: adsk.fusion.JointMotion, proto_joint: joint_pb2.Joint) -> None:
    # if fusionJoint.geometryOrOriginOne.objectType == "adsk::fusion::JointGeometry"
    # create the DOF depending on what kind of information the joint has
    
    fillJointMotionFuncSwitcher = {
        0: noop, # this should be ignored
        1: fillRevoluteJointMotion,
        2: fillSliderJointMotion,
        3: noop,  # TODO: Implement - Ball Joint at least
        4: noop,  # TODO: Implement
        5: noop,  # TODO: Implement
        6: noop,  # TODO: Implement
    }

    fillJointMotionFunc = fillJointMotionFuncSwitcher.get(
        fusionMotionDefinition.jointType, lambda: None
    )

    fillJointMotionFunc(fusionMotionDefinition, proto_joint)

def fillRevoluteJointMotion(revoluteMotion: adsk.fusion.RevoluteJointMotion, proto_joint: joint_pb2.Joint):
    """#### Fill Protobuf revolute joint motion data

    Args:
        revoluteMotion (adsk.fusion.RevoluteJointMotion): Fusion 360 Revolute Joint Data
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

    if (revoluteMotion.rotationLimits):
        dof.limits.lower = revoluteMotion.rotationLimits.minimumValue
        dof.limits.upper = revoluteMotion.rotationLimits.maximumValue

    rotationAxisVector = revoluteMotion.rotationAxisVector
    if rotationAxisVector:
        dof.axis.x = -rotationAxisVector.x
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
        sliderMotion (adsk.fusion.SliderJointMotion): Fusion 360 Slider Joint Data
        protoJoint (joint_pb2.Joint): Protobuf joint that is being modified

    """

    proto_joint.joint_motion_type = joint_pb2.JointMotion.SLIDER

    dof = proto_joint.prismatic.prismatic_freedom

    if sliderMotion.slideLimits:
        dof.limits.lower = sliderMotion.slideLimits.maximumValue
        dof.limits.upper = sliderMotion.slideLimits.minimumValue

    dof.value = sliderMotion.slideValue


def noop(*argv):
    
    """Easy way to keep track of no-op code that required function pointers"""
    pass

def _searchForGrounded(occ: adsk.fusion.Occurrence) -> Union[adsk.fusion.Occurrence, None]:
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

def _jointOrigin(
    fusionJoint: Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]
) -> adsk.core.Point3D:
    """#### Joint Origin Internal Finder that was orignally created for Synthesis by Liam Wang

    Args:
        joint (Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]): A Fusion 360 Joint that is either a Proper or As Build Joint

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
        return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
        offsetX = (
            0 if geometryOrOrigin.offsetX is None else geometryOrOrigin.offsetX.value
        )
        offsetY = (
            0 if geometryOrOrigin.offsetY is None else geometryOrOrigin.offsetY.value
        )
        offsetZ = (
            0 if geometryOrOrigin.offsetZ is None else geometryOrOrigin.offsetZ.value
        )
        # noinspection PyArgumentList
        return adsk.core.Point3D.create(
            origin.x + offsetX, origin.y + offsetY, origin.z + offsetZ
        )