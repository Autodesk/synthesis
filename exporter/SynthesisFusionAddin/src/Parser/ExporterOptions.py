"""
Container for all options pertaining to the Fusion Exporter.
These options are saved per-design and are passed to the parser upon design export.
"""

import json
import os
import platform
from dataclasses import dataclass, field, fields

import adsk.core
from adsk.fusion import CalculationAccuracy, TriangleMeshQualityOptions

from src import INTERNAL_ID
from src.Logging import logFailure, timed
from src.Types import (
    KG,
    ExportLocation,
    ExportMode,
    Gamepiece,
    Joint,
    ModelHierarchy,
    PhysicalDepth,
    PreferredUnits,
    Wheel,
    encodeNestedObjects,
    makeObjectFromJson,
)


@dataclass
class ExporterOptions:
    # Python's `os` module can return `None` when attempting to find the home directory if the
    # user's computer has conflicting configs of some sort. This has happened and should be accounted
    # for accordingly.
    fileLocation: str | None = field(
        default=(os.getenv("HOME") if platform.system() == "Windows" else os.path.expanduser("~"))
    )
    name: str | None = field(default=None)
    version: str | None = field(default=None)
    materials: int = field(default=0)
    exportMode: ExportMode = field(default=ExportMode.ROBOT)
    wheels: list[Wheel] = field(default_factory=list)
    joints: list[Joint] = field(default_factory=list)
    gamepieces: list[Gamepiece] = field(default_factory=list)
    preferredUnits: PreferredUnits = field(default=PreferredUnits.IMPERIAL)

    # Always stored in kg regardless of 'preferredUnits'
    robotWeight: KG = field(default=0.0)
    autoCalcRobotWeight: bool = field(default=False)
    autoCalcGamepieceWeight: bool = field(default=False)

    frictionOverride: bool = field(default=False)
    frictionOverrideCoeff: float = field(default=0.5)

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
