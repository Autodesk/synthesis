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
    print(f"Export starting...")
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    start = time.perf_counter()

    exporter = GLTFDesignExporter(ao)
    glbFilePath = '{0}{1}_{2}.{3}'.format('C:/temp/', ao.document.name.replace(" ", "_"), int(time.time()), "glb")
    exporter.saveGLB(glbFilePath)

    savedFile = time.perf_counter()
    print(f"Export completed in {savedFile-start} seconds")
    print(f"savedFile {savedFile} seconds")

    # dict = gltf_asdict(gltf) # debugging only

    # printHierarchy(ao.root_comp)
    print()  # put breakpoint here and view the protoDocumentAsDict local variable


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()

