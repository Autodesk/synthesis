import os
import shutil
import sys
import tempfile
import zipfile
import ctypes


def resource_path(relative_path):
    if getattr(sys, "frozen", False) and hasattr(sys, "_MEIPASS"):
        base_path = sys._MEIPASS
    else:
        base_path = os.path.dirname(__file__)

    return os.path.join(base_path, relative_path)


def extract_file(resource_name, dest_folder):
    resource_full_path = resource_path(resource_name)
    if not os.path.exists(resource_full_path):
        raise FileNotFoundError(f"Resource '{resource_name}' not found.")

    shutil.copy(resource_full_path, os.path.join(dest_folder, resource_name))


def move_folder(src_folder, dest_folder):
    if not os.path.exists(src_folder):
        print(f"Source folder '{src_folder}' does not exist.")
        return

    if not os.path.exists(dest_folder):
        print(f"Destination folder '{dest_folder}' does not exist. Creating it.")
        os.makedirs(dest_folder)

    dest_path = os.path.join(dest_folder, os.path.basename(src_folder))
    if os.path.exists(dest_path):
        print("Path exists, removing it...")
        shutil.rmtree(dest_path)

    shutil.move(src_folder, dest_folder)
    print(f"Successfully moved '{src_folder}' to '{dest_folder}'.")


def main():
    destination_folder = os.path.expandvars(r"%appdata%\Autodesk\ApplicationPlugins")
    os.makedirs(destination_folder, exist_ok=True)
    with tempfile.TemporaryDirectory() as temp_dir:
        extract_file("SynthesisExporter.zip", temp_dir)
        with zipfile.ZipFile(os.path.join(temp_dir, "SynthesisExporter.zip"), "r") as zip_ref:
            zip_ref.extractall(temp_dir)

        src_folder = os.path.join(temp_dir, "synthesis.bundle")
        move_folder(src_folder, destination_folder)


if __name__ == "__main__":
    main()
