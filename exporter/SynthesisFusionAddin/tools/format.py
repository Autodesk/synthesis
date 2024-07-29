import os
import subprocess
import sys


def main(args: list[str] = sys.argv[1:]) -> None:
    dir = args[0] if len(args) else "."
    if "pyproject.toml" not in os.listdir(dir):
        print("WARNING: Configuration file for autoformatters was not found. Are you sure you specified the root DIR?")

    for command in ["isort", "black"]:
        try:
            print(f"Formatting with {command}...")
            subprocess.call([command, dir], shell=False, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        except FileNotFoundError:
            print(f'"{command}" could not be found. Please resolve dependencies.')
            return
        except BaseException as error:
            print(f'An unknown error occurred while running "{command}"\n{error}')

    print("Done! All files successfully formatted.")


if __name__ == "__main__":
    main()
