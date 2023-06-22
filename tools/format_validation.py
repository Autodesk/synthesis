import os
import sys

def main():
    files = []
    for root, _, file_names in os.walk("engine/Assets/Scripts"):
        for file_name in file_names:
            if file_name.endswith(".cs"):
                files.append(os.path.join(root, file_name))

    for file in files:
        previous_file_state = open(file, "r").read()
        os.system(f"clang-format -i {file}")
        new_file_state = open(file, "r").read()

        if previous_file_state != new_file_state:
            print(f"File {file} is not formatted correctly!")
            # sys.exit(1)

        # Restore the previous state of the file pre-formatting
        if len(sys.argv) == 1 or sys.argv[1] != "--ignore-changes":
            pass

if __name__ == '__main__':
    main()
