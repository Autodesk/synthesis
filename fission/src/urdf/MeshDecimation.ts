import { MeshoptSimplifier } from "meshoptimizer"
import type { ParsedMesh } from "./STLParser"

// Onshape's STL export tessellates every part far finer than a robot simulator needs - a plain
// rectangular FRC tube stock STL carries 83k triangles for a shape that needs a few hundred. That
// geometry is baked into the in-memory mirabuf.Assembly and duplicated per <link>, and it doubles
// as the Jolt collision surface (convex hull for dynamic bodies, concave BVH mesh for static
// ones), so it costs memory twice and shape-build time on top.
//
// Two things make this tricky, and both bit earlier attempts at this file:
//
// 1. STL is a triangle *soup* - every triangle owns its own 3 vertices, even where they coincide
//    exactly with a neighbour's. meshoptimizer builds its own position remap internally and treats
//    vertices that share a position as attribute "wedges"; a position with 3+ wedges is classified
//    as locked and can never be collapsed. Every vertex of a soup mesh has one wedge per incident
//    triangle, so *every* vertex ends up locked and simplify() returns the mesh untouched. Passing
//    a remapped index buffer is not enough - the duplicate positions must be gone from the vertex
//    buffer too, otherwise the internal remap still sees them. Hence weldPositions() below returns
//    a compacted buffer, not just a remap.
//
// 2. Triangle count is the wrong thing to ask for. Asking for "2% of the original triangles" on the
//    tube above collapses its 1.6mm walls: the result measured 47% of the original volume with 350
//    non-manifold and 700 wrongly-wound edges. Instead we give the simplifier an *absolute*
//    geometric error budget (ErrorAbsolute) and let it stop wherever that budget runs out, then
//    verify the result independently. Thin features survive because collapsing them costs more
//    error than the budget allows.
//
// Every result is checked against the input (see validate()) and the original mesh is returned
// unchanged if anything looks off. Correctness beats memory savings here.

// Below this triangle count the mesh is already cheap; leave it alone.
const TRIANGLE_THRESHOLD = 2000
// Absolute error budgets, in metres (URDF geometry is metric; the metres->cm scale happens later in
// URDFConverter). Tried loosest-first, stopping at the first result that passes validation.
//
// These are calibrated, not guessed. meshopt's error metric is a quadric estimate, not a true
// Hausdorff bound: measured against the original surface it undershoots by up to ~3.5x, so a 0.5mm
// budget is what actually keeps worst-case surface deviation under ~2mm across both sample kits
// (scripts/urdf-mem-debug/sweep-kits.mjs measures this directly - rerun it if these change).
//
// The bottom rungs exist for hardware: an FRC kit's heaviest STLs by triangle count are often
// screws (one file measured 111k triangles for a handful of button-head bolts), and a budget sized
// for a chassis tube eats a screw's thread and head taper - measurably, 15-20% of its volume. Those
// parts still give up 75-85% of their triangles once the budget is small enough to leave the threads
// alone, so it's worth walking down to 0.03mm rather than skipping them.
const ERROR_BUDGETS_M = [0.0005, 0.00025, 0.000125, 0.0000625, 0.00003125]
// ...but never spend more error than this fraction of the part's own bounding-box diagonal, so a
// 5mm spacer isn't handed a budget the size of itself.
const RELATIVE_BUDGET_CAP = 0.01
// Rejection thresholds for the simplified result, all measured against the welded input.
const MAX_VOLUME_DEVIATION = 0.03
const MAX_AREA_DEVIATION = 0.1
const MAX_AXIS_DEVIATION = 0.01
// Rays per axis for the solidity probe below, and the share of them allowed to come back
// inconsistent. Measured separation is wide: meshes with a folded surface score 0.4-1.7% while clean
// ones score 0-0.02% (the handful of non-zero rays on a clean mesh are ones that graze an edge).
const SOLIDITY_RESOLUTION = 64
const MAX_UNSOLID_RAY_RATIO = 0.001
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
}

