import time

import adsk
import adsk.core
import adsk.fusion
import numpy as np

import apper
from apper import AppObjects, item_id
from ..utils.DebugHierarchy import *
from ..utils.GLTFUtils import *
from ..utils.GLTFDesignExporter import GLTFDesignExporter

ATTR_GROUP_NAME = "SynthesisFusionExporter"  # attribute group name for use with apper's item_id


def exportRobot():
    print(f"GLTF export starting...")
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    start = time.perf_counter()
    startRealtime = time.time()

    exporter = GLTFDesignExporter(ao)
    filePath = '{0}{1}_{2}.{3}'.format('C:/temp/', ao.document.name.replace(" ", "_"), int(time.time()), "glb")
    perfResults, bufferResults = exporter.saveGLB(filePath)

    end = time.perf_counter()
    endRealtime = time.time()
    finishedMessage = f"GLTF export completed in {round(end - start, 4)} seconds ({round(endRealtime - startRealtime, 4)} realtime)\n" \
                      f"File saved to {filePath}\n\n" \
                      f"==== Export Performance Results ====\n" \
                      f"{perfResults}\n\n"\
                      f"==== Buffer Performance Results ====\n" \
                      f"{bufferResults}"
    ao.ui.messageBox(finishedMessage)


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()
