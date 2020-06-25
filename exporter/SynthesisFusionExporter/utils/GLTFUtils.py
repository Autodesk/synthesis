import base64
from struct import calcsize
import pathlib
import warnings

from ..utils import gltf2_io_constants, gltf2_blender_utils

from pygltflib import *


class GltfBufferAccumulator():
    def __init__(self, gltf):
        self.buffer = Buffer()
        gltf.buffers.append(self.buffer)
        self.gltfIndex = len(gltf.buffers) - 1
        self.offset = 0

        # add the data
        stream = "data:application/octet-stream;base64,"
        self.buffer.uri = stream  # first part of the datastream is set up

    def addBytes(self, bytes, length):
        self.buffer.uri += bytes
        self.offset += length

    def getLength(self):
        return self.offset

    def getGltfIndex(self):
        return self.gltfIndex

    def close(self):
        self.buffer.byteLength = self.offset
        return self.buffer


def addPrimitiveData(gltf, bufferAccum, indices, vertices):
    indicesBufferView = BufferView()  # indices buffer view
    verticesBufferView = BufferView()  # vertices buffer view
    indicesAccessor = Accessor()
    verticesAccessor = Accessor()

    # add to gltf
    gltf.bufferViews.append(indicesBufferView)
    gltf.bufferViews.append(verticesBufferView)
    gltf.accessors.append(indicesAccessor)
    gltf.accessors.append(verticesAccessor)

    indicesBufferViewIndex = len(gltf.bufferViews) - 2
    verticesBufferViewIndex = len(gltf.bufferViews) - 1
    bufferIndex = bufferAccum.getGltfIndex()

    # accessor for indices
    indicesAccessor.bufferView = indicesBufferViewIndex
    indicesAccessor.byteOffset = 0
    indicesAccessor.componentType = UNSIGNED_SHORT
    indicesAccessor.count = int(len(indices))
    indicesAccessor.type = SCALAR

    # accessor for vertices
    verticesAccessor.bufferView = verticesBufferViewIndex
    verticesAccessor.byteOffset = 0
    verticesAccessor.componentType = FLOAT
    verticesAccessor.count = int(len(vertices)/3)
    verticesAccessor.type = VEC3

    verticesAccessor.max = gltf2_blender_utils.max_components(vertices, gltf2_io_constants.DataType.Vec3)
    verticesAccessor.min = gltf2_blender_utils.min_components(vertices, gltf2_io_constants.DataType.Vec3)

    chunk = b""
    pack = "<H"
    for v in indices:
        chunk += struct.pack(pack, v)

    indicesBufferView.buffer = bufferIndex
    indicesBufferView.byteOffset = bufferAccum.getLength()
    indicesBufferView.byteLength = len(chunk)
    indicesBufferView.target = ELEMENT_ARRAY_BUFFER
    bufferAccum.addBytes(base64.b64encode(chunk).decode("utf-8"), len(chunk))

    chunk = b""
    pack = "<f"
    for v in vertices:
        chunk += struct.pack(pack, v)

    # record_size = byte_length * num_of_fields
    verticesBufferView.buffer = bufferIndex
    verticesBufferView.byteOffset = bufferAccum.getLength()
    verticesBufferView.byteLength = len(chunk)
    verticesBufferView.target = ARRAY_BUFFER
    bufferAccum.addBytes(base64.b64encode(chunk).decode("utf-8"), len(chunk))

    return indicesBufferViewIndex, verticesBufferViewIndex