// Collapses bit-identical positions into a compacted vertex buffer and reindexes. Exact comparison
// is enough in practice: on every Onshape STL checked, an exact weld already produces a closed,
// consistently-wound manifold (a tolerance-based weld found no additional merges), because the
// tessellator emits the same float bits for a shared corner in every facet that touches it.
function weldPositions(mesh: ParsedMesh): Welded {
    const remap = MeshoptSimplifier.generatePositionRemap(mesh.verts, 3)
    const sourceVertCount = mesh.verts.length / 3
    const compacted = new Uint32Array(sourceVertCount)
    let uniqueCount = 0
    for (let i = 0; i < sourceVertCount; i++) {
        if (remap[i] === i) compacted[i] = uniqueCount++
    }

    const verts = new Float32Array(uniqueCount * 3)
    for (let i = 0; i < sourceVertCount; i++) {
        if (remap[i] !== i) continue
        const out = compacted[i] * 3
        verts[out] = mesh.verts[i * 3]
        verts[out + 1] = mesh.verts[i * 3 + 1]
        verts[out + 2] = mesh.verts[i * 3 + 2]
    }

    const indices = new Uint32Array(mesh.indices.length)
    for (let i = 0; i < indices.length; i++) indices[i] = compacted[remap[mesh.indices[i]]]

    return { verts, indices }
}

