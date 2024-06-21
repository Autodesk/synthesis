import os
import subprocess
import sys

EXPORTER_DIR = "exporter/SynthesisFusionAddin"
files = (lambda d: [os.path.abspath(os.path.join(root, filename)) for root, _, filenames in os.walk(d)
         for filename in filenames if filename.endswith(".py")])(EXPORTER_DIR)
oldFileStates = [open(file, "r").readlines() for file in files]
subprocess.call(["isort", EXPORTER_DIR], bufsize=1, shell=False)
newFileStates = [open(file, "r").readlines() for file in files]
exitCode = 0
for i, (oldFileState, newFileState) in enumerate(zip(oldFileStates, newFileStates)):
  for j, (previousLine, newLine) in enumerate(zip(oldFileState, newFileState)):
    if previousLine != newLine:
      print(f"File {files[i]} is not formatted correctly!\nLine: {j + 1}")
      exitCode = 1

if not exitCode: print("All files are formatted correctly with isort!")
sys.exit(exitCode)
