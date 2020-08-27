import time

import adsk.core

import apper
from apper import AppObjects
from ..gltf.FusionGltfExporter import exportDesign, FusionGltfExporter


class ExportCommand(apper.Fusion360CommandBase):

    def __init__(self, name: str, options: dict):
        super().__init__(name, options)
        self.ao = AppObjects()
        self.exporter = FusionGltfExporter(self.ao)

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        self.exporter.exportDesignUI(self.ao.app.activeDocument)
