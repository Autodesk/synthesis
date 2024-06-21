import os
import subprocess
import sys

FILES_DIRS = ["exporter/SynthesisFusionAddin"]
FILE_TARGETS = [".py", ".pyi"]

def main():
    if sys.platform != "linux":
        print("Warning: This script was designed to be run by github action linux machines")

    files = []
    for dir in FILES_DIRS:
        for root, _, filenames in os.walk(dir):
            for filename in filenames:
                if os.path.splitext(filename)[1] in FILE_TARGETS:
                    files.append(os.path.abspath(os.path.join(root, filename)))

    file_states = [open(file, "r").readlines() for file in files]
    subprocess.call(["isort", "."], bufsize=1, shell=False,)
    new_file_states = [open(file, "r").readlines() for file in files]

    exit_code = 0

    for i, (previous_file_state, new_file_state) in enumerate(zip(file_states, new_file_states)):
        for i, (previous_line, new_line) in enumerate(zip(previous_file_state, new_file_state)):
            if previous_line != new_line:
                print(f"File {files[i]} is not formatted correctly!")
                print(f"Line: {i + 1}")
                exit_code = 1
                break

    if exit_code == 0:
        print("All files are formatted correctly!")

    sys.exit(exit_code)


if __name__ == '__main__':
    main()
