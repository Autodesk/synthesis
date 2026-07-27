import type { ParsedMesh } from "./STLParser"

// Minimal glTF 2.0 geometry reader.
// Assumes single embedded base64 buffer, no TEXCOORD_0, no node transforms, no Draco.

interface GLTFAccessor {
    bufferView?: number
    componentType: number
    count: number
    type: string
    byteOffset?: number
}

interface GLTFBufferView {
    buffer: number
    byteOffset?: number
    byteStride?: number
}

interface GLTFBuffer {
    uri?: string
    byteLength: number
}

interface GLTFPrimitive {
    attributes: Record<string, number>
    indices?: number
    mode?: number
}

interface GLTFMesh {
    primitives: GLTFPrimitive[]
}

interface GLTFNode {
    mesh?: number
    children?: number[]
    matrix?: number[]
    translation?: [number, number, number]
    rotation?: [number, number, number, number]
    scale?: [number, number, number]
}

interface GLTFDocument {
    accessors: GLTFAccessor[]
    bufferViews: GLTFBufferView[]
    buffers: GLTFBuffer[]
    meshes: GLTFMesh[]
    nodes: GLTFNode[]
    scene?: number
    scenes?: { nodes: number[] }[]
    extensionsRequired?: string[]
}

// glTF matrices are column-major 4x4, stored flat.
type Mat4 = number[]

const IDENTITY_MAT4: Mat4 = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

const GLTF_TYPE_COMPONENTS: Record<string, number> = { SCALAR: 1, VEC2: 2, VEC3: 3 }

const INDEX_COMPONENT_SIZE: Record<number, number> = { 5121: 1, 5123: 2, 5125: 4 }

function base64ToBytes(base64: string): Uint8Array {
    const binary = atob(base64)
    const bytes = new Uint8Array(binary.length)
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i)
    return bytes
}

function resolveGLTFBuffer(buffer: GLTFBuffer, meshPath: string, meshFiles: Map<string, Uint8Array>): Uint8Array {
    if (!buffer.uri) throw new Error("GLB-embedded glTF buffers are not supported - export as .gltf with a uri")
    if (buffer.uri.startsWith("data:")) return base64ToBytes(buffer.uri.slice(buffer.uri.indexOf(",") + 1))

    const dir = meshPath.includes("/") ? meshPath.slice(0, meshPath.lastIndexOf("/") + 1) : ""
    const uri = decodeURIComponent(buffer.uri)
    const bytes = meshFiles.get(dir + uri) ?? meshFiles.get(uri) ?? meshFiles.get(uri.split("/").pop()!)
    if (!bytes) throw new Error(`glTF external buffer not found: ${buffer.uri}`)
    return bytes
}

function readFloatAccessor(doc: GLTFDocument, accessorIndex: number, buffers: Uint8Array[]): Float32Array {
    const accessor = doc.accessors[accessorIndex]
    if (accessor.componentType !== 5126) {
        throw new Error(`Unsupported glTF componentType ${accessor.componentType} for a float attribute`)
    }
    const numComponents = GLTF_TYPE_COMPONENTS[accessor.type]
    if (!numComponents || accessor.bufferView === undefined) {
        throw new Error(
            `Unsupported glTF accessor (type=${accessor.type}, sparse/bufferView-less accessors unsupported)`
        )
    }

    const bufferView = doc.bufferViews[accessor.bufferView]
    const buffer = buffers[bufferView.buffer]
    const view = new DataView(buffer.buffer, buffer.byteOffset, buffer.byteLength)
    const elementSize = numComponents * 4
    const stride = bufferView.byteStride ?? elementSize
    const base = (bufferView.byteOffset ?? 0) + (accessor.byteOffset ?? 0)

    const out = new Float32Array(accessor.count * numComponents)
    for (let i = 0; i < accessor.count; i++) {
        const elementOffset = base + i * stride
        for (let c = 0; c < numComponents; c++) {
            out[i * numComponents + c] = view.getFloat32(elementOffset + c * 4, true)
        }
    }

    return out
}

