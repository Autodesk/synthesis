"""
 Generates a Packaged build for the Hellion-Fusion (Unity) Project that can be distributed.

 1. Copy all files matching a exp into the `package` dir
 2. Use pyminifer to create a minified version of each file
 3. Copy the manifest file into the target directory
 4. Bundle the package into a zip that will be distributed |
 4. Bundle the package with NSIS or similiar installer  
"""

from shutil import copyfile, copytree, ignore_patterns, rmtree, copy2
import os, sys

import zipfile


class bcolors:
    """Found at: https://stackoverflow.com/questions/287871/how-to-print-colored-text-in-python

    - I use the same thing for the terminal scripts for coloring
    """

    HEADER = "\033[95m"
    OKBLUE = "\033[94m"
    OKCYAN = "\033[96m"
    OKGREEN = "\033[92m"
    WARNING = "\033[93m"
    FAIL = "\033[91m"
    ENDC = "\033[0m"
    BOLD = "\033[1m"
    UNDERLINE = "\033[4m"


OUT = "package"
OUT_SRC = os.path.join("package", "src")
OUT_LOG = os.path.join("package", "logs")

ZIP_NAME = "SynthesisFusionAddin.zip"

MIRA = True
OUT_MIRA = os.path.join("package", "mira")


class wincolors:
    ENDC = "{0m"
    OKGREEN = "<ESC"


def copyFiles():
    if os.path.exists(OUT):
        warn("Cleaning Previous Package Directory", "")
        rmtree(OUT)
    bold("Copying Source Files")

    copytree(
        "src",
        OUT_SRC,
        ignore=ignore_patterns("*.pyc", "tmp*", "scss", "__*", "*.css.map", ".*"),
        dirs_exist_ok=True,
        copy_function=copy2_verbose,
    )

    copytree(
        "logs",
        OUT_LOG,
        ignore=ignore_patterns("*.pyc", "tmp*", "scss", "__*", ".*"),
        dirs_exist_ok=True,
        copy_function=copy2_verbose,
    )

    if MIRA:
        copytree(
            "mira",
            OUT_MIRA,
            ignore=ignore_patterns(
                "*.pyc", "tmp*", "scss", "__*", "*.css.map", ".*", "*.proto"
            ),
            dirs_exist_ok=True,
            copy_function=copy2_verbose,
        )

    bold("Copying Manifest File")
    copy2_verbose("UnityExporter.Manifest", "package/UnityExporter.Manifest")
    copy2_verbose("UnityExporter.py", "package/UnityExporter.py")
    copy2_verbose("config_default.ini", "package/config.ini")
    copy2_verbose("help.txt", "package/help.txt")
    copy2_verbose("PrivacyPolicy.pdf", "package/PrivacyPolicy.pdf")


def copy2_verbose(src, dst):
    """Awesome function to print the state of the copying of files

    Modified from:
    - https://stackoverflow.com/questions/26496821/python-shutil-copytree-is-there-away-to-track-the-status-of-the-copying

    Args:
        src (str): source file name
        dst (str): destination file name
    """
    try:
        copy2(src, dst)
        good(f"Copying", src)
    except:
        bad("Failed Copying", src)


def minify():
    bold("Minifying and Obfuscating the output")

    cwd = os.getcwd()
    path = os.path.join(cwd, "package")

    for root, dirs, files in os.walk(path):
        # This is somehow a workaround for permission errors - nice
        root_new = root.replace("src", "src")
        for f in files:
            if os.path.splitext(f)[1] == ".py":
                fullpath = os.path.join(root, f)
                fullpathNew = os.path.join(root_new, f)
                command = "pyminifier --outfile={0} {1}".format(fullpathNew, fullpath)

                """ Security no like my minimize work around
                ret = os.system(command)
                if ret == 1:
                    bad(f"Could not minify: cd {f}")
                else:
                    good(f"Minified", f)
                """


def zip_file():
    bold("Zipping File")
    try:
        import zlib

        compression = zipfile.ZIP_DEFLATED
    except:
        compression = zipfile.ZIP_STORED

    modes = {
        zipfile.ZIP_DEFLATED: "deflated",
        zipfile.ZIP_STORED: "stored",
    }

    zf = zipfile.ZipFile(ZIP_NAME, "w", zipfile.ZIP_DEFLATED)

    try:
        cwd = os.getcwd()
        file_path = os.path.join(cwd, "package")

        zipdir("package", zf)
        path = os.path.join(os.getcwd(), ZIP_NAME)
        good(
            "Zip Generated",
            "{0} - {1} Kb - {2}".format(ZIP_NAME, os.stat(ZIP_NAME).st_size / 1000, path),
        )
    finally:
        zf.close()


def zipdir(path, ziph):
    for root, dirs, files in os.walk(path):
        for file in files:
            fullpath = os.path.join(root, file)
            # Just to get rid of the prefix package dir
            path = os.path.relpath(fullpath, "package")
            ziph.write(path)


def checkmark():
    return "\u2713 "


# This could have easily been a macro with some pointers to functions but its simple this way
def good_fmt(msg: str) -> str:
    return f"{checkmark()} [{bcolors.OKGREEN}{msg}{bcolors.ENDC}]"


def bad_fmt(msg: str) -> str:
    return f" [{bcolors.FAIL}{msg}{bcolors.ENDC}]"


def warn_fmt(msg: str) -> str:
    return f" [{bcolors.WARNING}{msg}{bcolors.ENDC}]"


def bold_fmt(msg: str) -> str:
    return f"{bcolors.BOLD}{msg}{bcolors.ENDC}"


def good(status: str, msg="") -> None:
    print("{0} - {1}".format(good_fmt(status), msg))


def bad(status: str, msg="") -> None:
    print("{0} - {1}".format(bad_fmt(status), msg))


def warn(status: str, msg="") -> None:
    print("{0} - {1}".format(warn_fmt(status), msg))


def bold(status: str, msg="") -> None:
    print("\n{0}  {1}\n".format(bold_fmt(status), msg))


def main():
    try:
        import pyminifier
    except ImportError:
        bad("Please run pip install pyminifier")
        return

    try:
        import black
    except ImportError:
        warn(
            "It is recommended to use \`black .\` with each build to keep style guides intact."
        )
        pass

    argv = sys.argv[1:]

    for option in argv:
        if option == "mira":
            global MIRA
            MIRA = True

    bold("Packaging Fusion 360 Unity Addin")
    copyFiles()
    minify()
    zip_file()
    bold("Finished Packaging, Publish and Extract to add to Fusion 360")


if __name__ == "__main__":
    main()
