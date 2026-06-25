import { mirabuf } from "@/proto/mirabuf"
import { URDF_AUTO_WHEEL_SOURCE, URDF_WHEEL_SOURCE_KEY } from "./URDFUserData"

// Direct TypeScript port of branp/158/cpp-wheel-detection: wheels.cpp
// Detects revolute-joint pairs that form a drivetrain and tags them as wheels
// in jointDefinition.userData so the physics system treats them correctly.

const AXIS_PARALLEL_COS = 0.99 // axes must be this parallel to be the same axle direction
const AXLE_ALIGN_COS = 0.98 // wheel-to-wheel displacement must align with the axis
const COINCIDENT_DISTANCE = 1e-4 // metres — same-point guard
const COLLINEAR_DISTANCE = 0.01 // metres (1 cm) — collinear midpoint tolerance

// --- Minimal vector math (mirrors the anonymous namespace in wheels.cpp) ---

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

// --- Data structures ---

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

// --- Algorithm stages (direct ports of the C++ free functions) ---

function extractCandidates(jointDefs: Record<string, mirabuf.joint.IJoint>): Candidate[] {
    const out: Candidate[] = []
    for (const [token, jDef] of Object.entries(jointDefs)) {
        if (jDef.jointMotionType !== mirabuf.joint.JointMotion.REVOLUTE) continue
        const axisVec = jDef.rotational?.rotationalFreedom?.axis
        if (!axisVec) continue
        const axis = normalize3({ x: axisVec.x ?? 0, y: axisVec.y ?? 0, z: axisVec.z ?? 0 })
        if (norm3(axis) < 0.5) continue
        const o = jDef.origin ?? {}
        // Joint origins stored in cm (positionToYup * 100); convert to metres for the algorithm
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
            const ta = candidates[i].token,
                tb = candidates[j].token
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
            const du = points[j].u - points[i].u,
                dv = points[j].v - points[i].v
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

// --- Logging helper ---

const fv = (v: Vec3) => `(${v.x.toFixed(3)}, ${v.y.toFixed(3)}, ${v.z.toFixed(3)})`

// --- Public entry point ---

export function detectAndTagWheels(assembly: mirabuf.Assembly): void {
    const jointDefs = assembly.data?.joints?.jointDefinitions as Record<string, mirabuf.joint.IJoint> | undefined
    if (!jointDefs) {
        console.log("[WheelDetect] No joint definitions found — skipping")
        return
    }

    console.group("[WheelDetect] Wheel detection")

    const candidates = extractCandidates(jointDefs)
    console.log(`[WheelDetect] Revolute candidates (${candidates.length}):`)
    for (const c of candidates) {
        console.log(`[WheelDetect]   "${c.token}"  origin=${fv(c.origin)}m  axis=${fv(c.axis)}`)
    }

    if (candidates.length < 2) {
        console.log("[WheelDetect] Fewer than 2 revolute joints — no drivetrain possible")
        console.groupEnd()
        return
    }

    const pairs = buildAxlePairs(candidates)
    console.log(`[WheelDetect] Axle pairs (${pairs.length}):`)
    for (const p of pairs) {
        console.log(
            `[WheelDetect]   "${candidates[p.a].token}" ↔ "${candidates[p.b].token}"` +
                `  mid=${fv(p.midpoint)}m  dir=${fv(p.direction)}`
        )
    }

    if (pairs.length === 0) {
        console.log("[WheelDetect] No valid axle pairs found (axes not parallel or not aligned with displacement)")
        console.groupEnd()
        return
    }

    const groups = groupByDirection(pairs)
    console.log(`[WheelDetect] Direction groups (${groups.length}):`)
    for (const g of groups) {
        console.log(
            `[WheelDetect]   dir=${fv(g.direction)}  pairs=[${g.indices.map(i => `"${pairs[i].key}"`).join(", ")}]`
        )
    }

    let selected: number[] = []
    for (const group of groups) {
        const [u, v] = perpendicularBasis(group.direction)
        const midpoints: Vec2[] = group.indices.map(idx => ({
            u: dot3(pairs[idx].midpoint, u),
            v: dot3(pairs[idx].midpoint, v),
        }))
        const collinear = largestCollinearSet(midpoints)
        console.log(
            `[WheelDetect]   dir=${fv(group.direction)}: collinear set size=${collinear.length}` +
                ` [${collinear.map(local => `"${pairs[group.indices[local]].key}"`).join(", ")}]`
        )
        if (collinear.length > selected.length) selected = collinear.map(local => group.indices[local])
    }

    if (selected.length === 0) {
        console.log("[WheelDetect] No collinear drivetrain found — no wheels tagged")
        console.groupEnd()
        return
    }

    const taggedJoints = new Set<string>()
    for (const idx of selected) {
        for (const token of [candidates[pairs[idx].a].token, candidates[pairs[idx].b].token]) {
            taggedJoints.add(token)
            const jDef = jointDefs[token]
            if (!jDef) continue
            if (!jDef.userData) jDef.userData = { data: {} }
            jDef.userData.data ??= {}
            jDef.userData.data["wheel"] = "true"
            jDef.userData.data["wheelType"] = "0"
            jDef.userData.data[URDF_WHEEL_SOURCE_KEY] = URDF_AUTO_WHEEL_SOURCE
        }
    }

    console.log(
        `[WheelDetect] Tagged ${taggedJoints.size} wheel joints: [${[...taggedJoints].map(t => `"${t}"`).join(", ")}]`
    )
    console.groupEnd()
}
