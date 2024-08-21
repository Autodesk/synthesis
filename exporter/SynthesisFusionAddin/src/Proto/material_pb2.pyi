"""
@generated by mypy-protobuf.  Do not edit manually!
isort:skip_file
"""

import builtins
import collections.abc
import google.protobuf.descriptor
import google.protobuf.internal.containers
import google.protobuf.internal.enum_type_wrapper
import google.protobuf.message
import sys
import types_pb2
import typing

if sys.version_info >= (3, 10):
    import typing as typing_extensions
else:
    import typing_extensions

DESCRIPTOR: google.protobuf.descriptor.FileDescriptor

@typing.final
class Materials(google.protobuf.message.Message):
    """*
    Represents a File or Set of Materials with Appearances and Physical Data

    Can be Stored in AssemblyData
    """

    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    @typing.final
    class PhysicalMaterialsEntry(google.protobuf.message.Message):
        DESCRIPTOR: google.protobuf.descriptor.Descriptor

        KEY_FIELD_NUMBER: builtins.int
        VALUE_FIELD_NUMBER: builtins.int
        key: builtins.str
        @property
        def value(self) -> global___PhysicalMaterial: ...
        def __init__(
            self,
            *,
            key: builtins.str = ...,
            value: global___PhysicalMaterial | None = ...,
        ) -> None: ...
        def HasField(self, field_name: typing.Literal["value", b"value"]) -> builtins.bool: ...
        def ClearField(self, field_name: typing.Literal["key", b"key", "value", b"value"]) -> None: ...

    @typing.final
    class AppearancesEntry(google.protobuf.message.Message):
        DESCRIPTOR: google.protobuf.descriptor.Descriptor

        KEY_FIELD_NUMBER: builtins.int
        VALUE_FIELD_NUMBER: builtins.int
        key: builtins.str
        @property
        def value(self) -> global___Appearance: ...
        def __init__(
            self,
            *,
            key: builtins.str = ...,
            value: global___Appearance | None = ...,
        ) -> None: ...
        def HasField(self, field_name: typing.Literal["value", b"value"]) -> builtins.bool: ...
        def ClearField(self, field_name: typing.Literal["key", b"key", "value", b"value"]) -> None: ...

    INFO_FIELD_NUMBER: builtins.int
    PHYSICALMATERIALS_FIELD_NUMBER: builtins.int
    APPEARANCES_FIELD_NUMBER: builtins.int
    @property
    def info(self) -> types_pb2.Info:
        """/ Identifiable information (id, name, version)"""

    @property
    def physicalMaterials(self) -> google.protobuf.internal.containers.MessageMap[builtins.str, global___PhysicalMaterial]:
        """/ Map of Physical Materials"""

    @property
    def appearances(self) -> google.protobuf.internal.containers.MessageMap[builtins.str, global___Appearance]:
        """/ Map of Appearances that are purely visual"""

    def __init__(
        self,
        *,
        info: types_pb2.Info | None = ...,
        physicalMaterials: collections.abc.Mapping[builtins.str, global___PhysicalMaterial] | None = ...,
        appearances: collections.abc.Mapping[builtins.str, global___Appearance] | None = ...,
    ) -> None: ...
    def HasField(self, field_name: typing.Literal["info", b"info"]) -> builtins.bool: ...
    def ClearField(self, field_name: typing.Literal["appearances", b"appearances", "info", b"info", "physicalMaterials", b"physicalMaterials"]) -> None: ...

global___Materials = Materials

