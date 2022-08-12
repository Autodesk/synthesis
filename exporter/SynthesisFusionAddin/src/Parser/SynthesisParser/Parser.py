import adsk.core, adsk.fusion
import traceback, gzip

from ...general_imports import *

from google.protobuf.json_format import MessageToJson

from proto.proto_out import assembly_pb2, types_pb2

from ...UI.Camera import captureThumbnail, clearIconCache

# from . import Joints, Materials, Components, Utilities

from . import Materials, Components, Joints, JointHierarchy, PDMessage

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
            fill_info(
                assembly_out,
                design.rootComponent,
                override_guid=design.parentDocument.name,
            )

            # set int to 0 in dropdown selection for dynamic
            assembly_out.dynamic = self.parseOptions.mode == 0

            # Physical Props here when ready

            # ts = time()

            progressDialog = app.userInterface.createProgressDialog()
            progressDialog.cancelButtonText = "Cancel"
            progressDialog.isBackgroundTranslucent = False
            progressDialog.isCancelButtonShown = True

            totalIterations = design.rootComponent.allOccurrences.count + 1

            progressDialog.title = "Exporting to Synthesis Format"
            progressDialog.minimumValue = 0
            progressDialog.maximumValue = totalIterations
            progressDialog.show(
                "Synthesis Export", "Currently on %v of %m", 0, totalIterations
            )

            # this is the formatter for the progress dialog now
            self.pdMessage = PDMessage.PDMessage(
                assembly_out.info.name,
                design.allComponents.count,
                design.rootComponent.allOccurrences.count,
                design.materials.count,
                design.appearances.count,  # this is very high for some reason
                progressDialog,
            )

            Materials._MapAllAppearances(
                design.appearances,
                assembly_out.data.materials,
                self.parseOptions,
                self.pdMessage,
            )

            Materials._MapAllPhysicalMaterials(
                design.materials,
                assembly_out.data.materials,
                self.parseOptions,
                self.pdMessage,
            )

            Components._MapAllComponents(
                design,
                self.parseOptions,
                self.pdMessage,
                assembly_out.data.parts,
                assembly_out.data.materials,
            )

            rootNode = types_pb2.Node()

            Components._ParseComponentRoot(
                design.rootComponent,
                self.pdMessage,
                self.parseOptions,
                assembly_out.data.parts,
                assembly_out.data.materials.appearances,
                rootNode,
            )

            Components._MapRigidGroups(design.rootComponent, assembly_out.data.joints)

            assembly_out.design_hierarchy.nodes.append(rootNode)

            # Problem Child
            Joints.populateJoints(
                design,
                assembly_out.data.joints,
                assembly_out.data.signals,
                self.pdMessage,
                self.parseOptions,
                assembly_out,
            )

            # add condition in here for advanced joints maybe idk
            # should pre-process to find if there are any grounded joints at all
            # that or add code to existing parser to determine leftovers

            Joints.createJointGraph(
                self.parseOptions.joints,
                self.parseOptions.wheels,
                assembly_out.joint_hierarchy,
                self.pdMessage,
            )

            JointHierarchy.BuildJointPartHierarchy(
                design, assembly_out.data.joints, self.parseOptions, self.pdMessage
            )

            # These don't have an effect, I forgot how this is suppose to work
            # progressDialog.message = "Taking Photo for thumbnail..."
            # progressDialog.title = "Finishing Export"
            self.pdMessage.currentMessage = "Taking Photo for Thumbnail..."
            self.pdMessage.update()

            # default image size
            imgSize = 250

            # Can only save, cannot get the bytes directly
            thumbnailLocation = captureThumbnail(imgSize)

            if thumbnailLocation != None:
                # Load bytes into memory and write them to proto
                binaryImage = None
                with open(thumbnailLocation, "rb") as in_file:
                    binaryImage = in_file.read()

                if binaryImage != None:
                    # change these settings in the captureThumbnail Function
                    assembly_out.thumbnail.width = imgSize
                    assembly_out.thumbnail.height = imgSize
                    assembly_out.thumbnail.transparent = True
                    assembly_out.thumbnail.data = binaryImage
                    assembly_out.thumbnail.extension = "png"
                    # clear the icon cache - src/resources/Icons
                    clearIconCache()

            self.pdMessage.currentMessage = "Compressing File..."
            self.pdMessage.update()

            if self.parseOptions.compress:
                self.logger.debug("Compressing file")
                with gzip.open(self.parseOptions.fileLocation, "wb", 9) as f:
                    self.pdMessage.currentMessage = "Saving File..."
                    self.pdMessage.update()
                    f.write(assembly_out.SerializeToString())
            else:
                f = open(self.parseOptions.fileLocation, "wb")
                f.write(assembly_out.SerializeToString())
                f.close()

            progressDialog.hide()

            if DEBUG:

                part_defs = assembly_out.data.parts.part_definitions
                parts = assembly_out.data.parts.part_instances
                joints = assembly_out.data.joints.joint_definitions
                signals = assembly_out.data.signals.signal_map

                joint_hierarchy_out = "Joint Hierarchy :\n"

                # This is just for testing
                for node in assembly_out.joint_hierarchy.nodes:
                    if node.value == "ground":
                        joint_hierarchy_out = f"{joint_hierarchy_out}  |- ground\n"
                    else:
                        newnode = assembly_out.data.joints.joint_instances[node.value]
                        jointdefinition = assembly_out.data.joints.joint_definitions[
                            newnode.joint_reference
                        ]

                        wheel_ = (
                            " wheel : true"
                            if (jointdefinition.user_data.data["wheel"] != "")
                            else ""
                        )

                        joint_hierarchy_out = f"{joint_hierarchy_out}  |- {jointdefinition.info.name} type: {jointdefinition.joint_motion_type} {wheel_}\n"

                    for child in node.children:
                        if child.value == "ground":
                            joint_hierarchy_out = f"{joint_hierarchy_out} |---> ground\n"
                        else:
                            newnode = assembly_out.data.joints.joint_instances[
                                child.value
                            ]
                            jointdefinition = assembly_out.data.joints.joint_definitions[
                                newnode.joint_reference
                            ]
                            wheel_ = (
                                " wheel : true"
                                if (jointdefinition.user_data.data["wheel"] != "")
                                else ""
                            )
                            joint_hierarchy_out = f"{joint_hierarchy_out}  |---> {jointdefinition.info.name} type: {jointdefinition.joint_motion_type} {wheel_}\n"

                joint_hierarchy_out += "\n\n"

                gm.ui.messageBox(
                    f"Appearances: {len(assembly_out.data.materials.appearances)} \nMaterials: {len(assembly_out.data.materials.physicalMaterials)} \nPart-Definitions: {len(part_defs)} \nParts: {len(parts)} \nSignals: {len(signals)} \nJoints: {len(joints)}\n {joint_hierarchy_out}",
                    "DEBUG - Fusion Synthesis",
                )

        except:
            self.logger.error("Failed:\n{}".format(traceback.format_exc()))

            if DEBUG:
                gm.ui.messageBox("Failed:\n{}".format(traceback.format_exc()))
            else:
                gm.ui.messageBox("An error occurred while exporting.")

            return False

        return True