function readIndexAccessor(doc: GLTFDocument, accessorIndex: number, buffers: Uint8Array[]): Uint32Array {
    const accessor = doc.accessors[accessorIndex]
    const componentSize = INDEX_COMPONENT_SIZE[accessor.componentType]
    if (accessor.type !== "SCALAR" || !componentSize || accessor.bufferView === undefined) {
        throw new Error(`Unsupported glTF index accessor (componentType=${accessor.componentType})`)
    }

    const bufferView = doc.bufferViews[accessor.bufferView]
    const buffer = buffers[bufferView.buffer]
    const view = new DataView(buffer.buffer, buffer.byteOffset, buffer.byteLength)
    const stride = bufferView.byteStride ?? componentSize
    const base = (bufferView.byteOffset ?? 0) + (accessor.byteOffset ?? 0)

    const out = new Uint32Array(accessor.count)
    for (let i = 0; i < accessor.count; i++) {
        const off = base + i * stride
        out[i] =
            accessor.componentType === 5121
                ? view.getUint8(off)
                : accessor.componentType === 5123
                  ? view.getUint16(off, true)
                  : view.getUint32(off, true)
    }

    return out
}

// Column-major 4x4 multiply: result = a * b
function mat4Mul(a: Mat4, b: Mat4): Mat4 {
    const out = new Array(16) // every slot is overwritten below, no need to zero it first
    for (let c = 0; c < 4; c++) {
        for (let r = 0; r < 4; r++) {
            let sum = 0
            for (let k = 0; k < 4; k++) sum += a[k * 4 + r] * b[c * 4 + k]
            out[c * 4 + r] = sum
        }
    }

    return out
}

// https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#Quaternion-derived_rotation_matrix
function quatScaleToMat4(
    t: [number, number, number],
    q: [number, number, number, number],
    s: [number, number, number]
): Mat4 {
    const [x, y, z, w] = q
    const x2 = x + x
    const y2 = y + y
    const z2 = z + z
    const xx = x * x2
    const xy = x * y2
    const xz = x * z2
    const yy = y * y2
    const yz = y * z2
    const zz = z * z2
    const wx = w * x2
    const wy = w * y2
    const wz = w * z2
    const [sx, sy, sz] = s

    // biome-ignore format: We would prefer to visualize this as a matrix
    return [
        (1 - (yy + zz)) * sx, (xy + wz) * sx, (xz - wy) * sx, 0,
        (xy - wz) * sy, (1 - (xx + zz)) * sy, (yz + wx) * sy, 0,
        (xz + wy) * sz, (yz - wx) * sz, (1 - (xx + yy)) * sz, 0,
        t[0], t[1], t[2], 1,
    ]
}

function nodeLocalMatrix(node: GLTFNode): Mat4 {
    if (node.matrix) return node.matrix
    // CAD exporters (Onshape) omit TRS entirely on nearly every node. Skip the quaternion math
    // and reuse the shared identity matrix instead of rebuilding an equivalent one from scratch.
    if (!node.translation && !node.rotation && !node.scale) return IDENTITY_MAT4
    return quatScaleToMat4(node.translation ?? [0, 0, 0], node.rotation ?? [0, 0, 0, 1], node.scale ?? [1, 1, 1])
}

function isIdentity(m: Mat4): boolean {
    return m.every((v, i) => v === IDENTITY_MAT4[i])
}

// https://en.wikipedia.org/wiki/Transformation_matrix#Affine_transformations
function transformPositions(positions: Float32Array, m: Mat4): Float32Array {
    if (isIdentity(m)) return positions
    const out = new Float32Array(positions.length)
    for (let i = 0; i < positions.length; i += 3) {
        const [x, y, z] = [positions[i], positions[i + 1], positions[i + 2]]
        out[i] = m[0] * x + m[4] * y + m[8] * z + m[12]
        out[i + 1] = m[1] * x + m[5] * y + m[9] * z + m[13]
        out[i + 2] = m[2] * x + m[6] * y + m[10] * z + m[14]
    }

    return out
}

// Uses the matrix's linear part only (no inverse-transpose). Correct for the rigid,
// uniform-scale node transforms CAD exporters actually emit.
// https://en.wikipedia.org/wiki/Normal_(geometry)#Transforming_normals
function transformNormals(normals: Float32Array, m: Mat4): Float32Array {
    if (isIdentity(m)) return normals
    const out = new Float32Array(normals.length)
    for (let i = 0; i < normals.length; i += 3) {
        const [x, y, z] = [normals[i], normals[i + 1], normals[i + 2]]
        const nx = m[0] * x + m[4] * y + m[8] * z
        const ny = m[1] * x + m[5] * y + m[9] * z
        const nz = m[2] * x + m[6] * y + m[10] * z
        const len = Math.hypot(nx, ny, nz) || 1
        out[i] = nx / len
        out[i + 1] = ny / len
        out[i + 2] = nz / len
    }

    return out
}

