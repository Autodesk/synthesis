import time

import adsk
import adsk.core
import adsk.fusion

from apper import Fusion360Utilities
from google.protobuf.json_format import MessageToDict

from gltf.extras.proto.gltf_extras_pb2 import Joint

def exportJoints(fusionJoints, projectId, perfWatch):
    joints = []
    allAffectedOccurrences = []
    for fusionJoint in fusionJoints:
        if isJointInvalid(fusionJoint):
            continue
        joint, affectedOccurrences = fillJoint(fusionJoint, projectId, perfWatch)
        allAffectedOccurrences += affectedOccurrences
        joints.append(MessageToDict(joint))
    return joints, [occ.fullPathName for occ in allAffectedOccurrences if occ is not None]

def isJointInvalid(fusionJoint):
    if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
        print("WARNING: Ignoring joint with unknown occurrences!")  # todo: Show these messages to the user
        return True
    if fusionJoint.jointMotion.jointType not in range(6):
        print("WARNING: Ignoring joint with unknown type!")
        return True
    return False

def fillJoint(fusionJoint, projectId, perfWatch):
    protoJoint = Joint()
    # protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
    protoJoint.header.name = fusionJoint.name
    protoJoint.header.uuid = Fusion360Utilities.item_id(fusionJoint, projectId)
    fillPoint3DConvertUnits(getJointOrigin(fusionJoint), protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked
    protoJoint.isSuppressed = fusionJoint.isSuppressed

    # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
    occurrenceTwo = fusionJoint.occurrenceOne
    occurrenceOne = fusionJoint.occurrenceTwo
    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, occurrenceTwo, projectId, perfWatch)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, occurrenceOne, projectId, perfWatch)

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
    # noinspection PyArgumentList
    fillJointMotionFunc(fusionJoint.jointMotion, protoJoint)
    return protoJoint, [occurrenceOne, occurrenceTwo]

def getJointOrigin(fusionJoint):
    geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
    if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
        return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
        # noinspection PyArgumentList
        return adsk.core.Point3D.create(origin.x + geometryOrOrigin.offsetX.value ,origin.y + geometryOrOrigin.offsetY.value ,origin.z + geometryOrOrigin.offsetZ.value)

def getJointedOccurrenceUUID(fusionJoint, fusionOccur, projectId, perfWatch):
    # if fusionOccur is None:
    #     return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    # return item_id(fusionOccur, ATTR_GROUP_NAME)
    if fusionOccur is None:
        return ""  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    perfWatch.switch_segment("item_id_joint")
    uuid = Fusion360Utilities.item_id(fusionOccur, projectId)
    perfWatch.stop()
    return uuid
    # return fusionOccur.fullPathName

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
