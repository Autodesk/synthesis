import adsk.core, adsk.fusion, traceback, logging
from typing import *
from proto.proto_out import joint_pb2, mirabuf_pb2, types_pb2
from .. import ParseOptions
from .Utilities import *

from ...general_imports import INTERNAL_ID


def ParseAllJoints(
    parseOptions: ParseOptions,
    design: adsk.fusion.Design,
    assembly_data: mirabuf_pb2.AssemblyData,
):

    logger = logging.getLogger(f"{INTERNAL_ID}.Parser.Joints")

    if parseOptions.joints:
        for joint in list(design.rootComponent.allJoints) + list(
            design.rootComponent.allAsBuiltJoints
        ):
            try:
                proto_joint = assembly_data.joints[joint.entityToken]
                fill_info(proto_joint, joint)
                ExportJoint(joint, proto_joint)
            except:
                logger.error("Failed:\n{}".format(traceback.format_exc()))
                continue
    pass


def ExportJoint(
    fus_joint: Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint],
    proto_joint: joint_pb2.Joint,
) -> None:
    """#### Exports the Joint in the Current Fusion 360 file that is either a Joint or AsBuiltJoint

    Revised from code created by : @https://github.com/liamjwang - he's awesome thanks liam

    Args:
        fus_joint (Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]): Fusion 360 component Joint

    Returns:
        joint_pb2.Joint: Protobuf Joint definition to add to Component
    """
    try:

        # if it's not shown or if its surpressed then don't export it I suppose?
        if fus_joint.isSuppressed:
            return None

        log = logging.getLogger("HellionFusion.Parser.Joints")

        if fus_joint.jointMotion.jointType not in range(6):
            log.error(
                f"Joint cannot be exported by the current version \n\ttype: {fus_joint.jointMotion.jointType}"
            )
            return None

        if fus_joint and fus_joint.occurrenceOne and fus_joint.occurrenceTwo:
            occurrenceOne = fus_joint.occurrenceOne
            occurrenceTwo = fus_joint.occurrenceTwo
        else:
            return None

        if occurrenceOne is None:
            try:
                occurrenceOne = fus_joint.geometryOrOriginOne.entityOne.assemblyContext
            except:
                pass

        if occurrenceTwo is None:
            try:
                occurrenceTwo = fus_joint.geometryOrOriginTwo.entityOne.assemblyContext
            except:
                pass

        if occurrenceTwo is None and occurrenceOne is None:
            log.error(
                f"Occurrences that connect joints could not be found\n\t1: {occurrenceOne}\n\t2: {occurrenceTwo}"
            )
            return None

        # why would this throw an exception - just return null please
        if fus_joint.objectType != "adsk::fusion::AsBuiltJoint":
            if fus_joint.offset:
                proto_joint.Offset = fus_joint.offset.value
            else:
                proto_joint.Offset = 0.0

            if fus_joint.angle:
                proto_joint.Angle = fus_joint.angle.value
            else:
                proto_joint.Angle = 0.0

        proto_joint.JointMotionType = fus_joint.jointMotion.jointType

        MapJointMotion(fus_joint.jointMotion, proto_joint)

        # proto_joint.name = fus_joint.name

        # This needs an actual occurrence reference as you can joint non-unique occurrences
        # proto_joint.Occurrence1 = occurrenceOne.attributes.itemByName('UnityFile', 'occurrenceRef').value # This now has the guid with the revision id unfortunetly
        # proto_joint.Occurrence2 = occurrenceTwo.attributes.itemByName('UnityFile', 'occurrenceRef').value

        proto_joint.Part1 = occurrenceOne.fullPathName
        proto_joint.Part2 = occurrenceTwo.fullPathName

        proper_origin = JointOrigin(fus_joint)

        if proper_origin:
            new_origin = proto_joint.origin
            new_origin.x = -proper_origin.x
            new_origin.y = proper_origin.y
            new_origin.z = proper_origin.z

        return proto_joint
    except RuntimeError:
        # Avoid the Joint bug for now - logs in parser
        return None


