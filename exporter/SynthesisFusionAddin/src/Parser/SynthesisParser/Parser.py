import adsk.core, adsk.fusion
import os, sys, math, uuid, traceback

from typing import List, Optional, Union, Tuple
from time import time

from ...general_imports import *

from proto.proto_out import assembly_pb2

# from . import Joints, Materials, Components, Utilities

from . import Materials, Components

from .Utilities import *


class Parser:
    """Parser for the Mira Buf format to use in AR or Simulation"""

    def __init__(self, options: any):
        """Creates a new parser with the supplied options

        Args:
            options (ParserOptions): parser options
        """
        self.parseOptions = options
        self.logger = logging.getLogger(f"{INTERNAL_ID}.Parser")

    def export(self) -> bool:
        try:
            app = adsk.core.Application.get()
            design = app.activeDocument.design

            assembly_out = assembly_pb2.Assembly()
            fill_info(assembly_out, design.rootComponent)
            # assembly_out.data = mirabuf_pb2.AssemblyData()
            assembly_out.dynamic = True
            # Physical Props here when ready
            # Leaf here when ready
            # Leaf here when ready

            ts = time()

            progressDialog = app.userInterface.createProgressDialog()
            progressDialog.cancelButtonText = "Cancel"
            progressDialog.isBackgroundTranslucent = False
            progressDialog.isCancelButtonShown = True

            totalIterations = design.rootComponent.allOccurrences.count + 1

            progressDialog.title = "Exporting to Synthesis Format"
            progressDialog.minimumValue = 0
            progressDialog.maximumValue = totalIterations
            progressDialog.show(
                "Synthesis Export Progress", "Currently on %v of %m", 0, totalIterations
            )

            Materials._MapAllAppearances(
                design.appearances,
                assembly_out.data.materials,
                self.parseOptions,
                progressDialog,
            )

            Components._MapAllComponents(
                design,
                self.parseOptions,
                progressDialog,
                assembly_out.data.parts,
                assembly_out.data.materials,
            )

            rootNode = types_pb2.Node()

            Components._ParseComponentRoot(
                design.rootComponent,
                progressDialog,
                self.parseOptions,
                assembly_out.data.parts,
                assembly_out.data.materials.materials,
                rootNode
            )

            assembly_out.design_hierarchy.nodes.append(rootNode)

            """

            Joints.ParseAllJoints(self.parseOptions, design, assembly_out.data)
            """

            f = open(self.parseOptions.fileLocation, "wb")
            f.write(assembly_out.SerializeToString())
            f.close()

            # Use websockets later ?

            progressDialog.hide()

            part_defs = assembly_out.data.parts.part_definitions
            parts = assembly_out.data.parts.part_instances

            gm.ui.messageBox(
                f"Materials: {len(assembly_out.data.materials.materials)}\nPart-Definitions: {len(part_defs)}\nParts: {len(parts)}"
            )

        except:
            self.logger.error("Failed:\n{}".format(traceback.format_exc()))
            return False

        return True
