"""
Container for all options pertaining to the Fusion Exporter.
These options are saved per-design and are passed to the parser upon design export.
"""

import json
import os
import platform
from dataclasses import dataclass, field, fields, is_dataclass
from enum import Enum, EnumType
from typing import get_origin

import adsk.core
from adsk.fusion import CalculationAccuracy, TriangleMeshQualityOptions

from ..Logging import logFailure, timed
from ..strings import INTERNAL_ID

# Not 100% sure what this is for - Brandon
JointParentType = Enum("JointParentType", ["ROOT", "END"])

WheelType = Enum("WheelType", ["STANDARD", "OMNI", "MECANUM"])
SignalType = Enum("SignalType", ["PWM", "CAN", "PASSIVE"])
ExportMode = Enum("ExportMode", ["ROBOT", "FIELD"])  # Dynamic / Static export
PreferredUnits = Enum("PreferredUnits", ["METRIC", "IMPERIAL"])
ExportLocation = Enum("ExportLocation", ["UPLOAD", "DOWNLOAD"])


@dataclass
class Wheel:
    jointToken: str = field(default=None)
    wheelType: WheelType = field(default=None)
    signalType: SignalType = field(default=None)


@dataclass
class Joint:
    jointToken: str = field(default=None)
    parent: JointParentType = field(default=None)
    signalType: SignalType = field(default=None)
    speed: float = field(default=None)
    force: float = field(default=None)

    # Transition: AARD-1865
    # Should consider changing how the parser handles wheels and joints as there is overlap between
    # `Joint` and `Wheel` that should be avoided
    # This overlap also presents itself in 'ConfigCommand.py' and 'JointConfigTab.py'
    isWheel: bool = field(default=False)


@dataclass
class Gamepiece:
    occurrenceToken: str = field(default=None)
    weight: float = field(default=None)
    friction: float = field(default=None)


class PhysicalDepth(Enum):
    # No Physical Properties are generated
    NoPhysical = 0

    # Only Body Physical Objects are generated
    Body = 1

    # Only Occurrence that contain Bodies and Bodies have Physical Properties
    SurfaceOccurrence = 2

    # Every Single Occurrence has Physical Properties even if empty
    AllOccurrence = 3


class ModelHierarchy(Enum):
    # Model exactly as it is shown in Fusion in the model view tree
    FusionAssembly = 0

    # Flattened Assembly with all bodies as children of the root object
    FlatAssembly = 1

    # A Model represented with parented objects that are part of a jointed tree
    PhysicalAssembly = 2

    # Generates the root assembly as a single mesh and stores the associated data
    SingleMesh = 3


@dataclass
class ExporterOptions:
    # Python's `os` module can return `None` when attempting to find the home directory if the
    # user's computer has conflicting configs of some sort. This has happened and should be accounted
    # for accordingly.
    fileLocation: str | None = field(
        default=(os.getenv("HOME") if platform.system() == "Windows" else os.path.expanduser("~"))
    )
    name: str = field(default=None)
    version: str = field(default=None)
    materials: int = field(default=0)
    exportMode: ExportMode = field(default=ExportMode.ROBOT)
    wheels: list[Wheel] = field(default=None)
    joints: list[Joint] = field(default=None)
    gamepieces: list[Gamepiece] = field(default=None)
    preferredUnits: PreferredUnits = field(default=PreferredUnits.IMPERIAL)

    # Always stored in kg regardless of 'preferredUnits'
    robotWeight: float = field(default=0.0)

    compressOutput: bool = field(default=True)
    exportAsPart: bool = field(default=False)

    exportLocation: ExportLocation = field(default=ExportLocation.UPLOAD)

    hierarchy: ModelHierarchy = field(default=ModelHierarchy.FusionAssembly)
    visualQuality: TriangleMeshQualityOptions = field(default=TriangleMeshQualityOptions.LowQualityTriangleMesh)
    physicalDepth: PhysicalDepth = field(default=PhysicalDepth.AllOccurrence)
    physicalCalculationLevel: CalculationAccuracy = field(default=CalculationAccuracy.LowCalculationAccuracy)

    @logFailure
    @timed
    def readFromDesign(self) -> "ExporterOptions":
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        for field in fields(self):
            attribute = designAttributes.itemByName(INTERNAL_ID, field.name)
            if attribute:
                attrJsonData = makeObjectFromJson(field.type, json.loads(attribute.value))
                setattr(self, field.name, attrJsonData)

        return self

    @logFailure
    @timed
    def writeToDesign(self) -> None:
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        for field in fields(self):
            data = json.dumps(getattr(self, field.name), default=encodeNestedObjects, indent=4)
            designAttributes.add(INTERNAL_ID, field.name, data)


# Transition: AARD-####
# Should be added into the refactored type module.
PRIMITIVES = (bool, str, int, float, type(None))


def encodeNestedObjects(obj: any) -> any:
    if isinstance(obj, Enum):
        return obj.value
    elif hasattr(obj, "__dict__"):
        return {key: encodeNestedObjects(value) for key, value in obj.__dict__.items()}
    else:
        assert isinstance(obj, PRIMITIVES)
        return obj


def makeObjectFromJson(objType: type, data: any) -> any:
    if isinstance(objType, EnumType):
        return objType(data)
    elif isinstance(objType, PRIMITIVES) or isinstance(data, PRIMITIVES):
        return data
    elif get_origin(objType) is list:
        return [makeObjectFromJson(objType.__args__[0], item) for item in data]

    obj = objType()
    assert is_dataclass(obj) and isinstance(data, dict), "Found unsupported type to decode."
    for field in fields(obj):
        if field.name in data:
            setattr(obj, field.name, makeObjectFromJson(field.type, data[field.name]))
        else:
            setattr(obj, field.name, field.default)

    return obj
