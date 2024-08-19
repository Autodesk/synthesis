# Synthesis Exporter

This is a Addin for Autodesk® Fusion™ that will export a [Mirabuf](https://github.com/HiceS/mirabuf) usable by the Synthesis simulator.

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

You can download Protobuf v23.3 [here](https://github.com/protocolbuffers/protobuf/releases/tag/v23.3).

#### Dev Tools

We use `VSCode` Primarily, download it to interact with our code or use your own at your own risk.

---

### How to Build + Run

1. Open `Autodesk Fusion`
2. Select `UTILITIES` from the top bar
3. Click `ADD-INS` Button
4. Click `Add-Ins` tab at the top of Scripts and Add-Ins dialog
5. Press + Button under **My Add-Ins**
6. Navigate to the containing folder for this Addin and click open at bottom - _clone-directory_/synthesis/exporters/SynthesisFusionAddin
7. Synthesis should be an option - select it and click run at the bottom of the dialog
8. There should now be a button that says Synthesis in your utilities menu
   - If there is no button there may be a problem - see below for [checking log file](#debug-non-start)

---

### How to Debug

#### Debug Non Start

Most of the runtime for the addin is saved under the `logs` directory in this folder

- Open `logs/synthesis.log`
  - If nothing appears something went very wrong (make a issue on this github)
  - If something appears and you cannot solve it feel free to make an issue anyway and include the file

#### General Debugging

1. Open `Autodesk Fusion`
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

We format using a Python formatter called `black` [![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black) in conjunction with [`isort`](https://pycqa.github.io/isort/).

- install by `pip3 install black && pip3 install isort` or `pip install black && pip install isort`
- use `isort .` followed by `black .` to format all relevant exporter python files.
  - or, alternatively, run `python ./tools/format.py` to do this for you!

**Note: black will always ignore files in the proto/proto_out folder since google formats those**

### Docstring standard

This standard is inconsistently applied, and that's ok

```python
def foo(bar: fizz="flower") -> Result[walrus, None]:
    """
    Turns a fizz into a walrus

    Parameters:
    bar - The fizz to be transformed (default = "flower") ; fizz standards are subject to change, old fizzes may no longer be valid

    Returns:
    Success - She new walrus
    Failure - None if the summoning fails ; the cause of failure will be printed, not returned

    Notes:
    - Only works as expected if the bar arg isn't a palindrome or an anagram of coffee. Otherwise unexpected (but still valid) walruses may be returned
    - Please do not name your fizz "rizz" either, it hurts the walrus's feelings

    TODO: Consult witch about inconsistent alchemical methods
    """
    # More alchemical fizz -> walrus code
    some_walrus = bar + "_coffee"
    return some_walrus

```

Note that not this much detail is necessary when writing function documentation, notes, defaults, and a differentiation between sucess and failure aren't always necessary.

#### Where to list potential causes of failure?

It depends on how many you can list

- 1: In the failure return case
- 2-3: In the notes section
- 4+: In a dedicated "potential causes of failure section" between the "returns" and "notes" sections

Additionally, printing the error instead of returning it is bad practice
