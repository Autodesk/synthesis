export interface ParsedMesh {
    verts: Float32Array
    normals: Float32Array
    indices: Uint32Array
    uv: Float32Array
}

function parseBinarySTL(data: Uint8Array): ParsedMesh {
    const view = new DataView(data.buffer, data.byteOffset, data.byteLength)
    const triCount = view.getUint32(80, true)

    const expectedSize = 84 + 50 * triCount
    if (expectedSize > data.byteLength) {
        throw new Error(
            `Malformed binary STL: header claims ${triCount} triangles (needs ${expectedSize} bytes) but file is only ${data.byteLength} bytes`
        )
    }

    const verts = new Float32Array(triCount * 9)
    const normals = new Float32Array(triCount * 9)
    const indices = new Uint32Array(triCount * 3)

    let offset = 84
    for (let i = 0; i < triCount; i++) {
        const nx = view.getFloat32(offset, true)
        const ny = view.getFloat32(offset + 4, true)
        const nz = view.getFloat32(offset + 8, true)
        offset += 12

        const vBase = i * 9
        for (let v = 0; v < 3; v++) {
            const o = vBase + v * 3
            verts[o] = view.getFloat32(offset, true)
            verts[o + 1] = view.getFloat32(offset + 4, true)
            verts[o + 2] = view.getFloat32(offset + 8, true)
            normals[o] = nx
            normals[o + 1] = ny
            normals[o + 2] = nz
            offset += 12
        }

        offset += 2 // attribute byte count

        const base = i * 3
        indices[base] = base
        indices[base + 1] = base + 1
        indices[base + 2] = base + 2
    }

    return { verts, normals, indices, uv: new Float32Array((verts.length / 3) * 2) }
}

function parseASCIISTL(text: string): ParsedMesh {
    const verts: number[] = []
    const normals: number[] = []
    const indices: number[] = []

    let nx = 0,
        ny = 0,
        nz = 0
    let vertIndex = 0

    for (const line of text.split("\n")) {
        const trimmed = line.trim()
        if (trimmed.startsWith("facet normal")) {
            const parts = trimmed.split(/\s+/)
            nx = parseFloat(parts[2])
            ny = parseFloat(parts[3])
            nz = parseFloat(parts[4])
        } else if (trimmed.startsWith("vertex")) {
            const parts = trimmed.split(/\s+/)
            verts.push(parseFloat(parts[1]), parseFloat(parts[2]), parseFloat(parts[3]))
            normals.push(nx, ny, nz)
            indices.push(vertIndex++)
        }
    }

    return {
        verts: Float32Array.from(verts),
        normals: Float32Array.from(normals),
        indices: Uint32Array.from(indices),
        uv: new Float32Array((verts.length / 3) * 2),
    }
}

function isBinarySTL(data: Uint8Array): boolean {
    if (data.length < 84) return false
    // Check for ASCII "solid" header; some binary files also start with "solid", so verify via size
    const header = new TextDecoder().decode(data.slice(0, 5))
    if (header !== "solid") return true
    const triCount = new DataView(data.buffer, data.byteOffset).getUint32(80, true)
    const expectedSize = 84 + 50 * triCount
    return Math.abs(data.length - expectedSize) < 10
}

export function parseSTL(data: Uint8Array): ParsedMesh {
    return isBinarySTL(data) ? parseBinarySTL(data) : parseASCIISTL(new TextDecoder().decode(data))
}
