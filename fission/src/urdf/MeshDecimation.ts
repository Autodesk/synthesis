import { MeshoptSimplifier } from "meshoptimizer"
import type { ParsedMesh } from "./STLParser"

// Mesh decimation for URDF STL/OBJ/glTF imports.
// This decimation step takes the 2026 kitbot import size from 3.11GB to 117MB.

// Every result is checked against the input (see validate()) and the original mesh is returned
// unchanged if anything looks off. Correctness beats memory savings here.

// Below this triangle count the mesh is already cheap; leave it alone.
const TRIANGLE_THRESHOLD = 2000

// Absolute error budgets in metres 
//
// These are calibrated values from the 2026 FRC kitbot. These values keep the measured surface
// within 0.5mm of the original while still having a triangle reduction count of about 87.1%.
//
// Bottom rungs exist for hardware like screws. Some of these parts have measured over 100k triangles
// regardless of how small they are so it's worth waking down to 0.03mm to avoid skipping those meshes.
const ERROR_BUDGETS_M = [0.0005, 0.00025, 0.000125, 0.0000625, 0.00003125]
// ...but never spend more error than this fraction of the part's own bounding-box diagonal, so a
// 5mm spacer isn't handed a budget the size of itself.
const RELATIVE_BUDGET_CAP = 0.01

// Rejection thresholds for the simplified result, all measured against the welded input.
const MAX_VOLUME_DEVIATION = 0.03
const MAX_AREA_DEVIATION = 0.1
const MAX_AXIS_DEVIATION = 0.01

const SOLIDITY_RESOLUTION = 64 // grid density per axis
const MAX_UNSOLID_RAY_RATIO = 0.001 // bad_rays / total_rays. fail any 0.1%

// Rebuilding the vertex/normal/index buffers isn't free, so don't bother for a marginal win.
const MIN_REDUCTION = 0.25

// Faces meeting at a sharper angle than this get split normals, so a machined edge stays crisp
// while a tessellated cylinder still shades smoothly at the reduced triangle count.
const CREASE_COS = Math.cos((35 * Math.PI) / 180)

export async function readyMeshDecimation(): Promise<void> {
    await MeshoptSimplifier.ready
}

interface Welded {
    verts: Float32Array
    indices: Uint32Array
    /** Per-vertex UV, 2 floats per entry in `verts`. Zero-length when the source mesh carries no UV
     * (STL, or a glTF/OBJ mesh with no texture coordinates) - skipped rather than carried as zeros
     * so the STL path does no extra work. */
    uv: Float32Array
}

// Collapses bit-identical positions into a compacted vertex buffer and reindexes
function weldPositions(mesh: ParsedMesh): Welded {
    const remap = MeshoptSimplifier.generatePositionRemap(mesh.verts, 3)
    const sourceVertCount = mesh.verts.length / 3
    const compacted = new Uint32Array(sourceVertCount)
    let uniqueCount = 0
    for (let i = 0; i < sourceVertCount; i++) {
        if (remap[i] === i) compacted[i] = uniqueCount++
    }

    const verts = new Float32Array(uniqueCount * 3)
    const hasUV = mesh.uv.length > 0
    const uv = new Float32Array(hasUV ? uniqueCount * 2 : 0)
    for (let i = 0; i < sourceVertCount; i++) {
        if (remap[i] !== i) continue
        const out = compacted[i] * 3
        verts[out] = mesh.verts[i * 3]
        verts[out + 1] = mesh.verts[i * 3 + 1]
        verts[out + 2] = mesh.verts[i * 3 + 2]
        if (hasUV) {
            const outUV = compacted[i] * 2
            uv[outUV] = mesh.uv[i * 2]
            uv[outUV + 1] = mesh.uv[i * 2 + 1]
        }
    }

    const indices = new Uint32Array(mesh.indices.length)
    for (let i = 0; i < indices.length; i++) indices[i] = compacted[remap[mesh.indices[i]]]

    return { verts, indices, uv }
}

