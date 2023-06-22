import os
import sys
from sys import platform

FILES_DIRS = ["engine/Assets/Scripts"]
FILE_TARGETS = [".cs"]
FORMAT_COMMAND = "clang-format -i -style=file"

def main():
    if sys.platform != "linux":
        print("Warning: This script was designed to be run by github action linux machines")

    files = []
    for dir in FILES_DIRS:
        for root, _, filenames in os.walk(dir):
            for filename in filenames:
                if os.path.splitext(filename)[1] in FILE_TARGETS:
                    files.append(os.path.join(root, filename))

    for file in files:
        with open(file, "r") as f:
            previous_file_state = f.readlines()

        os.system(f"{FORMAT_COMMAND} {file}")
        with open(file, "r") as f:
            new_file_state = f.readlines()

        exit_code = 0

        # Compare the previous file state with the new file state after formatting
        for i, (previous_line, new_line) in enumerate(zip(previous_file_state, new_file_state)):
            previous_line = previous_line.strip()
            new_line = new_line.strip()

            if previous_line != new_line:
                print(f"File {file} is not formatted correctly!")
                print(f"Previous: {previous_line}")
                print(f"New: {new_line}")
                print(f"Index: {i}")
                break

                exit_code = 1

    if exit_code == 0:
        print("All files are formatted correctly!")

    sys.exit(exit_code)

if __name__ == '__main__':
    main()
