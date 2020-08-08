import copy
import struct
import time
import traceback
from datetime import date, datetime


from io import BytesIO
from typing import Dict, List
from typing import Optional, Union, Tuple, Callable

import adsk
import adsk.core
import adsk.fusion
from pygltflib import GLTF2, Asset, Scene, Node, Mesh, Primitive, Attributes, Accessor, BufferView, Buffer, Material, PbrMetallicRoughness
from pygltflib import gltf_asdict, json_serial
import json
from dataclasses_json.core import _ExtendedEncoder as JsonEncoder

from google.protobuf.json_format import MessageToDict

from apper import AppObjects, Fusion360Utilities
from .extras.ExportPhysicalProperties import exportPhysicalProperties, combinePhysicalProperties
from .utils.FusionUtils import fusionColorToRGBAArray, isSameMaterial, fusionAttenLengthToAlpha
from .utils.GLTFUtils import isEmptyLeafNode
from .utils.pyutils.counters import EventCounter
from .utils.pyutils.timers import SegmentedStopwatch
from .GLTFConstants import ComponentType, DataType
from .utils.ByteUtils import *
from .utils.MathUtils import isIdentityMatrix3D
from .extras.ExportJoints import exportJoints



def exportDesign(showFileDialog=False, enableMaterials=True, enableMaterialOverrides=True, enableFaceMaterials=True, exportVisibleBodiesOnly=True, fileType:str = "glb", quality="8"):
    try:
        ao = AppObjects()

        if ao.document.dataFile is None:
            ao.ui.messageBox("Export Cancelled: You must save your Fusion design before exporting.")
            return

        exporter = GLTFDesignExporter(ao, enableMaterials, enableMaterialOverrides, enableFaceMaterials, exportVisibleBodiesOnly, quality)
        useGlb = fileType == "glb"
        if showFileDialog:
            dialog = ao.ui.createFileDialog() # type: adsk.core.FileDialog
            dialog.filter = "glTF Binary (*.glb)" if useGlb else "glTF JSON (*.gltf)"
            dialog.isMultiSelectEnabled = False
            dialog.title = "Select glTF Export Location"
            dialog.initialFilename = f'{ao.document.name.replace(" ", "_")}.{"glb" if useGlb else "gltf"}'
            results = dialog.showSave()
            if results != 0 and results != 2: # For some reason the generated python API enums were wrong, so we're just using the literals
                ao.ui.messageBox(f"The glTF export was cancelled.")
                return
            filePath = dialog.filename

        else:
            filePath = f'C:/temp/{ao.document.name.replace(" ", "_")}_{int(time.time())}.glb'
        exportResults = exporter.saveGltf(filePath, useGlb)
        if exportResults is None:
            ao.ui.messageBox(f"The glTF export was cancelled.")
            return
        perfResults, bufferResults, warnings, modelStats, eventCounter, duration = exportResults

        finishedMessageDebug = (f"glTF export completed in {duration} seconds.\n"
                           f"File saved to {filePath}\n\n"
                           f"==== Export Performance Results ====\n"
                           f"{perfResults}\n"
                           f"==== Buffer Writing Results ====\n"
                           f"{bufferResults}\n"
                           f"==== Model Stats ====\n"
                           f"{modelStats}\n"
                           f"==== Events Counter ====\n"
                           f"{eventCounter}\n"
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

def gltf_to_json(gltf) -> str:
    return to_json(gltf, default=json_serial, indent=None, allow_nan=False, skipkeys=True)

def delete_empty_keys_ignore_keys(dictionary, ignoreKeys):
    """
    Delete keys with the value ``None`` in a dictionary, recursively.

    This alters the input so you may wish to ``copy`` the dict first.

    Courtesy Chris Morgan and modified from:
    https://stackoverflow.com/questions/4255400/exclude-empty-null-values-from-json-serialization
    """
    for key, value in list(dictionary.items()):
        if value is None or (hasattr(value, '__iter__') and len(value) == 0):
            del dictionary[key]
        elif isinstance(value, dict):
            if key not in ignoreKeys:
                delete_empty_keys_ignore_keys(value, ignoreKeys)
        elif isinstance(value, list):
            for item in value:
                if isinstance(item, dict):
                    delete_empty_keys_ignore_keys(item, ignoreKeys)
    return dictionary  # For convenience

def to_json(gltf,
            *,
            skipkeys: bool = False,
            ensure_ascii: bool = True,
            check_circular: bool = True,
            allow_nan: bool = True,
            indent: Optional[Union[int, str]] = None,
            separators: Tuple[str, str] = None,
            default: Callable = None,
            sort_keys: bool = True,
            **kw) -> str:
    """
    to_json and from_json from dataclasses_json
    courtesy https://github.com/lidatong/dataclasses-json
    """

    data = gltf_asdict(gltf)
    data = delete_empty_keys_ignore_keys(data, ['extras'])
    return json.dumps(data,
                      cls=JsonEncoder,
                      skipkeys=skipkeys,
                      ensure_ascii=ensure_ascii,
                      check_circular=check_circular,
                      allow_nan=allow_nan,
                      indent=indent,
                      separators=separators,
                      default=default,
                      sort_keys=sort_keys,
                      **kw)

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
    GLTF_VERSION = 2
    GLB_HEADER_SIZE = 12
    GLB_CHUNK_SIZE = 8
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
    materialNameToGltfIndex: Dict[str, GLTFIndex]  # dict for gltf material caching, from fusion appearance name to material gltf index

    warnings: List[str]

    def __init__(self, ao: AppObjects, enableMaterials: bool = False, enableMaterialOverrides: bool = False, enableFaceMaterials: bool = False, exportVisibleBodiesOnly = True, quality: str = "8"):  # todo: allow the export of designs besides the one in the foreground?
        self.exportVisibleBodiesOnly = exportVisibleBodiesOnly
        self.enableMaterials = enableMaterials
        self.enableMaterialOverrides = enableMaterialOverrides
        self.enableFaceMaterials = enableFaceMaterials
        self.ao = ao
        self.meshQuality = int(quality)

        self.warnings = []

        # These stopwatches are for export performance testing only.
        self.perfWatch = SegmentedStopwatch(time.perf_counter)
        self.bufferWatch = SegmentedStopwatch(time.perf_counter)
        self.eventCounter = EventCounter()

        self.gltf = GLTF2()  # The root glTF object.

        self.gltf.asset = Asset()
        self.gltf.asset.generator = self.GLTF_GENERATOR_ID

        self.gltf.scene = 0  # set the default scene to index 0

        # The glB format only allows one buffer to be embedded in the main file.
        # We'll call this buffer the primaryBuffer.
        self.primaryBuffer = Buffer()
        self.primaryBufferIndex = len(self.gltf.buffers)
        self.gltf.buffers.append(self.primaryBuffer)

        # The actual binary data for the buffer will get stored in this memory stream
        self.primaryBufferStream = io.BytesIO()

        self.componentRevIdToMeshTemplate = {}
        self.componentRevIdToMatOverrideDict = {}
        self.materialNameToGltfIndex = {}

        self.usedCompIdMap = {}

        self.allAffectedOccurrences = []

    def saveGltf(self, filepath: str, useGlb: bool):
        """
        Exports the current fusion document into a glb file.

        Args:
            filepath: The full path to the file to create

        Returns: Performance logs

        """

        self.progressBar = self.ao.ui.createProgressDialog()
        self.progressBar.isCancelButtonShown = False
        self.progressBar.reset()
        self.progressBar.show(f"Exporting {self.ao.document.name} to glTF", "Preparing for export...", 0, 100, 0)

        # noinspection PyUnresolvedReferences
        adsk.doEvents() # show progress bar

        if self.enableMaterials and self.checkIfAppearancesAreBugged():
            result = self.ao.ui.messageBox(f"The materials on this design cannot be exported due to a bug in the Fusion 360 API.\n"
                                           f"Do you want to continue the export with materials turned off?\n"
                                           f"(press no to attempt material export, press cancel to cancel export)", "", adsk.core.MessageBoxButtonTypes.YesNoCancelButtonType)
            if result == 0 or result == 2: # yes
                self.enableMaterials = False
            elif result == 3: # no
                pass
            else: # cancel
                return

        try:
            self.gltf.extras['joints'], self.allAffectedOccurrences = exportJoints(self.ao.design.rootComponent.allJoints, self.GLTF_GENERATOR_ID, self.perfWatch)
        except RuntimeError: # todo: report this bug
            result = self.ao.ui.messageBox(f"Could not export joints due to a bug in the Fusion API.\n"
                                           f"Do you want to continue the export without joints?", "", adsk.core.MessageBoxButtonTypes.YesNoButtonType)
            if result == 0 or result == 2: # yes
                pass
            else: # no
                return

        start = time.perf_counter()

        self.defaultAppearance = self.getDefaultAppearance()

        self.gltf.scene = self.exportScene()  # export the current fusion document

        # Clean tags
        for mesh in self.gltf.meshes:
            for prim in mesh.primitives:
                if self.MAT_OVERRIDEABLE_TAG in prim.extras:  # if material is overrideable
                    prim.extras.pop(self.MAT_OVERRIDEABLE_TAG)

        self.progressBar.message = "Serializing data to file..."
        self.progressBar.progressValue = self.progressBar.maximumValue - 1

        self.perfWatch.switch_segment('encoding and file writing')

        # convert gltf to json and serialize
        self.primaryBuffer.byteLength = calculateAlignment(self.primaryBufferStream.seek(0, io.SEEK_END))  # must calculate before encoding JSON

        # ==== do NOT make changes to the glTF object beyond this point ====
        json = gltf_to_json(self.gltf)
        # with open(filepath+".debug.json", "wt") as jsonFile:
        #     jsonFile.write(json)
        jsonBytes = bytearray(json.encode("utf-8"))  # type: bytearray

        # add padding bytes to the end of each chunk data
        alignByteArrayToBoundary(jsonBytes)
        alignBytesIOToBoundary(self.primaryBufferStream)

        # get the memoryView of the primary buffer stream
        primaryBufferData = self.primaryBufferStream.getbuffer()

        if (useGlb):
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
        else:
            self.gltf._glb_data = primaryBufferData.tobytes()
            self.gltf.save(filepath)

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
                 f"joints: {len(self.gltf.extras['joints']) if 'joints' in self.gltf.extras else 'could not export'}\n"
                 )

        if len(self.warnings) == 0:
            self.warnings.append("None :)")

        end = time.perf_counter()

        return str(self.perfWatch), str(self.bufferWatch), "\n".join(self.warnings), stats, str(self.eventCounter), round(end - start, 4)

    def exportScene(self) -> GLTFIndex:
        """Exports the open fusion design to a glTF scene.
        
        Returns: The index of the exported scene in the glTF scenes list.
        """
        scene = Scene()

        # We export components into meshes first while recording a mapping from fusion component unique ids to glTF mesh indices so we can refer to mesh instances as we export the nodes.
        # This allows us to avoid serializing duplicate meshes for components which occur multiple times in one assembly, similar to fusion's system of components and occurrences.

        self.perfWatch.switch_segment('exporting node tree')
        self.progressBar.message = "Reading list of fusion components..."
        self.progressBar.maximumValue = len(self.ao.design.allComponents) + 1
        # self.progressBar.message = "Exporting occurrence tree..."
        # self.progressBar.progressValue+=1
        rootNode = self.exportRootNode(self.ao.root_comp)
        if rootNode is not None:
            scene.nodes.append(rootNode)
        self.perfWatch.stop()

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

        scale = self.DESIGN_SCALE
        node.matrix = [
            scale, 0, 0, 0,
            0, scale, 0, 0,
            0, 0, scale, 0,
            0, 0, 0, 1,
        ]

        node.children = [index for index in [self.exportNode(occur, -1) for occur in rootComponent.occurrences] if index is not None]

        if isEmptyLeafNode(node):
            return
        self.gltf.nodes.append(node)
        return len(self.gltf.nodes) - 1

    def exportNode(self, occur: adsk.fusion.Occurrence, materialOverride: GLTFIndex) -> Optional[GLTFIndex]:
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
        if not isIdentityMatrix3D(flatMatrix):
            node.matrix = [flatMatrix[i + j * 4] for i in range(4) for j in range(4)]  # transpose the flat 4x4 matrix3d

        if not self.exportVisibleBodiesOnly or occur.isVisible:
            node.mesh = self.exportMeshWithOverrideCached(occur.component, materialOverride)
        # node.extras['isGrounded'] = occur.isGrounded

        self.perfWatch.switch_segment("item_id_tree")
        if occur.fullPathName in self.allAffectedOccurrences:
            node.extras["uuid"] = Fusion360Utilities.item_id(occur, self.GLTF_GENERATOR_ID)
        # path_name = occur.fullPathName
        # if path_name in self.usedCompIdMap:
        #     self.eventCounter.event("**Found duplicate id")
        # self.usedCompIdMap[path_name] = True
        self.perfWatch.stop()

        node.children = [index for index in [self.exportNode(occur, materialOverride) for occur in occur.childOccurrences] if index is not None]

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

        try:
            mesh.extras['physicalProperties'] = MessageToDict(combinePhysicalProperties([exportPhysicalProperties(bRepBody.physicalProperties) for bRepBody in bodyList]))
        except:
            self.warnings.append(f"Unable to get physical properties for component {mesh.name}")

        self.componentRevIdToMeshTemplate[revisionId] = mesh
        self.componentRevIdToMatOverrideDict[revisionId] = {}
        numMeshes = len(self.componentRevIdToMeshTemplate)
        self.progressBar.progressValue = numMeshes
        self.progressBar.message = f"Calculating meshes for component {numMeshes} of {self.progressBar.maximumValue}..."
        return mesh

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
            exportFacesTogether = isSameMaterial(faces)
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
        meshCalculator.setQuality(self.meshQuality)  # todo mesh quality settings

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

        alignBytesIOToBoundary(self.primaryBufferStream)  # buffers should start/end at aligned values for efficiency
        bufferByteOffset = calculateAlignment(self.primaryBufferStream.tell())  # start of the buffer to be created

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
                self.eventCounter.event("used cached mesh with materials")
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

    def checkIfAppearancesAreBugged(self) -> bool:
        """Checks if the appearances of a fusion document are bugged.

        According to the Fusion 360 API documentation, the id property of a adsk.core.Appearance should be unique.
        For many models imported into Fusion 360 (as opposed to being designed in Fusion), the material ids are duplicated.
        This leads to a bug where the Fusion 360 API does not return the correct materials for a model, thus making it impossible to export the materials.

        Returns: True if the appearances are bugged.

        """
        usedIdMap = {}
        for appearance in self.ao.design.appearances:
            if appearance.id in usedIdMap:
                return True
            usedIdMap[appearance.id] = True
        return False

    def getDefaultAppearance(self) -> Optional[adsk.core.Appearance]:
        fusionMatLib = self.ao.app.materialLibraries.itemById("C1EEA57C-3F56-45FC-B8CB-A9EC46A9994C") # Fusion 360 Material Library
        if fusionMatLib is None:
            return None
        aluminum = fusionMatLib.appearances.itemById("PrismMaterial-002_physmat_aspects:Prism-028") # Aluminum - Satin
        return aluminum
