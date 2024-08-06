import os
import subprocess
import sys

ROOT_EXPORTER_DIR = "exporter/SynthesisFusionAddin"


def main() -> None:
    files = [
        os.path.abspath(os.path.join(root, filename))
        for root, _, filenames in os.walk(ROOT_EXPORTER_DIR)
        for filename in filenames
        if filename.endswith(".py")
    ]
    oldFileStates = [open(file, "r").readlines() for file in files]
    subprocess.call(["isort", ROOT_EXPORTER_DIR], bufsize=1, shell=False)
    newFileStates = [open(file, "r").readlines() for file in files]
    exitCode = 0
    for i, (oldFileState, newFileState) in enumerate(zip(oldFileStates, newFileStates)):
        for j, (previousLine, newLine) in enumerate(zip(oldFileState, newFileState)):
            if previousLine == newLine:
                continue

            print(f"File {files[i]} is not formatted correctly!\nLine: {j + 1}")
            oldFileStateRange = range(max(0, j - 10), min(len(oldFileState), j + 11))
            print("\nOld file state:\n" + "\n".join(oldFileState[k].strip() for k in oldFileStateRange))
            newFileStateRange = range(max(0, j - 10), min(len(newFileState), j + 11))
            print("\nNew file state:\n" + "\n".join(newFileState[k].strip() for k in newFileStateRange))
            exitCode = 1
            break

    if not exitCode:
        print("All files are formatted correctly with isort!")

    sys.exit(exitCode)


if __name__ == "__main__":
    main()
