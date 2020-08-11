import time
import traceback
from io import BytesIO

import adsk.core
import adsk.fusion
from google.protobuf.json_format import MessageToDict

from apper import AppObjects, Fusion360Utilities
from .extras.ExportJoints import exportJoints
from .extras.ExportPhysicalProperties import exportPhysicalProperties, combinePhysicalProperties
from .utils.ByteUtils import *
from .utils.FusionUtils import *
from .utils.MathUtils import *
from .utils.PygltfUtils import *


def exportDesign(showFileDialog=False, enableMaterials=True, enableMaterialOverrides=True, enableFaceMaterials=True, exportVisibleBodiesOnly=True, fileType: FileType = FileType.GLB, quality="8"):
    try:
        ao = AppObjects()

        if ao.document.dataFile is None:
            ao.ui.messageBox("Export Cancelled: You must save your Fusion design before exporting.")
            return

        exporter = GLTFDesignExporter(ao, enableMaterials, enableMaterialOverrides, enableFaceMaterials, exportVisibleBodiesOnly, quality)
        if showFileDialog:
            dialog = ao.ui.createFileDialog()  # type: adsk.core.FileDialog
            dialog.filter = "glTF Binary (*.glb)" if fileType == FileType.GLB else "glTF JSON (*.gltf)"
            dialog.isMultiSelectEnabled = False
            dialog.title = "Select glTF Export Location"
            dialog.initialFilename = f'{ao.document.name.replace(" ", "_")}.{FileType.getExtension(fileType)}'
            results = dialog.showSave()
            if results != 0 and results != 2:  # For some reason the generated python API enums were wrong, so we're just using the literals
                ao.ui.messageBox(f"The glTF export was cancelled.")
                return
            filePath = dialog.filename

        else:
            filePath = f'C:/temp/{ao.document.name.replace(" ", "_")}_{int(time.time())}.glb'
        exportResults = exporter.saveGltf(filePath, fileType)
        if exportResults is None:
            ao.ui.messageBox(f"The glTF export was cancelled.")
            return
        warnings, modelStats, duration = exportResults

        finishedMessageDebug = (f"glTF export completed in {duration} seconds.\n"
                                f"File saved to {filePath}\n\n"
                                f"==== Model Stats ====\n"
                                f"{modelStats}\n"
                                f"==== Warnings ====\n"
                                f"{warnings}\n"
                                )
        print(finishedMessageDebug)
        finishedMessage = f"glTF export completed successfully in {duration} seconds.\nFile saved to: {filePath}"
        ao.ui.messageBox(finishedMessage, "Synthesis glTF Exporter")
    except:
        app = adsk.core.Application.get()
        ui = app.userInterface
        if ui:
            ui.messageBox(f'glTF export failed!\nPlease contact frc@autodesk.com to report this bug.\n\n{traceback.format_exc()}')