def JointOrigin(
    fusionJoint: Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]
) -> adsk.core.Point3D:
    """#### Joint Origin Internal Finder that was orignally created for Synthesis by Liam Wang

    Args:
        joint (Union[adsk.fusion.Joint, adsk.fusion.AsBuiltJoint]): A Fusion 360 Joint that is either a Proper or As Build Joint

    Returns:
        autodesk.core.Vector3: Protobuf Vector3 of the new Joint
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


def MapJointMotion(
    fusionMotionDefinition: adsk.fusion.JointMotion, hellionJoint: joint_pb2.Joint
) -> None:
    """#### Maps Fusion Joint data to a oneof field to specify additional data to create joint in Unity

    Args:
        fusionMotionDefinition (adsk.fusion.JointMotion): Fusion Base Motion Class
        hellionJoint (joint_pb2.Joint): Protobuf Joint class that will be modified
    """

    fillJointMotionFuncSwitcher = {
        0: fillRigidJointMotion,
        1: fillRevoluteJointMotion,
        2: fillSliderJointMotion,
        3: noop,  # TODO: Implement
        4: noop,  # TODO: Implement
        5: noop,  # TODO: Implement
        6: noop,  # TODO: Implement
    }

    fillJointMotionFunc = fillJointMotionFuncSwitcher.get(
        fusionMotionDefinition.jointType, lambda: None
    )
    fillJointMotionFunc(fusionMotionDefinition, hellionJoint)


def fillRevoluteJointMotion(
    revoluteMotion: adsk.fusion.RevoluteJointMotion, hellionJoint: joint_pb2.Joint
) -> None:
    """#### Fill Protobuf revolute joint motion data

    Args:
        revoluteMotion (adsk.fusion.RevoluteJointMotion): Fusion 360 Revolute Joint Data
        hellionJoint (joint_pb2.Joint): Protobuf joint that is being modified

    Protobuf Definition:

        - Axis RotationAxis = 1;
        - Vector3 AxisVector = 2;
        - double MaxValue = 3; // in radians
        - double MinValue = 4; // in radians
        - double CurrentValue = 5; // in radians
    """
    _revMotion = hellionJoint.RevoluteMotion

    # 0-3 XYZ 4 Custom ?
    _revMotion.RotationAxis = revoluteMotion.rotationAxis

    # Copy Axis vector into proper format
    HVec3(revoluteMotion.rotationAxisVector, _revMotion.AxisVector)

    if revoluteMotion.rotationLimits:
        _revMotion.MaxValue = revoluteMotion.rotationLimits.maximumValue
        _revMotion.MinValue = revoluteMotion.rotationLimits.minimumValue
        _revMotion.CurrentValue = revoluteMotion.rotationValue


def fillSliderJointMotion(
    sliderMotion: adsk.fusion.SliderJointMotion, hellionJoint: joint_pb2.Joint
) -> None:
    """#### Fill Protobuf slider joint motion data

    Args:
        sliderMotion (adsk.fusion.SliderJointMotion): Fusion 360 Slider Joint Data
        hellionJoint (joint_pb2.Joint): Protobuf joint that is being modified

    Protobuf Definition:

        - Axis SlideAxis = 1;
        - Vector3_D AxisVector = 2;
        - double MaxValue = 3; // in radians
        - double MinValue = 4; // in radians
        - double CurrentValue = 5; // in radians

    """

    _slideMotion = hellionJoint.SlideMotion

    # 0-3 XYZ 4 Custom ?
    _slideMotion.SlideAxis = sliderMotion.slideDirection

    # Copy Axis vector into proper format
    HVec3(sliderMotion.slideDirectionVector, _slideMotion.AxisVector)

    if sliderMotion.slideLimits:
        _slideMotion.MaxValue = sliderMotion.slideLimits.maximumValue
        _slideMotion.MinValue = sliderMotion.slideLimits.minimumValue
        _slideMotion.CurrentValue = sliderMotion.slideValue


def fillRigidJointMotion(
    rigidMotion: adsk.fusion.RigidJointMotion, hellionJoint: joint_pb2.Joint
) -> None:
    """Defines the Rigid Motion Type with no attributing data since there is none

    Args:
        rigidMotion (adsk.fusion.RigidJointMotion): Motion Object
        hellionJoint (joint_pb2.Joint): Protobuf Joint Object
    """
    _rigidMotion = hellionJoint.RigidMotion


def noop(*argv) -> None:
    """Easy way to keep track of no-op code that required function pointers"""
    pass


def HVec3(
    FVec: Union[adsk.core.Point3D, adsk.core.Vector3D], HVec3: types_pb2.Vector3_D
) -> None:
    """Helper function to convert Fusion Point3D elements to Protobuf compatible data

    Args:
        FVec (adsk.core.Point3D, adsk.core.Vector3): Fusion Point3D or Vector3D
        HVec3 (joint_pb2.Vector3_D): Protobuf Vector3 Data
    """
    # Just modifies the passed in HVec Target.
    if FVec is not None:
        HVec3.x = -FVec.x  # This is still necessary somehow
        HVec3.y = FVec.y
        HVec3.z = FVec.z
    else:
        HVec3.x = 0
        HVec3.y = 0
        HVec3.z = 0
