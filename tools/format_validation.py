import os
import sys

FILES_DIRS = ["engine/Assets/Scripts"]
FILE_TARGETS = [".cs"]
FORMAT_COMMAND = "clang-format -i -style=file"

def main():
    files = []
    for dir in FILES_DIRS:
        for root, _, filenames in os.walk(dir):
            for filename in filenames:
                if os.path.splitext(filename)[1] in FILE_TARGETS:
                    files.append(os.path.join(root, filename))

    for file in files:
        with open(file, "r") as f:
            previous_file_state = f.read()

        os.system(f"{FORMAT_COMMAND} {file}")
        with open(file, "r") as f:
            new_file_state = f.read()

        if previous_file_state != new_file_state:
            print(f"File {file} is not formatted correctly!")
            sys.exit(1)

    print("Success!")

if __name__ == '__main__':
    main()
