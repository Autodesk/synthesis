import time

import adsk
import adsk.core
import adsk.fusion
import numpy as np

import apper
from apper import AppObjects, item_id
from ..utils.DebugHierarchy import *
from ..utils.GLTFUtils import *

ATTR_GROUP_NAME = "SynthesisFusionExporter"  # attribute group name for use with apper's item_id


def exportRobot():
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    gltf = GLTF2()
    fillGltf(ao, gltf)

    # jsonFilePath = '{0}{1}_{2}.{3}'.format('C:/temp/', "test", int(time.time()), "gltf")
    # gltf.save_json(jsonFilePath)

    gltf.convert_buffers(BufferFormat.BINARYBLOB)
    glbFilePath = '{0}{1}_{2}.{3}'.format('C:/temp/', ao.document.name.replace(" ", "_"), int(time.time()), "glb")
    gltf.save_binary(glbFilePath)

    dict = gltf_asdict(gltf) # debugging only

    # printHierarchy(ao.root_comp)
    print()  # put breakpoint here and view the protoDocumentAsDict local variable


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()


# -----------Gltf-----------

def fillGltf(ao, gltf):
    bufferAccum = GltfBufferAccumulator(gltf)

    fillAssetMetadata(ao, gltf.asset)
    componentUuidToIndexMap = fillMeshes(ao.design.allComponents, gltf, bufferAccum)
    # fillJoints(ao.design.rootComponent.allJoints, gltf)
    # fillMaterialsAndAppearances(ao.design, gltf)
    fillScene(ao.design.rootComponent, gltf, componentUuidToIndexMap)

    bufferAccum.close()


def fillAssetMetadata(ao, asset):
    fusionDocument = ao.document
    fusionCurrentUser = ao.app.currentUser
    #
    # asset.fusionVersion = fusionDocument.version
    # asset.name = fusionDocument.name
    # asset.versionNumber = fusionDocument.dataFile.versionNumber
    # asset.description = fusionDocument.dataFile.description
    # asset.id = fusionDocument.dataFile.id
    # asset.exportTime = int(time.time())
    #
    # asset.userName = fusionCurrentUser.userName
    # asset.id = fusionCurrentUser.userId
    # asset.displayName = fusionCurrentUser.displayName
    # asset.email = fusionCurrentUser.email


# -----------Occurrence Tree-----------

def fillScene(rootComponent, gltf, componentUuidToIndexMap):
    scene = Scene()
    scene.nodes.append(fillRootNode(rootComponent, gltf, componentUuidToIndexMap))
    gltf.scenes.append(scene)


def fillRootNode(rootComponent, gltf, componentUuidToIndexMap):
    node = Node()
    node.name = rootComponent.name
    node.mesh = componentUuidToIndexMap.get(item_id(rootComponent, ATTR_GROUP_NAME), None)
    node.extras['uuid'] = item_id(rootComponent, ATTR_GROUP_NAME)

    node.children = [fillNode(occur, gltf, componentUuidToIndexMap) for occur in rootComponent.occurrences]
    gltf.nodes.append(node)
    return len(gltf.nodes) - 1


def fillNode(occur, gltf, componentUuidToIndexMap):
    node = Node()
    node.name = occur.name
    # print("[N] "+occur.name)
    node.matrix = np.reshape(occur.transform.asArray(), (4, 4), order='F').flatten().tolist()  # transpose the flat array
    node.mesh = componentUuidToIndexMap.get(item_id(occur.component, ATTR_GROUP_NAME), None)
    # node.extras['uuid'] = item_id(occur, ATTR_GROUP_NAME)
    node.extras['isGrounded'] = occur.isGrounded

    node.children = [fillNode(occur, gltf, componentUuidToIndexMap) for occur in occur.childOccurrences]
    gltf.nodes.append(node)
    return len(gltf.nodes) - 1


# -----------Components-----------

# <editor-fold desc="Components">

def fillMeshes(fusionComponents, gltf, bufferAccum):
    componentUuidToIndex = {}

    for fusionComponent in fusionComponents:
        if len(fusionComponent.bRepBodies) == 0: continue
        uuid, index = fillMesh(fusionComponent, gltf, bufferAccum)
        componentUuidToIndex[uuid] = index

    return componentUuidToIndex


def fillMesh(fusionComponent, gltf, bufferAccum):
    mesh = Mesh()
    meshUUID = item_id(fusionComponent, ATTR_GROUP_NAME)
    mesh.extras['uuid'] = meshUUID
    mesh.name = fusionComponent.name
    # print("[M] "+fusionComponent.name)
    mesh.extras['description'] = fusionComponent.description
    mesh.extras['revisionId'] = fusionComponent.revisionId
    mesh.extras['partNumber'] = fusionComponent.partNumber
    # fillBoundingBox3D(fusionComponent.boundingBox, protoComponent.boundingBox)
    # protoComponent.materialId = fusionComponent.material.id
    # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

    mesh.primitives = [fillPrimitiveFromBrep(bRepBody, gltf, bufferAccum) for bRepBody in fusionComponent.bRepBodies]

    gltf.meshes.append(mesh)
    return meshUUID, len(gltf.meshes) - 1


