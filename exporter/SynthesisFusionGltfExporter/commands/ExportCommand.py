import time

import adsk.core

import apper
from apper import AppObjects
from ..gltfutils.GLTFDesignExporter import exportDesign


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportDesign()