function sequentialIndices(count: number): Uint32Array {
    const out = new Uint32Array(count)
    for (let i = 0; i < count; i++) out[i] = i
    return out
}

interface PrimitiveGeometry {
    verts: Float32Array
    normals: Float32Array
    uv: Float32Array
    indices: Uint32Array
}

function mergeGeometries(chunks: PrimitiveGeometry[]): ParsedMesh {
    let totalVerts = 0
    let totalIndices = 0
    for (const chunk of chunks) {
        totalVerts += chunk.verts.length
        totalIndices += chunk.indices.length
    }

    const verts = new Float32Array(totalVerts)
    const normals = new Float32Array(totalVerts)
    const uv = new Float32Array((totalVerts / 3) * 2)
    const indices = new Uint32Array(totalIndices)

    let vertOffset = 0
    let uvOffset = 0
    let indexOffset = 0
    let vertexOffset = 0
    for (const chunk of chunks) {
        verts.set(chunk.verts, vertOffset)
        normals.set(chunk.normals, vertOffset)
        uv.set(chunk.uv, uvOffset)
        for (let i = 0; i < chunk.indices.length; i++) indices[indexOffset + i] = chunk.indices[i] + vertexOffset

        vertOffset += chunk.verts.length
        uvOffset += chunk.uv.length
        indexOffset += chunk.indices.length
        vertexOffset += chunk.verts.length / 3
    }

    return { verts, normals, uv, indices }
}

export function parseGLTF(data: Uint8Array, meshPath: string, meshFiles: Map<string, Uint8Array>): ParsedMesh {
    const doc: GLTFDocument = JSON.parse(new TextDecoder().decode(data))

    if (doc.extensionsRequired?.some(ext => ext.includes("draco"))) {
        throw new Error("Draco-compressed glTF meshes are not supported")
    }

    const buffers = doc.buffers.map(b => resolveGLTFBuffer(b, meshPath, meshFiles))

    const childIndices = new Set<number>()
    for (const node of doc.nodes) for (const child of node.children ?? []) childIndices.add(child)
    const rootIndices =
        doc.scenes?.[doc.scene ?? 0]?.nodes ?? doc.nodes.map((_, i) => i).filter(i => !childIndices.has(i))

    const chunks: PrimitiveGeometry[] = []

    function visit(nodeIndex: number, parentTransform: Mat4) {
        const node = doc.nodes[nodeIndex]
        const local = nodeLocalMatrix(node)
        // Skip the 4x4 multiply (and its allocation) whenever either side is identity, true for
        // almost every node in practice, since CAD exporters rarely set node-level transforms.
        const world = isIdentity(local)
            ? parentTransform
            : isIdentity(parentTransform)
              ? local
              : mat4Mul(parentTransform, local)

        if (node.mesh !== undefined) {
            for (const prim of doc.meshes[node.mesh].primitives) {
                if (prim.mode !== undefined && prim.mode !== 4) continue // only TRIANGLES supported

                const positionIdx = prim.attributes.POSITION
                const normalIdx = prim.attributes.NORMAL
                if (positionIdx === undefined || normalIdx === undefined) {
                    throw new Error("glTF primitive is missing POSITION or NORMAL attribute")
                }

                const verts = transformPositions(readFloatAccessor(doc, positionIdx, buffers), world)
                const normals = transformNormals(readFloatAccessor(doc, normalIdx, buffers), world)
                const texcoordIdx = prim.attributes.TEXCOORD_0
                const uv =
                    texcoordIdx !== undefined
                        ? readFloatAccessor(doc, texcoordIdx, buffers)
                        : new Float32Array((verts.length / 3) * 2)

                const vertexCount = verts.length / 3
                const indices =
                    prim.indices !== undefined
                        ? readIndexAccessor(doc, prim.indices, buffers)
                        : sequentialIndices(vertexCount)

                chunks.push({ verts, normals, uv, indices })
            }
        }

        for (const child of node.children ?? []) visit(child, world)
    }

    for (const rootIndex of rootIndices) visit(rootIndex, IDENTITY_MAT4)

    return mergeGeometries(chunks)
}
