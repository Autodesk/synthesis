import io
import time
import struct
import numpy as np

from typing import Any, Dict, List
from typing import Callable, Optional, Tuple, TypeVar, Union

import adsk
import adsk.core
import adsk.fusion

from pygltflib import GLTF2, Asset, Scene, Node, Mesh, Primitive, Attributes, Accessor, BufferView, Buffer
from apper import AppObjects
from .timers import SegmentedStopwatch

from ..utils.GLTFConstants import ComponentType, DataType

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
        self.perfWatch = SegmentedStopwatch(time.perf_counter)
        self.bufferWatch = SegmentedStopwatch(time.perf_counter)

        self.gltf = GLTF2()

        self.gltf.asset = Asset()
        self.gltf.asset.generator = "Fusion 360 exporter for Synthesis"

        self.primaryBuffer = Buffer()
        self.primaryBufferId = len(self.gltf.buffers)
        self.gltf.buffers.append(self.primaryBuffer)

    def saveGLB(self, filename: str):
        with open(filename, 'wb') as fileByteStream:
            self.saveGLBToStream(fileByteStream)

        return str(self.perfWatch), str(self.bufferWatch)

    def saveGLBToStream(self, stream: io.BytesIO):
        self.primaryBufferStream = io.BytesIO()

        self.gltf.scene = self.exportScene()  # export the current document

        self.perfWatch.switch_segment('encoding and file writing')

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

        self.perfWatch.stop()

    def exportScene(self):
        scene = Scene()

        componentRevIDToIndexMap = self.exportMeshes(self.ao.design.allComponents)

        self.perfWatch.switch_segment('exporting node tree')
        scene.nodes.append(self.exportRootNode(self.ao.root_comp, componentRevIDToIndexMap))
        self.perfWatch.stop()

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
        self.perfWatch.switch_segment('calculating mesh')

        meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
        meshCalculator.setQuality(11)  # todo mesh quality settings
        mesh = meshCalculator.calculate()

        self.perfWatch.switch_segment('writing mesh to bytebuffer')

        primitive.attributes = Attributes()
        primitive.attributes.POSITION = self.exportAccessor(mesh.nodeCoordinatesAsFloat, DataType.Vec3, ComponentType.Float, True)  # limits required for positions
        primitive.indices = self.exportAccessor(mesh.nodeIndices, DataType.Scalar, None, False)

        self.perfWatch.stop()

        return primitive

    def exportAccessor(self, array: List[int], dataType: DataType, componentType: Optional[ComponentType], calculateLimits: bool):
        """
        @param array:
        @param dataType:
        @param componentType: Set to None to autodetect smallest necessary unsigned integer type (for indices)
        @param calculateLimits:
        @return:
        """
        componentCount = len(array)
        componentsPerData = DataType.num_elements(dataType)
        dataCount = int(componentCount / componentsPerData)

        assert componentCount != 0

        accessor = Accessor()

        accessor.count = dataCount
        accessor.type = dataType

        self.bufferWatch.switch_segment("calculating min/max")

        if calculateLimits or componentType is None:  # must calculate limits if auto type detection is necessary
            accessor.max = tuple(max(array[i::componentsPerData]) for i in range(componentsPerData))
            accessor.min = tuple(min(array[i::componentsPerData]) for i in range(componentsPerData))

        alignToBoundary(self.primaryBufferStream, b'\x00')
        byteOffset = calculateAlignment(self.primaryBufferStream.tell())

        self.bufferWatch.stop()

        if componentType is None:  # auto detect type
            componentMin = min(accessor.min)
            componentMax = max(accessor.max)

            ubyteMin, ubyteMax = ComponentType.get_limits(ComponentType.UnsignedByte)
            ushortMin, ushortMax = ComponentType.get_limits(ComponentType.UnsignedShort)
            uintMin, uintMax = ComponentType.get_limits(ComponentType.UnsignedInt)

            if ubyteMin <= componentMin and componentMax < ubyteMax:
                componentType = ComponentType.UnsignedByte
            elif ushortMin <= componentMin and componentMax < ushortMax:
                componentType = ComponentType.UnsignedShort
            elif uintMin <= componentMin and componentMax < uintMax:
                componentType = ComponentType.UnsignedInt
            else:
                raise ValueError(f"Failed to autodetect unsigned integer type for limits ({componentMin}, {componentMax})")

            self.bufferWatch.switch_segment(f"component type detected: {str(componentType).split('.')[1]}")
            self.bufferWatch.stop()

        self.bufferWatch.switch_segment("writing to stream")

        accessor.componentType = componentType
        packFormat = "<" + ComponentType.to_type_code(componentType)
        for item in array:
            self.primaryBufferStream.write(struct.pack(packFormat, item))

        self.bufferWatch.stop()

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
