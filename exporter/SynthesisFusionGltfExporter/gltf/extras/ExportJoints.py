from typing import *

import adsk
import adsk.core
import adsk.fusion
from google.protobuf.json_format import MessageToDict

from apper import Fusion360Utilities
from gltf.extras.proto.gltf_extras_pb2 import Joint


def exportJoints(fusionJoints: List[adsk.fusion.Joint], groupName: str, rootNodeUUID: str, exportWarnings: List[str]) -> Tuple[List[Dict], List[str]]:
    jointsDict = []
    allAffectedOccurrences = []
    for fusionJoint in fusionJoints:
        result = fillJoint(fusionJoint, groupName, rootNodeUUID, exportWarnings)
        if result is None:
            continue
        joint, affectedOccurrences = result
        allAffectedOccurrences += affectedOccurrences
        jointsDict.append(MessageToDict(joint, including_default_value_fields=True))
    return jointsDict, [occ.fullPathName if occ is not None else "" for occ in allAffectedOccurrences]

def fillJoint(fusionJoint: adsk.fusion.Joint, groupName: str, rootUUID: str, exportWarnings: List[str]) -> Optional[Tuple[Joint, List[adsk.fusion.Occurrence]]]:
    if fusionJoint.jointMotion.jointType not in range(6):
        exportWarnings.append(f"Ignoring joint with unknown type: {fusionJoint.name}")
        return None

    occurrenceOne = fusionJoint.occurrenceOne
    occurrenceTwo = fusionJoint.occurrenceTwo

    if occurrenceOne is None:  # These are needed because there's a bug where .occurrenceOne and .occurrenceTwo don't work when a parent occ is jointed to a child occ. TODO: Report bug
        try:
            occurrenceOne = fusionJoint.geometryOrOriginOne.entityOne.assemblyContext
        except:
            pass

    if occurrenceTwo is None:
        try:
            occurrenceTwo = fusionJoint.geometryOrOriginTwo.entityOne.assemblyContext
        except:
            pass

    if occurrenceTwo is None and occurrenceOne is None:
        exportWarnings.append(f"Ignoring joint with unknown occurrence references: {fusionJoint.name}")
        return None

    protoJoint = Joint()
    protoJoint.header.name = fusionJoint.name
    protoJoint.header.uuid = Fusion360Utilities.item_id(fusionJoint, groupName)
    fillPoint3DConvertUnits(getJointOrigin(fusionJoint), protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked
    protoJoint.isSuppressed = fusionJoint.isSuppressed

    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(occurrenceTwo, groupName, rootUUID)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(occurrenceOne, groupName, rootUUID)

    fillJointMotionFuncSwitcher = {
        0: fillRigidJointMotion,
        1: fillRevoluteJointMotion,
        2: fillSliderJointMotion,
        3: fillCylindricalJointMotion,
        4: fillPinSlotJointMotion,
        5: fillPlanarJointMotion,
        6: fillBallJointMotion,
    }

    fillJointMotionFunc = fillJointMotionFuncSwitcher.get(fusionJoint.jointMotion.jointType, lambda: None)
    fillJointMotionFunc(fusionJoint.jointMotion, protoJoint)
    return protoJoint, [occurrenceOne, occurrenceTwo]

def getJointOrigin(fusionJoint: adsk.fusion.Joint) -> adsk.core.Point3D:
    geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
    if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
        return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
        offsetX = 0 if geometryOrOrigin.offsetX is None else geometryOrOrigin.offsetX.value
        offsetY = 0 if geometryOrOrigin.offsetY is None else geometryOrOrigin.offsetY.value
        offsetZ = 0 if geometryOrOrigin.offsetZ is None else geometryOrOrigin.offsetZ.value
        # noinspection PyArgumentList
        return adsk.core.Point3D.create(origin.x + offsetX, origin.y + offsetY, origin.z + offsetZ)

def getJointedOccurrenceUUID(fusionOccur: adsk.fusion.Occurrence, groupName: str, rootUUID: str) -> str:
    if fusionOccur is None:
        return rootUUID  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    return Fusion360Utilities.item_id(fusionOccur, groupName)

def fillRigidJointMotion(fusionJointMotion, protoJoint):
    protoJoint.rigidJointMotion.SetInParent()

def fillRevoluteJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.revoluteJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

def fillSliderJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.sliderJointMotion

    fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

def fillCylindricalJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.cylindricalJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

def fillPinSlotJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.pinSlotJointMotion

    fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
    protoJointMotion.slideValue = fusionJointMotion.slideValue
    fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

def fillPlanarJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.planarJointMotion

    fillVector3D(fusionJointMotion.normalDirectionVector, protoJointMotion.normalDirectionVector)

    fillVector3D(fusionJointMotion.primarySlideDirectionVector, protoJointMotion.primarySlideDirectionVector)
    protoJointMotion.primarySlideValue = fusionJointMotion.primarySlideValue
    fillJointLimits(fusionJointMotion.primarySlideLimits, protoJointMotion.primarySlideLimits)

    fillVector3D(fusionJointMotion.secondarySlideDirectionVector, protoJointMotion.secondarySlideDirectionVector)
    protoJointMotion.secondarySlideValue = fusionJointMotion.secondarySlideValue
    fillJointLimits(fusionJointMotion.secondarySlideLimits, protoJointMotion.secondarySlideLimits)

    protoJointMotion.rotationValue = fusionJointMotion.rotationValue
    fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

def fillBallJointMotion(fusionJointMotion, protoJoint):
    protoJointMotion = protoJoint.ballJointMotion

    fillVector3D(fusionJointMotion.rollDirectionVector, protoJointMotion.rollDirectionVector)
    protoJointMotion.rollValue = fusionJointMotion.rollValue
    fillJointLimits(fusionJointMotion.rollLimits, protoJointMotion.rollLimits)

    fillVector3D(fusionJointMotion.pitchDirectionVector, protoJointMotion.pitchDirectionVector)
    protoJointMotion.pitchValue = fusionJointMotion.pitchValue
    fillJointLimits(fusionJointMotion.pitchLimits, protoJointMotion.pitchLimits)

    fillVector3D(fusionJointMotion.yawDirectionVector, protoJointMotion.yawDirectionVector)
    protoJointMotion.yawValue = fusionJointMotion.yawValue
    fillJointLimits(fusionJointMotion.yawLimits, protoJointMotion.yawLimits)

def fillJointLimits(fusionJointLimits, protoJointLimits):
    protoJointLimits.isMaximumValueEnabled = fusionJointLimits.isMaximumValueEnabled
    protoJointLimits.isMinimumValueEnabled = fusionJointLimits.isMinimumValueEnabled
    protoJointLimits.isRestValueEnabled = fusionJointLimits.isRestValueEnabled
    protoJointLimits.maximumValue = fusionJointLimits.maximumValue
    protoJointLimits.minimumValue = fusionJointLimits.minimumValue
    protoJointLimits.restValue = fusionJointLimits.restValue

def fillVector3D(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x
    protoVector3D.y = fusionVector3D.y
    protoVector3D.z = fusionVector3D.z

def fillPoint3DConvertUnits(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x/100
    protoVector3D.y = fusionVector3D.y/100
    protoVector3D.z = fusionVector3D.z/100