class GLTFDesignExporter(object):
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
    gltf: GLTF2

    # The glB format only allows one buffer to be embedded in the main file.
    # We'll call this buffer the primaryBuffer.
    primaryBufferIndex: int
    primaryBuffer: Buffer

    primaryBufferStream: BytesIO  # this memory stream will temporarily contain the binary data during the export, and will get written to chunk 1 of the glb file

    progressBar: adsk.core.ProgressDialog

    componentRevIdToMeshTemplate: Dict[FusionRevId, Mesh]  # dict for mesh caching, from component ids to generated meshes with the default materials
    componentRevIdToMatOverrideDict: Dict[FusionRevId, Dict[GLTFIndex, GLTFIndex]]  # dict for material overridden mesh caching, from gltf material override id to gltf mesh with that material override

    defaultAppearance: adsk.core.Appearance  # aluminum
    materialIdToGltfIndex: Dict[str, GLTFIndex]  # dict for gltf material caching, from fusion appearance name to material gltf index

    warnings: List[str]

    def __init__(self, ao: AppObjects, enableMaterials: bool = False, enableMaterialOverrides: bool = False, enableFaceMaterials: bool = False, exportVisibleBodiesOnly=True,
                 quality: str = "8"):  # todo: allow the export of designs besides the one in the foreground?
        self.exportVisibleBodiesOnly = exportVisibleBodiesOnly
        self.enableMaterials = enableMaterials
        self.enableMaterialOverrides = enableMaterialOverrides
        self.enableFaceMaterials = enableFaceMaterials
        self.ao = ao  # type: AppObjects
        self.meshQuality = int(quality)

        self.warnings = []

        self.gltf = GLTF2()  # The root glTF object.

        self.gltf.asset = Asset()
        self.gltf.asset.generator = self.GLTF_GENERATOR_ID

        self.gltf.scene = 0  # set the default scene to index 0

        # The glB format only allows one buffer to be embedded in the main file.
        # We'll call this buffer the primaryBuffer.
        self.gltf.buffers.append(Buffer())
        self.primaryBufferIndex = len(self.gltf.buffers) - 1

        # The actual binary data for the buffer will get stored in this memory stream
        self.primaryBufferStream = io.BytesIO()

        self.rootItemId = Fusion360Utilities.item_id(self.ao.design, self.GLTF_GENERATOR_ID)

        self.componentRevIdToMeshTemplate = {}
        self.componentRevIdToMatOverrideDict = {}
        self.materialIdToGltfIndex = {}

        self.usedCompIdMap = {}

        self.jointedOccurrencePaths = []

    def saveGltf(self, filepath: str, fileType: FileType):
        """
        Exports the current fusion document into a glb file.

        Args:
            filepath: The full path to the file to create
            useGlb: Export file type

        Returns: Performance logs

        """

        self.progressBar = self.ao.ui.createProgressDialog()
        self.progressBar.isCancelButtonShown = False
        self.progressBar.reset()
        self.progressBar.show(f"Exporting {self.ao.document.name} to glTF", "Preparing for export...", 0, 100, 0)

        # noinspection PyUnresolvedReferences
        adsk.doEvents()  # show progress bar

        if self.enableMaterials and checkIfAppearancesAreBugged(self.ao.design):
            result = self.ao.ui.messageBox(f"The materials on this design cannot be exported due to a bug in the Fusion 360 API.\n"
                                           f"Do you want to continue the export with materials turned off?\n"
                                           f"(press no to attempt material export, press cancel to cancel export)", "", adsk.core.MessageBoxButtonTypes.YesNoCancelButtonType)
            if result == 0 or result == 2:  # yes
                self.enableMaterials = False
            elif result == 3:  # no
                pass
            else:  # cancel
                return

        try:
            self.gltf.extras['joints'], self.jointedOccurrencePaths = exportJoints(self.ao.design.rootComponent.allJoints, self.GLTF_GENERATOR_ID, self.rootItemId, self.warnings)
        except RuntimeError:  # todo: report this bug
            result = self.ao.ui.messageBox(f"Could not export joints due to a bug in the Fusion API.\n"
                                           f"Do you want to continue the export without joints?", "", adsk.core.MessageBoxButtonTypes.YesNoButtonType)
            if result == 0 or result == 2:  # yes
                pass
            else:  # no
                return

        start = time.perf_counter()

        self.defaultAppearance = getDefaultAppearance(self.ao.app)

        self.gltf.scene = self.exportScene()  # export the current fusion document

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

        stats = (f"scenes: {len(self.gltf.scenes)}\n"
                 f"nodes: {len(self.gltf.nodes)}\n"
                 f"meshes: {len(self.gltf.meshes)}\n"
                 f"primitives: {sum([len(x.primitives) for x in self.gltf.meshes])}\n"
                 f"materials: {len(self.gltf.materials)}\n"
                 f"accessors: {len(self.gltf.accessors)}\n"
                 f"bufferViews: {len(self.gltf.bufferViews)}\n"
                 f"buffers: {len(self.gltf.buffers)}\n"
                 f"joints: {len(self.gltf.extras['joints']) if 'joints' in self.gltf.extras else 'could not export'}\n"
                 )

        if len(self.warnings) == 0:
            self.warnings.append("None :)")

        end = time.perf_counter()

        return self.warnings, stats, round(end - start, 4)

    def exportScene(self) -> GLTFIndex:
        """Exports the open fusion design to a glTF scene.
        
        Returns: The index of the exported scene in the glTF scenes list.

        """
        scene = Scene()

        # We export components into meshes first while recording a mapping from fusion component unique ids to glTF mesh indices so we can refer to mesh instances as we export the nodes.
        # This allows us to avoid serializing duplicate meshes for components which occur multiple times in one assembly, similar to fusion's system of components and occurrences.

        self.progressBar.message = "Reading list of fusion components..."
        self.progressBar.maximumValue = len(self.ao.design.allComponents) + 1

        rootNode = self.exportRootNode(self.ao.root_comp)
        if rootNode is not None:
            scene.nodes.append(rootNode)


        self.gltf.scenes.append(scene)
        return len(self.gltf.scenes) - 1

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
            node.extras["uuid"] = self.rootItemId

        if isEmptyLeafNode(node):
            return
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

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
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

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

        try:
            mesh.extras['physicalProperties'] = MessageToDict(combinePhysicalProperties([exportPhysicalProperties(bRepBody.physicalProperties) for bRepBody in bRepBodiesList]), including_default_value_fields=True)
        except:
            self.warnings.append(f"Unable to get physical properties for component {mesh.name}")

        revisionId = fusionComponent.revisionId
        self.componentRevIdToMeshTemplate[revisionId] = mesh
        self.componentRevIdToMatOverrideDict[revisionId] = {}

        # Progress bar
        numMeshes = len(self.componentRevIdToMeshTemplate)
        self.progressBar.progressValue = numMeshes
        self.progressBar.message = f"Calculating meshes for component {numMeshes} of {self.progressBar.maximumValue}..."

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

        if exportFacesTogether:  # todo add tag for meshes with no overridable materials which don't need to have multiple copies of the template
            return [self.exportPrimitiveBRepBodyOrFace(fusionBRepBody)]
        else:
            return [self.exportPrimitiveBRepBodyOrFace(face) for face in faces]

    def exportPrimitiveBRepBodyOrFace(self, fusionBRep: Union[adsk.fusion.BRepBody, adsk.fusion.BRepFace]) -> Optional[Primitive]:
        """Exports a glTF primitive from a fusion bRepBody or bRepFace using only one material.

        Args:
            fusionBRep: The fusion bRepBody or bRepFace to export.

        Returns: The glTF primitive object if the mesh was successfully exported.

        """
        primitive = Primitive()

        meshCalculator = fusionBRep.meshManager.createMeshCalculator()
        if meshCalculator is None:
            return None
        meshCalculator.setQuality(self.meshQuality)

        mesh = meshCalculator.calculate()

        if mesh is None:
            return None

        coords = mesh.nodeCoordinatesAsFloat
        indices = mesh.nodeIndices
        if len(indices) == 0 or len(coords) == 0:
            return None

        primitive.attributes = Attributes()
        # primitive.attributes.NORMAL = self.exportAccessor(mesh.normalVectorsAsFloat, DataType.Vec3, ComponentType.Float, True)  # Looks fine without normals
        primitive.attributes.POSITION = self.exportAccessor(coords, DataType.Vec3, ComponentType.Float, True)  # glTF requires limits for position coordinates.
        primitive.indices = self.exportAccessor(indices, DataType.Scalar, None, False)  # Autodetect component type on a per-mesh basis. glTF does not require limits for indices.

        if self.enableMaterials:
            primitive.material = self.exportMaterialFromAppearanceCached(fusionBRep.appearance)

            appearSourceType = fusionBRep.appearanceSourceType
            if appearSourceType != 1 and appearSourceType != 3: # not body or face, https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-908a0043-6f96-4f61-b541-b7585bd1f32e
                primitive.extras[self.MAT_OVERRIDEABLE_TAG] = True
        else:
            primitive.material = self.exportMaterialFromAppearanceCached(self.defaultAppearance)

        return primitive

    def exportAccessor(self, array: List[Union[int, float]], dataType: DataType, componentType: Optional[ComponentType], calculateLimits: bool) -> GLTFIndex:
        """Creates an accessor to store an array of data.

        Args:
            array: The array of data to store in a flat array, eg. [x, y, x, y] NOT [[x, y], [x, y]].
            dataType: The glTF data type to store.
            componentType: The glTF component type to store the data. Set to None to autodetect the smallest necessary unsigned integer type (for indices)
            calculateLimits: Whether or not it is necessary to calculate the limits of the data. If the componentType must be auto-detected, the limits will be calculated anyways.

        Returns: The index of the exported accessor in the glTF accessors list.

        """
        componentCount = len(array)  # the number of components, e.g. 12 floats
        componentsPerData = DataType.numElements(dataType)  # the number of components in one datum, e.g. 3 floats per Vec3
        dataCount = int(componentCount / componentsPerData)  # the number of data, e.g. 4 Vec3s

        assert componentCount != 0  # don't try to export an empty array
        assert dataCount * componentsPerData == componentCount  # componentsPerData should divide evenly

        accessor = Accessor()

        accessor.count = dataCount
        accessor.type = dataType


        if calculateLimits or componentType is None:  # must calculate limits if auto type detection is necessary
            accessor.max = tuple(max(array[i::componentsPerData]) for i in range(componentsPerData))  # tuple of max values in each position of the data type
            accessor.min = tuple(min(array[i::componentsPerData]) for i in range(componentsPerData))  # tuple of min values in each position of the data type

        alignBytesIOToBoundary(self.primaryBufferStream)  # buffers should start/end at aligned values for efficiency
        bufferByteOffset = calculateAlignment(self.primaryBufferStream.tell())  # start of the buffer to be created


        if componentType is None:
            # Auto detect smallest unsigned integer type.
            componentMin = min(accessor.min)
            componentMax = max(accessor.max)

            ubyteMin, ubyteMax = ComponentType.getValueLimits(ComponentType.UnsignedByte)
            ushortMin, ushortMax = ComponentType.getValueLimits(ComponentType.UnsignedShort)
            uintMin, uintMax = ComponentType.getValueLimits(ComponentType.UnsignedInt)

            if ubyteMin <= componentMin and componentMax < ubyteMax:
                componentType = ComponentType.UnsignedByte
            elif ushortMin <= componentMin and componentMax < ushortMax:
                componentType = ComponentType.UnsignedShort
            elif uintMin <= componentMin and componentMax < uintMax:
                componentType = ComponentType.UnsignedInt
            else:
                self.warnings.append(f"Failed to autodetect unsigned integer type for limits ({componentMin}, {componentMax})")


        # Pack the supplied data into the primary glB buffer stream.
        accessor.componentType = componentType
        packFormat = "<" + ComponentType.getTypeCode(componentType)
        for item in array:
            self.primaryBufferStream.write(struct.pack(packFormat, item))

        bufferByteLength = calculateAlignment(self.primaryBufferStream.tell() - bufferByteOffset)  # Calculate the length of the bufferView to be created.

        accessor.bufferView = self.exportBufferView(bufferByteOffset, bufferByteLength)  # Create the glTF bufferView object with the calculated start and length.

        self.gltf.accessors.append(accessor)
        return len(self.gltf.accessors) - 1

    def exportBufferView(self, byteOffset: int, byteLength: int) -> GLTFIndex:
        """Creates a glTF bufferView with the specified offset and length, referencing the default glB buffer.

        Args:
            byteOffset: Index of the starting byte in the referenced buffer.
            byteLength: Length in bytes of the bufferView.

        Returns: The index of the exported bufferView in the glTF bufferViews list.
        """
        bufferView = BufferView()
        bufferView.buffer = self.primaryBufferIndex  # index of the default glB buffer.
        bufferView.byteOffset = byteOffset
        bufferView.byteLength = byteLength

        self.gltf.bufferViews.append(bufferView)
        return len(self.gltf.bufferViews) - 1

    def exportMeshWithOverrideCached(self, fusionComponent: adsk.fusion.Component, overrideMatIndex: GLTFIndex) -> Optional[GLTFIndex]:
        """Makes a copy of a glTF mesh with the provided glTF override material, or returns a cached material-overridden mesh if one exists.

        This method requires the mesh template map (componentRevIdToMeshTemplate) to be filled.

        Args:
            fusionComponent: The fusion component which the mesh should be derived from.
            overrideMatIndex: The index in the glTF object's material array of the material the returned mesh should be overridden with.

        Returns: The index of the material-overridden mesh in the meshes list of the glTF object.

        """
        # If we've already created a gltf mesh with the same material override, just use that one
        if fusionComponent.revisionId not in self.componentRevIdToMeshTemplate:
            meshTemplate = self.exportMesh(fusionComponent)
        else:
            overrideDict = self.componentRevIdToMatOverrideDict[fusionComponent.revisionId]
            if overrideMatIndex in overrideDict:
                return overrideDict[overrideMatIndex]
            meshTemplate = self.componentRevIdToMeshTemplate.get(fusionComponent.revisionId, None)

        # Create a mesh with the material override from the template
        if meshTemplate is None:
            return
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
        self.gltf.meshes.append(mesh)
        return len(self.gltf.meshes) - 1

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
            materialIndex = self.materialIdToGltfIndex[materialId]
            if materialIndex == -1:  # was previously unable to export material
                return None
            return materialIndex  # appearance already exists in the glTF document

        materialIndex = self.exportMaterialFromAppearance(appearance)
        if materialIndex is None:
            self.materialIdToGltfIndex[materialId] = -1
            return None  # was unable to export material

        self.materialIdToGltfIndex[materialId] = materialIndex  # cache the appearance

        return materialIndex

    def exportMaterialFromAppearance(self, fusionAppearance: adsk.core.Appearance) -> Optional[GLTFIndex]:
        """Exports a glTF material from a fusion Appearance and add the exported material to the glTF object.

        Args:
            fusionAppearance: The fusion appearance to export.

        Returns: The index of the exported material in the materials list of the glTF object.

        """
        props = fusionAppearance.appearanceProperties

        material = Material()
        material.alphaCutoff = None  # this is a bug with the gltf python lib
        material.name = fusionAppearance.name

        pbr = PbrMetallicRoughness()
        pbr.baseColorFactor = None  # this is a bug with the gltf python lib
        pbr.metallicFactor = 0.0
        roughnessProp = props.itemById("surface_roughness")
        pbr.roughnessFactor = roughnessProp.value if roughnessProp is not None else 0.1

        baseColor = None

        modelItem = props.itemById("interior_model")
        if modelItem is None:
            return None
        matModelType = modelItem.value

        if matModelType == 0:  # Opaque
            baseColor = props.itemById("opaque_albedo").value
            if props.itemById("opaque_emission").value:
                material.emissiveFactor = fusionColorToRGBAArray(props.itemById("opaque_luminance_modifier").value)[:3]
        elif matModelType == 1:  # Metal
            pbr.metallicFactor = 1.0
            baseColor = props.itemById("metal_f0").value
        elif matModelType == 2:  # Layered
            baseColor = props.itemById("layered_diffuse").value
        elif matModelType == 3:  # Transparent
            baseColor = props.itemById("transparent_color").value
            material.alphaMode = "BLEND"
        elif matModelType == 5:  # Glazing
            baseColor = props.itemById("glazing_transmission_color").value
        else:  # ??? idk what type 4 material is
            self.warnings.append(f"Unsupported material modeling type: {material.name}")

        if baseColor is None:
            self.warnings.append(f"Ignoring material that does not have color: {material.name}")
            return None

        pbr.baseColorFactor = fusionColorToRGBAArray(baseColor)[:3] + [fusionAttenLengthToAlpha(props.itemById("transparent_distance"))]
        material.pbrMetallicRoughness = pbr

        self.gltf.materials.append(material)
        return len(self.gltf.materials) - 1


