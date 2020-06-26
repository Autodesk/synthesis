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

    exporter = GLTFDesignExporter(ao)
    glbFilePath = '{0}{1}_{2}.{3}'.format('C:/temp/', ao.document.name.replace(" ", "_"), int(time.time()), "glb")
    exporter.saveGLB(glbFilePath)

    savedFile = time.perf_counter()
    finishedMessage = f"GLTF export completed in {round(savedFile - start, 1)} seconds\n" \
            f"File saved to {glbFilePath}"
    print(finishedMessage)
    ao.ui.messageBox(finishedMessage)


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()

