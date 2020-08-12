from enum import Enum, IntEnum

# Generic constants for working with the glTF standard

# GLB constants
GLTF_VERSION = 2
GLB_HEADER_SIZE = 12
GLB_CHUNK_SIZE = 8

class FileType(IntEnum):
    GLB = 0
    GLTF = 1

    @classmethod
    def getExtension(cls, component_type):
        return {
            FileType.GLB: 'glb',
            FileType.GLTF: 'gltf',
        }[component_type]

    @classmethod
    def fromString(cls, string):
        return {
            'glb': FileType.GLB,
            'gltf': FileType.GLTF,
        }[string]

class ComponentType(IntEnum):
    Byte = 5120
    UnsignedByte = 5121
    Short = 5122
    UnsignedShort = 5123
    UnsignedInt = 5125
    Float = 5126

    @classmethod
    def getTypeCode(cls, component_type):
        return {
            ComponentType.Byte: 'b',
            ComponentType.UnsignedByte: 'B',
            ComponentType.Short: 'h',
            ComponentType.UnsignedShort: 'H',
            ComponentType.UnsignedInt: 'I',
            ComponentType.Float: 'f'
        }[component_type]

    @classmethod
    def getByteSize(cls, component_type):
        return {
            ComponentType.Byte: 1,
            ComponentType.UnsignedByte: 1,
            ComponentType.Short: 2,
            ComponentType.UnsignedShort: 2,
            ComponentType.UnsignedInt: 4,
            ComponentType.Float: 4
        }[component_type]

    @classmethod
    def getValueLimits(cls, component_type):
        return {
            ComponentType.Byte: (-128, 127),
            ComponentType.UnsignedByte: (0, 255),
            ComponentType.Short: (-32768, 32767),
            ComponentType.UnsignedShort: (0, 65535),
            ComponentType.UnsignedInt: (0, 4294967295),
            ComponentType.Float: None
        }[component_type]


class DataType:
    Scalar = "SCALAR"
    Vec2 = "VEC2"
    Vec3 = "VEC3"
    Vec4 = "VEC4"
    Mat2 = "MAT2"
    Mat3 = "MAT3"
    Mat4 = "MAT4"

    @classmethod
    def numElements(cls, data_type):
        return {
            DataType.Scalar: 1,
            DataType.Vec2: 2,
            DataType.Vec3: 3,
            DataType.Vec4: 4,
            DataType.Mat2: 4,
            DataType.Mat3: 9,
            DataType.Mat4: 16
        }[data_type]
