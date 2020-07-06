# Copyright 2018 The glTF-Blender-IO authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from enum import Enum, IntEnum


class ComponentType(IntEnum):
    Byte = 5120
    UnsignedByte = 5121
    Short = 5122
    UnsignedShort = 5123
    UnsignedInt = 5125
    Float = 5126

    @classmethod
    def to_type_code(cls, component_type):
        return {
            ComponentType.Byte: 'b',
            ComponentType.UnsignedByte: 'B',
            ComponentType.Short: 'h',
            ComponentType.UnsignedShort: 'H',
            ComponentType.UnsignedInt: 'I',
            ComponentType.Float: 'f'
        }[component_type]

    @classmethod
    def get_size(cls, component_type):
        return {
            ComponentType.Byte: 1,
            ComponentType.UnsignedByte: 1,
            ComponentType.Short: 2,
            ComponentType.UnsignedShort: 2,
            ComponentType.UnsignedInt: 4,
            ComponentType.Float: 4
        }[component_type]

    @classmethod
    def get_limits(cls, component_type):
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
    def num_elements(cls, data_type):
        return {
            DataType.Scalar: 1,
            DataType.Vec2: 2,
            DataType.Vec3: 3,
            DataType.Vec4: 4,
            DataType.Mat2: 4,
            DataType.Mat3: 9,
            DataType.Mat4: 16
        }[data_type]

    @classmethod
    def vec_type_from_num(cls, num_elems):
        if not (0 < num_elems < 5):
            raise ValueError("No vector type with {} elements".format(num_elems))
        return {
            1: DataType.Scalar,
            2: DataType.Vec2,
            3: DataType.Vec3,
            4: DataType.Vec4
        }[num_elems]

    @classmethod
    def mat_type_from_num(cls, num_elems):
        if not (4 <= num_elems <= 16):
            raise ValueError("No matrix type with {} elements".format(num_elems))
        return {
            4: DataType.Mat2,
            9: DataType.Mat3,
            16: DataType.Mat4
        }[num_elems]
