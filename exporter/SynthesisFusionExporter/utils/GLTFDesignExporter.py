import io
import time
import numpy as np

import adsk
import adsk.core
import adsk.fusion

from pygltflib import *
from apper import AppObjects

from ..utils import GLTFConstants, GLTFUtils

GLTF_VERSION = 2
GLB_HEADER_SIZE = 12
GLB_CHUNK_SIZE = 8

IDENTITY_MATRIX_3D = (
    1, 0, 0, 0,
    0, 1, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1,
)


def isIdentityMatrix(matrix, tolerance=0.00001):
    for i in range(len(IDENTITY_MATRIX_3D)):
        if abs(matrix[i] - IDENTITY_MATRIX_3D[i]) > tolerance:
            return False
    return True


def calculateAlignment(currentSize: int, byteAlignment: int = 4):
    if currentSize % byteAlignment == 0: return currentSize
    return currentSize + (byteAlignment - currentSize % byteAlignment)


def alignToBoundary(stream: io.BytesIO, pad: bytes, byteAlignment: int = 4):
    currentSize = len(stream.getbuffer())
    stream.seek(0, io.SEEK_END)
    for i in range(calculateAlignment(currentSize, byteAlignment) - currentSize):
        stream.write(pad)


class GLTFDesignExporter:

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

        glbLength = GLB_HEADER_SIZE + \
                    GLB_CHUNK_SIZE + len(json_blob) + \
                    GLB_CHUNK_SIZE + len(self.primaryBufferStream.getbuffer())

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
        scene = Scene()

        componentRevIDToIndexMap = self.exportMeshes(self.ao.design.allComponents)
        scene.nodes.append(self.exportRootNode(self.ao.root_comp, componentRevIDToIndexMap))

        self.gltf.scenes.append(scene)
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

        flatMatrix = occur.transform.asArray()
        if not isIdentityMatrix(flatMatrix):
            node.matrix = np.reshape(flatMatrix, (4, 4), order='F').flatten().tolist()  # transpose the flat array

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
        mesh.name = fusionComponent.name
        mesh.extras['description'] = fusionComponent.description
        mesh.extras['revisionId'] = fusionComponent.revisionId
        mesh.extras['partNumber'] = fusionComponent.partNumber
        # protoComponent.materialId = fusionComponent.material.id
        # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

        mesh.primitives = [self.exportPrimitiveBrep(bRepBody) for bRepBody in fusionComponent.bRepBodies]

        self.gltf.meshes.append(mesh)
        return len(self.gltf.meshes) - 1

    def exportPrimitiveBrep(self, fusionBRepBody):
        primitive = Primitive()
        primitive.extras['name'] = fusionBRepBody.name
        # protoMeshBody.appearanceId = fusionBRepBody.appearance.id
        # protoMeshBody.materialId = fusionBRepBody.material.id
        # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)
        meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
        meshCalculator.setQuality(11)  # todo mesh quality settings
        mesh = meshCalculator.calculate()

        primitive.attributes = Attributes()
        primitive.attributes.POSITION = self.exportVec3Accessor(mesh.nodeCoordinatesAsFloat)
        primitive.indices = self.exportIndicesAccessor(mesh.nodeIndices)

        return primitive

    def exportIndicesAccessor(self, array: List[int]):  # todo: combine accessor exporting methods
        count = int(len(array) / GLTFConstants.DataType.num_elements(GLTFConstants.DataType.Scalar))
        assert count != 0

        accessor = Accessor()

        accessor.count = count
        accessor.type = GLTFConstants.DataType.Scalar

        accessor.max = GLTFUtils.max_components(array, GLTFConstants.DataType.Scalar)
        accessor.min = GLTFUtils.min_components(array, GLTFConstants.DataType.Scalar)

        alignToBoundary(self.primaryBufferStream, b'\x00')
        byteOffset = calculateAlignment(self.primaryBufferStream.tell())

        accessor.componentType = GLTFConstants.ComponentType.UnsignedShort  # todo: smallest component type needed
        for item in array:
            self.primaryBufferStream.write(struct.pack("<H", item))

        byteLength = calculateAlignment(self.primaryBufferStream.tell() - byteOffset)

        accessor.bufferView = self.exportBufferView(byteOffset, byteLength)

        self.gltf.accessors.append(accessor)
        return len(self.gltf.accessors) - 1

    def exportVec3Accessor(self, array: List[int]):  # todo: combine accessor exporting methods
        count = int(len(array) / GLTFConstants.DataType.num_elements(GLTFConstants.DataType.Vec3))
        assert count != 0

        accessor = Accessor()

        accessor.count = count
        accessor.type = GLTFConstants.DataType.Vec3

        accessor.max = GLTFUtils.max_components(array, GLTFConstants.DataType.Vec3)
        accessor.min = GLTFUtils.min_components(array, GLTFConstants.DataType.Vec3)

        alignToBoundary(self.primaryBufferStream, b'\x00')
        byteOffset = calculateAlignment(self.primaryBufferStream.tell())

        accessor.componentType = GLTFConstants.ComponentType.Float
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
