import { mirabuf } from "@/proto/mirabuf"
import { URDF_AUTO_WHEEL_SOURCE, URDF_WHEEL_SOURCE_KEY } from "./URDFUserData"

const AXIS_PARALLEL_COS = 0.99 // axes must be this parallel to be the same axle direction
const AXLE_ALIGN_COS = 0.98 // wheel-to-wheel displacement must align with the axis
const COINCIDENT_DISTANCE = 1e-4 // metres, same-point guard
const COLLINEAR_DISTANCE = 0.01 // metres (1 cm), collinear midpoint tolerance

interface Vec3 {
    x: number
    y: number
    z: number
}
interface Vec2 {
    u: number
    v: number
}

const add3 = (a: Vec3, b: Vec3): Vec3 => ({ x: a.x + b.x, y: a.y + b.y, z: a.z + b.z })
const sub3 = (a: Vec3, b: Vec3): Vec3 => ({ x: a.x - b.x, y: a.y - b.y, z: a.z - b.z })
const scale3 = (a: Vec3, s: number): Vec3 => ({ x: a.x * s, y: a.y * s, z: a.z * s })
const dot3 = (a: Vec3, b: Vec3): number => a.x * b.x + a.y * b.y + a.z * b.z
const cross3 = (a: Vec3, b: Vec3): Vec3 => ({
    x: a.y * b.z - a.z * b.y,
    y: a.z * b.x - a.x * b.z,
    z: a.x * b.y - a.y * b.x,
})
const norm3 = (a: Vec3): number => Math.sqrt(dot3(a, a))
const normalize3 = (a: Vec3): Vec3 => {
    const n = norm3(a)
    return n < 1e-9 ? { x: 0, y: 0, z: 0 } : scale3(a, 1 / n)
}

function canonicalDir(v: Vec3): Vec3 {
    const n = normalize3(v)
    const ax = Math.abs(n.x),
        ay = Math.abs(n.y),
        az = Math.abs(n.z)
    const dominant = ax >= ay && ax >= az ? n.x : ay >= az ? n.y : n.z
    return dominant < 0 ? scale3(n, -1) : n
}

function perpendicularBasis(d: Vec3): [Vec3, Vec3] {
    const helper: Vec3 = Math.abs(d.x) < 0.9 ? { x: 1, y: 0, z: 0 } : { x: 0, y: 1, z: 0 }
    const u = normalize3(cross3(d, helper))
    const v = cross3(d, u)
    return [u, v]
}

function distToLine(p: Vec2, anchor: Vec2, dir: Vec2): number {
    const ap = { u: p.u - anchor.u, v: p.v - anchor.v }
    return Math.abs(ap.u * dir.v - ap.v * dir.u)
}

interface Candidate {
    token: string
    origin: Vec3 // metres
    axis: Vec3 // normalised
}

interface AxlePair {
    a: number
    b: number
    midpoint: Vec3
    direction: Vec3 // canonical
    key: string
}

function extractCandidates(jointDefs: Record<string, mirabuf.joint.IJoint>): Candidate[] {
    const out: Candidate[] = []
    for (const [token, jDef] of Object.entries(jointDefs)) {
        if (jDef.jointMotionType !== mirabuf.joint.JointMotion.REVOLUTE) continue
        const axisVec = jDef.rotational?.rotationalFreedom?.axis
        if (!axisVec) continue
        const axis = normalize3({ x: axisVec.x ?? 0, y: axisVec.y ?? 0, z: axisVec.z ?? 0 })
        if (norm3(axis) < 0.5) continue
        const o = jDef.origin ?? {}

        // Joint origins stored in cm (positionToYup * 100)
        out.push({ token, origin: { x: (o.x ?? 0) / 100, y: (o.y ?? 0) / 100, z: (o.z ?? 0) / 100 }, axis })
    }

    return out.sort((a, b) => (a.token < b.token ? -1 : 1))
}

