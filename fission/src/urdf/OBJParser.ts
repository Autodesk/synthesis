import type { ParsedMesh } from "./STLParser"

// https://en.wikipedia.org/wiki/Wavefront_.obj_file
export function parseOBJ(data: Uint8Array): ParsedMesh {
    const text = new TextDecoder().decode(data)
    const lines = text.split("\n")

    // First pass: tally counts so every buffer can be sized once instead of grown via push().
    let vcount = 0
    let ncount = 0
    let uvCount = 0
    let maxCorners = 0

    for (const line of lines) {
        const trimmed = line.trim()
        if (!trimmed) continue

        const tagEnd = trimmed.search(/\s/)
        const tag = tagEnd === -1 ? trimmed : trimmed.slice(0, tagEnd)

        switch (tag) {
            case "v":
                vcount++
                break
            case "vn":
                ncount++
                break
            case "vt":
                uvCount++
                break
            case "f": {
                const cornerCount =
                    tagEnd === -1
                        ? 0
                        : trimmed
                              .slice(tagEnd + 1)
                              .trim()
                              .split(/\s+/).length
                if (cornerCount >= 3) maxCorners += (cornerCount - 2) * 3
                break
            }
        }
    }

    const srcVerts = new Float32Array(vcount * 3)
    const srcNormals = new Float32Array(ncount * 3)
    const srcUVs = new Float32Array(uvCount * 2)

    // Upper bound: fan triangulation never produces more corners than counted above, but
    // vertex dedup means the unique vert/normal/uv buffers are trimmed to actual size at the end.
    const outVerts = new Float32Array(maxCorners * 3)
    const outNormals = new Float32Array(maxCorners * 3)
    const outUVs = new Float32Array(maxCorners * 2)
    const outIndices = new Uint32Array(maxCorners)

    let vi3 = 0
    let ni3 = 0
    let uv2 = 0
    let outCorner = 0
    let nextIndex = 0

    const cache = new Map<string, number>()

    function addVertex(vi: number, ni: number, ui: number) {
        const key = vi + "/" + ni + "/" + ui
        const cached = cache.get(key)
        if (cached !== undefined) {
            outIndices[outCorner++] = cached
            return
        }

        const vSeen = vi3 / 3
        const vIdx = vi < 0 ? vSeen + vi : vi - 1
        if (vIdx < 0 || vIdx >= vSeen) return
        const idx3 = vIdx * 3

        const nSeen = ni3 / 3
        const uvSeen = uv2 / 2
        const nIdxRaw = ni > 0 ? ni - 1 : ni < 0 ? nSeen + ni : -1
        const uIdxRaw = ui > 0 ? ui - 1 : ui < 0 ? uvSeen + ui : -1
        const nValid = nIdxRaw >= 0 && nIdxRaw < nSeen
        const uValid = uIdxRaw >= 0 && uIdxRaw < uvSeen

        const outBase3 = nextIndex * 3
        const outBase2 = nextIndex * 2

        outVerts[outBase3] = srcVerts[idx3]
        outVerts[outBase3 + 1] = srcVerts[idx3 + 1]
        outVerts[outBase3 + 2] = srcVerts[idx3 + 2]

        if (nValid) {
            const nIdx3 = nIdxRaw * 3
            outNormals[outBase3] = srcNormals[nIdx3]
            outNormals[outBase3 + 1] = srcNormals[nIdx3 + 1]
            outNormals[outBase3 + 2] = srcNormals[nIdx3 + 2]
        } else {
            outNormals[outBase3 + 2] = 1
        }

        if (uValid) {
            const uIdx2 = uIdxRaw * 2
            outUVs[outBase2] = srcUVs[uIdx2]
            outUVs[outBase2 + 1] = srcUVs[uIdx2 + 1]
        }

        cache.set(key, nextIndex)
        outIndices[outCorner++] = nextIndex
        nextIndex++
    }

    // Reused scratch buffer for face corners; only grows for the largest ngon encountered.
    const cornerScratch: [number, number, number][] = []

    for (const line of lines) {
        const trimmed = line.trim()
        if (!trimmed) continue
        const parts = trimmed.split(/\s+/)

        switch (parts[0]) {
            case "v":
                srcVerts[vi3++] = parseFloat(parts[1])
                srcVerts[vi3++] = parseFloat(parts[2])
                srcVerts[vi3++] = parseFloat(parts[3])
                break
            case "vn":
                srcNormals[ni3++] = parseFloat(parts[1])
                srcNormals[ni3++] = parseFloat(parts[2])
                srcNormals[ni3++] = parseFloat(parts[3])
                break
            case "vt":
                srcUVs[uv2++] = parseFloat(parts[1])
                srcUVs[uv2++] = parseFloat(parts[2] ?? "0")
                break
            case "f": {
                const cornerCount = parts.length - 1
                if (cornerCount < 3) break

                while (cornerScratch.length < cornerCount) cornerScratch.push([0, 0, 0])

                for (let i = 0; i < cornerCount; i++) {
                    const raw = parts[i + 1].split("/")
                    const c = cornerScratch[i]
                    c[0] = raw[0] ? parseInt(raw[0], 10) : 0
                    c[1] = raw[2] ? parseInt(raw[2], 10) : 0
                    c[2] = raw[1] ? parseInt(raw[1], 10) : 0
                }

                // Fan triangulation for quads/ngons
                // https://en.wikipedia.org/wiki/Polygon_triangulation#Convex_polygon_triangulation
                for (let i = 1; i < cornerCount - 1; i++) {
                    addVertex(cornerScratch[0][0], cornerScratch[0][1], cornerScratch[0][2])
                    addVertex(cornerScratch[i][0], cornerScratch[i][1], cornerScratch[i][2])
                    addVertex(cornerScratch[i + 1][0], cornerScratch[i + 1][1], cornerScratch[i + 1][2])
                }
                break
            }
        }
    }

    return {
        verts: outVerts.slice(0, nextIndex * 3),
        normals: outNormals.slice(0, nextIndex * 3),
        indices: outIndices.slice(0, outCorner),
        uv: outUVs.slice(0, nextIndex * 2),
    }
}
