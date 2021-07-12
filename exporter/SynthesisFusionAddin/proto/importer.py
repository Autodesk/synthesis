"""
Module to include a builtin to facilitate importing packages into the workspace.

Usage:
    `   from .Import import *

        fusionImport('numpy')
        fusionImport('numpy', 'protobuf')

        # This will import the entire library but you can still call
        from numpy import array
    `
"""
import importlib, builtins, subprocess, platform
from sys import modules
from os import system, chdir, __file__, getcwd, path
from pathlib import Path, PurePath

import adsk.core, adsk.fusion, traceback


def fusionImport(*argv):
    """Builtin Function to import a module using pip from Fusion 360

    Raises:
        RuntimeError: Raises Runtime Error if cannot be installed
        TypeError: Raises Type Error is isn't supplied as string
    """
    for item in argv:
        if type(item) is str:

            # because you can specify versions , numpy==1.19.3
            # importlib treats . as a subpkg
            # this will give you numpy
            itemName = item.split("==")[0]

            # attempt to find the package if it alredy is imported
            if __checkDeps(itemName) is True:

                if modules[itemName] is None:
                    print("Adding name to globals")
                    modules[itemName] = importlib.import_module(itemName)

                print(f"Package {itemName} is loaded and needs to be seperately imported")
                continue

            # at this point its not imported
            print(f"Importing {itemName}")

            try:
                # This will import a global package but with a local item you need to specify a subpkg in the optional param
                importlib.import_module(itemName)
                # modules[itemName] = importlib.import_module(itemName)
                # print(f'{numpy.version.version}')
            except ImportError:
                # Cannot be imported so attempt to fetch it with pip
                if __installCross(item) is False:
                    # Cannot install dependency
                    print("Failed to install dependency")
                    raise RuntimeError(f"Fusion 360 cannot find and install {item}")
        else:
            raise TypeError(
                f"Cannot instal dependency listed as it isn't supplied as a string"
            )

    # invalidates caches to force reload the imported module
    # may not be necessary
    # importlib.invalidate_caches()


def __installCross(pipDep: str) -> bool:
    """Attempts to fetch pip script and resolve dependencies with less user interaction

    Args:
        pipDep (pipDep): string name of import

    Raises:
        ImportError: Unrecognized Platform

    Returns:
        bool: Success

    Notes:
        Liam originally came up with this style after realizing accessing the python dir was too unreliable.
    """

    app = adsk.core.Application.get()
    ui = app.userInterface

    if app.isOffLine:
        ui.messageBox(
            "Unable to install dependencies when Fusion is offline. Please connect to the internet and try again!"
        )
        return False

    progressBar = ui.createProgressDialog()
    progressBar.isCancelButtonShown = False
    progressBar.reset()
    progressBar.show("Addin", f"Installing dependencies...", 0, 4, 0)

    # this is important to reduce the chance of hang on startup
    adsk.doEvents()

    system = platform.system()

    if system == "Windows":
        pythonFolder = Path(__file__).parents[
            1
        ]  # Assumes the location of the fusion python executable is two folders up from the os lib location

    elif system == "Darwin":  # macos
        pythonFolder = Path(__file__).parents[2] / "bin"
        progressBar.message = f"Fetching pip..."
        adsk.doEvents()

        # fetches pip if it doesn't already exist in the correct directory
        if not path.exists(path.join(pythonFolder, "get-pip.py")):
            # fetch the pip installer
            subprocess.run(
                f"curl https://bootstrap.pypa.io/get-pip.py -o \"{pythonFolder / 'get-pip.py'}\"",
                shell=True,
            )

            # runs the process to install pip
            subprocess.run(
                f"\"{pythonFolder / 'python'}\" \"{pythonFolder / 'get-pip.py'}\"",
                shell=True,
            )
    else:
        raise ImportError(
            f"Unsupported platform! This add-in only supports windows and macos"
        )

    progressBar.progressValue += 1
    progressBar.message = f"Installing {pipDep}..."
    adsk.doEvents()

    print(f"Running process: " + f"\"{pythonFolder / 'python'}\" -m pip install {pipDep}")

    subprocess.run(f"\"{pythonFolder / 'python'}\" -m pip install {pipDep}", shell=True)

    # this is somehow necessary
    if system == "Darwin":
        pipAntiDeps = ["dataclasses", "typing"]
        for depName in pipAntiDeps:
            progressBar.message = f"Uninstalling {depName}..."
            adsk.doEvents()
            subprocess.run(
                f"\"{pythonFolder / 'python'}\" -m pip uninstall {depName} -y",
                shell=True,
            )

    progressBar.hide()

    try:

        print("getting item")
        return True
        # This will import a global package but with a local item you need to specify a subpkg in the optional param
        importlib.import_module(pipDep)

        print("imported the module after download")

        # now its installed so check if exists in directories
        if __checkDeps(pipDep) is False:
            return False

        # installed and loaded
        return True
    except ImportError:
        return False


def __checkDeps(item: str) -> bool:
    """Checks if the current dependency is loaded

    Args:
        item (str): Name of the package to install

    Returns:
        bool: Success
    """
    try:
        item_res = importlib.util.find_spec(item)
        if item_res is not None:
            return True
        return False
    # this only happens on windows somehow
    except ModuleNotFoundError:
        return False


# adds the builtin function to make it available everywhere
# arguably a bad idea but here we are
builtins.fusionimport = fusionImport
