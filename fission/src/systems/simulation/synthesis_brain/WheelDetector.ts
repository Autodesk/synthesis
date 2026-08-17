/**
 * Automatic wheel detection and joint drivetrain assignment for mirabuf assemblies.
 *
 * The algorithm works in four passes:
 *
 *   1. CANDIDATES - collect every revolute joint that has a well-defined axis.
 *      Joints whose axis is near-vertical (the robot up axis) are classified as
 *      steering joints; the rest are wheel candidates.  Each wheel candidate is
 *      augmented with the closest steering joint whose origin is within
 *      STEER_ORIGIN_DISTANCE, making it "yaw-rotatable" (i.e. a swerve wheel).
 *
 *   2. AXLE PAIRS - for every pair of candidates the test depends on type:
 *        Fixed / Fixed  – axes are parallel AND the wheel-to-wheel displacement
 *                         aligns with that axis (original behaviour).
 *        Swerve / Swerve – each wheel's axis is reachable after rotating its
 *                          module around its steering axis toward the pair
 *                          displacement direction (canYawAxisToDirection).
 *        Mixed          – the fixed wheel's axis must align with the pair
 *                         direction AND the swerve wheel must be able to yaw
 *                         into that same direction.
 *      The pair's geometric direction is always
 *        canonicalDir(normalize(originB − originA)),
 *      so AxlePair.direction is the physical axle vector, not the raw joint axis.
 *
 *   3. GROUP BY DIRECTION - axle pairs that share a drive axis are grouped
 *      together so swerve/mixed-axis robots don't cross-contaminate.
 *
 *   4. LARGEST COLLINEAR SET - within each direction group, the axle midpoints
 *      are projected onto the plane perpendicular to the drive axis and the
 *      largest subset whose projected points lie on a single line is selected.
 *      That line is the robot's centreline; the winning pairs are the drivetrain.
 *
 * -- Valid drivetrain (midpoints M1, M2 are collinear along the centreline) --
 *
 *        left         right
 *         |             |
 *        [W]------------[W]  <- front axle, midpoint M1
 *         |      |       |
 *         |  ----|----   |   centreline
 *         |      |       |
 *        [W]------------[W]  <- rear axle, midpoint M2
 *         |             |
 *
 *              M1 and M2 are collinear -> drivetrain detected (ok)
 *
 * -- Valid swerve drivetrain (modules exported at 45°, detected via yaw reachability) --
 *
 *        [W/45°]--------[W/45°]  <- wheel axes rotated 45°, but steer joints allow
 *                                    yawing to the lateral pair direction -> ok
 *
 * -- Invalid drivetrain (midpoints M1, M2 are not collinear) --
 *
 *        [W]------------[W]  <- axle pair 1, midpoint M1
 *                       |
 *                      [W]---[W]  <- axle pair 2, midpoint M2 (shifted right)
 *
 *              M1 and M2 are offset - no collinear set of size > 1
 *              found across all groups -> no drivetrain tagged (x)
 *
 * -- Invalid drivetrain (one wheel shifted fore/aft so no second axle pair forms) --
 *
 *        left         right
 *         |
 *        [W]------------[W]  <- rear axle pair formed, midpoint M1 (ok)
 *         |
 *        [W]      [W]        <- front-left OK, but front-right is shifted rearward
 *                  ^
 *                  displacement from front-left to front-right is diagonal -
 *                  does not align with the rotation axis -> axle pair condition b
 *                  fails -> no second pair -> only 2 wheels tagged (x)
 *
 * -- Invalid drivetrain (rear-right shifted backwards, breaking the rear axle pair) --
 *
 *        left         right
 *
 *        [W]------------[W]  <- front axle pair formed, midpoint M1 (ok)
 *
 *        [W]                 <- rear-left in expected position
 *                       [W]  <- rear-right shifted backwards along robot body
 *                        ^
 *                   displacement rear-left to rear-right has a fore/aft component;
 *                   dot with lateral axis < AXLE_ALIGN_COS -> pair rejected.
 *                   only 1 axle pair total -> drivetrain collinear check never
 *                   runs -> no wheels tagged (x)
 **/

import * as THREE from "three"
import { mirabuf } from "@/proto/mirabuf"

const AXIS_PARALLEL_COS = 0.99 // axes must be this parallel to be the same axle direction
const AXLE_ALIGN_COS = 0.98 // wheel-to-wheel displacement must align with the axis (fixed wheels)
const STEER_AXIS_UP_COS = 0.95 // steer joint axis must be this aligned with Y-up to qualify
const STEER_ORIGIN_DISTANCE = 0.3 // metres, max distance between steer origin and wheel origin for association
const AXIAL_COMPONENT_TOLERANCE = 0.15 // tolerance for canYawAxisToDirection axial-component comparison
const COINCIDENT_DISTANCE = 1e-4 // metres, same-point guard
const COLLINEAR_DISTANCE = 0.01 // metres (1 cm), collinear midpoint tolerance
const MIN_AXIS_LENGTH = 0.5 // unit vectors are length 1; below this the axis is degenerate/missing
const ZERO_LENGTH_EPSILON = 1e-9 // guards division by span when normalizing a direction vector

