import time

import adsk.core

import apper
from apper import AppObjects
from fusionutils.DebugHierarchy import *
from ..gltfutils.GLTFDesignExporter import GLTFDesignExporter

def exportRobot():
    ao = AppObjects()

    if ao.document.dataFile is None:
        print("Error: You must save your fusion document before exporting!")
        return

    start = time.perf_counter()
    startRealtime = time.time()

    exporter = GLTFDesignExporter(ao, enableMaterials=True, enableMaterialOverrides=True, enableFaceMaterials=True)
    filePath = '{0}{1}_{2}.{3}'.format('C:/temp/', ao.document.name.replace(" ", "_"), int(time.time()), "glb")
    perfResults, bufferResults, warnings, modelStats, eventCounter = exporter.saveGLB(filePath)

    end = time.perf_counter()
    endRealtime = time.time()
    finishedMessage = (f"glTF export completed in {round(end - start, 4)} seconds ({round(endRealtime - startRealtime, 4)} realtime)\n"
                       f"File saved to {filePath}\n\n"
                       f"==== Export Performance Results ====\n"
                       f"{perfResults}\n"
                       f"==== Buffer Writing Results ====\n"
                       f"{bufferResults}\n"
                       f"==== Model Stats ====\n"
                       f"{modelStats}\n"
                       f"==== Events Counter ====\n"
                       f"{eventCounter}\n"
                       f"==== Warnings ====\n"
                       f"{warnings}\n"
                       )
    ao.ui.messageBox(finishedMessage)


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportRobot()
