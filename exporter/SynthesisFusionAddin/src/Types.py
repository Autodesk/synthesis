import os
import pathlib
import platform
from dataclasses import dataclass, field, fields, is_dataclass
from enum import Enum, EnumType
from typing import TypeAlias, Union, get_origin

import adsk.fusion

# Not 100% sure what this is for - Brandon
JointParentType = Enum("JointParentType", ["ROOT", "END"])

WheelType = Enum("WheelType", ["STANDARD", "OMNI", "MECANUM"])
SignalType = Enum("SignalType", ["PWM", "CAN", "PASSIVE"])
ExportMode = Enum("ExportMode", ["ROBOT", "FIELD"])  # Dynamic / Static export
ExportLocation = Enum("ExportLocation", ["UPLOAD", "DOWNLOAD"])
UnitSystem = Enum("UnitSystem", ["METRIC", "IMPERIAL"])

FUSION_UNIT_SYSTEM: dict[int, Enum] = {
    adsk.fusion.DistanceUnits.MillimeterDistanceUnits: UnitSystem.METRIC,
    adsk.fusion.DistanceUnits.CentimeterDistanceUnits: UnitSystem.METRIC,
    adsk.fusion.DistanceUnits.MeterDistanceUnits: UnitSystem.METRIC,
    adsk.fusion.DistanceUnits.InchDistanceUnits: UnitSystem.IMPERIAL,
    adsk.fusion.DistanceUnits.FootDistanceUnits: UnitSystem.IMPERIAL,
}


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


KG: TypeAlias = float
LBS: TypeAlias = float
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


class OString:
    def __init__(self, path: object, fileName: str):
        """Generate a string for the operating system that matches fusion requirements

        Args:
            literals (Path): Path to the file using the pathlib library
            fileName (str): File name with extension eg. 'test.hell'

        Raises:
            OSError: If OS could not be recognized will return a custom function with no traceback
        """
        self.path = path
        self.fileName = fileName
        self.platform = self._os()

    def __repr__(self) -> str:
        """Generates a descript string representation for this format

        Returns:
            str: OString [ - ['test', 'test2]  - 'test.hell' ]
        """
        # return f"OString [\n-\t[{self.literals!r} \n-\t{self.fileName}\n]"
        return f"{os.path.join(self.path, self.fileName)}"

    def __eq__(self, value: object) -> bool:
        """Equals operator for this class

        Args:
            value (OString): Comparison object

        Returns:
            bool: Did the OString objects match?
        """
        if isinstance(value, OString):
            if self.path == value.path and self.fileName == value.fileName and self.platform == value.platform:
                return True
        return False

    @staticmethod
    def _os() -> str:
        """Find the operating system and link it to the string for generating paths

        Raises:
            OSError: Could not detect operating system

        Returns:
            str: String matching ['Windows', 'Darwin', 'Linux']
        """
        # get operating system for functional changes
        osName = platform.system()
        if osName == "Windows" or osName == "win32":
            return "Windows"
        elif osName == "Darwin":
            return "Darwin"
        elif osName == "Linux":
            return "Linux"
        else:
            raise OSError(2, "No Operating System Recognized", f"{osName}")

    def AssertEquals(self, comparing: object):
        """Compares the two OString objects

        Args:
            comparing (OString): String to be compared with

        Returns:
            bool: Did the comparing string match?
        """
        return comparing == self

    def getPath(self) -> Union[str, object]:
        """Returns a OSPath from literals and filename

        Returns:
            Path | str: OsPath that is cross platform
        """
        return os.path.join(self.path, self.fileName)

    def getDirectory(self) -> Union[str, object]:
        """Returns a OSPath from literals and filename

        Returns:
            Path | str: OsPath that is cross platform
        """
        return self.path

    def exists(self) -> bool:
        """Check to see if Directory and File exist in the current system

        Returns:
            bool: Do they both exist?
        """
        if os.path.exists(self.getDirectory()) and os.path.exists(self.getPath()):
            return True
        return False

    def serialize(self) -> str:
        """Serialize the OString to be storred in a temp doc

        Returns:
            str: Serialized string
        """
        return self.getPath()

    @classmethod
    def deserialize(cls, serialized) -> object:
        path, file = os.path.split(serialized)
        if path is None or file is None:
            raise RuntimeError(f"Can not parse OString Path supplied \n {serialized}")
        else:
            return cls(path, file)

    @classmethod
    def LocalPath(cls, fileName: str) -> object:
        """Gets the local path in the absolute form for this file

        Args:
            fileName (str): Name of the file that is defining the OString

        Returns:
            OString: OString class instance with a automatic local path to the file
        """
        path = os.path.dirname(os.path.realpath(__file__))
        return cls(path.split(os.sep), fileName)

    @classmethod
    def AddinPath(cls, fileName: str) -> object:
        """Gets the local path in the absolute form for this file

        Args:
            fileName (str): Name of the file that is defining the OString

        Returns:
            OString: OString class instance with a automatic local path to the file
        """
        path = pathlib.Path(__file__).parent.parent.parent
        return cls(path, fileName)

    @classmethod
    def AppDataPath(cls, fileName: str) -> object:
        """Attempts to generate a file path in the Appdata Directory listed below

         Used by TempPath in the windows environment
         - '%Appdata%/Local/Temp/file.hell

        Args:
            fileName (str): Name and Extension of the file

        Returns:
            OString | None: Usable Os String or None
        """
        if cls._os() == "Windows":
            if os.getenv("APPDATA") is not None:
                path = os.path.join(os.getenv("APPDATA"), "..", "Local", "Temp")
                return cls(path, fileName)
        return None

    @classmethod
    def ThumbnailPath(cls, fileName: str) -> object:
        # this is src
        src = pathlib.Path(__file__).parent.parent
        res = os.path.join(src, "Resources", "Icons")

        return cls(res, fileName)

    @classmethod
    def TempPath(cls, fileName: str) -> object:
        """Find a temporary path that will work on any OS to write a file to and read from

        Args:
            fileName (str): file name and extenstion

        Returns:
            OString: OString class that can be retrieved for writing to
        """
        _os = cls._os()

        if _os == "Windows":
            return cls.AppDataPath(fileName)
        elif _os == "Darwin":
            baseFile = pathlib.Path(__file__).parent.parent.parent
            path = os.path.join(baseFile, "TemporaryOutput")

            if not os.path.isdir(path):
                os.mkdir(path)

            return cls(path, fileName)

        return None