interface MeshStats {
    /** Surface area. */
    area: number
    /** Divergence-theorem volume. Only meaningful on a closed surface. */
    volume: number
    /** Per-axis bounding box extent. */
    size: [number, number, number]
    /** Bounding box diagonal. */
    diagonal: number
    /** Edges used by exactly one triangle: holes. */
    boundaryEdges: number
    /** Edges used by three or more triangles: pinched/self-touching surface. */
    nonManifoldEdges: number
    /** Directed edges traversed twice: neighbouring triangles wound opposite ways. */
    reversedEdges: number
    /** Triangles with a repeated index or zero area. */
    degenerateTriangles: number
    /** Connected shells. URDF parts are routinely multi-body (a shaft plus its retaining ring).
     * Use this count to avoid accidentally merging two topologically disjoint components. */
    shells: number
}

function countShells(indices: Uint32Array, vertCount: number): number {
    const parent = new Uint32Array(vertCount)
    for (let i = 0; i < vertCount; i++) parent[i] = i
    const find = (x: number): number => {
        while (parent[x] !== x) {
            parent[x] = parent[parent[x]]
            x = parent[x]
        }
        return x
    }

    const used = new Uint8Array(vertCount)
    for (let i = 0; i < indices.length; i += 3) {
        for (let k = 0; k < 3; k++) used[indices[i + k]] = 1
        for (let k = 1; k < 3; k++) {
            const a = find(indices[i])
            const b = find(indices[i + k])
            if (a !== b) parent[a] = b
        }
    }

    const roots = new Set<number>()
    for (let i = 0; i < vertCount; i++) if (used[i]) roots.add(find(i))
    return roots.size
}

interface EdgeStats {
    area: number
    volume: number
    degenerateTriangles: number
    boundaryEdges: number
    nonManifoldEdges: number
    reversedEdges: number
}

// Walks every triangle once, accumulating signed area/volume and edge-adjacency counts. Edge keys
// are packed as `a * vertCount + b`
function collectEdgeStats(verts: Float32Array, indices: Uint32Array, vertCount: number): EdgeStats {
    let area = 0
    let volume = 0
    let degenerateTriangles = 0
    const undirected = new Map<number, number>()
    const directed = new Set<number>()
    let reversedEdges = 0

    for (let t = 0; t < indices.length; t += 3) {
        const i0 = indices[t]
        const i1 = indices[t + 1]
        const i2 = indices[t + 2]
        if (i0 === i1 || i1 === i2 || i0 === i2) {
            degenerateTriangles++
            continue
        }

        const a = i0 * 3
        const b = i1 * 3
        const c = i2 * 3
        const ux = verts[b] - verts[a]
        const uy = verts[b + 1] - verts[a + 1]
        const uz = verts[b + 2] - verts[a + 2]
        const vx = verts[c] - verts[a]
        const vy = verts[c + 1] - verts[a + 1]
        const vz = verts[c + 2] - verts[a + 2]
        const nx = uy * vz - uz * vy
        const ny = uz * vx - ux * vz
        const nz = ux * vy - uy * vx
        const twiceArea = Math.hypot(nx, ny, nz)
        if (twiceArea === 0) {
            degenerateTriangles++
            continue
        }

        area += 0.5 * twiceArea
        volume +=
            (verts[a] * (verts[b + 1] * verts[c + 2] - verts[b + 2] * verts[c + 1]) -
                verts[a + 1] * (verts[b] * verts[c + 2] - verts[b + 2] * verts[c]) +
                verts[a + 2] * (verts[b] * verts[c + 1] - verts[b + 1] * verts[c])) /
            6

        for (const [u, v] of [
            [i0, i1],
            [i1, i2],
            [i2, i0],
        ]) {
            const forward = u * vertCount + v
            if (directed.has(forward)) reversedEdges++
            else directed.add(forward)
            const key = u < v ? forward : v * vertCount + u
            undirected.set(key, (undirected.get(key) ?? 0) + 1)
        }
    }

    let boundaryEdges = 0
    let nonManifoldEdges = 0
    for (const count of undirected.values()) {
        if (count === 1) boundaryEdges++
        else if (count > 2) nonManifoldEdges++
    }

    return { area, volume, degenerateTriangles, boundaryEdges, nonManifoldEdges, reversedEdges }
}

