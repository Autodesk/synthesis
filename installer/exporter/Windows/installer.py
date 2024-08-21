import os
import shutil
import sys
import tempfile
import zipfile
import ctypes


def getResourcePath(relativePath: str | os.PathLike[str]) -> None:
    if getattr(sys, "frozen", False) and hasattr(sys, "_MEIPASS"):
        basePath = sys._MEIPASS
    else:
        basePath = os.path.dirname(__file__)

    return os.path.join(basePath, relativePath)


def extractFile(resourceName: str | os.PathLike[str], destinationFolder: str | os.PathLike[str]) -> None:
    fullResourcePath = getResourcePath(resourceName)
    if not os.path.exists(fullResourcePath):
        raise FileNotFoundError(f"Resource '{resourceName}' not found.")

    shutil.copy(fullResourcePath, os.path.join(destinationFolder, resourceName))


def move_folder(sourceFolder: str | os.PathLike[str], destinationFolder: str | os.PathLike[str]) -> None:
    if not os.path.exists(sourceFolder):
        raise FileNotFoundError(f"Source folder '{sourceFolder}' not found.")

    dest_path = os.path.join(destinationFolder, os.path.basename(sourceFolder))
    if os.path.exists(dest_path):
        print("Path exists, removing it...")
        shutil.rmtree(dest_path)

    shutil.move(sourceFolder, destinationFolder)
    print(f"Successfully moved '{sourceFolder}' to '{destinationFolder}'.")


def main() -> None:
    if not ctypes.windll.shell32.IsUserAnAdmin():
        ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, " ".join(sys.argv), None, 1)

    destinationFolder = os.path.expandvars(r"%appdata%\Autodesk\ApplicationPlugins")
    os.makedirs(destinationFolder, exist_ok=True)
    with tempfile.TemporaryDirectory() as tempDir:
        extractFile("SynthesisExporter.zip", tempDir)
        with zipfile.ZipFile(os.path.join(tempDir, "SynthesisExporter.zip"), "r") as zip:
            zip.extractall(tempDir)

        sourceFolder = os.path.join(tempDir, "synthesis.bundle")
        move_folder(sourceFolder, destinationFolder)


if __name__ == "__main__":
    main()
