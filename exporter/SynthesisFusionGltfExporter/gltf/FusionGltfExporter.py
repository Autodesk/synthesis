import time
import traceback

# noinspection PyUnresolvedReferences
import adsk.core
# noinspection PyUnresolvedReferences
import adsk.fusion
from google.protobuf.json_format import MessageToDict

# noinspection PyUnresolvedReferences
from apper import AppObjects, Fusion360Utilities
from .extras.ExportJoints import exportJoints
from .extras.ExportPhysicalProperties import exportPhysicalProperties, combinePhysicalProperties
from .utils.FusionToPygltfTranslation import *
from .utils.MathUtils import *
from .utils.PygltfUtils import *

from ..config import *

import platform

class ExportCancelledError(Exception):
    pass

class FusionGltfExporter(object):
    """Class for exporting fusion designs into the glTF binary file format, aka glB.

    You should create a new instance for EVERY export # todo: remove this requirement and add caching

    Fusion API objects are translated into glTF as follows:

    Fusion Design -> glTF scene
    Fusion Occurrence -> glTF node
    Fusion Component -> glTF mesh
    Fusion BRepBody -> glTF primitive
    Fusion MeshBody -> glTF primitive
    Fusion Appearances -> glTF materials
    Fusion Materials -> (contained in the 'extras' of glTF materials)
    Fusion Joints -> ??? glTF doesn't really have a concept of node motion around points that aren't nodes

    Attributes:
        gltf: Main glTF storage object, gets JSON serialized for export.
        primaryBuffer: The buffer referencing the one inline buffer in the final glB file.
        primaryBufferIndex: The index of the primaryBuffer in the glTF buffers list.
        primaryBufferStream: The memory stream for temporary storage of the glB buffer data.

    todo allow incremental export with one GLTFDesignExporter
    todo preserve material names even for materials that could not be exported (in case user wants to assign different material later)

    # xtodo rotation fix # update - looks like view cube orientation isn't accessible by the api https://forums.autodesk.com/t5/fusion-360-api-and-scripts/vieworientation-doesn-t-work-consistently-how-to-set-current/m-p/9464031/highlight/true#M9922
    """

    # types
    GLTFIndex = int
    FusionRevId = int

    # constants
    GLTF_GENERATOR_ID = "Autodesk.Synthesis.Fusion"
    MAT_OVERRIDEABLE_TAG = "fusExpMatOverrideable"
    DESIGN_SCALE = 0.01  # fusion uses cm, glTF uses meters, so scale the root transform matrix

    # fields
    ao: AppObjects

    rootNodeUUID: str
    jointedOccurrencePaths: List[str]

    gltf: GLTF2

    primaryBufferIndex: int
    primaryBuffer: Buffer
    primaryBufferStream: BytesIO  # this memory stream will temporarily contain the binary data during the export, and will get written to chunk 1 of the glb file

    componentRevIdToMeshTemplate: Dict[FusionRevId, Mesh]  # dict for mesh caching, from component ids to generated meshes with the default materials
    componentRevIdToMatOverrideDict: Dict[FusionRevId, Dict[GLTFIndex, GLTFIndex]]  # dict for material overridden mesh caching, from gltf material override id to gltf mesh with that material override

    materialIdToGltfIndex: Dict[str, GLTFIndex]  # dict for gltf material caching, from fusion appearance name to material gltf index
    defaultAppearance: adsk.core.Appearance  # aluminum

    # ui/debug fields
    progressBar: adsk.core.ProgressDialog
    exportWarnings: List[str]

    # settings
    includeSynthesisData: bool
    enableMaterials: bool
    enableMaterialOverrides: bool
    enableFaceMaterials: bool
    exportVisibleBodiesOnly: bool
    meshQuality: int

    def __init__(self, ao: AppObjects):  # todo: allow the export of designs besides the one in the foreground?
        self.ao = ao

        self.bRepIdToFaceMeshes = {}
        self.bRepIdToBodyMeshes = {}

    def exportDesignUI(self, document: adsk.core.Document, showFileDialog=False, enableMaterials=True, enableMaterialOverrides=True, enableFaceMaterials=True, exportVisibleBodiesOnly=True, fileType: FileType = FileType.GLB, quality: int = 8, includeSynthesisData=True):
        try:

            if document.dataFile is None:
                self.ao.ui.messageBox("Export Cancelled: You must save your Fusion design before exporting.")
                return

            if showFileDialog:
                dialog = self.ao.ui.createFileDialog()  # type: adsk.core.FileDialog
                dialog.filter = "glTF Binary (*.glb)" if fileType == FileType.GLB else "glTF JSON (*.gltf)"
                dialog.isMultiSelectEnabled = False
                dialog.title = "Select glTF Export Location"
                dialog.initialFilename = f'{document.name.replace(" ", "_")}.{FileType.getExtension(fileType)}'
                results = dialog.showSave()
                if results != 0 and results != 2:  # For some reason the generated python API enums were wrong, so we're just using the literals
                    self.ao.ui.messageBox(f"The glTF export was cancelled.")
                    return
                filePath = dialog.filename

            else:
                filePath = f'C:/temp/{document.name.replace(" ", "_")}_{int(time.time())}.glb'
            exportResults = self.saveGltf(document, filePath, fileType, enableMaterials, enableMaterialOverrides, enableFaceMaterials, exportVisibleBodiesOnly, quality, includeSynthesisData)
            if exportResults is None:
                self.ao.ui.messageBox(f"The glTF export was cancelled.")
                return
            exportWarnings, modelStats, duration = exportResults

            if len(exportWarnings) == 0:
                exportWarnings.append("None :)")

            modelStatsString = '\n'.join(modelStats)
            warningsString = '\n'.join(exportWarnings)
            finishedMessageDebug = (f"glTF export completed in {duration} seconds.\n"
                                    f"File saved to {filePath}\n\n"
                                    f"==== Model Stats ====\n"
                                    f"{modelStatsString}\n\n"
                                    f"==== Warnings ====\n"
                                    f"{warningsString}\n"
                                    )
            finishedMessage = (f"glTF export completed in {duration} seconds.\n"
                                    f"File saved to {filePath}\n\n"
                                    f"==== Warnings ====\n"
                                    f"{warningsString}\n"
                                    )
            print(finishedMessageDebug)
            self.ao.ui.messageBox(finishedMessageDebug if EXPORTER_DEBUG else finishedMessage, "Synthesis glTF Exporter")
        except ExportCancelledError:
            self.ao.ui.messageBox(f"The glTF export was cancelled.")
        except:
            # noinspection PyArgumentList
            app = adsk.core.Application.get()
            ui = app.userInterface
            if ui:
                ui.messageBox(f'glTF export failed!\nPlease contact frc@autodesk.com to report this bug.\n\n{traceback.format_exc()}')

    def saveGltf(self, document: adsk.core.Document, filepath: str, fileType: FileType, enableMaterials: bool = False, enableMaterialOverrides: bool = False, enableFaceMaterials: bool = False, exportVisibleBodiesOnly=True, meshQuality: int = 8, includeSynthesisData: bool = True):
        """
        Exports the current fusion document into a glb file.

        Args:
            filepath: The full path to the file to create
            fileType: Export file type

        Returns: Performance logs

        """
        self.includeSynthesisData = includeSynthesisData
        self.enableMaterials = enableMaterials
        self.enableMaterialOverrides = enableMaterialOverrides
        self.enableFaceMaterials = enableFaceMaterials
        self.exportVisibleBodiesOnly = exportVisibleBodiesOnly
        self.meshQuality = meshQuality

        self.gltf = GLTF2()  # The root glTF object.

        self.gltf.asset = Asset()
        systemStr = platform.system()
        self.gltf.asset.generator = f"{self.GLTF_GENERATOR_ID}.{systemStr}"

        self.gltf.scene = 0  # set the default scene to index 0

        # The glB format only allows one buffer to be embedded in the main file.
        # We'll call this buffer the primaryBuffer.
        self.primaryBufferIndex = appendGetIndex(self.gltf.buffers, Buffer())

        # The actual binary data for the buffer will get stored in this memory stream
        self.primaryBufferStream = io.BytesIO()

        self.rootNodeUUID = Fusion360Utilities.item_id(document.design, self.GLTF_GENERATOR_ID)

        self.componentRevIdToMeshTemplate = {}
        self.componentRevIdToMatOverrideDict = {}
        self.materialIdToGltfIndex = {}
        self.defaultAppearance = getDefaultAppearance(self.ao.app)

        self.jointedOccurrencePaths = []

        self.exportWarnings = []

        self.progressBar = self.ao.ui.createProgressDialog()
        self.progressBar.isCancelButtonShown = True
        self.progressBar.reset()
        self.progressBar.show(f"Exporting {document.name} to glTF", "Preparing for export...", 0, 100, 0)

        # noinspection PyUnresolvedReferences
        adsk.doEvents()  # show progress bar

        if not self.settingsPreCheck(document):
            self.progressBar.hide()
            return

        rootComponent = document.design.rootComponent  # type: adsk.fusion.Component

        if self.includeSynthesisData:
            try:
                self.gltf.extras['joints'], self.jointedOccurrencePaths = exportJoints(list(rootComponent.allJoints) + list(rootComponent.allAsBuiltJoints), self.GLTF_GENERATOR_ID, self.rootNodeUUID, self.exportWarnings)
            except RuntimeError:  # todo: report this bug
                print(traceback.format_exc())
                result = self.ao.ui.messageBox(f"Could not export joints due to a bug in the Fusion API.\n"
                                               f"Do you want to continue the export without joints?", "", adsk.core.MessageBoxButtonTypes.YesNoButtonType)
                if result == 0 or result == 2:  # yes
                    pass
                else:  # no
                    self.progressBar.hide()
                    return

        start = time.perf_counter()

        self.progressBar.message = "Reading list of fusion components..."
        self.progressBar.maximumValue = len(document.design.allComponents) + 1

        self.gltf.scene = self.exportScene(rootComponent)  # export the current fusion document

        # Clean tags
        for mesh in self.gltf.meshes:
            for prim in mesh.primitives:
                if self.MAT_OVERRIDEABLE_TAG in prim.extras:  # if material is overrideable
                    prim.extras.pop(self.MAT_OVERRIDEABLE_TAG)

        self.progressBar.message = "Serializing data to file..."
        self.progressBar.progressValue = self.progressBar.maximumValue - 1

        # convert gltf to json and serialize
        self.gltf.buffers[self.primaryBufferIndex].byteLength = calculateAlignment(self.primaryBufferStream.seek(0, io.SEEK_END))  # must calculate before encoding JSON

        # ==== do NOT make changes to the glTF object beyond this point ====
        # add padding bytes to the end of each chunk data
        alignBytesIOToBoundary(self.primaryBufferStream)

        # get the memoryView of the primary buffer stream
        primaryBufferData = self.primaryBufferStream.getbuffer()

        if fileType == FileType.GLB:
            writeGlbToFile(self.gltf, primaryBufferData, filepath)
        else:
            writeGltfToFile(self.gltf, primaryBufferData, filepath)

        self.progressBar.hide()

        stats = [f"scenes: {len(self.gltf.scenes)}",
                 f"nodes: {len(self.gltf.nodes)}",
                 f"meshes: {len(self.gltf.meshes)}",
                 f"primitives: {sum([len(x.primitives) for x in self.gltf.meshes])}",
                 f"materials: {len(self.gltf.materials)}",
                 f"accessors: {len(self.gltf.accessors)}",
                 f"bufferViews: {len(self.gltf.bufferViews)}",
                 f"buffers: {len(self.gltf.buffers)}",
                 f"joints: {len(self.gltf.extras['joints']) if 'joints' in self.gltf.extras else 0}",
                 ]

        end = time.perf_counter()
        exportTime = round(end - start, 4)

        return self.exportWarnings, stats, exportTime

    def settingsPreCheck(self, document: adsk.core.Document):
        """Validates user settings.

        Returns: True if the export should continue.

        """
        if self.enableMaterials and checkIfAppearancesAreBugged(document.design):
            result = self.ao.ui.messageBox(f"The materials on this design cannot be exported due to a bug in the Fusion 360 API.\n"
                                           f"Do you want to continue the export with materials turned off?\n"
                                           f"(press no to attempt material export, press cancel to cancel export)", "", adsk.core.MessageBoxButtonTypes.YesNoCancelButtonType)
            if result == 0 or result == 2:  # yes
                self.enableMaterials = False
            elif result == 3:  # no
                pass
            else:  # cancel
                return False

        return True

    def exportScene(self, rootComponent: adsk.fusion.Component) -> GLTFIndex:
        """Exports the open fusion design to a glTF scene.
        
        Returns: The index of the exported scene in the glTF scenes list.

        """
        scene = Scene()

        # We export components into meshes first while recording a mapping from fusion component unique ids to glTF mesh indices so we can refer to mesh instances as we export the nodes.
        # This allows us to avoid serializing duplicate meshes for components which occur multiple times in one assembly, similar to fusion's system of components and occurrences.

        rootNode = self.exportRootNode(rootComponent)
        if rootNode is not None:
            scene.nodes.append(rootNode)

        return appendGetIndex(self.gltf.scenes, scene)

    def exportRootNode(self, rootComponent: adsk.fusion.Component) -> Optional[GLTFIndex]:
        """Recursively exports the component hierarchy of the open fusion document to glTF nodes, starting with the root component.
        
        Args:
            rootComponent: The root component of the open fusion document.

        Returns: The index of the exported node in the glTF nodes list.

        """
        node = Node()
        node.name = rootComponent.name
        node.mesh = self.exportMeshWithOverrideCached(rootComponent, -1)
        node.scale = [self.DESIGN_SCALE] * 3
        node.children = [index for index in [self.exportNode(occur, -1) for occur in rootComponent.occurrences] if index is not None]

        if "" in self.jointedOccurrencePaths:
            node.extras["uuid"] = self.rootNodeUUID

        if isEmptyLeafNode(node):
            return
        return appendGetIndex(self.gltf.nodes, node)

    def exportNode(self, occur: adsk.fusion.Occurrence, overrideMatIndex: GLTFIndex) -> Optional[GLTFIndex]:
        """Recursively exports the component hierarchy of the open fusion document to glTF nodes, starting with an occurrence in the fusion hierarchy.

        Args:
            occur: An occurrence to be exported.
            overrideMatIndex: The current occurrence-level material override in effect.

        Returns: The index of the exported node in the glTF nodes list.
        """
        node = Node()
        node.name = occur.name

        if self.enableMaterials and self.enableMaterialOverrides:
            appearance = occur.appearance
            if appearance is not None:
                overrideMatIndex = self.exportMaterialFromAppearanceCached(appearance)

        node.matrix = notIdentityMatrix3DOrNone(transposeFlatMatrix3D(occur.transform.asArray()))  # transpose the flat 4x4 matrix3d

        if occur.isVisible or not self.exportVisibleBodiesOnly:
            node.mesh = self.exportMeshWithOverrideCached(occur.component, overrideMatIndex)

        if occur.fullPathName in self.jointedOccurrencePaths:
            node.extras["uuid"] = Fusion360Utilities.item_id(occur, self.GLTF_GENERATOR_ID)

        node.children = [index for index in [self.exportNode(occur, overrideMatIndex) for occur in occur.childOccurrences] if index is not None]

        if isEmptyLeafNode(node):
            return
        return appendGetIndex(self.gltf.nodes, node)


    def exportMeshWithOverrideCached(self, fusionComponent: adsk.fusion.Component, overrideMatIndex: GLTFIndex) -> Optional[GLTFIndex]:
        """Makes a copy of a glTF mesh with the provided glTF override material, or returns a cached material-overridden mesh if one exists.

        This method requires the mesh template map (componentRevIdToMeshTemplate) to be filled.

        Args:
            fusionComponent: The fusion component which the mesh should be derived from.
            overrideMatIndex: The index in the glTF object's material array of the material the returned mesh should be overridden with.

        Returns: The index of the material-overridden mesh in the meshes list of the glTF object.

        """
        # If we've already created a glTF mesh with the same material override, just use that one
        if fusionComponent.revisionId not in self.componentRevIdToMeshTemplate:
            meshTemplate = self.exportMesh(fusionComponent)
        else:
            overrideDict = self.componentRevIdToMatOverrideDict[fusionComponent.revisionId]
            if overrideMatIndex in overrideDict:
                return overrideDict[overrideMatIndex]
            meshTemplate = self.componentRevIdToMeshTemplate.get(fusionComponent.revisionId, None)

        # Create a mesh with the material override from the template
        if meshTemplate is None:
            return None
        newMeshIndex = self.exportMeshWithOverride(meshTemplate, overrideMatIndex)

        self.componentRevIdToMatOverrideDict[fusionComponent.revisionId][overrideMatIndex] = newMeshIndex
        return newMeshIndex

    def exportMeshWithOverride(self, templateMesh: Mesh, overrideMat: GLTFIndex) -> GLTFIndex:
        """Makes a copy of a glTF mesh with the provided glTF override material.

        Each primitive in the duplicated mesh will be colored with the provided override material as long as the material is allowed to be overridden (as defined by fusion).

        Args:
            templateMesh: The template mesh to duplicate.
            overrideMat: The material to apply to each material-overridable primitive in the duplicated mesh.

        Returns: The index of the material-overridden mesh in the meshes list of the glTF object.

        """
        if overrideMat == -1:  # caller wants mesh with no override material
            mesh = templateMesh  # just give them back the template
        else:  # caller wants mesh with override material
            # shallow copy the mesh but with the override material index for all overridable materials
            mesh = Mesh()
            mesh.name = templateMesh.name
            mesh.extras = templateMesh.extras
            mesh.extensions = templateMesh.extensions
            for oldPrim in templateMesh.primitives:
                prim = copy.copy(oldPrim)  # shallow copy, use same accessors
                if self.MAT_OVERRIDEABLE_TAG in prim.extras:  # if material is overrideable
                    prim.extras.pop(self.MAT_OVERRIDEABLE_TAG)
                    prim.material = overrideMat
                mesh.primitives.append(prim)
        return appendGetIndex(self.gltf.meshes, mesh)


    def exportMesh(self, fusionComponent: adsk.fusion.Component) -> Optional[Mesh]:
        """Exports a fusion component to a glTF mesh.

        Args:
            fusionComponent: The fusion component to export.

        Returns: The mesh that was exported.

        """
        bRepBodies = fusionComponent.bRepBodies
        if len(bRepBodies) == 0:
            return

        mesh = Mesh()
        mesh.name = fusionComponent.name

        bRepBodiesList = list(bRepBodies)  # Improve performance by only generating list from API once

        mesh.primitives = [prim for primList in [self.exportPrimitivesBRepBodyAutosplit(bRepBody) for bRepBody in bRepBodiesList] for prim in primList if prim is not None]
        if len(mesh.primitives) == 0:
            return

        if self.includeSynthesisData:
            try:
                mesh.extras['physicalProperties'] = MessageToDict(combinePhysicalProperties([exportPhysicalProperties(bRepBody.physicalProperties) for bRepBody in bRepBodiesList]), including_default_value_fields=True)
            except:
                self.exportWarnings.append(f"Unable to get physical properties for component {mesh.name}")

        revisionId = fusionComponent.revisionId
        self.componentRevIdToMeshTemplate[revisionId] = mesh
        self.componentRevIdToMatOverrideDict[revisionId] = {}

        # Progress bar
        numMeshes = len(self.componentRevIdToMeshTemplate)
        self.progressBar.progressValue = numMeshes
        self.progressBar.message = f"Calculating meshes for component {numMeshes} of {self.progressBar.maximumValue}..."

        if self.progressBar.wasCancelled:
            raise ExportCancelledError()

        return mesh

    def exportPrimitivesBRepBodyAutosplit(self, fusionBRepBody: adsk.fusion.BRepBody) -> List[Primitive]:
        """Exports a fusion bRep body to a list of glTF primitives, splitting the body into faces if face materials are enabled.

        Args:
            fusionBRepBody: The fusion bRep body to export.

        Returns: The exported primitives.

        """

        if not fusionBRepBody.isVisible and self.exportVisibleBodiesOnly:
            return []

        faces = []

        if self.enableMaterials and self.enableFaceMaterials:
            faces = list(fusionBRepBody.faces)  # type: List[adsk.fusion.BRepFace] # this api call is REALLY slow, which is why we go to the trouble of re-using this list
            exportFacesTogether = isSameMaterial(faces)  # split faces if faces have different materials assigned
        else:
            exportFacesTogether = True  # never split faces if face materials are off

        # todo add tag for meshes with no overridable materials which don't need to have multiple copies of the template
        return [self.exportPrimitiveBRepBodyOrFace(face) for face in self.calculateMeshBRepCached(fusionBRepBody, faces, exportFacesTogether)]

    def calculateMeshBRepCached(self, fusionBRepBody: adsk.fusion.BRepBody, faces: List[adsk.fusion.BRepFace], exportFacesTogether: bool) -> List[Tuple[adsk.fusion.TriangleMesh, Union[adsk.fusion.BRepFace, adsk.fusion.BRepBody]]]:
        bRepId = f"{fusionBRepBody.revisionId}-{str(self.meshQuality)}-{str(0 if exportFacesTogether else 1)}"  # TODO: this is a terrible way to include settings in the cache, come up with something better

        if bRepId in self.bRepIdToFaceMeshes:
            return self.bRepIdToFaceMeshes[bRepId]  # appearance already exists in the glTF document

        if exportFacesTogether:
            rawMeshes = [calculateMeshForBRep(fusionBRepBody, self.meshQuality)]
        else:
            rawMeshes = [calculateMeshForBRep(face, self.meshQuality) for face in faces]

        self.bRepIdToFaceMeshes[bRepId] = rawMeshes  # cache the appearance
        return rawMeshes

    def exportPrimitiveBRepBodyOrFace(self, rawMesh: Tuple[adsk.fusion.TriangleMesh, Union[adsk.fusion.BRepFace, adsk.fusion.BRepBody]]) -> Optional[Primitive]:
        """Exports a glTF primitive from a fusion bRepBody or bRepFace using only one material.

        Args:
            rawMesh: The fusion bRepBody or bRepFace to export.

        Returns: The glTF primitive object if the mesh was successfully exported.

        """
        primitive = Primitive()

        mesh, bodyOrFace = rawMesh

        if mesh is None:
            return None

        normals = mesh.normalVectorsAsFloat
        coords = mesh.nodeCoordinatesAsFloat
        indices = mesh.nodeIndices

        if len(indices) == 0 or len(coords) == 0:
            return None

        primitive.attributes = Attributes()
        primitive.attributes.NORMAL = exportAccessor(self.gltf, self.primaryBufferIndex, self.primaryBufferStream, normals, DataType.Vec3, ComponentType.Float, True, self.exportWarnings)  # Looks fine without normals
        primitive.attributes.POSITION = exportAccessor(self.gltf, self.primaryBufferIndex, self.primaryBufferStream, coords, DataType.Vec3, ComponentType.Float, True, self.exportWarnings)  # glTF requires limits for position coordinates.
        primitive.indices = exportAccessor(self.gltf, self.primaryBufferIndex, self.primaryBufferStream, indices, DataType.Scalar, None, False, self.exportWarnings)  # Autodetect component type on a per-mesh basis. glTF does not require limits for indices.

        if primitive.attributes.POSITION is None or primitive.indices is None:
            self.exportWarnings.append(f"Invalid mesh generated for a bRepBody or bRepFace")
            return None

        if self.enableMaterials:
            primitive.material = self.exportMaterialFromAppearanceCached(bodyOrFace.appearance)

            appearSourceType = bodyOrFace.appearanceSourceType
            if not (appearSourceType == 1 or appearSourceType == 3): # not body or face, https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-908a0043-6f96-4f61-b541-b7585bd1f32e
                primitive.extras[self.MAT_OVERRIDEABLE_TAG] = True
        else:
            primitive.material = self.exportMaterialFromAppearanceCached(self.defaultAppearance)

        return primitive


    def exportMaterialFromAppearanceCached(self, appearance: adsk.core.Appearance) -> Optional[GLTFIndex]:
        """Checks if the fusion Appearance already exists in a glTF document, exports the Appearance if it doesn't.

        Args:
            appearance:

        Returns: The index of the exported material in the materials list of the glTF object.

        """
        if appearance is None:  # body or face didn't come with an appearance
            return None

        materialId = appearance.id

        if materialId in self.materialIdToGltfIndex:
            return self.materialIdToGltfIndex[materialId]  # appearance already exists in the glTF document

        materialIndex = self.exportMaterialFromAppearance(appearance)
        self.materialIdToGltfIndex[materialId] = materialIndex  # cache the appearance
        return materialIndex

    def exportMaterialFromAppearance(self, fusionAppearance: adsk.core.Appearance) -> Optional[GLTFIndex]:
        """Exports a glTF material from a fusion Appearance and add the exported material to the glTF object.

        Args:
            fusionAppearance: The fusion appearance to export.

        Returns: The index of the exported material in the materials list of the glTF object.

        """
        material = fusionMatToGltf(fusionAppearance, self.exportWarnings)
        if material is None:
            return None
        return appendGetIndex(self.gltf.materials, material)