function computeVertexBounds(verts: Float32Array): { size: [number, number, number]; diagonal: number } {
    const min = [Infinity, Infinity, Infinity]
    const max = [-Infinity, -Infinity, -Infinity]
    const vertCount = verts.length / 3
    for (let i = 0; i < vertCount; i++) {
        for (let k = 0; k < 3; k++) {
            const value = verts[i * 3 + k]
            if (value < min[k]) min[k] = value
            if (value > max[k]) max[k] = value
        }
    }

    const size: [number, number, number] = [max[0] - min[0], max[1] - min[1], max[2] - min[2]]
    return { size, diagonal: Math.hypot(size[0], size[1], size[2]) }
}

function measure({ verts, indices }: Welded): MeshStats {
    const vertCount = verts.length / 3
    return {
        ...collectEdgeStats(verts, indices, vertCount),
        ...computeVertexBounds(verts),
        shells: countShells(indices, vertCount),
    }
}

interface AxisBounds {
    min: [number, number, number]
    max: [number, number, number]
}

function computeIndexedBounds(verts: Float32Array, indices: Uint32Array): AxisBounds {
    const min: [number, number, number] = [Infinity, Infinity, Infinity]
    const max: [number, number, number] = [-Infinity, -Infinity, -Infinity]
    for (let i = 0; i < indices.length; i++) {
        for (let k = 0; k < 3; k++) {
            const value = verts[indices[i] * 3 + k]
            if (value < min[k]) min[k] = value
            if (value > max[k]) max[k] = value
        }
    }

    return { min, max }
}

// Assigns each triangle to every grid cell its projected bounding box overlaps, so a ray cast from
// a cell only has to test the triangles that could plausibly cover it.
function bucketTrianglesByCell(
    u: number,
    v: number,
    bounds: AxisBounds,
    cellU: number,
    cellV: number,
    verts: Float32Array,
    indices: Uint32Array
): Map<number, number[]> {
    const { min } = bounds
    const triCount = indices.length / 3
    const buckets = new Map<number, number[]>()
    for (let t = 0; t < triCount; t++) {
        const a = indices[t * 3] * 3
        const b = indices[t * 3 + 1] * 3
        const c = indices[t * 3 + 2] * 3
        const loU = Math.floor((Math.min(verts[a + u], verts[b + u], verts[c + u]) - min[u]) / cellU)
        const hiU = Math.floor((Math.max(verts[a + u], verts[b + u], verts[c + u]) - min[u]) / cellU)
        const loV = Math.floor((Math.min(verts[a + v], verts[b + v], verts[c + v]) - min[v]) / cellV)
        const hiV = Math.floor((Math.max(verts[a + v], verts[b + v], verts[c + v]) - min[v]) / cellV)
        for (let iu = Math.max(0, loU); iu <= Math.min(SOLIDITY_RESOLUTION - 1, hiU); iu++) {
            for (let iv = Math.max(0, loV); iv <= Math.min(SOLIDITY_RESOLUTION - 1, hiV); iv++) {
                const key = iu * SOLIDITY_RESOLUTION + iv
                const bucket = buckets.get(key)
                if (bucket) bucket.push(t)
                else buckets.set(key, [t])
            }
        }
    }

    return buckets
}

// Casts one ray through its candidate triangles and returns each crossing as [position along the
// ray axis, +1/-1 direction]. A triangle edge-on to the ray can't be classified as an entry or an
// exit and is skipped.
function castRay(
    rayU: number,
    rayV: number,
    axis: number,
    u: number,
    v: number,
    tris: number[],
    verts: Float32Array,
    indices: Uint32Array
): Array<[number, number]> {
    const hits: Array<[number, number]> = []
    for (const t of tris) {
        const a = indices[t * 3] * 3
        const b = indices[t * 3 + 1] * 3
        const c = indices[t * 3 + 2] * 3
        const au = verts[a + u]
        const av = verts[a + v]
        const bu = verts[b + u]
        const bv = verts[b + v]
        const cu = verts[c + u]
        const cv = verts[c + v]
        const doubleArea = (bu - au) * (cv - av) - (cu - au) * (bv - av)
        if (doubleArea === 0) continue
        const wc = ((bu - au) * (rayV - av) - (rayU - au) * (bv - av)) / doubleArea
        const wa = ((cu - bu) * (rayV - bv) - (rayU - bu) * (cv - bv)) / doubleArea
        const wb = ((au - cu) * (rayV - cv) - (rayU - cu) * (av - cv)) / doubleArea
        if (wa <= 0 || wb <= 0 || wc <= 0) continue

        const ux = verts[b] - verts[a]
        const uy = verts[b + 1] - verts[a + 1]
        const uz = verts[b + 2] - verts[a + 2]
        const vx = verts[c] - verts[a]
        const vy = verts[c + 1] - verts[a + 1]
        const vz = verts[c + 2] - verts[a + 2]
        const normalAlongAxis = [uy * vz - uz * vy, uz * vx - ux * vz, ux * vy - uy * vx][axis]
        if (normalAlongAxis === 0) continue
        hits.push([
            wa * verts[a + axis] + wb * verts[b + axis] + wc * verts[c + axis],
            normalAlongAxis < 0 ? 1 : -1,
        ])
    }

    return hits
}