interface MeshStats {
    /** Surface area. */
    area: number
    /** Divergence-theorem volume - only meaningful on a closed surface, which is the point: a hole
     * or a flipped patch shows up as a volume that no longer matches. */
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
    /** Connected shells. Onshape parts are routinely multi-body (a shaft plus its retaining ring);
     * fusing two of them together is the "merges features that are spatially close but
     * topologically disjoint" failure mode, and it shows up here as a drop. */
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

function measure({ verts, indices }: Welded): MeshStats {
    const vertCount = verts.length / 3
    const min = [Infinity, Infinity, Infinity]
    const max = [-Infinity, -Infinity, -Infinity]
    let area = 0
    let volume = 0
    let degenerateTriangles = 0

    // Edge keys are packed as `a * vertCount + b`, which stays exact in a double for any vertex
    // count a browser could hold.
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

    for (let i = 0; i < vertCount; i++) {
        for (let k = 0; k < 3; k++) {
            const value = verts[i * 3 + k]
            if (value < min[k]) min[k] = value
            if (value > max[k]) max[k] = value
        }
    }

    let boundaryEdges = 0
    let nonManifoldEdges = 0
    for (const count of undirected.values()) {
        if (count === 1) boundaryEdges++
        else if (count > 2) nonManifoldEdges++
    }

    const size: [number, number, number] = [max[0] - min[0], max[1] - min[1], max[2] - min[2]]
    return {
        area,
        volume,
        size,
        diagonal: Math.hypot(size[0], size[1], size[2]),
        boundaryEdges,
        nonManifoldEdges,
        reversedEdges,
        degenerateTriangles,
        shells: countShells(indices, vertCount),
    }
}

// Sweeps a grid of parallel rays down each axis and checks that every ray's surface crossings
// alternate enter/exit/enter/exit. That is the one property edge collapse can break while leaving
// every cheap check happy: on small doubly-curved thin features (a nylock nut's nylon insert crown
// was the case that found this) the simplified surface folds through itself, which keeps the mesh a
// closed, consistently-wound manifold with the correct volume, area and bounding box - and renders
// as a crumpled, partly see-through mess. Volume, area and dihedral angles all fail to distinguish
// it; ray parity separates it by more than an order of magnitude.
function unsolidRayRatio({ verts, indices }: Welded): number {
    const min = [Infinity, Infinity, Infinity]
    const max = [-Infinity, -Infinity, -Infinity]
    for (let i = 0; i < indices.length; i++) {
        for (let k = 0; k < 3; k++) {
            const value = verts[indices[i] * 3 + k]
            if (value < min[k]) min[k] = value
            if (value > max[k]) max[k] = value
        }
    }

    const triCount = indices.length / 3
    let rays = 0
    let bad = 0
    const hits: Array<[number, number]> = []

    for (let axis = 0; axis < 3; axis++) {
        const u = (axis + 1) % 3
        const v = (axis + 2) % 3
        const cellU = (max[u] - min[u]) / SOLIDITY_RESOLUTION
        const cellV = (max[v] - min[v]) / SOLIDITY_RESOLUTION
        if (!(cellU > 0) || !(cellV > 0)) continue

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

        for (const [key, tris] of buckets) {
            // Offset the ray from the cell corner by an irrational-looking fraction so it doesn't
            // land exactly on a shared edge, where it would be counted twice or not at all.
            const rayU = min[u] + (Math.floor(key / SOLIDITY_RESOLUTION) + 0.4531) * cellU
            const rayV = min[v] + ((key % SOLIDITY_RESOLUTION) + 0.6237) * cellV
            hits.length = 0

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
                // A triangle edge-on to the ray can't be classified as an entry or an exit.
                if (normalAlongAxis === 0) continue
                hits.push([
                    wa * verts[a + axis] + wb * verts[b + axis] + wc * verts[c + axis],
                    normalAlongAxis < 0 ? 1 : -1,
                ])
            }

            if (hits.length === 0) continue
            rays++
            hits.sort((x, y) => x[0] - y[0])
            let inside = 0
            for (const [, direction] of hits) {
                inside += direction
                // Depth outside {0, 1} means the ray left the solid before entering it, or entered
                // twice without leaving: the surface crosses itself.
                if (inside < 0 || inside > 1) {
                    bad++
                    inside = -1
                    break
                }
            }
            if (inside > 0) bad++
        }
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
// occupies the same space as the input. Volume is the sharpest of these: it catches both holes and
// collapsed thin walls, which surface area on its own does not (a tube whose walls have been merged
// keeps roughly the right area while losing half its volume).
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

// meshopt's compactMesh() rewrites the index buffer it is handed *in place* and returns the remap it
// applied, so the remap must not be applied to those indices a second time. Doing so leaves every
// triangle with three identical indices - a silently invisible mesh.
function compactVertices(source: Float32Array, indices: Uint32Array): Welded {
    const [remap, uniqueCount] = MeshoptSimplifier.compactMesh(indices)
    const verts = new Float32Array(uniqueCount * 3)
    for (let i = 0; i < remap.length; i++) {
        const out = remap[i]
        if (out === 0xffffffff) continue
        verts[out * 3] = source[i * 3]
        verts[out * 3 + 1] = source[i * 3 + 1]
        verts[out * 3 + 2] = source[i * 3 + 2]
    }
    return { verts, indices }
}

// Rebuilds shading data for the decimated mesh. Decimation invalidates STL's per-facet normals, and
// simply averaging all faces at a vertex rounds off machined edges. Instead, the faces around each
// vertex are grouped into clusters of similar orientation and each cluster gets its own vertex: a
// box keeps three crisp normals per corner, a tessellated cylinder keeps one smooth one.
function buildShadingData({ verts, indices }: Welded): ParsedMesh {
    const triCount = indices.length / 3
    const vertCount = verts.length / 3

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

    // Incident faces per vertex, CSR-style.
    const offsets = new Uint32Array(vertCount + 1)
    for (let i = 0; i < indices.length; i++) offsets[indices[i] + 1]++
    for (let i = 0; i < vertCount; i++) offsets[i + 1] += offsets[i]
    const incident = new Uint32Array(indices.length)
    const cursor = Uint32Array.from(offsets.subarray(0, vertCount))
    for (let t = 0; t < triCount; t++) {
        for (let k = 0; k < 3; k++) incident[cursor[indices[t * 3 + k]]++] = t
    }

    const outVerts: number[] = []
    const outNormals: number[] = []
    const outIndices = new Uint32Array(indices.length)
    const clusterOf = new Int32Array(triCount).fill(-1)

    for (let v = 0; v < vertCount; v++) {
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
        }
    }

    return {
        verts: Float32Array.from(outVerts),
        normals: Float32Array.from(outNormals),
        // STL carries no UV data; URDFConverter substitutes a shared zero-UV buffer sized to the
        // final vertex count, so leaving this empty is what it expects.
        uv: new Float32Array(0),
        indices: outIndices,
    }
}

/**
 * Reduces an over-tessellated STL mesh under a bounded geometric error budget. Returns the input
 * mesh unchanged if it is already small, if decimation isn't available, if the source isn't a clean
 * closed manifold, or if no budget on the ladder produces a result that survives validation.
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

        const result = compactVertices(welded.verts, simplified)
        if (!validate(original, measure(result))) continue

        // This check is very expensive, run it last.
        if (unsolidRayRatio(result) > MAX_UNSOLID_RAY_RATIO) continue

        return buildShadingData(result)
    }

    return mesh
}
