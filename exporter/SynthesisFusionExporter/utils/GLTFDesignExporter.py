import io
import time
import numpy as np

import adsk
import adsk.core
import adsk.fusion

from pygltflib import *
from apper import AppObjects

from ..utils import gltf2_io_constants, gltf2_blender_utils

GLTF_VERSION = 2
GLTFHeaderSize = 12
SectionHeaderSize = 8

def calculateAlignment(currentSize: int, byteAlignment: int = 4):
    if currentSize % byteAlignment == 0: return currentSize
    return currentSize + (byteAlignment - currentSize % byteAlignment)


def alignToBoundary(stream: io.BytesIO, pad: bytes, byteAlignment: int = 4):
    currentSize = len(stream.getbuffer())
    stream.seek(0, io.SEEK_END)
    for i in range(calculateAlignment(currentSize, byteAlignment) - currentSize):
        stream.write(pad)


class GLTFDesignExporter():

    def __init__(self, ao: AppObjects):
        self.ao = ao

        self.gltf = GLTF2()

        self.gltf.asset = Asset()
        self.gltf.asset.generator = "Fusion 360 exporter for Synthesis"

        self.primaryBuffer = Buffer()
        self.primaryBufferId = len(self.gltf.buffers)
        self.gltf.buffers.append(self.primaryBuffer)

    def saveGLB(self, filename: str):
        with open(filename, 'wb') as fileByteStream:
            self.saveGLBToStream(fileByteStream)

    def saveGLBToStream(self, stream: io.BytesIO):
        self.primaryBufferStream = io.BytesIO()

        self.gltf.scene = self.exportScene()  # export the current document

        self.primaryBuffer.byteLength = calculateAlignment(len(self.primaryBufferStream.getbuffer()))

        json_blob = self.gltf.gltf_to_json().encode("utf-8")

        # align JSON to boundary
        if len(json_blob) % 4 != 0:
            json_blob += b'   '[0:4 - len(json_blob) % 4]

        alignToBoundary(self.primaryBufferStream, b'\x00')

        glbLength = GLTFHeaderSize + \
                    SectionHeaderSize + len(json_blob) + \
                    SectionHeaderSize + len(self.primaryBufferStream.getbuffer())

        # header
        stream.write(b'glTF')
        stream.write(struct.pack('<I', GLTF_VERSION))
        stream.write(struct.pack('<I', glbLength))

        # json chunk
        stream.write(struct.pack('<I', len(json_blob)))
        stream.write(bytes("JSON", 'utf-8'))
        stream.write(json_blob)

        # buffer chunk
        stream.write(struct.pack('<I', len(self.primaryBufferStream.getbuffer())))
        stream.write(bytes("BIN\x00", 'utf-8'))
        stream.write(self.primaryBufferStream.getbuffer())

        stream.flush()

    def exportScene(self):
        start = time.perf_counter()

        componentRevIDToIndexMap = self.exportMeshes(self.ao.design.allComponents)

        meshesDone = time.perf_counter()


        scene = Scene()
        scene.nodes.append(self.exportRootNode(self.ao.root_comp, componentRevIDToIndexMap))
        self.gltf.scenes.append(scene)
        nodesDone = time.perf_counter()
        print(f"Export completed in {meshesDone - start} seconds")
        print(f"Export completed in {nodesDone - meshesDone} seconds")
        print(f"nodesDone  {nodesDone} seconds")

        return len(self.gltf.scenes) - 1

    def exportRootNode(self, rootComponent, componentRevIDToIndexMap):
        node = Node()
        node.name = rootComponent.name
        node.mesh = componentRevIDToIndexMap.get(rootComponent.revisionId, None)

        node.children = [self.exportNode(occur, componentRevIDToIndexMap) for occur in rootComponent.occurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportNode(self, occur, componentRevIDToIndexMap):
        node = Node()
        node.name = occur.name
        node.matrix = np.reshape(occur.transform.asArray(), (4, 4), order='F').flatten().tolist()  # transpose the flat array
        node.mesh = componentRevIDToIndexMap.get(occur.component.revisionId, None)
        # node.extras['isGrounded'] = occur.isGrounded

        node.children = [self.exportNode(occur, componentRevIDToIndexMap) for occur in occur.childOccurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportMeshes(self, fusionComponents):
        componentRevIDToIndexMap = {}

        for fusionComponent in fusionComponents:
            if len(fusionComponent.bRepBodies) == 0:
                continue
            index = self.exportMesh(fusionComponent)
            componentRevIDToIndexMap[fusionComponent.revisionId] = index

        return componentRevIDToIndexMap

    def exportMesh(self, fusionComponent):
        mesh = Mesh()
        # meshUUID = item_id(fusionComponent, ATTR_GROUP_NAME)
        # mesh.extras['uuid'] = meshUUID
        mesh.name = fusionComponent.name
        # print("[M] "+fusionComponent.name)
        mesh.extras['description'] = fusionComponent.description
        mesh.extras['revisionId'] = fusionComponent.revisionId
        mesh.extras['partNumber'] = fusionComponent.partNumber
        # fillBoundingBox3D(fusionComponent.boundingBox, protoComponent.boundingBox)
        # protoComponent.materialId = fusionComponent.material.id
        # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

        mesh.primitives = [self.exportPrimitiveBrep(bRepBody) for bRepBody in fusionComponent.bRepBodies]

        self.gltf.meshes.append(mesh)
        return len(self.gltf.meshes) - 1

    def exportPrimitiveBrep(self, fusionBRepBody):
        primitive = Primitive()
        # primitive.extras['uuid'] = item_id(fusionBRepBody, ATTR_GROUP_NAME)
        primitive.extras['name'] = fusionBRepBody.name
        # protoMeshBody.appearanceId = fusionBRepBody.appearance.id
        # protoMeshBody.materialId = fusionBRepBody.material.id
        # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)
        # fillBoundingBox3D(fusionBRepBody.boundingBox, protoMeshBody.boundingBox3D)
        # start = time.perf_counter()
        meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
        meshCalculator.setQuality(11)  # todo mesh quality settings
        mesh = meshCalculator.calculate()

        # calculatedMesh = time.perf_counter()

        # indicesBufferViewIndex, verticesBufferViewIndex = addPrimitiveData(gltf, bufferAccum, mesh.nodeIndices, mesh.nodeCoordinatesAsFloat)
        # end = time.perf_counter()
        # print(f"Calculated mesh in {calculatedMesh-start} seconds")
        # print(f"Added primitive data in {end-calculatedMesh} seconds")

        primitive.attributes = Attributes()
        primitive.attributes.POSITION = self.exportVec3Accessor(mesh.nodeCoordinatesAsFloat)
        primitive.indices = self.exportIndicesAccessor(mesh.nodeIndices)

        return primitive

    def exportIndicesAccessor(self, array: List[int]):
        count = int(len(array)/gltf2_io_constants.DataType.num_elements(gltf2_io_constants.DataType.Scalar))
        assert count != 0

        accessor = Accessor()

        accessor.count = count
        accessor.type = gltf2_io_constants.DataType.Scalar

        accessor.max = gltf2_blender_utils.max_components(array, gltf2_io_constants.DataType.Scalar)
        accessor.min = gltf2_blender_utils.min_components(array, gltf2_io_constants.DataType.Scalar)

        alignToBoundary(self.primaryBufferStream, b'\x00')
        byteOffset = calculateAlignment(self.primaryBufferStream.tell())

        accessor.componentType = gltf2_io_constants.ComponentType.UnsignedShort  # todo: smallest component type needed
        for item in array:
            self.primaryBufferStream.write(struct.pack("<H", item))

        byteLength = calculateAlignment(self.primaryBufferStream.tell() - byteOffset)

        accessor.bufferView = self.exportBufferView(byteOffset, byteLength)

        self.gltf.accessors.append(accessor)
        return len(self.gltf.accessors) - 1

    def exportVec3Accessor(self, array: List[int]):
        count = int(len(array)/gltf2_io_constants.DataType.num_elements(gltf2_io_constants.DataType.Vec3))
        assert count != 0

        accessor = Accessor()

        accessor.count = count
        accessor.type = gltf2_io_constants.DataType.Vec3

        accessor.max = gltf2_blender_utils.max_components(array, gltf2_io_constants.DataType.Vec3)
        accessor.min = gltf2_blender_utils.min_components(array, gltf2_io_constants.DataType.Vec3)

        alignToBoundary(self.primaryBufferStream, b'\x00')
        byteOffset = calculateAlignment(self.primaryBufferStream.tell())

        accessor.componentType = gltf2_io_constants.ComponentType.Float
        for item in array:
            self.primaryBufferStream.write(struct.pack("<f", item))

        byteLength = calculateAlignment(self.primaryBufferStream.tell() - byteOffset)

        accessor.bufferView = self.exportBufferView(byteOffset, byteLength)

        self.gltf.accessors.append(accessor)
        return len(self.gltf.accessors) - 1

    def exportBufferView(self, byteOffset, byteLength):
        bufferView = BufferView()
        bufferView.buffer = self.primaryBufferId
        bufferView.byteOffset = byteOffset
        bufferView.byteLength = byteLength

        self.gltf.bufferViews.append(bufferView)
        return len(self.gltf.bufferViews) - 1

#
# # -----------Gltf-----------
#
# def fillGltf(ao, gltf):
#     bufferAccum = GltfBufferAccumulator(gltf)
#
#     fillAssetMetadata(ao, gltf.asset)
#     componentUuidToIndexMap = fillMeshes(ao.design.allComponents, gltf, bufferAccum)
#     # fillJoints(ao.design.rootComponent.allJoints, gltf)
#     # fillMaterialsAndAppearances(ao.design, gltf)
#     fillScene(ao.design.rootComponent, gltf, componentUuidToIndexMap)
#
#     bufferAccum.close()
#
#
# def fillAssetMetadata(ao, asset):
#     fusionDocument = ao.document
#     fusionCurrentUser = ao.app.currentUser
#     #
#     # asset.fusionVersion = fusionDocument.version
#     # asset.name = fusionDocument.name
#     # asset.versionNumber = fusionDocument.dataFile.versionNumber
#     # asset.description = fusionDocument.dataFile.description
#     # asset.id = fusionDocument.dataFile.id
#     # asset.exportTime = int(time.time())
#     #
#     # asset.userName = fusionCurrentUser.userName
#     # asset.id = fusionCurrentUser.userId
#     # asset.displayName = fusionCurrentUser.displayName
#     # asset.email = fusionCurrentUser.email
#
#
# # -----------Occurrence Tree-----------
#
# def fillScene(rootComponent, gltf, componentUuidToIndexMap):
#     scene = Scene()
#     scene.nodes.append(fillRootNode(rootComponent, gltf, componentUuidToIndexMap))
#     gltf.scenes.append(scene)
#
#
# # -----------Components-----------
#
# # <editor-fold desc="Components">
#
#
# def fillPhysicalProperties(fusionPhysical, protoPhysical):
#     protoPhysical.density = fusionPhysical.density
#     protoPhysical.mass = fusionPhysical.mass
#     protoPhysical.volume = fusionPhysical.volume
#     protoPhysical.area = fusionPhysical.area
#     fillVector3D(fusionPhysical.centerOfMass, protoPhysical.centerOfMass)
#
#
# # </editor-fold>
#
# # -----------Joints-----------
#
# # <editor-fold desc="Joints">
#
# def fillJoints(fusionJoints, protoJoints):
#     for fusionJoint in fusionJoints:
#         if isJointInvalid(fusionJoint): continue
#         fillJoint(fusionJoint, protoJoints.add())
#
#
# def isJointInvalid(fusionJoint):
#     if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
#         print("WARNING: Ignoring joint with unknown occurrences!")  # todo: Show these messages to the user
#         return True
#     if fusionJoint.jointMotion.jointType not in range(6):
#         print("WARNING: Ignoring joint with unknown type!")
#         return True
#     return False
#
#
# def fillJoint(fusionJoint, protoJoint):
#     # protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
#     protoJoint.header.name = fusionJoint.name
#     fillVector3D(getJointOrigin(fusionJoint), protoJoint.origin)
#     protoJoint.isLocked = fusionJoint.isLocked
#     protoJoint.isSuppressed = fusionJoint.isSuppressed
#
#     # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
#     protoJoint.occurrenceOneUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
#     protoJoint.occurrenceTwoUUID = getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)
#
#     fillJointMotionFuncSwitcher = {
#         0: fillRigidJointMotion,
#         1: fillRevoluteJointMotion,
#         2: fillSliderJointMotion,
#         3: fillCylindricalJointMotion,
#         4: fillPinSlotJointMotion,
#         5: fillPlanarJointMotion,
#         6: fillBallJointMotion,
#     }
#
#     fillJointMotionFunc = fillJointMotionFuncSwitcher.get(fusionJoint.jointMotion.jointType, lambda: None)
#     fillJointMotionFunc(fusionJoint.jointMotion, protoJoint)
#
#
# def getJointOrigin(fusionJoint):
#     geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
#     if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
#         return geometryOrOrigin.origin
#     else:  # adsk::fusion::JointOrigin
#         origin = geometryOrOrigin.geometry.origin
#         return adsk.core.Point3D.create(  # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
#             origin.x + geometryOrOrigin.offsetX.value,
#             origin.y + geometryOrOrigin.offsetY.value,
#             origin.z + geometryOrOrigin.offsetZ.value)
#
#
# def getJointedOccurrenceUUID(fusionJoint, fusionOccur):
#     # if fusionOccur is None:
#     # return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
#     # return item_id(fusionOccur, ATTR_GROUP_NAME)
#     return None
#
#
# def fillRigidJointMotion(fusionJointMotion, protoJoint):
#     protoJoint.rigidJointMotion.SetInParent()
#
#
# def fillRevoluteJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.revoluteJointMotion
#
#     fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
#     protoJointMotion.rotationValue = fusionJointMotion.rotationValue
#     fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)
#
#
# def fillSliderJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.sliderJointMotion
#
#     fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
#     protoJointMotion.slideValue = fusionJointMotion.slideValue
#     fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)
#
#
# def fillCylindricalJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.cylindricalJointMotion
#
#     fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
#     protoJointMotion.rotationValue = fusionJointMotion.rotationValue
#     fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)
#
#     protoJointMotion.slideValue = fusionJointMotion.slideValue
#     fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)
#
#
# def fillPinSlotJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.pinSlotJointMotion
#
#     fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
#     protoJointMotion.rotationValue = fusionJointMotion.rotationValue
#     fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)
#
#     fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
#     protoJointMotion.slideValue = fusionJointMotion.slideValue
#     fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)
#
#
# def fillPlanarJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.planarJointMotion
#
#     fillVector3D(fusionJointMotion.normalDirectionVector, protoJointMotion.normalDirectionVector)
#
#     fillVector3D(fusionJointMotion.primarySlideDirectionVector, protoJointMotion.primarySlideDirectionVector)
#     protoJointMotion.primarySlideValue = fusionJointMotion.primarySlideValue
#     fillJointLimits(fusionJointMotion.primarySlideLimits, protoJointMotion.primarySlideLimits)
#
#     fillVector3D(fusionJointMotion.secondarySlideDirectionVector, protoJointMotion.secondarySlideDirectionVector)
#     protoJointMotion.secondarySlideValue = fusionJointMotion.secondarySlideValue
#     fillJointLimits(fusionJointMotion.secondarySlideLimits, protoJointMotion.secondarySlideLimits)
#
#     protoJointMotion.rotationValue = fusionJointMotion.rotationValue
#     fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)
#
#
# def fillBallJointMotion(fusionJointMotion, protoJoint):
#     protoJointMotion = protoJoint.ballJointMotion
#
#     fillVector3D(fusionJointMotion.rollDirectionVector, protoJointMotion.rollDirectionVector)
#     protoJointMotion.rollValue = fusionJointMotion.rollValue
#     fillJointLimits(fusionJointMotion.rollLimits, protoJointMotion.rollLimits)
#
#     fillVector3D(fusionJointMotion.pitchDirectionVector, protoJointMotion.pitchDirectionVector)
#     protoJointMotion.pitchValue = fusionJointMotion.pitchValue
#     fillJointLimits(fusionJointMotion.pitchLimits, protoJointMotion.pitchLimits)
#
#     fillVector3D(fusionJointMotion.yawDirectionVector, protoJointMotion.yawDirectionVector)
#     protoJointMotion.yawValue = fusionJointMotion.yawValue
#     fillJointLimits(fusionJointMotion.yawLimits, protoJointMotion.yawLimits)
#
#
# def fillJointLimits(fusionJointLimits, protoJointLimits):
#     protoJointLimits.isMaximumValueEnabled = fusionJointLimits.isMaximumValueEnabled
#     protoJointLimits.isMinimumValueEnabled = fusionJointLimits.isMinimumValueEnabled
#     protoJointLimits.isRestValueEnabled = fusionJointLimits.isRestValueEnabled
#     protoJointLimits.maximumValue = fusionJointLimits.maximumValue
#     protoJointLimits.minimumValue = fusionJointLimits.minimumValue
#     protoJointLimits.restValue = fusionJointLimits.restValue
#
#
# # </editor-fold>
#
# # -----------Materials-----------
#
# # <editor-fold desc="Materials">
#
# def fillMaterialsAndAppearances(fusionMaterials, protoMaterials):
#     for fusionMaterial in fusionMaterials:
#         fillMaterial(fusionMaterial, protoMaterials.add())
#
#
# def fillMaterial(fusionMaterial, protoMaterial):
#     protoMaterial.id = fusionMaterial.id
#     protoMaterial.name = fusionMaterial.name
#     protoMaterial.appearanceId = fusionMaterial.appearance.id
#     # todo add protobuf def: MaterialProperties properties
#     # fillMaterialsProperties()
#
#
# def fillMaterialsProperties(fusionMaterials, protoMaterials):
#     protoMaterials.density = fusionMaterials.density
#     protoMaterials.yieldStrength = fusionMaterials.yieldStrength
#     protoMaterials.tensileStrength = fusionMaterials.tensileStrength
#
#
# # </editor-fold>
#
# # -----------Appearances-----------
#
# # <editor-fold desc="Appearances">
#
# def fillAppearances(fusionAppearances, protoAppearances):
#     for childAppearance in fusionAppearances:
#         fillAppearance(childAppearance, protoAppearances.add())
#
#
# def fillAppearance(fusionAppearance, protoAppearance):
#     protoAppearance.id = fusionAppearance.id
#     protoAppearance.name = fusionAppearance.name
#     protoAppearance.hasTexture = fusionAppearance.hasTexture
#     # todo add protobuf def: AppearanceProperties properties
#
#
# def fillAppearanceProperties(fusionAppearanceProps, protoAppearanceProps):
#     pass  # todo
#
#
# # </editor-fold>
#
# # -----------Generic-----------
#
# def fillColor(fusionColor, protoColor):
#     pass  # todo
#
#
# def fillBoundingBox3D(fusionBoundingBox, protoBoundingBox):
#     fillVector3D(fusionBoundingBox.maxPoint, protoBoundingBox.maxPoint)
#     fillVector3D(fusionBoundingBox.minPoint, protoBoundingBox.minPoint)
#
#
# def fillVector3D(fusionVector3D, protoVector3D):
#     protoVector3D.x = fusionVector3D.x
#     protoVector3D.y = fusionVector3D.y
#     protoVector3D.z = fusionVector3D.z
#
#
# def fillMatrix3D(fusionTransform, protoTransform):
#     assert len(protoTransform.cells) == 0  # Don't try to fill a matrix that's already full
#     protoTransform.cells.extend(fusionTransform.asArray())