interface Vec2 {
    u: number
    v: number
}

interface Candidate {
    token: string
    origin: THREE.Vector3 // metres
    axis: THREE.Vector3 // normalised wheel spin axis
    steerAxis?: THREE.Vector3 // normalised steer (azimuth) axis, present for swerve modules
    steerOrigin?: THREE.Vector3 // steer joint origin in metres
    isYawRotatable: boolean
}

interface AxlePair {
    a: number
    b: number
    midpoint: THREE.Vector3
    direction: THREE.Vector3 // canonical geometric pair direction
    key: string
}

function canonicalDir(v: THREE.Vector3): THREE.Vector3 {
    const n = v.clone().normalize()
    const ax = Math.abs(n.x),
        ay = Math.abs(n.y),
        az = Math.abs(n.z)
    const dominant = ax >= ay && ax >= az ? n.x : ay >= az ? n.y : n.z
    return dominant < 0 ? n.multiplyScalar(-1) : n
}

function perpendicularBasis(d: THREE.Vector3): [THREE.Vector3, THREE.Vector3] {
    const helper = Math.abs(d.x) < 0.9 ? new THREE.Vector3(1, 0, 0) : new THREE.Vector3(0, 1, 0)
    const u = d.clone().cross(helper).normalize()
    const v = d.clone().cross(u)
    return [u, v]
}

function distToLine(p: Vec2, anchor: Vec2, dir: Vec2): number {
    return Math.abs((p.u - anchor.u) * dir.v - (p.v - anchor.v) * dir.u)
}

/**
 * Returns true when a wheel whose spin axis is `wheelAxis` can be yawed around
 * `steerAxis` so that its spin axis becomes parallel to `targetDirection`.
 *
 * Rotation around steerAxis preserves the component of any vector along that
 * axis, so the target is reachable only if both vectors have the same magnitude
 * of axial component.
 */
function canYawAxisToDirection(
    wheelAxis: THREE.Vector3,
    steerAxis: THREE.Vector3,
    targetDirection: THREE.Vector3
): boolean {
    const wParallel = Math.abs(wheelAxis.dot(steerAxis))
    const tParallel = Math.abs(targetDirection.dot(steerAxis))
    return Math.abs(wParallel - tParallel) < AXIAL_COMPONENT_TOLERANCE
}

function extractCandidates(jointDefs: Record<string, mirabuf.joint.IJoint>): Candidate[] {
    const allRevolute: { token: string; origin: THREE.Vector3; axis: THREE.Vector3 }[] = []

    for (const [token, jDef] of Object.entries(jointDefs)) {
        if (jDef.jointMotionType !== mirabuf.joint.JointMotion.REVOLUTE) continue
        const axisVec = jDef.rotational?.rotationalFreedom?.axis
        if (!axisVec) continue
        const axisRaw = new THREE.Vector3(axisVec.x ?? 0, axisVec.y ?? 0, axisVec.z ?? 0)
        if (axisRaw.length() < MIN_AXIS_LENGTH) continue
        const o = jDef.origin ?? {}

        // Joint origins stored in cm (positionToYup * 100)
        allRevolute.push({
            token,
            origin: new THREE.Vector3((o.x ?? 0) / 100, (o.y ?? 0) / 100, (o.z ?? 0) / 100),
            axis: axisRaw.normalize(),
        })
    }

    const up = new THREE.Vector3(0, 1, 0)

    // Joints whose axis is near-vertical are azimuth / steering joints, not wheels.
    const steerJoints = allRevolute.filter(j => Math.abs(j.axis.dot(up)) >= STEER_AXIS_UP_COS)
    const wheelJoints = allRevolute.filter(j => Math.abs(j.axis.dot(up)) < STEER_AXIS_UP_COS)

    const out: Candidate[] = []
    for (const w of wheelJoints) {
        let nearestSteer: (typeof steerJoints)[0] | undefined
        let nearestDist = STEER_ORIGIN_DISTANCE
        for (const s of steerJoints) {
            const d = s.origin.distanceTo(w.origin)
            if (d < nearestDist) {
                nearestDist = d
                nearestSteer = s
            }
        }
        out.push({
            token: w.token,
            origin: w.origin,
            axis: w.axis,
            steerAxis: nearestSteer?.axis,
            steerOrigin: nearestSteer?.origin,
            isYawRotatable: nearestSteer !== undefined,
        })
    }

    return out.sort((a, b) => (a.token < b.token ? -1 : 1))
}

