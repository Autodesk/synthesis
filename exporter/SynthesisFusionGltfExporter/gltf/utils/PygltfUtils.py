from .DictUtils import dictDeleteEmptyKeys
from .GLTFConstants import *
from pygltflib import *

from io import BytesIO
from .ByteUtils import *
from .MathUtils import isIdentityMatrix3D

# Utilities specific to the pygltf library

GLTFIndex = int

def exportAccessor(gltf: GLTF2, primaryBufferIndex: GLTFIndex, primaryBufferStream: BytesIO, array: List[Union[int, float]], dataType: DataType, componentType: Optional[ComponentType], calculateLimits: bool,
                   exportWarnings: List[str]) -> GLTFIndex:
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

    alignBytesIOToBoundary(primaryBufferStream)  # buffers should start/end at aligned values for efficiency
    bufferByteOffset = calculateAlignment(primaryBufferStream.tell())  # start of the buffer to be created

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
            exportWarnings.append(f"Failed to autodetect unsigned integer type for limits ({componentMin}, {componentMax})")

    # Pack the supplied data into the primary glB buffer stream.
    accessor.componentType = componentType
    packFormat = "<" + ComponentType.getTypeCode(componentType)
    for item in array:
        primaryBufferStream.write(struct.pack(packFormat, item))

    bufferByteLength = calculateAlignment(primaryBufferStream.tell() - bufferByteOffset)  # Calculate the length of the bufferView to be created.

    accessor.bufferView = exportBufferView(gltf, primaryBufferIndex, bufferByteOffset, bufferByteLength)  # Create the glTF bufferView object with the calculated start and length.

    gltf.accessors.append(accessor)
    return len(gltf.accessors) - 1


def exportBufferView(gltf: GLTF2, primaryBufferIndex: int, byteOffset: int, byteLength: int) -> GLTFIndex:
    """Creates a glTF bufferView with the specified offset and length, referencing the default glB buffer.

    Args:
        byteOffset: Index of the starting byte in the referenced buffer.
        byteLength: Length in bytes of the bufferView.

    Returns: The index of the exported bufferView in the glTF bufferViews list.
    """
    bufferView = BufferView()
    bufferView.buffer = primaryBufferIndex  # index of the default glB buffer.
    bufferView.byteOffset = byteOffset
    bufferView.byteLength = byteLength

    gltf.bufferViews.append(bufferView)
    return len(gltf.bufferViews) - 1

def isEmptyLeafNode(node: Node) -> bool:
    return len(node.children) == 0 and node.mesh is None and node.camera is None and len(node.extensions) == 0 and len(node.extras) == 0

def notIdentityMatrix3DOrNone(flatMatrix: List[float]) -> Optional[List[float]]:
    if not isIdentityMatrix3D(flatMatrix):
        return flatMatrix
    return None

def writeGltfToFile(gltf, primaryBufferData, filepath):
    gltf._glb_data = primaryBufferData.tobytes()
    gltf.save(filepath)

def writeGlbToFile(gltf, primaryBufferData, filepath):
    with open(filepath, 'wb') as stream:
        writeGlbToStream(gltf, primaryBufferData, stream)

def writeGlbToStream(gltf, primaryBufferData, stream):
    jsonBytes = bytearray(gltfToJson(gltf, default=json_serial, indent=1, allow_nan=False, skipkeys=True).encode("utf-8"))  # type: bytearray

    alignByteArrayToBoundary(jsonBytes)

    glbLength = GLB_HEADER_SIZE + \
                GLB_CHUNK_SIZE + len(jsonBytes) + \
                GLB_CHUNK_SIZE + len(primaryBufferData)
    # header
    stream.write(b'glTF')  # magic
    stream.write(struct.pack('<I', GLTF_VERSION))  # version
    stream.write(struct.pack('<I', glbLength))  # length
    # json chunk
    stream.write(struct.pack('<I', len(jsonBytes)))  # chunk length
    stream.write(bytes("JSON", 'utf-8'))  # chunk type
    stream.write(jsonBytes)  # chunk data
    # buffer chunk
    stream.write(struct.pack('<I', len(primaryBufferData)))  # chunk length
    stream.write(bytes("BIN\x00", 'utf-8'))  # chunk type

    stream.write(primaryBufferData)  # chunk data
    stream.flush()  # flush to file

def gltfToJson(gltf: GLTF2,
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
    data = dictDeleteEmptyKeys(data, ['extras'])
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
