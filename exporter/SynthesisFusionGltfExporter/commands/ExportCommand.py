import time

import adsk.core

import apper
from apper import AppObjects
from fusionutils.DebugHierarchy import *
from ..gltfutils.GLTFDesignExporter import GLTFDesignExporter
from .ExportPaletteCommand import exportDesign


class ExportCommand(apper.Fusion360CommandBase):

    def on_execute(self, command: adsk.core.Command, inputs: adsk.core.CommandInputs, args, input_values):
        exportDesign()