function buildAxlePairs(candidates: Candidate[]): AxlePair[] {
    const pairs: AxlePair[] = []
    for (let i = 0; i < candidates.length; i++) {
        for (let j = i + 1; j < candidates.length; j++) {
            const ci = candidates[i]
            const cj = candidates[j]
            const disp = cj.origin.clone().sub(ci.origin)
            const span = disp.length()
            if (span < COINCIDENT_DISTANCE) continue
            const pairDir = disp.clone().divideScalar(span)

            if (ci.isYawRotatable && cj.isYawRotatable) {
                // Swerve / swerve: each module just needs to be able to yaw onto the pair direction.
                if (!ci.steerAxis || !canYawAxisToDirection(ci.axis, ci.steerAxis, pairDir)) continue
                if (!cj.steerAxis || !canYawAxisToDirection(cj.axis, cj.steerAxis, pairDir)) continue
            } else if (!ci.isYawRotatable && !cj.isYawRotatable) {
                // Fixed / fixed: original logic — axes must be parallel and displacement must align.
                if (Math.abs(ci.axis.dot(cj.axis)) < AXIS_PARALLEL_COS) continue
                if (Math.abs(pairDir.dot(ci.axis)) < AXLE_ALIGN_COS) continue
            } else {
                // Mixed: fixed wheel's axis sets the axle direction; swerve must be able to reach it.
                const fixed = ci.isYawRotatable ? cj : ci
                const swerve = ci.isYawRotatable ? ci : cj
                if (Math.abs(pairDir.dot(fixed.axis)) < AXLE_ALIGN_COS) continue
                if (!swerve.steerAxis || !canYawAxisToDirection(swerve.axis, swerve.steerAxis, pairDir)) continue
            }

            const ta = ci.token
            const tb = cj.token
            pairs.push({
                a: i,
                b: j,
                midpoint: ci.origin.clone().add(cj.origin).multiplyScalar(0.5),
                // Use the geometric pair direction, not the raw joint axis.
                direction: canonicalDir(pairDir),
                key: ta < tb ? `${ta}|${tb}` : `${tb}|${ta}`,
            })
        }
    }

    return pairs.sort((a, b) => (a.key < b.key ? -1 : 1))
}

function groupByDirection(pairs: AxlePair[]): { direction: THREE.Vector3; indices: number[] }[] {
    const groups: { direction: THREE.Vector3; indices: number[] }[] = []
    for (let i = 0; i < pairs.length; i++) {
        const g = groups.find(g => Math.abs(g.direction.dot(pairs[i].direction)) >= AXIS_PARALLEL_COS)
        if (g) g.indices.push(i)
        else groups.push({ direction: pairs[i].direction, indices: [i] })
    }

    return groups
}

function largestCollinearSet(points: Vec2[]): number[] {
    if (points.length <= 2) return points.map((_, i) => i)
    let best: number[] = []
    const onLine: number[] = []
    for (let i = 0; i < points.length; i++) {
        for (let j = i + 1; j < points.length; j++) {
            const du = points[j].u - points[i].u
            const dv = points[j].v - points[i].v
            const len = Math.sqrt(du * du + dv * dv)
            if (len < ZERO_LENGTH_EPSILON) continue
            const dir: Vec2 = { u: du / len, v: dv / len }
            onLine.length = 0
            for (let k = 0; k < points.length; k++) {
                if (distToLine(points[k], points[i], dir) <= COLLINEAR_DISTANCE) onLine.push(k)
            }

            if (onLine.length > best.length) best = onLine.slice()
        }
    }

    return best
}

export function detectAndTagWheels(assembly: mirabuf.Assembly): boolean {
    const jointDefs = assembly.data?.joints?.jointDefinitions as Record<string, mirabuf.joint.IJoint> | undefined
    if (!jointDefs) return false

    const hasExistingWheels = Object.values(jointDefs).some(jDef => jDef.userData?.data?.["wheel"] === "true")
    if (hasExistingWheels) return false

    const candidates = extractCandidates(jointDefs)
    if (candidates.length < 2) return false

    const pairs = buildAxlePairs(candidates)
    if (pairs.length === 0) return false

    const groups = groupByDirection(pairs)

    let selected: number[] = []
    for (const group of groups) {
        const [u, v] = perpendicularBasis(group.direction)
        const midpoints: Vec2[] = group.indices.map(idx => ({
            u: pairs[idx].midpoint.dot(u),
            v: pairs[idx].midpoint.dot(v),
        }))
        const collinear = largestCollinearSet(midpoints)
        if (collinear.length > selected.length) selected = collinear.map(local => group.indices[local])
    }

    if (selected.length < 2) {
        console.error("No drivetrain found. Wheels will not be auto-assigned")
        return false
    }

    for (const idx of selected) {
        for (const token of [candidates[pairs[idx].a].token, candidates[pairs[idx].b].token]) {
            const jDef = jointDefs[token]
            if (!jDef) continue
            if (!jDef.userData) jDef.userData = { data: {} }
            jDef.userData.data ??= {}
            jDef.userData.data["wheel"] = "true"
            jDef.userData.data["wheelType"] = "0"
        }
    }
    return true
}