// A ray through a solid should alternate outside/inside/outside as it crosses the surface, so the
// running enter/exit tally should stay within {0, 1} and return to 0. Anything else means the
// surface crosses itself along this ray
function isRayInconsistent(hits: Array<[number, number]>): boolean {
    hits.sort((x, y) => x[0] - y[0])
    let inside = 0
    for (const [, direction] of hits) {
        inside += direction
        if (inside < 0 || inside > 1) return true
    }

    return inside > 0
}

function countUnsolidRaysOnAxis(
    axis: number,
    verts: Float32Array,
    indices: Uint32Array,
    bounds: AxisBounds
): { rays: number; bad: number } {
    const u = (axis + 1) % 3
    const v = (axis + 2) % 3
    const cellU = (bounds.max[u] - bounds.min[u]) / SOLIDITY_RESOLUTION
    const cellV = (bounds.max[v] - bounds.min[v]) / SOLIDITY_RESOLUTION
    if (!(cellU > 0) || !(cellV > 0)) return { rays: 0, bad: 0 }

    const buckets = bucketTrianglesByCell(u, v, bounds, cellU, cellV, verts, indices)

    let rays = 0
    let bad = 0
    for (const [key, tris] of buckets) {
        // Offset the ray from the cell corner by an irrational-looking fraction so it doesn't land
        // exactly on a shared edge, where it would be counted twice or not at all.
        const rayU = bounds.min[u] + (Math.floor(key / SOLIDITY_RESOLUTION) + 0.4531) * cellU
        const rayV = bounds.min[v] + ((key % SOLIDITY_RESOLUTION) + 0.6237) * cellV
        const hits = castRay(rayU, rayV, axis, u, v, tris, verts, indices)
        if (hits.length === 0) continue
        rays++
        if (isRayInconsistent(hits)) bad++
    }

    return { rays, bad }
}

// Sweeps a grid of parallel rays down each axis and checks that every ray's surface crossings
// alternate enter/exit/enter/exit. 
//
// This can be a very expensive check to run however its needed. Every other lightweight check
// that fully covers a venn diagram of different failure modes simply can't replace this validation step.
//
// Every cheap check could be happy, on a small doubly-curved thin feature the simplified surface could fold
// through itself, which keeps the mesh a closed, consistently-wound manifold with the correct volume, 
// area and bounding box, and render compleatly wrong as a crumpled partly see-through mess. Volume, 
// area and dihedral angles all will fail to distinguish it from the orignial geometry
function unsolidRayRatio({ verts, indices }: Welded): number {
    const bounds = computeIndexedBounds(verts, indices)
    let rays = 0
    let bad = 0
    for (let axis = 0; axis < 3; axis++) {
        const result = countUnsolidRaysOnAxis(axis, verts, indices, bounds)
        rays += result.rays
        bad += result.bad
    }

    return rays > 0 ? bad / rays : 1
}

function isClosedManifold(stats: MeshStats): boolean {
    return (
        stats.boundaryEdges === 0 &&
        stats.nonManifoldEdges === 0 &&
        stats.reversedEdges === 0 &&
        stats.degenerateTriangles === 0
    )
}

