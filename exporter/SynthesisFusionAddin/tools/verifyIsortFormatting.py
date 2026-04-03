import subprocess
import sys

ROOT_EXPORTER_DIR = "exporter/SynthesisFusionAddin"


def main() -> None:
    result = subprocess.call(
        ["isort", "--check-only", "--diff", "--settings-path", ROOT_EXPORTER_DIR, ROOT_EXPORTER_DIR],
        shell=False,
    )
    if not result:
        print("All files are formatted correctly with isort!")
    sys.exit(result)


if __name__ == "__main__":
    main()
