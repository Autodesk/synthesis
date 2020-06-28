import io
import time
import struct
from io import BytesIO

from typing import Any, Dict, List, BinaryIO
from typing import Callable, Optional, Tuple, TypeVar, Union

import adsk
import adsk.core
import adsk.fusion

from pygltflib import GLTF2, Asset, Scene, Node, Mesh, Primitive, Attributes, Accessor, BufferView, Buffer, Material, PbrMetallicRoughness
from apper import AppObjects
from .timers import SegmentedStopwatch

from gltfutils.GLTFConstants import ComponentType, DataType


class GLTFDesignExporter(object):  # todo: can this exporter be made generic (non fusion specific?)
    """Class for exporting fusion designs into the glTF binary file format, aka glB.

    Fusion API objects are translated into glTF as follows:

    Fusion Design -> glTF scene
    Fusion Occurrence -> glTF node
    Fusion Component -> glTF mesh
    Fusion BRepBody -> glTF primitive
    Fusion MeshBody -> glTF primitive
    Fusion Appearances -> glTF materials todo
    Fusion Materials -> (contained in the 'extras' of glTF materials) todo
    Fusion Joints -> ??? glTF doesn't really have a concept of node motion around points that aren't nodes todo

    Attributes:
        gltf: Main glTF storage object, gets JSON serialized for export.
        primaryBuffer: The buffer referencing the one inline buffer in the final glB file.
        primaryBufferId: The index of the primaryBuffer in the glTF buffers list.
        primaryBufferStream: The memory stream for temporary storage of the glB buffer data.

    todo unit conversion
    todo allow multiple exports (incremental) with one GLTFDesignExporter
    todo coordinate system conversion
    """

    # types
    GLTFIndex = int
    FusionRevId = int

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

    # fields
    gltf: GLTF2

    primaryBufferId: int
    primaryBuffer: Buffer
    primaryBufferStream: BytesIO

    componentRevIDToIndexMap: Dict[FusionRevId, GLTFIndex]

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
        return byte/255

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
        return max(min((464 - 7 * attenLength.value) / 1938, 1), 0.03) # todo: this conversion is just made up

    def __init__(self, ao: AppObjects):  # todo: allow the export of designs besides the one in the foreground?
        self.ao = ao

        # These stopwatches are for export performance testing only.
        self.perfWatch = SegmentedStopwatch(time.perf_counter)
        self.bufferWatch = SegmentedStopwatch(time.perf_counter)

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

    def saveGLB(self, filepath: str) -> Tuple[str, str]:
        """
        Exports the current fusion document into a glb file.

        Args:
            filepath: The full path to the file to create

        Returns: Performance logs

        """
        with open(filepath, 'xb') as fileByteStream:
            self.saveGLBToStream(fileByteStream)

        return str(self.perfWatch), str(self.bufferWatch)

    def saveGLBToStream(self, stream: BinaryIO) -> None:
        """
        Exports the current fusion document into glTF binary format and write the data to a stream.

        Args:
            stream: A BinaryIO stream, usually a file stream created with open()
        """

        self.gltf.scene = self.exportScene()  # export the current fusion document

        self.perfWatch.switch_segment('encoding and file writing')

        # convert gltf to json and serialize
        self.primaryBuffer.byteLength = self.calculateAlignment(self.primaryBufferStream.seek(0, io.SEEK_END)) # must calculate before encoding JSON

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

    def exportScene(self) -> GLTFIndex:
        """Exports the open fusion design to a glTF scene.
        
        Returns: The index of the exported scene in the glTF scenes list.
        """
        scene = Scene()

        # We export components into meshes first while recording a mapping from fusion component unique ids to glTF mesh indices so we can refer to mesh instances as we export the nodes.
        # This allows us to avoid serializing duplicate meshes for components which occur multiple times in one assembly, similar to fusion's system of components and occurrences.

        self.exportMeshes(self.ao.design.allComponents)

        self.perfWatch.switch_segment('exporting node tree')
        scene.nodes.append(self.exportRootNode(self.ao.root_comp))
        self.perfWatch.stop()

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
        node.mesh = self.componentRevIDToIndexMap.get(rootComponent.revisionId, None)

        node.children = [self.exportNode(occur) for occur in rootComponent.occurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportNode(self, occur: adsk.fusion.Occurrence) -> GLTFIndex:
        """Recursively exports the component hierarchy of the open fusion document to glTF nodes, starting with an occurrence in the fusion hierarchy.

        Args:
            occur: An occurrence to be exported.

        Returns: The index of the exported node in the glTF nodes list.
        """
        node = Node()
        node.name = occur.name

        flatMatrix = occur.transform.asArray()
        if not self.isIdentityMatrix3D(flatMatrix):
            node.matrix = [flatMatrix[i + j * 4] for i in range(4) for j in range(4)]  # transpose the flat 4x4 matrix3d

        node.mesh = self.componentRevIDToIndexMap.get(occur.component.revisionId, None)
        # node.extras['isGrounded'] = occur.isGrounded

        node.children = [self.exportNode(occur) for occur in occur.childOccurrences]
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportMeshes(self, fusionComponents: List[adsk.fusion.Component]) -> None:
        """Exports a list of fusion components to glTF meshes.

        Args:
            fusionComponents: The list of all unique fusion components in the open document.

        Returns: A mapping from unique ids of fusion components to their index in the glTF mesh list.
        """
        self.componentRevIDToIndexMap = {}

        for fusionComponent in fusionComponents: # accessing the list of components is slow, ~1sec/500 components
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
        mesh.extras['description'] = fusionComponent.description
        mesh.extras['revisionId'] = revisionId
        mesh.extras['partNumber'] = fusionComponent.partNumber
        # protoComponent.materialId = fusionComponent.material.id
        # fillPhysicalProperties(fusionComponent.physicalProperties, protoComponent.physicalProperties)

        mesh.primitives = [prim for prim in [self.exportPrimitiveBrep(bRepBody) for bRepBody in bodies] if prim is not None]


        self.gltf.meshes.append(mesh)
        self.componentRevIDToIndexMap[revisionId] = len(self.gltf.meshes) - 1

    def exportPrimitiveBrep(self, fusionBRepBody: adsk.fusion.BRepBody) -> Optional[Primitive]:
        """Exports a fusion bRep body to a glTF primitive.

        Args:
            fusionBRepBody: The fusion bRep body to export.

        Returns: The exported primitive object.
        """
        primitive = Primitive()
        primitive.extras['name'] = fusionBRepBody.name
        # protoMeshBody.appearanceId = fusionBRepBody.appearance.id
        # protoMeshBody.materialId = fusionBRepBody.material.id
        # fillPhysicalProperties(fusionBRepBody.physicalProperties, protoMeshBody.physicalProperties)

        self.perfWatch.switch_segment('calculating mesh')

        meshCalculator = fusionBRepBody.meshManager.createMeshCalculator()
        if meshCalculator is None:
            return None
        meshCalculator.setQuality(11)  # todo mesh quality settings
        mesh = meshCalculator.calculate()
        if mesh is None:
            return None

        self.perfWatch.switch_segment('writing mesh to bytebuffer')

        primitive.attributes = Attributes()
        primitive.attributes.POSITION = self.exportAccessor(mesh.nodeCoordinatesAsFloat, DataType.Vec3, ComponentType.Float, True)  # glTF requires limits for position coordinates.
        primitive.indices = self.exportAccessor(mesh.nodeIndices, DataType.Scalar, None, False)  # Autodetect component type on a per-mesh basis. glTF does not require limits for indices.

        self.perfWatch.switch_segment('exporting materials')

        bodyAppearance = fusionBRepBody.appearance
        if bodyAppearance is not None:
            appearIndex = self.exportGltfMaterialFromFusAppear(bodyAppearance)
            if appearIndex is not None:
                primitive.material = appearIndex

        self.perfWatch.stop()

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
                raise ValueError(f"Failed to autodetect unsigned integer type for limits ({componentMin}, {componentMax})")

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

    def exportGltfMaterialFromFusAppear(self, fusionAppearance: adsk.core.Appearance) -> Optional[GLTFIndex]:
        props = fusionAppearance.appearanceProperties

        material = Material()
        material.alphaCutoff = None # this is a bug with the gltf python lib
        material.name = fusionAppearance.name

        pbr = PbrMetallicRoughness()
        pbr.baseColorFactor = None # this is a bug with the gltf python lib
        pbr.metallicFactor = 0.0
        pbr.roughnessFactor = props.itemById("surface_roughness").value

        baseColor = None

        modelItem = props.itemById("interior_model")
        if modelItem is None:
            return None
        matModelType = modelItem.value

        if matModelType == 0: # Opaque
            baseColor = props.itemById("opaque_albedo").value
            if props.itemById("opaque_emission").value:
                material.emissiveFactor = self.fusionColorToArray(props.itemById("opaque_luminance_modifier").value)[:3]
        elif matModelType == 1: # Metal
            pbr.metallicFactor = 1.0
            baseColor = props.itemById("metal_f0").value
        elif matModelType == 2: # Layered
            baseColor = props.itemById("layered_diffuse").value
        elif matModelType == 3: # Transparent
            baseColor = props.itemById("transparent_color").value
            material.alphaMode = "BLEND"
        elif matModelType == 5: # Glazing
            baseColor = props.itemById("glazing_transmission_color").value
        else: # ??? idk what type 4 material is
            print(f"[WARNING] Unsupported material type: {material.name}")

        if baseColor is None:
            return None

        pbr.baseColorFactor = self.fusionColorToArray(baseColor)[:3] + [self.fusionAttenLengthToAlpha(props.itemById("transparent_distance"))]
        material.pbrMetallicRoughness = pbr

        self.ao.ui.messageBox(material.name)

        self.gltf.materials.append(material)
        return len(self.gltf.materials) - 1
