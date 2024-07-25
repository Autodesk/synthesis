"""
Container for all options pertaining to the Fusion Exporter.
These options are saved per-design and are passed to the parser upon design export.
"""

import json
import os
import platform
from dataclasses import dataclass, field, fields
from enum import Enum, EnumType
from typing import get_origin

import adsk.core
from adsk.fusion import CalculationAccuracy, TriangleMeshQualityOptions

from ..Logging import logFailure, timed
from ..strings import INTERNAL_ID
from ..Types import (
    KG,
    ExportLocation,
    ExportMode,
    Gamepiece,
    Joint,
    ModelHierarchy,
    PhysicalDepth,
    PreferredUnits,
    Wheel,
)


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
    robotWeight: KG = field(default=0.0)

    frictionOverride: bool = field(default=False)
    frictionOverrideCoeff: float | None = field(default=None)

    compressOutput: bool = field(default=True)
    exportAsPart: bool = field(default=False)

    exportLocation: ExportLocation = field(default=ExportLocation.UPLOAD)

    hierarchy: ModelHierarchy = field(default=ModelHierarchy.FusionAssembly)
    visualQuality: TriangleMeshQualityOptions = field(default=TriangleMeshQualityOptions.LowQualityTriangleMesh)
    physicalDepth: PhysicalDepth = field(default=PhysicalDepth.AllOccurrence)
    physicalCalculationLevel: CalculationAccuracy = field(default=CalculationAccuracy.LowCalculationAccuracy)

    @timed
    def readFromDesign(self) -> "ExporterOptions":
        try:
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
        except:
            return ExporterOptions()

    @logFailure
    @timed
    def writeToDesign(self) -> None:
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        for field in fields(self):
            data = json.dumps(
                getattr(self, field.name),
                default=lambda obj: (
                    obj.value
                    if isinstance(obj, Enum)
                    else (
                        {
                            key: (lambda value: (value if not isinstance(value, Enum) else value.value))(value)
                            for key, value in obj.__dict__.items()
                        }
                        if hasattr(obj, "__dict__")
                        else obj
                    )
                ),
                indent=4,
            )
            designAttributes.add(INTERNAL_ID, field.name, data)

    # There should be a way to clean this up - Brandon
    def _makeObjectFromJson(self, objectType: type, data: any) -> any:
        primitives = (bool, str, int, float, type(None))
        if isinstance(objectType, EnumType):
            return objectType(data)
        elif (
            objectType in primitives or type(data) in primitives
        ):  # Required to catch `fusion.TriangleMeshQualityOptions`
            return data
        elif get_origin(objectType) is list:
            return [self._makeObjectFromJson(objectType.__args__[0], item) for item in data]

        newObject = objectType()
        attrs = [x for x in dir(newObject) if not x.startswith("__") and not callable(getattr(newObject, x))]
        for attr in attrs:
            currType = objectType.__annotations__.get(attr, None)
            if get_origin(currType) is list:
                setattr(
                    newObject,
                    attr,
                    [self._makeObjectFromJson(currType.__args__[0], item) for item in data[attr]],
                )
            elif currType in primitives:
                setattr(newObject, attr, data[attr])
            elif isinstance(currType, object):
                setattr(newObject, attr, self._makeObjectFromJson(currType, data[attr]))

        return newObject