// A simplified mesh is only accepted if it is still a closed, consistently-wound manifold and still
// occupies the same space as the input.
function validate(original: MeshStats, simplified: MeshStats): boolean {
    if (!isClosedManifold(simplified)) return false
    if (simplified.shells !== original.shells) return false
    if (original.volume <= 0) return false
    if (Math.abs(simplified.volume - original.volume) / original.volume > MAX_VOLUME_DEVIATION) return false
    if (original.area > 0 && Math.abs(simplified.area - original.area) / original.area > MAX_AREA_DEVIATION) {
        return false
    }

    for (let k = 0; k < 3; k++) {
        const extent = original.size[k]
        if (extent > 0 && Math.abs(simplified.size[k] - extent) / extent > MAX_AXIS_DEVIATION) return false
    }

    return true
}

function compactVertices(source: Float32Array, sourceUV: Float32Array, indices: Uint32Array): Welded {
    const [remap, uniqueCount] = MeshoptSimplifier.compactMesh(indices)
    const verts = new Float32Array(uniqueCount * 3)
    const hasUV = sourceUV.length > 0
    const uv = new Float32Array(hasUV ? uniqueCount * 2 : 0)
    for (let i = 0; i < remap.length; i++) {
        const out = remap[i]
        if (out === 0xffffffff) continue
        verts[out * 3] = source[i * 3]
        verts[out * 3 + 1] = source[i * 3 + 1]
        verts[out * 3 + 2] = source[i * 3 + 2]
        if (hasUV) {
            uv[out * 2] = sourceUV[i * 2]
            uv[out * 2 + 1] = sourceUV[i * 2 + 1]
        }
    }

    return { verts, indices, uv }
}

function computeFaceNormals(verts: Float32Array, indices: Uint32Array, triCount: number): Float32Array {
    const faceNormals = new Float32Array(triCount * 3)
    for (let t = 0; t < triCount; t++) {
        const a = indices[t * 3] * 3
        const b = indices[t * 3 + 1] * 3
        const c = indices[t * 3 + 2] * 3
        const ux = verts[b] - verts[a]
        const uy = verts[b + 1] - verts[a + 1]
        const uz = verts[b + 2] - verts[a + 2]
        const vx = verts[c] - verts[a]
        const vy = verts[c + 1] - verts[a + 1]
        const vz = verts[c + 2] - verts[a + 2]

        // Left unnormalized: the magnitude is twice the triangle area, which is exactly the weight
        // we want when averaging a cluster.
        faceNormals[t * 3] = uy * vz - uz * vy
        faceNormals[t * 3 + 1] = uz * vx - ux * vz
        faceNormals[t * 3 + 2] = ux * vy - uy * vx
    }

    return faceNormals
}

interface IncidentFaces {
    offsets: Uint32Array
    incident: Uint32Array
}

// Incident faces per vertex, CSR-style.
function buildIncidentFacesCSR(indices: Uint32Array, vertCount: number, triCount: number): IncidentFaces {
    const offsets = new Uint32Array(vertCount + 1)
    for (let i = 0; i < indices.length; i++) offsets[indices[i] + 1]++
    for (let i = 0; i < vertCount; i++) offsets[i + 1] += offsets[i]
    const incident = new Uint32Array(indices.length)
    const cursor = Uint32Array.from(offsets.subarray(0, vertCount))
    for (let t = 0; t < triCount; t++) {
        for (let k = 0; k < 3; k++) incident[cursor[indices[t * 3 + k]]++] = t
    }

    return { offsets, incident }
}

// Groups the faces around one vertex into clusters of similar orientation (within CREASE_COS) and
// appends one output vertex/normal per cluster, then repoints each face's corner index at that
// cluster
function clusterFacesAroundVertex(
    v: number,
    verts: Float32Array,
    uv: Float32Array,
    indices: Uint32Array,
    faceNormals: Float32Array,
    { offsets, incident }: IncidentFaces,
    clusterOf: Int32Array,
    outVerts: number[],
    outNormals: number[],
    outUV: number[],
    outIndices: Uint32Array
): void {
    const start = offsets[v]
    const end = offsets[v + 1]
    for (let i = start; i < end; i++) clusterOf[incident[i]] = -1

    for (let i = start; i < end; i++) {
        const seed = incident[i]
        if (clusterOf[seed] !== -1) continue
        const sx = faceNormals[seed * 3]
        const sy = faceNormals[seed * 3 + 1]
        const sz = faceNormals[seed * 3 + 2]
        const seedLen = Math.hypot(sx, sy, sz) || 1

        const newIndex = outVerts.length / 3
        let nx = 0
        let ny = 0
        let nz = 0
        for (let j = i; j < end; j++) {
            const face = incident[j]
            if (clusterOf[face] !== -1) continue
            const fx = faceNormals[face * 3]
            const fy = faceNormals[face * 3 + 1]
            const fz = faceNormals[face * 3 + 2]
            const faceLen = Math.hypot(fx, fy, fz) || 1
            const alignment = (sx * fx + sy * fy + sz * fz) / (seedLen * faceLen)
            if (alignment < CREASE_COS) continue
            clusterOf[face] = newIndex
            nx += fx
            ny += fy
            nz += fz
            for (let k = 0; k < 3; k++) {
                if (indices[face * 3 + k] === v) outIndices[face * 3 + k] = newIndex
            }
        }

        const len = Math.hypot(nx, ny, nz) || 1
        outVerts.push(verts[v * 3], verts[v * 3 + 1], verts[v * 3 + 2])
        outNormals.push(nx / len, ny / len, nz / len)
        if (uv.length > 0) outUV.push(uv[v * 2], uv[v * 2 + 1])
    }
}

