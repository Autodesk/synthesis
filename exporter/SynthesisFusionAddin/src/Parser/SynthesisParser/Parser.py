import adsk.core, adsk.fusion
import os, sys, math, uuid, traceback

from typing import List, Optional, Union, Tuple
from time import time

from ...general_imports import *

from proto.proto_out import assembly_pb2

# from . import Joints, Materials, Components, Utilities

from . import Materials, Components, Joints, JointHierarchy

from .Utilities import *


class Parser:

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

            assembly_out.dynamic = True

            # Physical Props here when ready

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
                assembly_out.data.materials.appearances,
                rootNode
            )

            Joints.populateJoints(
                design,
                assembly_out.data.joints,
                progressDialog,
                self.parseOptions
            )

            Joints.createJointGraph(
                self.parseOptions.joints,
                assembly_out.joint_hierarchy
            )

            assembly_out.design_hierarchy.nodes.append(rootNode)

            f = open(self.parseOptions.fileLocation, "wb")
            f.write(assembly_out.SerializeToString())
            f.close()

            progressDialog.hide()

            part_defs = assembly_out.data.parts.part_definitions
            parts = assembly_out.data.parts.part_instances
            joints = assembly_out.data.joints.joint_definitions

            joint_hierarchy_out = "Joint Hierarchy :\n"

            for node in assembly_out.joint_hierarchy.nodes:
                if node.value == "ground":
                    joint_hierarchy_out = f"{joint_hierarchy_out}  |- ground\n"
                else:
                    joint_hierarchy_out = f"{joint_hierarchy_out}  |- {assembly_out.data.joints.joint_instances[node.value].info.name}\n"

                for child in node.children:
                    if child.value == "ground":
                        joint_hierarchy_out = f"{joint_hierarchy_out} |--- ground\n"
                    else:
                        joint_hierarchy_out = f"{joint_hierarchy_out}  |--- {assembly_out.data.joints.joint_instances[child.value].info.name}\n"

            joint_hierarchy_out += "\n\n"

            gm.ui.messageBox(
                f"Materials: {len(assembly_out.data.materials.appearances)} \nPart-Definitions: {len(part_defs)} \nParts: {len(parts)} \nJoints: {len(joints)}\n {joint_hierarchy_out}"
            )

        except:
            self.logger.error("Failed:\n{}".format(traceback.format_exc()))
            return False

        return True