def fillPrimitiveFromBrep(fusionBRepBody, gltf, bufferAccum):
    primitive = Primitive()
    # primitive.extras['uuid'] = item_id(fusionBRepBody, ATTR_GROUP_NAME)
    primitive.extras['name'] = fusionBRepBody.name
    # protoMeshBody.appearanceId = fusionBRepBody.appearance.id
    # protoMeshBody.materialId = fusionBRepBody.material.id
    # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)
    # fillBoundingBox3D(fusionBRepBody.boundingBox, protoMeshBody.boundingBox3D)
    meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
    meshCalculator.setQuality(11)  # todo mesh quality settings
    mesh = meshCalculator.calculate()

    indicesBufferViewIndex, verticesBufferViewIndex = addPrimitiveData(gltf, bufferAccum, mesh.nodeIndices, mesh.nodeCoordinatesAsFloat)
    primitive.attributes = Attributes()
    primitive.attributes.POSITION = verticesBufferViewIndex
    primitive.indices = indicesBufferViewIndex

    return primitive


def fillPhysicalProperties(fusionPhysical, protoPhysical):
    protoPhysical.density = fusionPhysical.density
    protoPhysical.mass = fusionPhysical.mass
    protoPhysical.volume = fusionPhysical.volume
    protoPhysical.area = fusionPhysical.area
    fillVector3D(fusionPhysical.centerOfMass, protoPhysical.centerOfMass)


# </editor-fold>

# -----------Joints-----------

# <editor-fold desc="Joints">

def fillJoints(fusionJoints, protoJoints):
    for fusionJoint in fusionJoints:
        if isJointInvalid(fusionJoint): continue
        fillJoint(fusionJoint, protoJoints.add())


def isJointInvalid(fusionJoint):
    if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
        print("WARNING: Ignoring joint with unknown occurrences!")  # todo: Show these messages to the user
        return True
    if fusionJoint.jointMotion.jointType not in range(6):
        print("WARNING: Ignoring joint with unknown type!")
        return True
    return False


def fillJoint(fusionJoint, protoJoint):
    protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
    protoJoint.header.name = fusionJoint.name
    fillVector3D(getJointOrigin(fusionJoint), protoJoint.origin)
    protoJoint.isLocked = fusionJoint.isLocked
    protoJoint.isSuppressed = fusionJoint.isSuppressed

    # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
    protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
    protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)

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


def getJointOrigin(fusionJoint):
    geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
    if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
        return geometryOrOrigin.origin
    else:  # adsk::fusion::JointOrigin
        origin = geometryOrOrigin.geometry.origin
        return adsk.core.Point3D.create(  # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
            origin.x + geometryOrOrigin.offsetX.value,
            origin.y + geometryOrOrigin.offsetY.value,
            origin.z + geometryOrOrigin.offsetZ.value)


def getJointedOccurrenceUUID(fusionJoint, fusionOccur):
    if fusionOccur is None:
        return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
    return item_id(fusionOccur, ATTR_GROUP_NAME)


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


# </editor-fold>

# -----------Materials-----------

# <editor-fold desc="Materials">

def fillMaterialsAndAppearances(fusionMaterials, protoMaterials):
    for fusionMaterial in fusionMaterials:
        fillMaterial(fusionMaterial, protoMaterials.add())


def fillMaterial(fusionMaterial, protoMaterial):
    protoMaterial.id = fusionMaterial.id
    protoMaterial.name = fusionMaterial.name
    protoMaterial.appearanceId = fusionMaterial.appearance.id
    # todo add protobuf def: MaterialProperties properties
    # fillMaterialsProperties()


def fillMaterialsProperties(fusionMaterials, protoMaterials):
    protoMaterials.density = fusionMaterials.density
    protoMaterials.yieldStrength = fusionMaterials.yieldStrength
    protoMaterials.tensileStrength = fusionMaterials.tensileStrength


# </editor-fold>

# -----------Appearances-----------

# <editor-fold desc="Appearances">

def fillAppearances(fusionAppearances, protoAppearances):
    for childAppearance in fusionAppearances:
        fillAppearance(childAppearance, protoAppearances.add())


def fillAppearance(fusionAppearance, protoAppearance):
    protoAppearance.id = fusionAppearance.id
    protoAppearance.name = fusionAppearance.name
    protoAppearance.hasTexture = fusionAppearance.hasTexture
    # todo add protobuf def: AppearanceProperties properties


def fillAppearanceProperties(fusionAppearanceProps, protoAppearanceProps):
    pass  # todo


# </editor-fold>

# -----------Generic-----------

def fillColor(fusionColor, protoColor):
    pass  # todo


def fillBoundingBox3D(fusionBoundingBox, protoBoundingBox):
    fillVector3D(fusionBoundingBox.maxPoint, protoBoundingBox.maxPoint)
    fillVector3D(fusionBoundingBox.minPoint, protoBoundingBox.minPoint)


def fillVector3D(fusionVector3D, protoVector3D):
    protoVector3D.x = fusionVector3D.x
    protoVector3D.y = fusionVector3D.y
    protoVector3D.z = fusionVector3D.z


def fillMatrix3D(fusionTransform, protoTransform):
    assert len(protoTransform.cells) == 0  # Don't try to fill a matrix that's already full
    protoTransform.cells.extend(fusionTransform.asArray())