@typing.final
class Appearance(google.protobuf.message.Message):
    """*
    Contains information on how a object looks
    Limited to just color for now
    """

    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    INFO_FIELD_NUMBER: builtins.int
    ALBEDO_FIELD_NUMBER: builtins.int
    ROUGHNESS_FIELD_NUMBER: builtins.int
    METALLIC_FIELD_NUMBER: builtins.int
    SPECULAR_FIELD_NUMBER: builtins.int
    roughness: builtins.float
    """/ roughness value 0-1"""
    metallic: builtins.float
    """/ metallic value 0-1"""
    specular: builtins.float
    """/ specular value 0-1"""
    @property
    def info(self) -> types_pb2.Info:
        """/ Identfiable information (id, name, version)"""

    @property
    def albedo(self) -> types_pb2.Color:
        """/ albedo map RGBA 0-255"""

    def __init__(
        self,
        *,
        info: types_pb2.Info | None = ...,
        albedo: types_pb2.Color | None = ...,
        roughness: builtins.float = ...,
        metallic: builtins.float = ...,
        specular: builtins.float = ...,
    ) -> None: ...
    def HasField(self, field_name: typing.Literal["albedo", b"albedo", "info", b"info"]) -> builtins.bool: ...
    def ClearField(self, field_name: typing.Literal["albedo", b"albedo", "info", b"info", "metallic", b"metallic", "roughness", b"roughness", "specular", b"specular"]) -> None: ...

global___Appearance = Appearance

