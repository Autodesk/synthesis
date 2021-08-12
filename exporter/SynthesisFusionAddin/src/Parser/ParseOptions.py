""" ParserOptions

    - This module targets the creation of the parser used for actually parsing the data
    - Since the parsing can be recursive you can pass a low overhead options construction into each function to detail the parsing
    - Should have factory methods to convert from a given configuration possibly
        - or maybe a configuration should replace this im not certain
        - this is essentially a flat configuration file with non serializable objects
"""

from typing import Union, List
from os import path
from dataclasses import dataclass

import os, platform
import adsk.core, adsk.fusion, traceback

# from .unity import Parse
from ..general_imports import A_EP, PROTOBUF

from .SynthesisParser.Parser import Parser

# Contains enums for parents of joints that have special cases
class JointParentType:  # validate for unique key and value
    ROOT = 0  # grounded root object
    END = 1


class WheelType:
    STANDARD = 0
    OMNI = 1
    MECANUM = 2


class SignalType:
    PWM = 0
    CAN = 1
    PASSIVE = 2


# will need to be constructed in the UI Configure on Export
@dataclass
class _Wheel:
    occurrence_token: str  # maybe just pass the component
    wheelType: WheelType
    signalType: SignalType
    joint_token: str


@dataclass
class _Joint:
    joint_token: str
    parent: Union[str, JointParentType]  # str can be root
    signalType: SignalType


@dataclass
class Gamepiece:
    occurrence_token: str
    weight: float
    friction: float


class PhysicalDepth:
    """Depth at which the Physical Properties are generated and saved
    - This is mostly dictatated by export type as flattening or any hierarchical modification takes presedence
    """

    NoPhysical = 0
    """ No Physical Properties are generated """
    Body = 1
    """ Only Body Physical Objects are generated """
    SurfaceOccurrence = 2
    """ Only Occurrence that contain Bodies and Bodies have Physical Properties """
    AllOccurrence = 3
    """ Every Single Occurrence has Physical Properties even if empty """


class ModelHierarchy:
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


class Mode:
    Synthesis = 0

"""

examplec = ExampleClass("location of file", "name", "0.1.2", wheel=[Wheel(), Wheel()])

class ExampleClass:

    def __init__(
        self,
        fileLocation: str,
        name: str,
        version: str,

        alternate_version=None,
        materials=1,
        mode=Mode.Synthesis,
        wheel=List[Wheel],
        joints=List[JointDescription]
    ):
        if (alternate_version):
            this.alternate_version = alternate_version


        pass
"""


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
        mode=Mode.Synthesis,
        wheels=List[_Wheel],
        joints=List[_Joint],  # [{Occurrence, wheeltype} , {entitytoken, wheeltype}]
        gamepieces=List[Gamepiece],
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

    def parse(self, sendReq: bool) -> Union[str, bool]:
        """Parses the file given the options

        Args:
            sendReq (bool): Do you want to send the request generated by the parser with sockets

        Returns:
            str | bool: Either a str indicating success or False indicating failure
        """
        if A_EP:
            A_EP.send_event("Parse", "started_parsing")

        test = Parser(self).export()
        return True