function buildAxlePairs(candidates: Candidate[]): AxlePair[] {
    const pairs: AxlePair[] = []
    for (let i = 0; i < candidates.length; i++) {
        for (let j = i + 1; j < candidates.length; j++) {
            if (Math.abs(dot3(candidates[i].axis, candidates[j].axis)) < AXIS_PARALLEL_COS) continue
            const disp = sub3(candidates[j].origin, candidates[i].origin)
            const span = norm3(disp)
            if (span < COINCIDENT_DISTANCE) continue
            if (Math.abs(dot3(scale3(disp, 1 / span), candidates[i].axis)) < AXLE_ALIGN_COS) continue
            const ta = candidates[i].token
            const tb = candidates[j].token
            pairs.push({
                a: i,
                b: j,
                midpoint: scale3(add3(candidates[i].origin, candidates[j].origin), 0.5),
                direction: canonicalDir(candidates[i].axis),
                key: ta < tb ? `${ta}|${tb}` : `${tb}|${ta}`,
            })
        }
    }

    return pairs.sort((a, b) => (a.key < b.key ? -1 : 1))
}

function groupByDirection(pairs: AxlePair[]): { direction: Vec3; indices: number[] }[] {
    const groups: { direction: Vec3; indices: number[] }[] = []
    for (let i = 0; i < pairs.length; i++) {
        const g = groups.find(g => Math.abs(dot3(g.direction, pairs[i].direction)) >= AXIS_PARALLEL_COS)
        if (g) g.indices.push(i)
        else groups.push({ direction: pairs[i].direction, indices: [i] })
    }

    return groups
}

function largestCollinearSet(points: Vec2[]): number[] {
    if (points.length <= 2) return points.map((_, i) => i)
    let best: number[] = []
    for (let i = 0; i < points.length; i++) {
        for (let j = i + 1; j < points.length; j++) {
            const du = points[j].u - points[i].u
            const dv = points[j].v - points[i].v
            const len = Math.sqrt(du * du + dv * dv)
            if (len < 1e-9) continue
            const dir: Vec2 = { u: du / len, v: dv / len }
            const onLine = points.reduce<number[]>((acc, p, k) => {
                if (distToLine(p, points[i], dir) <= COLLINEAR_DISTANCE) acc.push(k)
                return acc
            }, [])

            if (onLine.length > best.length) best = onLine
        }
    }

    return best
}

export function detectAndTagWheels(assembly: mirabuf.Assembly): void {
    const jointDefs = assembly.data?.joints?.jointDefinitions as Record<string, mirabuf.joint.IJoint> | undefined
    if (!jointDefs) return

    const candidates = extractCandidates(jointDefs)
    if (candidates.length < 2) return

    const pairs = buildAxlePairs(candidates)
    if (pairs.length === 0) return

    const groups = groupByDirection(pairs)

    let selected: number[] = []
    for (const group of groups) {
        const [u, v] = perpendicularBasis(group.direction)
        const midpoints: Vec2[] = group.indices.map(idx => ({
            u: dot3(pairs[idx].midpoint, u),
            v: dot3(pairs[idx].midpoint, v),
        }))
        const collinear = largestCollinearSet(midpoints)
        if (collinear.length > selected.length) selected = collinear.map(local => group.indices[local])
    }

    if (selected.length === 0) return

    for (const idx of selected) {
        for (const token of [candidates[pairs[idx].a].token, candidates[pairs[idx].b].token]) {
            const jDef = jointDefs[token]
            if (!jDef) continue
            if (!jDef.userData) jDef.userData = { data: {} }
            jDef.userData.data ??= {}
            jDef.userData.data["wheel"] = "true"
            jDef.userData.data["wheelType"] = "0"
            jDef.userData.data[URDF_WHEEL_SOURCE_KEY] = URDF_AUTO_WHEEL_SOURCE
        }
    }
}