@typing.final
class PhysicalMaterial(google.protobuf.message.Message):
    """*
    Data to represent any given Physical Material
    """

    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    class _MaterialType:
        ValueType = typing.NewType("ValueType", builtins.int)
        V: typing_extensions.TypeAlias = ValueType

    class _MaterialTypeEnumTypeWrapper(google.protobuf.internal.enum_type_wrapper._EnumTypeWrapper[PhysicalMaterial._MaterialType.ValueType], builtins.type):
        DESCRIPTOR: google.protobuf.descriptor.EnumDescriptor
        METAL: PhysicalMaterial._MaterialType.ValueType  # 0
        PLASTIC: PhysicalMaterial._MaterialType.ValueType  # 1

    class MaterialType(_MaterialType, metaclass=_MaterialTypeEnumTypeWrapper): ...
    METAL: PhysicalMaterial.MaterialType.ValueType  # 0
    PLASTIC: PhysicalMaterial.MaterialType.ValueType  # 1

    @typing.final
    class Thermal(google.protobuf.message.Message):
        """*
        Thermal Properties Set Definition for Simulation.
        """

        DESCRIPTOR: google.protobuf.descriptor.Descriptor

        THERMAL_CONDUCTIVITY_FIELD_NUMBER: builtins.int
        SPECIFIC_HEAT_FIELD_NUMBER: builtins.int
        THERMAL_EXPANSION_COEFFICIENT_FIELD_NUMBER: builtins.int
        thermal_conductivity: builtins.float
        """/ W/(m*K)"""
        specific_heat: builtins.float
        """/ J/(g*C)"""
        thermal_expansion_coefficient: builtins.float
        """/ um/(m*C)"""
        def __init__(
            self,
            *,
            thermal_conductivity: builtins.float = ...,
            specific_heat: builtins.float = ...,
            thermal_expansion_coefficient: builtins.float = ...,
        ) -> None: ...
        def ClearField(self, field_name: typing.Literal["specific_heat", b"specific_heat", "thermal_conductivity", b"thermal_conductivity", "thermal_expansion_coefficient", b"thermal_expansion_coefficient"]) -> None: ...

    @typing.final
    class Mechanical(google.protobuf.message.Message):
        """*
        Mechanical Properties Set Definition for Simulation.
        """

        DESCRIPTOR: google.protobuf.descriptor.Descriptor

        YOUNG_MOD_FIELD_NUMBER: builtins.int
        POISSON_RATIO_FIELD_NUMBER: builtins.int
        SHEAR_MOD_FIELD_NUMBER: builtins.int
        DENSITY_FIELD_NUMBER: builtins.int
        DAMPING_COEFFICIENT_FIELD_NUMBER: builtins.int
        young_mod: builtins.float
        """naming scheme changes here
        / GPa
        """
        poisson_ratio: builtins.float
        """/ ?"""
        shear_mod: builtins.float
        """/ MPa"""
        density: builtins.float
        """/ g/cm^3"""
        damping_coefficient: builtins.float
        """/ ?"""
        def __init__(
            self,
            *,
            young_mod: builtins.float = ...,
            poisson_ratio: builtins.float = ...,
            shear_mod: builtins.float = ...,
            density: builtins.float = ...,
            damping_coefficient: builtins.float = ...,
        ) -> None: ...
        def ClearField(self, field_name: typing.Literal["damping_coefficient", b"damping_coefficient", "density", b"density", "poisson_ratio", b"poisson_ratio", "shear_mod", b"shear_mod", "young_mod", b"young_mod"]) -> None: ...

    @typing.final
    class Strength(google.protobuf.message.Message):
        """*
        Strength Properties Set Definition for Simulation.
        """

        DESCRIPTOR: google.protobuf.descriptor.Descriptor

        YIELD_STRENGTH_FIELD_NUMBER: builtins.int
        TENSILE_STRENGTH_FIELD_NUMBER: builtins.int
        THERMAL_TREATMENT_FIELD_NUMBER: builtins.int
        yield_strength: builtins.float
        """/ MPa"""
        tensile_strength: builtins.float
        """/ MPa"""
        thermal_treatment: builtins.bool
        """/ yes / no"""
        def __init__(
            self,
            *,
            yield_strength: builtins.float = ...,
            tensile_strength: builtins.float = ...,
            thermal_treatment: builtins.bool = ...,
        ) -> None: ...
        def ClearField(self, field_name: typing.Literal["tensile_strength", b"tensile_strength", "thermal_treatment", b"thermal_treatment", "yield_strength", b"yield_strength"]) -> None: ...

    INFO_FIELD_NUMBER: builtins.int
    DESCRIPTION_FIELD_NUMBER: builtins.int
    THERMAL_FIELD_NUMBER: builtins.int
    MECHANICAL_FIELD_NUMBER: builtins.int
    STRENGTH_FIELD_NUMBER: builtins.int
    DYNAMIC_FRICTION_FIELD_NUMBER: builtins.int
    STATIC_FRICTION_FIELD_NUMBER: builtins.int
    RESTITUTION_FIELD_NUMBER: builtins.int
    DEFORMABLE_FIELD_NUMBER: builtins.int
    MATTYPE_FIELD_NUMBER: builtins.int
    description: builtins.str
    """/ short description of physical material"""
    dynamic_friction: builtins.float
    """/ Frictional force for dampening - Interpolate (0-1)"""
    static_friction: builtins.float
    """/ Frictional force override at stop - Interpolate (0-1)"""
    restitution: builtins.float
    """/ Restitution of the object - Interpolate (0-1)"""
    deformable: builtins.bool
    """/ should this object deform when encountering large forces - TODO: This needs a proper message and equation field"""
    matType: global___PhysicalMaterial.MaterialType.ValueType
    """/ generic type to assign some default params"""
    @property
    def info(self) -> types_pb2.Info:
        """/ Identifiable information (id, name, version, etc)"""

    @property
    def thermal(self) -> global___PhysicalMaterial.Thermal:
        """/ Thermal Physical properties of the model OPTIONAL"""

    @property
    def mechanical(self) -> global___PhysicalMaterial.Mechanical:
        """/ Mechanical properties of the model OPTIONAL"""

    @property
    def strength(self) -> global___PhysicalMaterial.Strength:
        """/ Physical Strength properties of the model OPTIONAL"""

    def __init__(
        self,
        *,
        info: types_pb2.Info | None = ...,
        description: builtins.str = ...,
        thermal: global___PhysicalMaterial.Thermal | None = ...,
        mechanical: global___PhysicalMaterial.Mechanical | None = ...,
        strength: global___PhysicalMaterial.Strength | None = ...,
        dynamic_friction: builtins.float = ...,
        static_friction: builtins.float = ...,
        restitution: builtins.float = ...,
        deformable: builtins.bool = ...,
        matType: global___PhysicalMaterial.MaterialType.ValueType = ...,
    ) -> None: ...
    def HasField(self, field_name: typing.Literal["info", b"info", "mechanical", b"mechanical", "strength", b"strength", "thermal", b"thermal"]) -> builtins.bool: ...
    def ClearField(self, field_name: typing.Literal["deformable", b"deformable", "description", b"description", "dynamic_friction", b"dynamic_friction", "info", b"info", "matType", b"matType", "mechanical", b"mechanical", "restitution", b"restitution", "static_friction", b"static_friction", "strength", b"strength", "thermal", b"thermal"]) -> None: ...

global___PhysicalMaterial = PhysicalMaterial
