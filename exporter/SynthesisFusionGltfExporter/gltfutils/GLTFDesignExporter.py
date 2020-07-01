import io
import time
import struct
from io import BytesIO

from typing import Dict, List, BinaryIO
from typing import Optional, Union

import copy

import adsk
import adsk.core
import adsk.fusion

from pygltflib import GLTF2, Asset, Scene, Node, Mesh, Primitive, Attributes, Accessor, BufferView, Buffer, Material, PbrMetallicRoughness
from apper import AppObjects
from pyutils.counters import EventCounter
from pyutils.timers import SegmentedStopwatch

from gltfutils.GLTFConstants import ComponentType, DataType

from ..proto.synthesis_importbuf_pb2 import Joint

from google.protobuf.json_format import MessageToDict


class GLTFDesignExporter(object):  # todo: can this exporter be made generic (non fusion specific?)
    """Class for exporting fusion designs into the glTF binary file format, aka glB.

    Fusion API objects are translated into glTF as follows:

    Fusion Design -> glTF scene
    Fusion Occurrence -> glTF node
    Fusion Component -> glTF mesh
    Fusion BRepBody -> glTF primitive
    Fusion MeshBody -> glTF primitive
    Fusion Appearances -> glTF materials
    Fusion Materials -> (contained in the 'extras' of glTF materials)
    Fusion Joints -> ??? glTF doesn't really have a concept of node motion around points that aren't nodes todo

    Attributes:
        gltf: Main glTF storage object, gets JSON serialized for export.
        primaryBuffer: The buffer referencing the one inline buffer in the final glB file.
        primaryBufferId: The index of the primaryBuffer in the glTF buffers list.
        primaryBufferStream: The memory stream for temporary storage of the glB buffer data.

    todo allow multiple exports (incremental) with one GLTFDesignExporter

    # todo rotation fix # update - looks like view cube orientation isn't accessible by the api https://forums.autodesk.com/t5/fusion-360-api-and-scripts/vieworientation-doesn-t-work-consistently-how-to-set-current/m-p/9464031/highlight/true#M9922
    """

    # types
    GLTFIndex = int
    FusionRevId = int
    FusionMatName = int

    # constants
    GLTF_VERSION = 2
    GLB_HEADER_SIZE = 12
    GLB_CHUNK_SIZE = 8
    IDENTITY_MATRIX_3D = (
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1,
    )
    MAT_OVERRIDEABLE_TAG = "fusExpMatOverrideable"

    # fields
    gltf: GLTF2

    primaryBufferId: int
    primaryBuffer: Buffer
    primaryBufferStream: BytesIO

    progressBar: adsk.core.ProgressDialog

    componentRevIdToMeshTemplate: Dict[FusionRevId, Mesh]
    componentRevIdToMatOverrideDict: Dict[FusionRevId, Dict[GLTFIndex, GLTFIndex]]  # dict from gltf material override id to gltf mesh with that material override

    defaultAppearance: adsk.core.Appearance
    materialNameToGltfIndex: Dict[FusionMatName, GLTFIndex]

    warnings: List[str]

    @classmethod
    def isIdentityMatrix3D(cls, matrix: List[float], tolerance: float = 0.00001) -> bool:
        """Determines whether the input matrix is equal to the 4x4 identity matrix.

        Args:
            matrix: The flat Matrix3D to compare.
            tolerance: The maximum distance from the true identity matrix to tolerate.


        Returns: True if the given matrix is equal to the identity matrix within the tolerance.
        """
        for i in range(len(cls.IDENTITY_MATRIX_3D)):

            if abs(matrix[i] - cls.IDENTITY_MATRIX_3D[i]) > tolerance:
                return False
        return True

    @classmethod
    def calculateAlignmentNumPadding(cls, currentSize: int, byteAlignment: int = 4) -> int:
        """Calculates the number of bytes needed to pad a data structure to the given alignment.

        Args:
            currentSize: The current size of the data structure that needs alignment.
            byteAlignment: The required byte alignment, e.g. 4 for 32 bit alignment.

        Returns: The number of bytes of padding which need to be added to the end of the structure.
        """
        if currentSize % byteAlignment == 0:
            return 0
        return byteAlignment - currentSize % byteAlignment

    @classmethod
    def calculateAlignment(cls, currentSize: int, byteAlignment: int = 4) -> int:
        """Calculate the new length of the a data structure after it has been aligned.

        Args:
            currentSize: The current length of the data structure in bytes.
            byteAlignment: The required byte alignment, e.g. 4 for 32 bit alignment.

        Returns: The new length of the data structure in bytes.
        """
        return currentSize + cls.calculateAlignmentNumPadding(currentSize, byteAlignment)

    @classmethod
    def alignBytesIOToBoundary(cls, stream: io.BytesIO, byteAlignment: int = 4) -> None:
        stream.write(b'\x00\x00\x00'[0:cls.calculateAlignmentNumPadding(stream.seek(0, io.SEEK_END), byteAlignment)])
        assert stream.tell() % byteAlignment == 0

    @classmethod
    def alignByteArrayToBoundary(cls, byteArray: bytearray, byteAlignment: int = 4) -> None:
        byteArray.extend(b'   '[0:cls.calculateAlignmentNumPadding(len(byteArray), byteAlignment)])
        assert len(byteArray) % byteAlignment == 0

    @classmethod
    def colorByteToFloat(cls, byte: int) -> float:
        return byte / 255

    @classmethod
    def fusionColorToArray(cls, color: adsk.core.Color) -> List[float]:
        return [
            cls.colorByteToFloat(color.red),
            cls.colorByteToFloat(color.green),
            cls.colorByteToFloat(color.blue),
            cls.colorByteToFloat(color.opacity),
        ]

    @classmethod
    def fusionAttenLengthToAlpha(cls, attenLength: adsk.core.FloatProperty) -> float:
        if attenLength is None:
            return 1
        return max(min((464 - 7 * attenLength.value) / 1938, 1), 0.03)  # todo: this conversion is just made up, figure out an accurate one

    @classmethod
    def canExportFacesTogether(cls, faces: List[adsk.fusion.BRepFace]):
        # if all faces use the same material
        materialName = faces[0].appearance.name
        for face in faces[1:]:
            if face.appearance.name != materialName:
                return False
        return True

    def __init__(self, ao: AppObjects, enableMaterials: bool = False, enableMaterialOverrides: bool = False, enableFaceMaterials: bool = False, exportVisibleBodiesOnly = True):  # todo: allow the export of designs besides the one in the foreground?
        self.exportVisibleBodiesOnly = exportVisibleBodiesOnly
        self.enableMaterials = enableMaterials
        self.enableMaterialOverrides = enableMaterialOverrides
        self.enableFaceMaterials = enableFaceMaterials
        self.ao = ao

        self.warnings = []

        # These stopwatches are for export performance testing only.
        self.perfWatch = SegmentedStopwatch(time.perf_counter)
        self.bufferWatch = SegmentedStopwatch(time.perf_counter)
        self.eventCounter = EventCounter()

        self.gltf = GLTF2()  # The root glTF object.

        self.gltf.asset = Asset()
        self.gltf.asset.generator = "Fusion 360 exporter for Synthesis"

        self.gltf.scene = 0  # set the default scene to index 0

        # The glB format only allows one buffer to be embedded in the main file.
        # We'll call this buffer the primaryBuffer.
        self.primaryBuffer = Buffer()
        self.primaryBufferId = len(self.gltf.buffers)
        self.gltf.buffers.append(self.primaryBuffer)

        # The actual binary data for the buffer will get stored in this memory stream
        self.primaryBufferStream = io.BytesIO()

    def saveGLB(self, filepath: str):
        """
        Exports the current fusion document into a glb file.

        Args:
            filepath: The full path to the file to create

        Returns: Performance logs

        """

        if self.checkIfAppearancesAreBugged():
            result = self.ao.ui.messageBox(f"The materials on this design cannot be exported due to a bug in the Fusion 360 API.\n"
                                        f"Do you want to continue the export with materials turned off?", "", adsk.core.MessageBoxButtonTypes.YesNoButtonType)
            if result != adsk.core.DialogResults.DialogYes:
                return None
            self.enableMaterials = False

        self.progressBar = self.ao.ui.createProgressDialog()
        self.progressBar.isCancelButtonShown = False

        self.progressBar.reset()
        self.progressBar.show(f"Exporting {self.ao.document.name} to GLB", "Waiting for save path selection...", 0, 100, 0)

        self.defaultAppearance = self.getDefaultAppearance()

        self.gltf.scene = self.exportScene()  # export the current fusion document

        self.progressBar.message = "Serializing data to file..."
        self.progressBar.progressValue+=1

        self.perfWatch.switch_segment('encoding and file writing')

        # convert gltf to json and serialize
        self.primaryBuffer.byteLength = self.calculateAlignment(self.primaryBufferStream.seek(0, io.SEEK_END))  # must calculate before encoding JSON

        # ==== do NOT make changes to the glTF object beyond this point ====
        jsonBytes = bytearray(self.gltf.gltf_to_json().encode("utf-8"))  # type: bytearray

        # add padding bytes to the end of each chunk data
        self.alignByteArrayToBoundary(jsonBytes)
        self.alignBytesIOToBoundary(self.primaryBufferStream)

        # get the memoryView of the primary buffer stream
        primaryBufferData = self.primaryBufferStream.getbuffer()

        glbLength = self.GLB_HEADER_SIZE + \
                    self.GLB_CHUNK_SIZE + len(jsonBytes) + \
                    self.GLB_CHUNK_SIZE + len(primaryBufferData)

        with open(filepath, 'wb') as stream:
            # header
            stream.write(b'glTF')  # magic
            stream.write(struct.pack('<I', self.GLTF_VERSION))  # version
            stream.write(struct.pack('<I', glbLength))  # length

            # json chunk
            stream.write(struct.pack('<I', len(jsonBytes)))  # chunk length
            stream.write(bytes("JSON", 'utf-8'))  # chunk type
            stream.write(jsonBytes)  # chunk data

            # buffer chunk
            stream.write(struct.pack('<I', len(primaryBufferData)))  # chunk length
            stream.write(bytes("BIN\x00", 'utf-8'))  # chunk type
            # noinspection PyTypeChecker
            stream.write(primaryBufferData)  # chunk data

            stream.flush()  # flush to file

            self.perfWatch.stop()

        self.progressBar.hide()

        # self.warnings.append(str(self.componentRevIdToMatOverrideDict))

        stats = (f"scenes: {len(self.gltf.scenes)}\n"
                 f"nodes: {len(self.gltf.nodes)}\n"
                 f"meshes: {len(self.gltf.meshes)}\n"
                 f"primitives: {sum([len(x.primitives) for x in self.gltf.meshes])}\n"
                 f"materials: {len(self.gltf.materials)}\n"
                 f"accessors: {len(self.gltf.accessors)}\n"
                 f"bufferViews: {len(self.gltf.bufferViews)}\n"
                 f"buffers: {len(self.gltf.buffers)}\n"
                 )

        if len(self.warnings) == 0:
            self.warnings.append("None :)")
        return str(self.perfWatch), str(self.bufferWatch), "\n".join(self.warnings), stats, str(self.eventCounter)

    def exportScene(self) -> GLTFIndex:
        """Exports the open fusion design to a glTF scene.
        
        Returns: The index of the exported scene in the glTF scenes list.
        """
        scene = Scene()

        # We export components into meshes first while recording a mapping from fusion component unique ids to glTF mesh indices so we can refer to mesh instances as we export the nodes.
        # This allows us to avoid serializing duplicate meshes for components which occur multiple times in one assembly, similar to fusion's system of components and occurrences.

        # self.deDuplicateAppearanceNames()

        self.exportMeshes(self.ao.design.allComponents)

        self.perfWatch.switch_segment('exporting node tree')
        self.progressBar.message = "Exporting occurrence tree..."
        self.progressBar.progressValue+=1
        scene.nodes.append(self.exportRootNode(self.ao.root_comp))
        self.perfWatch.stop()

        scene.extras['joints'] = self.exportJoints(self.ao.design.rootComponent.allJoints)

        self.gltf.scenes.append(scene)
        return len(self.gltf.scenes) - 1

    def exportRootNode(self, rootComponent: adsk.fusion.Component) -> GLTFIndex:
        """Recursively exports the component hierarchy of the open fusion document to glTF nodes, starting with the root component.
        
        Args:
            rootComponent: The root component of the open fusion document.

        Returns: The index of the exported node in the glTF nodes list.
        """
        node = Node()
        node.name = rootComponent.name

        node.mesh = self.exportMeshWithOverrideCached(rootComponent.revisionId, -1)

        scale = 0.01 # fusion uses cm, glTF uses meters, so scale the root transform matrix
        node.matrix = [
            scale, 0, 0, 0,
            0, scale, 0, 0,
            0, 0, scale, 0,
            0, 0, 0, 1,
        ]

        node.children = [self.exportNode(occur, -1) for occur in rootComponent.occurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportNode(self, occur: adsk.fusion.Occurrence, materialOverride: GLTFIndex) -> GLTFIndex:
        """Recursively exports the component hierarchy of the open fusion document to glTF nodes, starting with an occurrence in the fusion hierarchy.

        Args:
            occur: An occurrence to be exported.
            materialOverride: The current occurrence-level material override in effect.

        Returns: The index of the exported node in the glTF nodes list.
        """
        node = Node()
        node.name = occur.name

        if self.enableMaterials and self.enableMaterialOverrides:
            appearance = occur.appearance
            if appearance is not None:
                self.eventCounter.event("got override material")
                materialOverride = self.exportMaterialFromAppearanceCached(appearance)

        flatMatrix = occur.transform.asArray()
        if not self.isIdentityMatrix3D(flatMatrix):
            node.matrix = [flatMatrix[i + j * 4] for i in range(4) for j in range(4)]  # transpose the flat 4x4 matrix3d

        if not self.exportVisibleBodiesOnly or occur.isVisible:
            node.mesh = self.exportMeshWithOverrideCached(occur.component.revisionId, materialOverride)
        # node.extras['isGrounded'] = occur.isGrounded

        node.children = [self.exportNode(occur, materialOverride) for occur in occur.childOccurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportMeshes(self, fusionComponents: List[adsk.fusion.Component]) -> None:
        """Exports a list of fusion components to glTF meshes.

        Args:
            fusionComponents: The list of all unique fusion components in the open document.

        Returns: A mapping from unique ids of fusion components to their index in the glTF mesh list.
        """
        self.componentRevIdToMeshTemplate = {}
        self.componentRevIdToMatOverrideDict = {}
        self.materialNameToGltfIndex = {}

        self.perfWatch.switch_segment("reading list of fusion components")
        self.progressBar.message = "Reading list of fusion components..."
        fusionComponents = list(fusionComponents)
        numComponents = len(fusionComponents)
        self.progressBar.maximumValue = numComponents + 2
        self.perfWatch.stop()

        for index, fusionComponent in enumerate(fusionComponents):  # accessing the list of components is slow, ~1sec/500 components
            self.progressBar.message = f"Calculating meshes for component {index} of {numComponents}..."
            self.progressBar.progressValue = index
            self.exportMesh(fusionComponent)

    def exportMesh(self, fusionComponent: adsk.fusion.Component) -> None:
        """Exports a fusion component to a glTF mesh.

        Args:
            fusionComponent: The fusion component to export.

        Returns: The index of the exported mesh in the glTF mesh list.
        """
        bodies = fusionComponent.bRepBodies
        if len(bodies) == 0:
            return

        revisionId = fusionComponent.revisionId

        mesh = Mesh()
        mesh.name = fusionComponent.name
        # mesh.extras['description'] = fusionComponent.description
        # mesh.extras['revisionId'] = revisionId
        # mesh.extras['partNumber'] = fusionComponent.partNumber
        # protoComponent.materialId = fusionComponent.material.id
        # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

        self.perfWatch.switch_segment("reading list of brepbodies")
        bodyList = list(bodies)
        self.perfWatch.stop()

        mesh.primitives = [prim for primList in [self.exportPrimitiveBRepBodyAutosplit(bRepBody) for bRepBody in bodyList] for prim in primList if prim is not None]

        if len(mesh.primitives) == 0:
            return

        self.componentRevIdToMeshTemplate[revisionId] = mesh
        self.componentRevIdToMatOverrideDict[revisionId] = {}

    def exportPrimitiveBRepBodyAutosplit(self, fusionBRepBody: adsk.fusion.BRepBody) -> List[Primitive]:
        """Exports a fusion bRep body to a glTF primitive.

        Args:
            fusionBRepBody: The fusion bRep body to export.

        Returns: The exported primitive object.
        """

        # primitive.extras['name'] = fusionBRepBody.name
        # protoMeshBody.appearanceId = fusionBRepBody.appearance.id
        # protoMeshBody.materialId = fusionBRepBody.material.id
        # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)

        if self.exportVisibleBodiesOnly and not fusionBRepBody.isVisible:
            self.eventCounter.event("ignored invisible brepbody")
            return []

        self.perfWatch.switch_segment('deciding whether to separate faces')

        faces = []

        if self.enableMaterials and self.enableFaceMaterials:
            if len(fusionBRepBody.faces) == 0:
                return []
            faces = list(fusionBRepBody.faces)  # type: List[adsk.fusion.BRepFace] # this is REALLY slow
            if self.canExportFacesTogether(faces):
                exportFacesTogether = True
            else:
                exportFacesTogether = False
        else:
            exportFacesTogether = True

        self.perfWatch.stop()

        if exportFacesTogether:  # todo add tag for meshes with no overridable materials which don't need to have multiple copies of the template
            self.eventCounter.event("exported brepbody as one mesh")
            return [self.exportPrimitiveBRepBodyOrFace(fusionBRepBody)]
        else:
            self.eventCounter.event("exported brepbody as split faces")
            return [self.exportPrimitiveBRepBodyOrFace(face) for face in faces]

    def exportPrimitiveBRepBodyOrFace(self, fusionBRep: Union[adsk.fusion.BRepBody, adsk.fusion.BRepFace]) -> Optional[Primitive]:
        primitive = Primitive()

        meshCalculator = fusionBRep.meshManager.createMeshCalculator()
        if meshCalculator is None:
            return None
        meshCalculator.setQuality(11)  # todo mesh quality settings

        self.perfWatch.switch_segment('calculating meshes')
        mesh = meshCalculator.calculate()
        self.perfWatch.stop()

        if mesh is None:
            return None

        coords = mesh.nodeCoordinatesAsFloat
        indices = mesh.nodeIndices
        if len(indices) == 0 or len(coords) == 0:
            return None

        self.perfWatch.switch_segment('writing mesh data to buffer')

        primitive.attributes = Attributes()
        # primitive.attributes.NORMAL = self.exportAccessor(mesh.normalVectorsAsFloat, DataType.Vec3, ComponentType.Float, True)
        primitive.attributes.POSITION = self.exportAccessor(coords, DataType.Vec3, ComponentType.Float, True)  # glTF requires limits for position coordinates.
        primitive.indices = self.exportAccessor(indices, DataType.Scalar, None, False)  # Autodetect component type on a per-mesh basis. glTF does not require limits for indices.

        self.perfWatch.stop()

        if self.enableMaterials:
            self.perfWatch.switch_segment('exporting materials')

            materialIndex = self.exportMaterialFromAppearanceCached(fusionBRep.appearance)
            primitive.material = materialIndex # doesn't matter if it's None

            appearSourceType = fusionBRep.appearanceSourceType
            if appearSourceType != 1 and appearSourceType != 3:
                self.eventCounter.event("found prim with overridable material")
                primitive.extras[self.MAT_OVERRIDEABLE_TAG] = True

            self.perfWatch.stop()
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

        Returns:
            GLTFIndex: The index of the exported accessor in the glTF accessors list.
        """
        componentCount = len(array)  # the number of components, e.g. 12 floats
        componentsPerData = DataType.num_elements(dataType)  # the number of components in one datum, e.g. 3 floats per Vec3
        dataCount = int(componentCount / componentsPerData)  # the number of data, e.g. 4 Vec3s

        assert componentCount != 0  # don't try to export an empty array
        assert dataCount * componentsPerData == componentCount  # componentsPerData should divide evenly

        accessor = Accessor()

        accessor.count = dataCount
        accessor.type = dataType

        self.bufferWatch.switch_segment("calculating min/max")

        if calculateLimits or componentType is None:  # must calculate limits if auto type detection is necessary
            accessor.max = tuple(max(array[i::componentsPerData]) for i in range(componentsPerData))  # tuple of max values in each position of the data type
            accessor.min = tuple(min(array[i::componentsPerData]) for i in range(componentsPerData))  # tuple of min values in each position of the data type

        self.alignBytesIOToBoundary(self.primaryBufferStream)  # buffers should start/end at aligned values for efficiency
        bufferByteOffset = self.calculateAlignment(self.primaryBufferStream.tell())  # start of the buffer to be created

        self.bufferWatch.stop()

        if componentType is None:
            # Auto detect smallest unsigned integer type.
            componentMin = min(accessor.min)
            componentMax = max(accessor.max)

            ubyteMin, ubyteMax = ComponentType.get_limits(ComponentType.UnsignedByte)
            ushortMin, ushortMax = ComponentType.get_limits(ComponentType.UnsignedShort)
            uintMin, uintMax = ComponentType.get_limits(ComponentType.UnsignedInt)

            if ubyteMin <= componentMin and componentMax < ubyteMax:
                componentType = ComponentType.UnsignedByte
            elif ushortMin <= componentMin and componentMax < ushortMax:
                componentType = ComponentType.UnsignedShort
            elif uintMin <= componentMin and componentMax < uintMax:
                componentType = ComponentType.UnsignedInt
            else:
                self.warnings.append(f"Failed to autodetect unsigned integer type for limits ({componentMin}, {componentMax})")

            self.bufferWatch.switch_segment(f"component type detected: {str(componentType).split('.')[1]}")
            self.bufferWatch.stop()

        self.bufferWatch.switch_segment("writing to stream")

        # Pack the supplied data into the primary glB buffer stream.
        accessor.componentType = componentType
        packFormat = "<" + ComponentType.to_type_code(componentType)
        for item in array:
            self.primaryBufferStream.write(struct.pack(packFormat, item))

        self.bufferWatch.stop()

        bufferByteLength = self.calculateAlignment(self.primaryBufferStream.tell() - bufferByteOffset)  # Calculate the length of the bufferView to be created.

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
        bufferView.buffer = self.primaryBufferId  # index of the default glB buffer.
        bufferView.byteOffset = byteOffset
        bufferView.byteLength = byteLength

        self.gltf.bufferViews.append(bufferView)
        return len(self.gltf.bufferViews) - 1

    def exportMeshWithOverrideCached(self, componentRevId: FusionRevId, overrideMatIndex: GLTFIndex) -> Optional[GLTFIndex]:
        """Makes a copy of a glTF mesh with the provided glTF override material, or returns a cached material-overridden mesh if one exists.

        This method requires the mesh template map (componentRevIdToMeshTemplate) to be filled.

        Args:
            componentRevId: The revision id of the fusion component which the mesh should be
            overrideMatIndex: The index in the glTF object's material array of the material the returned mesh should be overridden with.

        Returns: The index of the material-overridden mesh in the meshes list of the glTF object.

        """
        # If we've already created a gltf mesh with the same material override, just use that one
        if componentRevId not in self.componentRevIdToMeshTemplate:
            return None  # mesh was empty
        overrideDict = self.componentRevIdToMatOverrideDict[componentRevId]
        if overrideMatIndex in overrideDict:
            self.eventCounter.event("used cached mesh with materials")
            return overrideDict[overrideMatIndex]

        meshTemplate = self.componentRevIdToMeshTemplate.get(componentRevId, None)
        assert meshTemplate is not None
        newMeshIndex = self.exportMeshWithOverride(meshTemplate, overrideMatIndex)

        overrideDict[overrideMatIndex] = newMeshIndex
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
            self.eventCounter.event("copied mesh without override")
            mesh = templateMesh  # just give them back the template
        else:  # caller wants mesh with override material
            self.eventCounter.event("copied mesh with override")
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
                    self.eventCounter.event("applied override material to prim")
                mesh.primitives.append(prim)
        self.gltf.meshes.append(mesh)
        return len(self.gltf.meshes) - 1

    def exportMaterialFromAppearanceCached(self, appearance: adsk.core.Appearance) -> Optional[GLTFIndex]:
        if appearance is None:  # body or face didn't come with an appearance
            return None

        materialName = appearance.name
        if materialName in self.materialNameToGltfIndex:
            materialIndex = self.materialNameToGltfIndex[materialName]
            if materialIndex == -1:  # was previously unable to export material
                return None
            self.eventCounter.event("used cached material")
            return materialIndex

        materialIndex = self.exportMaterialFromAppearance(appearance)
        if materialIndex is None:
            self.materialNameToGltfIndex[materialName] = -1  # was unable to export material
            return None

        self.materialNameToGltfIndex[materialName] = materialIndex

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
                material.emissiveFactor = self.fusionColorToArray(props.itemById("opaque_luminance_modifier").value)[:3]
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

        pbr.baseColorFactor = self.fusionColorToArray(baseColor)[:3] + [self.fusionAttenLengthToAlpha(props.itemById("transparent_distance"))]
        material.pbrMetallicRoughness = pbr

        self.gltf.materials.append(material)
        return len(self.gltf.materials) - 1

    def checkIfAppearancesAreBugged(self) -> bool:
        """Checks if the appearances of a fusion document are bugged.

        According to the Fusion 360 API documentation, the id property of a adsk.core.Appearance should be unique.
        For many models imported into Fusion 360 (as opposed to being designed in Fusion), the material ids are duplicated.
        This leads to a bug where the Fusion 360 API does not return the correct materials for a model, thus making it impossible to export the materials.

        Returns: True if the appearances are bugged.

        """
        usedIdMap = {}
        for material in self.ao.design.materials:
            if material.id in usedIdMap:
                return True
            usedIdMap[material.id] = True
        return False

    def getDefaultAppearance(self) -> Optional[adsk.core.Appearance]:
        fusionMatLib = self.ao.app.materialLibraries.itemById("C1EEA57C-3F56-45FC-B8CB-A9EC46A9994C") # Fusion 360 Material Library
        if fusionMatLib is None:
            return None
        aluminum = fusionMatLib.appearances.itemById("PrismMaterial-002_physmat_aspects:Prism-028") # Aluminum - Satin
        return aluminum

    def exportJoints(self, fusionJoints):
        joints = []
        for fusionJoint in fusionJoints:
            if self.isJointInvalid(fusionJoint):
                continue
            joints.append(MessageToDict(self.fillJoint(fusionJoint)))
        return joints

    def isJointInvalid(self, fusionJoint):
        if fusionJoint.occurrenceOne is None and fusionJoint.occurrenceTwo is None:
            print("WARNING: Ignoring joint with unknown occurrences!")  # todo: Show these messages to the user
            return True
        if fusionJoint.jointMotion.jointType not in range(6):
            print("WARNING: Ignoring joint with unknown type!")
            return True
        return False

    def fillJoint(self, fusionJoint):
        protoJoint = Joint()
        # protoJoint.header.uuid = item_id(fusionJoint, ATTR_GROUP_NAME)
        protoJoint.header.name = fusionJoint.name
        self.fillVector3D(self.getJointOrigin(fusionJoint), protoJoint.origin)
        protoJoint.isLocked = fusionJoint.isLocked
        protoJoint.isSuppressed = fusionJoint.isSuppressed

        # If occurrenceOne or occurrenceTwo is null, the joint is jointed to the root component
        protoJoint.occurrenceOneUUID = self.getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceOne)
        protoJoint.occurrenceTwoUUID = self.getJointedOccurrenceUUID(fusionJoint, fusionJoint.occurrenceTwo)

        fillJointMotionFuncSwitcher = {
            0: self.fillRigidJointMotion,
            1: self.fillRevoluteJointMotion,
            2: self.fillSliderJointMotion,
            3: self.fillCylindricalJointMotion,
            4: self.fillPinSlotJointMotion,
            5: self.fillPlanarJointMotion,
            6: self.fillBallJointMotion,
        }

        fillJointMotionFunc = fillJointMotionFuncSwitcher.get(fusionJoint.jointMotion.jointType, lambda: None)
        fillJointMotionFunc(fusionJoint.jointMotion, protoJoint)
        return protoJoint

    def getJointOrigin(self, fusionJoint):
        geometryOrOrigin = fusionJoint.geometryOrOriginOne if fusionJoint.geometryOrOriginOne.objectType == 'adsk::fusion::JointGeometry' else fusionJoint.geometryOrOriginTwo
        if geometryOrOrigin.objectType == 'adsk::fusion::JointGeometry':
            return geometryOrOrigin.origin
        else:  # adsk::fusion::JointOrigin
            origin = geometryOrOrigin.geometry.origin
            return adsk.core.Point3D.create(  # todo: Is this the correct way to calculate a joint origin's true location? Why isn't this exposed in the API?
                origin.x + geometryOrOrigin.offsetX.value,
                origin.y + geometryOrOrigin.offsetY.value,
                origin.z + geometryOrOrigin.offsetZ.value)

    def getJointedOccurrenceUUID(self, fusionJoint, fusionOccur):
        # if fusionOccur is None:
        #     return item_id(fusionJoint.parentComponent, ATTR_GROUP_NAME)  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
        # return item_id(fusionOccur, ATTR_GROUP_NAME)
        if fusionOccur is None:
            return ""  # If the occurrence of a joint is null, the joint is jointed to the parent component (which should always be the root object)
        return fusionOccur.fullPathName

    def fillRigidJointMotion(self, fusionJointMotion, protoJoint):
        protoJoint.rigidJointMotion.SetInParent()

    def fillRevoluteJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.revoluteJointMotion

        self.fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
        protoJointMotion.rotationValue = fusionJointMotion.rotationValue
        self.fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    def fillSliderJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.sliderJointMotion

        self.fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
        protoJointMotion.slideValue = fusionJointMotion.slideValue
        self.fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

    def fillCylindricalJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.cylindricalJointMotion

        self.fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
        protoJointMotion.rotationValue = fusionJointMotion.rotationValue
        self.fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

        protoJointMotion.slideValue = fusionJointMotion.slideValue
        self.fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

    def fillPinSlotJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.pinSlotJointMotion

        self.fillVector3D(fusionJointMotion.rotationAxisVector, protoJointMotion.rotationAxisVector)
        protoJointMotion.rotationValue = fusionJointMotion.rotationValue
        self.fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

        self.fillVector3D(fusionJointMotion.slideDirectionVector, protoJointMotion.slideDirectionVector)
        protoJointMotion.slideValue = fusionJointMotion.slideValue
        self.fillJointLimits(fusionJointMotion.slideLimits, protoJointMotion.slideLimits)

    def fillPlanarJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.planarJointMotion

        self.fillVector3D(fusionJointMotion.normalDirectionVector, protoJointMotion.normalDirectionVector)

        self.fillVector3D(fusionJointMotion.primarySlideDirectionVector, protoJointMotion.primarySlideDirectionVector)
        protoJointMotion.primarySlideValue = fusionJointMotion.primarySlideValue
        self.fillJointLimits(fusionJointMotion.primarySlideLimits, protoJointMotion.primarySlideLimits)

        self.fillVector3D(fusionJointMotion.secondarySlideDirectionVector, protoJointMotion.secondarySlideDirectionVector)
        protoJointMotion.secondarySlideValue = fusionJointMotion.secondarySlideValue
        self.fillJointLimits(fusionJointMotion.secondarySlideLimits, protoJointMotion.secondarySlideLimits)

        protoJointMotion.rotationValue = fusionJointMotion.rotationValue
        self.fillJointLimits(fusionJointMotion.rotationLimits, protoJointMotion.rotationLimits)

    def fillBallJointMotion(self, fusionJointMotion, protoJoint):
        protoJointMotion = protoJoint.ballJointMotion

        self.fillVector3D(fusionJointMotion.rollDirectionVector, protoJointMotion.rollDirectionVector)
        protoJointMotion.rollValue = fusionJointMotion.rollValue
        self.fillJointLimits(fusionJointMotion.rollLimits, protoJointMotion.rollLimits)

        self.fillVector3D(fusionJointMotion.pitchDirectionVector, protoJointMotion.pitchDirectionVector)
        protoJointMotion.pitchValue = fusionJointMotion.pitchValue
        self.fillJointLimits(fusionJointMotion.pitchLimits, protoJointMotion.pitchLimits)

        self.fillVector3D(fusionJointMotion.yawDirectionVector, protoJointMotion.yawDirectionVector)
        protoJointMotion.yawValue = fusionJointMotion.yawValue
        self.fillJointLimits(fusionJointMotion.yawLimits, protoJointMotion.yawLimits)

    def fillJointLimits(self, fusionJointLimits, protoJointLimits):
        protoJointLimits.isMaximumValueEnabled = fusionJointLimits.isMaximumValueEnabled
        protoJointLimits.isMinimumValueEnabled = fusionJointLimits.isMinimumValueEnabled
        protoJointLimits.isRestValueEnabled = fusionJointLimits.isRestValueEnabled
        protoJointLimits.maximumValue = fusionJointLimits.maximumValue
        protoJointLimits.minimumValue = fusionJointLimits.minimumValue
        protoJointLimits.restValue = fusionJointLimits.restValue

    def fillVector3D(self, fusionVector3D, protoVector3D):
        protoVector3D.x = fusionVector3D.x
        protoVector3D.y = fusionVector3D.y
        protoVector3D.z = fusionVector3D.z
