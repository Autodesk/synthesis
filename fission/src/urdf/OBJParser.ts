import type { ParsedMesh } from "./STLParser"

// https://en.wikipedia.org/wiki/Wavefront_.obj_file
export function parseOBJ(data: Uint8Array): ParsedMesh {
    const text = new TextDecoder().decode(data)

    const srcVerts: [number, number, number][] = []
    const srcNormals: [number, number, number][] = []
    const srcUVs: [number, number][] = []

    const outVerts: number[] = []
    const outNormals: number[] = []
    const outUVs: number[] = []
    const outIndices: number[] = []

    const cache = new Map<string, number>()
    let nextIndex = 0

    function addVertex(vi: number, ni: number, ui: number) {
        const key = `${vi}/${ni}/${ui}`
        const cached = cache.get(key)
        if (cached !== undefined) {
            outIndices.push(cached)
            return
        }

        const idx = vi < 0 ? srcVerts.length + vi : vi - 1
        const v = srcVerts[idx]
        if (!v) return

        const nIdx = ni > 0 ? ni - 1 : ni < 0 ? srcNormals.length + ni : -1
        const uIdx = ui > 0 ? ui - 1 : ui < 0 ? srcUVs.length + ui : -1

        outVerts.push(...v)
        outNormals.push(...(nIdx >= 0 && srcNormals[nIdx] ? srcNormals[nIdx] : [0, 0, 1]))
        outUVs.push(...(uIdx >= 0 && srcUVs[uIdx] ? srcUVs[uIdx] : [0, 0]))

        cache.set(key, nextIndex)
        outIndices.push(nextIndex++)
    }

    for (const line of text.split("\n")) {
        const parts = line.trim().split(/\s+/)
        switch (parts[0]) {
            case "v":
                srcVerts.push([parseFloat(parts[1]), parseFloat(parts[2]), parseFloat(parts[3])])
                break
            case "vn":
                srcNormals.push([parseFloat(parts[1]), parseFloat(parts[2]), parseFloat(parts[3])])
                break
            case "vt":
                srcUVs.push([parseFloat(parts[1]), parseFloat(parts[2] ?? "0")])
                break
            case "f": {
                const face = parts.slice(1).map(p => {
                    const [vi, ui, ni] = p.split("/").map(x => (x ? parseInt(x) : 0))
                    return { vi: vi ?? 0, ui: ui ?? 0, ni: ni ?? 0 }
                })

                // Fan triangulation for quads/ngons
                // https://en.wikipedia.org/wiki/Polygon_triangulation#Convex_polygon_triangulation
                for (let i = 1; i < face.length - 1; i++) {
                    addVertex(face[0].vi, face[0].ni, face[0].ui)
                    addVertex(face[i].vi, face[i].ni, face[i].ui)
                    addVertex(face[i + 1].vi, face[i + 1].ni, face[i + 1].ui)
                }
                break
            }
        }
    }

    return {
        verts: Float32Array.from(outVerts),
        normals: Float32Array.from(outNormals),
        indices: Uint32Array.from(outIndices),
        uv: Float32Array.from(outUVs),
    }
}
