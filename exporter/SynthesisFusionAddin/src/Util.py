import os


def makeDirectories(directory: str | os.PathLike[str]) -> str | os.PathLike[str]:
    """Ensures than an input directory exists and attempts to create it if it doesn't."""
    os.makedirs(directory, exist_ok=True)
    return directory
