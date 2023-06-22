import os
import sys
from sys import platform

FILES_DIRS = ["engine/Assets/Scripts"]
FILE_TARGETS = [".cs"]
FORMAT_COMMAND = "clang-format -i -style=file"

def main():
    # Uses the clang format file as a marker for the root of the project
    if not os.path.exists(os.path.join(os.getcwd(), ".clang-format")):
        print("Error: .clang-format not found. Are you in the root of the project?")
        return

    if sys.platform == "win32":
        clang_format = None
        for path in os.environ["PATH"].split(os.pathsep):
            if os.path.exists(os.path.join(path, "clang-format.exe")):
                clang_format = os.path.join(path, "clang-format.exe")
                break

        if clang_format is None:
            print("Error: clang-format.exe not found in PATH")
            return

        files = []
        for dir in FILES_DIRS:
            for root, _, filenames in os.walk(dir):
                for filename in filenames:
                    if os.path.splitext(filename)[1] in FILE_TARGETS:
                        files.append(os.path.join(root, filename))

        print(f"Found {len(files)} files.")

        for file in files:
            print(f"Formatting {file}...")
            os.system(f"{FORMAT_COMMAND} {file}")

        print("Done!")

if __name__ == '__main__':
    main()