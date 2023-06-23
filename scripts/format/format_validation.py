import os
import sys

FILES_DIRS = ["engine/Assets/Scripts"]
FILE_TARGETS = [".cs"]
FORMAT_COMMAND = "clang-format -i -style=file"

"""
As of 6/23/2023, clang-format-16 is not available on Unbuntu through apt-get.
Trying to validate these files on clang-format-15 will cause a segfault. This is the current workaround.
"""
IGNORED_FILES = set([
    "engine/Assets/Scripts/UI/Dynamic/DynamicUIManager.cs"
    "engine/Assets/Scripts/WebSockets/WebSocketManager.cs"
])

def main():
    if sys.platform != "linux":
        print("Warning: This script was designed to be run by github action linux machines")

    files = []
    for dir in FILES_DIRS:
        for root, _, filenames in os.walk(dir):
            for filename in filenames:
                if os.path.splitext(filename)[1] in FILE_TARGETS and filename not in IGNORED_FILES:
                    files.append(os.path.join(root, filename))

    for file in files:
        with open(file, "r") as f:
            previous_file_state = f.readlines()

        os.system(f"{FORMAT_COMMAND} {file}")
        with open(file, "r") as f:
            new_file_state = f.readlines()

        exit_code = 0

        for i, (previous_line, new_line) in enumerate(zip(previous_file_state, new_file_state)):
            if previous_line != new_line:
                print(f"File {file} is not formatted correctly!")
                print(f"Line: {i + 1}")
                exit_code = 1
                break

    if exit_code == 0:
        print("All files are formatted correctly!")

    sys.exit(exit_code)

if __name__ == '__main__':
    main()
