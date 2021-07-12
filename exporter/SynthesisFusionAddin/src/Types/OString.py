from typing import List, Union
import os, platform
import pathlib


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
            if (
                self.path == value.path
                and self.fileName == value.fileName
                and self.platform == value.platform
            ):
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
            return None

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