// Rebuilds shading data for the decimated mesh. Decimation invalidates STL's per-facet normals, and
// simply averaging all faces at a vertex rounds off machined edges. Instead, the faces around each
// vertex are grouped into clusters of similar orientation and each cluster gets its own vertex (see
// clusterFacesAroundVertex above).
function buildShadingData({ verts, indices, uv }: Welded): ParsedMesh {
    const triCount = indices.length / 3
    const vertCount = verts.length / 3

    const faceNormals = computeFaceNormals(verts, indices, triCount)
    const incidentFaces = buildIncidentFacesCSR(indices, vertCount, triCount)

    const outVerts: number[] = []
    const outNormals: number[] = []
    const outUV: number[] = []
    const outIndices = new Uint32Array(indices.length)
    const clusterOf = new Int32Array(triCount).fill(-1)

    for (let v = 0; v < vertCount; v++) {
        clusterFacesAroundVertex(
            v,
            verts,
            uv,
            indices,
            faceNormals,
            incidentFaces,
            clusterOf,
            outVerts,
            outNormals,
            outUV,
            outIndices
        )
    }

    return {
        verts: Float32Array.from(outVerts),
        normals: Float32Array.from(outNormals),
        uv: Float32Array.from(outUV), // empty when the source mesh (STL, or UV-less glTF/OBJ) carried none
        indices: outIndices,
    }
}

/**
 * Reduces an over-tessellated mesh under a bounded geometric error budget. Returns the input
 * mesh unchanged if it is already small, if decimation isn't available, if the source isn't a clean
 * closed manifold, or if no budget on the ladder produces a result that survives validation - the
 * ladder is calibrated against STL tessellation density, so a denser glTF/OBJ export of the same
 * part is more likely to fall through every rung and come back untouched (safe, just less reduction).
 */
export function decimateMesh(mesh: ParsedMesh): ParsedMesh {
    const triangleCount = mesh.indices.length / 3
    if (triangleCount <= TRIANGLE_THRESHOLD || !MeshoptSimplifier.supported) return mesh

    const welded = weldPositions(mesh)
    const original = measure(welded)

    // Simplifying an already-broken mesh (open shell, self-touching surface) means validation can't
    // tell what the simplifier did from what was wrong to begin with. Leave those alone.
    if (!isClosedManifold(original) || original.volume <= 0) return mesh

    for (const budget of ERROR_BUDGETS_M) {
        const error = Math.min(budget, original.diagonal * RELATIVE_BUDGET_CAP)
        // Target index count is deliberately floored rather than budgeted: the error bound is what
        // decides how far this goes.
        const [simplified] = MeshoptSimplifier.simplify(welded.indices, welded.verts, 3, 12, error, [
            "ErrorAbsolute",
            "LockBorder",
        ])
        if (simplified.length < 12) continue
        if (simplified.length / 3 > triangleCount * (1 - MIN_REDUCTION)) continue

        const result = compactVertices(welded.verts, welded.uv, simplified)
        if (!validate(original, measure(result))) continue

        // This check is very expensive, run it last.
        if (unsolidRayRatio(result) > MAX_UNSOLID_RAY_RATIO) continue

        return buildShadingData(result)
    }

    return mesh
}
