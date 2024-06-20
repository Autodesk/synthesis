""" ParserOptions

    - This module targets the creation of the parser used for actually parsing the data
    - Since the parsing can be recursive you can pass a low overhead options construction into each function to detail the parsing
    - Should have factory methods to convert from a given configuration possibly
        - or maybe a configuration should replace this im not certain
        - this is essentially a flat configuration file with non serializable objects
"""

import json
from typing import get_origin
from enum import Enum, EnumType
from dataclasses import dataclass, fields, field
import adsk.core

from adsk.fusion import CalculationAccuracy, TriangleMeshQualityOptions

from ..strings import INTERNAL_ID


JointParentType = Enum("JointParentType", ["ROOT", "END"]) # Not 100% sure what this is for - Brandon
WheelType = Enum("WheelType", ["STANDARD", "OMNI"])
SignalType = Enum("SignalType", ["PWM", "CAN", "PASSIVE"])
ExportMode = Enum("ExportMode", ["ROBOT", "FIELD"]) # Dynamic / Static export


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


@dataclass
class Gamepiece:
    occurrenceToken: str = field(default=None)
    weight: float = field(default=None)
    friction: float = field(default=None)


class PhysicalDepth(Enum):
    """
    Depth at which the Physical Properties are generated and saved
    This is mostly dictated by export type as flattening or any hierarchical modification takes precedence
    """

    """ No Physical Properties are generated """
    NoPhysical = 0

    """ Only Body Physical Objects are generated """
    Body = 1

    """ Only Occurrence that contain Bodies and Bodies have Physical Properties """
    SurfaceOccurrence = 2

    """ Every Single Occurrence has Physical Properties even if empty """
    AllOccurrence = 3


class ModelHierarchy(Enum):
    """
    Enum Class to describe how the model format should look on export to suit different needs
    """

    """ Model exactly as it is shown in Fusion 360 in the model view tree """
    FusionAssembly = 0

    """ Flattened Assembly with all bodies as children of the root object """
    FlatAssembly = 1

    """ A Model represented with parented objects that are part of a jointed tree """
    PhysicalAssembly = 2

    """ Generates the root assembly as a single mesh and stores the associated data """
    SingleMesh = 3


@dataclass
class ExporterOptions:
    # TODO: Clean up all these `field(default=None)` things. This could potentially be better - Brandon
    fileLocation: str = field(default=None)
    name: str = field(default=None)
    version: str = field(default=None)
    materials: int = field(default=0)  # TODO: Find out what this is for
    exportMode: ExportMode = field(default=ExportMode.ROBOT)
    wheels: list[Wheel] = field(default=None)
    joints: list[Joint] = field(default=None)
    gamepieces: list[Gamepiece] = field(default=None)
    robotWeight: float = field(default=0.0) # kg
    compressOutput: bool = field(default=True)
    exportAsPart: bool = field(default=False)

    hierarchy: ModelHierarchy = field(default=ModelHierarchy.FusionAssembly)
    visualQuality: TriangleMeshQualityOptions = field(default=TriangleMeshQualityOptions.LowQualityTriangleMesh)
    physicalDepth: PhysicalDepth = field(default=PhysicalDepth.AllOccurrence)
    physicalCalculationLevel: CalculationAccuracy = field(default=CalculationAccuracy.LowCalculationAccuracy)

    def read(self) -> None:
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        for field in fields(self):
            attribute = designAttributes.itemByName(INTERNAL_ID, field.name)
            if attribute:
                setattr(
                    self,
                    field.name,
                    self._makeObjectFromJson(field.type, json.loads(attribute.value)),
                )

        return self

    def write(self) -> None:
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        for field in fields(self):
            data = json.dumps(
                getattr(self, field.name),
                default=lambda obj: (
                    obj.value
                    if isinstance(obj, Enum)
                    else {
                        key: (
                            lambda value: value
                            if not isinstance(value, Enum)
                            else value.value
                        )(value)
                        for key, value in obj.__dict__.items()
                    }
                    if hasattr(obj, "__dict__")
                    else obj
                ),
                indent=4,
            )
            designAttributes.add(INTERNAL_ID, field.name, data)

    # TODO: There should be a way to clean this up - Brandon
    def _makeObjectFromJson(self, objectType: type, data: any) -> any:
        primitives = (bool, str, int, float, type(None))
        if (
            objectType in primitives or type(data) in primitives
        ):  # Required to catch `fusion.TriangleMeshQualityOptions`
            return data
        elif isinstance(objectType, EnumType):
            return objectType(data)
        elif get_origin(objectType) is list:
            return [
                self._makeObjectFromJson(objectType.__args__[0], item)
                for item in data
            ]

        newObject = objectType()
        attrs = [
            x
            for x in dir(newObject)
            if not x.startswith("__") and not callable(getattr(newObject, x))
        ]
        for attr in attrs:
            currType = objectType.__annotations__.get(attr, None)
            if get_origin(currType) is list:
                setattr(
                    newObject,
                    attr,
                    [
                        self._makeObjectFromJson(currType.__args__[0], item)
                        for item in data[attr]
                    ],
                )
            elif currType in primitives:
                setattr(newObject, attr, data[attr])
            elif isinstance(currType, object):
                setattr(newObject, attr, self._makeObjectFromJson(currType, data[attr]))

        return newObject
