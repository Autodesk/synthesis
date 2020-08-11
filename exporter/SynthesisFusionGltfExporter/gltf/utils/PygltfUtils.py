from .ByteUtils import alignByteArrayToBoundary
from .DictUtils import dictDeleteEmptyKeys
from .GLTFConstants import *
from pygltflib import *

from .MathUtils import isIdentityMatrix3D


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
