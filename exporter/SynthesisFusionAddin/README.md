# Synthesis Exporter
This is a Addin for *Autodesk Fusion 360* that will export a solid model file into a format that is usable by the Synthesis Unity Project.

## Features
- [x] Materials
- [x] Apperances
- [x] Instances
- [x] Joints
- [x] Static and Dynamic components
- [x] Mesh Quality
- [x] Thumbnail Capture
- [x] Local Save
- [x] Simple Motors
- [x] GZip compression
- [ ] Motion Links
- [ ] Complex Motors
- [ ] Sending compressed file
- [ ] File Diffs


### Pre-requisites

#### Protobuf
 - To build protobuf from source look at the docs here [Protobuf](https://github.com/protocolbuffers/protobuf/releases)
    - `cd proto && sudo ./build.sh` (OSX)
    - `` (Windows) - Someone fill this in
 - To use pre-built Python Libraries you can grab the latest here [Mirabuf](https://github.com/HiceS/mirabuf/releases) (usually `python.zip`)
    - Export all but `setup.py` to `proto/proto_out`

#### Dev Tools

We use `VSCode` Primarily, download it to interact with our code or use your own at your own risk.

---

### How to Build + Run
1. Open `Autodesk Fusion 360`
2. Select `UTILITIES` from the top bar
3. Click `ADD-INS` Button
4. Click `Add-Ins` tab at the top of Scripts and Add-Ins dialog
5. Press + Button under **My Add-Ins** 
6. Navigate to the containing folder for this Addin and click open at bottom - _clone-directory_/synthesis/exporters/SynthesisFusionAddin
7. Synthesis should be an option - select it and click run at the bottom of the dialog
8. There should now be a button that says Synthesis in your utilities menu
    - If there is no button there may be a problem - see below for checking log file
---

### How to Debug

#### Debug Non Start

Most of the runtime for the addin is saved under the `logs` directory in this folder

- Open `logs/synthesis.log`
    - If nothing appears something went very wrong (make a issue on this github)
    - If something appears and you cannot solve it feel free to make an issue anyway and include the file

#### General Debugging

1. Open `Autodesk Fusion 360`
2. Select `UTILITIES` from the top bar
3. Click `ADD-INS` Button
4. Click `Add-Ins` tab at the top of Scripts and Add-Ins dialog
5. Press + Button under **My Add-Ins** 
6. Navigate to the containing folder for this Addin and click open at bottom - _clone-directory_/synthesis/exporters/SynthesisFusionAddin
7. Synthesis should be an option - select it and click `Debug` at the bottom of the dialog
    - This is in a dropdown with the Run Button
8. This should open VSCode - Now run with `FN+5`
    - Now you may add break points or debug at will

---

### How to Package

Packaging is mainly for compressing the files into a smaller footprint

Contact us for information on how to use the packaging script to obfuscate all of the files using `pyminifier`.

---
### How to Format

We format using a Python formatter called `black` [![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black)

- install by `pip3 install black` or `pip install black`
- use `black ./src`, Formats all files in src directory

__Note: black will always ignore files in the proto/proto_out folder since google formats those__

---

### Using the Exporter

1. Pre-requisites 
(VS Code)
(Fusion 360)
(PROTOC - provide a link (through pip3))
(Windows & OSX)
(git submodule update)

2. How to Build 
(1. build protos, 2. Open Fusion, etc.)

3. How to Debug (Where is log file - how to read it)

4. How to package (Currently TBD)

5. How to use the software - you could attach screenshots (general how to use not really a tutorial) (if linking screenshots, you need to create a sample false issue and drag and drop to get link)