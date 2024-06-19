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
import adsk.core, adsk.fusion

from ..strings import INTERNAL_ID


JointParentType = Enum("JointParentType", ["ROOT", "END"])
WheelType = Enum("WheelType", ["STANDARD", "OMNI"])
SignalType = Enum("SignalType", ["PWM", "CAN", "PASSIVE"])
ExportMode = Enum("ExportMode", ["ROBOT", "FIELD"])


# will need to be constructed in the UI Configure on Export
@dataclass
class _Wheel:
    joint_token: str  # maybe just pass the component
    wheelType: WheelType
    signalType: SignalType


@dataclass
class _Joint:
    joint_token: str
    parent: str | JointParentType  # str can be root
    signalType: SignalType
    speed: float
    force: float


@dataclass
class Gamepiece:
    occurrence_token: str
    weight: float
    friction: float


class PhysicalDepth(Enum):
    """Depth at which the Physical Properties are generated and saved
    - This is mostly dictated by export type as flattening or any hierarchical modification takes precedence
    """

    NoPhysical = 0
    """ No Physical Properties are generated """
    Body = 1
    """ Only Body Physical Objects are generated """
    SurfaceOccurrence = 2
    """ Only Occurrence that contain Bodies and Bodies have Physical Properties """
    AllOccurrence = 3
    """ Every Single Occurrence has Physical Properties even if empty """


class ModelHierarchy(Enum):
    """
    Enum Class to describe how the model format should look on export to suit different needs
    """

    FusionAssembly = 0
    """ Model exactly as it is shown in Fusion 360 in the model view tree """

    FlatAssembly = 1
    """ Flattened Assembly with all bodies as children of the root object """

    PhysicalAssembly = 2
    """ A Model represented with parented objects that are part of a jointed tree """

    SingleMesh = 3
    """ Generates the root assembly as a single mesh and stores the associated data """


class ParseOptions:
    """Options to supply to the parser object that will generate the output file"""

    def __init__(
        self,
        fileLocation: str,
        name: str,
        version: str,
        hierarchy=ModelHierarchy.FusionAssembly,
        visual=adsk.fusion.TriangleMeshQualityOptions.LowQualityTriangleMesh,
        physical=adsk.fusion.CalculationAccuracy.LowCalculationAccuracy,
        physicalDepth=PhysicalDepth.AllOccurrence,
        materials=1,
        mode=ExportMode.ROBOT,
        wheels=list[_Wheel],
        joints=list[_Joint],  # [{Occurrence, wheeltype} , {entitytoken, wheeltype}]
        gamepieces=list[Gamepiece],
        weight=float,
        compress=bool,
    ):
        """Generates the Parser Options for the given export

        Args:
            - fileLocation (str): Location of file with file name (given during file explore action)
            - name (str): name of the assembly
            - version (str): root assembly version
            - hierarchy (ModelHierarchy.FusionAssembly, optional): The exported model hierarchy. Defaults to ModelHierarchy.FusionAssembly
            - visual (adsk.fusion.TriangleMeshQualityOptions, optional): Triangle Mesh Export Quality. Defaults to adsk.fusion.TriangleMeshQualityOptions.HighQualityTriangleMesh.
            - physical (adsk.fusion.CalculationAccuracy, optional): Calculation Level of the physical properties. Defaults to adsk.fusion.CalculationAccuracy.MediumCalculationAccuracy.
            - physicalDepth (PhysicalDepth, optional): Enum to define the level of physical attributes exported. Defaults to PhysicalDepth.AllOccurrence.
            - materials (int, optional): Export Materials type: defaults to STANDARD 1
            - joints (bool, optional): Export Joints. Defaults to True.
            - wheels (list (strings)): List of Occurrence.entityTokens that
        """
        self.fileLocation = fileLocation
        self.name = name
        self.version = version
        self.hierarchy = hierarchy
        self.visual = visual
        self.physical = physical
        self.physicalDepth = physicalDepth
        self.materials = materials
        self.mode = mode
        self.wheels = wheels
        self.joints = joints
        self.gamepieces = gamepieces
        self.weight = weight  # full weight of robot in KG
        self.compress = compress


# TODO: This should be the only parse option class
@dataclass
class ExporterOptions:
    # TODO: Clean up all these `field(default=None)` things. This could be better - Brandon
    fileLocation: str = field(default=None)
    name: str = field(default=None)
    version: str = field(default=None)
    materials: int = field(default=0)  # TODO: Find out what this is for
    mode: ExportMode = field(
        default=ExportMode.ROBOT
    )  # TODO: Maybe rename 'mode' to 'exportMode'
    wheels: list[_Wheel] = field(default=None)
    joints: list[_Joint] = field(default=None)
    gamepieces: list[Gamepiece] = field(default=None)
    weight: float = field(default=0.0)
    compress: bool = field(default=True)
    exportAsPart: bool = field(default=False)

    hierarchy: ModelHierarchy = field(default=ModelHierarchy.FusionAssembly)
    visual: adsk.fusion.TriangleMeshQualityOptions = field(
        default=adsk.fusion.TriangleMeshQualityOptions.LowQualityTriangleMesh
    )
    physicalDepth: PhysicalDepth = field(default=PhysicalDepth.AllOccurrence)

    def read(self):
        designAttributes = adsk.core.Application.get().activeProduct.attributes
        # TODO: This does not work for Lists of custom classes
        for field in fields(self):
            attribute = designAttributes.itemByName(INTERNAL_ID, field.name)
            if attribute:
                setattr(
                    self,
                    field.name,
                    self.readHelper(field.type, json.loads(attribute.value)),
                )

        return self

    def readHelper(self, objectType, data):
        primitives = (bool, str, int, float, type(None))
        if (
            objectType in primitives or type(data) in primitives
        ):  # Required to catch `fusion.TriangleMeshQualityOptions`
            return data
        elif isinstance(objectType, EnumType):
            return objectType(data)

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
                        self.readHelper(currType.__args__[0], item)
                        for item in data[attr]
                    ],
                )
            elif currType in primitives:
                setattr(newObject, attr, data[attr])
            elif isinstance(currType, object):
                setattr(newObject, attr, self.readHelper(currType, data[attr]))

        return newObject

    def write(self):
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
