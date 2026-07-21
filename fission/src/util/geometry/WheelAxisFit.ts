import * as THREE from "three"

/** A wheel's rotation axis, pivot origin, and radius, in the local space of whatever point set they were derived from. */
export interface WheelAxis {
    center: THREE.Vector3
    axis: THREE.Vector3
    radius: number
}

/** Any unit vector perpendicular to `axis` -- used to carry a radius through a transform without
 *  assuming uniform scale (see transformWheelAxis). */
function anyPerpendicular(axis: THREE.Vector3): THREE.Vector3 {
    const helper = Math.abs(axis.x) < 0.9 ? new THREE.Vector3(1, 0, 0) : new THREE.Vector3(0, 1, 0)
    return helper.cross(axis).normalize()
}

const LOCAL_AXES = [new THREE.Vector3(1, 0, 0), new THREE.Vector3(0, 1, 0), new THREE.Vector3(0, 0, 1)]

/** Index (0/1/2 -> x/y/z) of a part's local-space AABB's smallest extent -- the axle direction, since a
 *  wheel is thin along its axle and wide in the wheel plane. */
function axleIndexFromAABB(points: THREE.Vector3[]): number {
    const bbox = new THREE.Box3().setFromPoints(points)
    const size = bbox.getSize(new THREE.Vector3())
    const extents = [size.x, size.y, size.z]

    let axleIndex = 0
    for (let i = 1; i < 3; i++) {
        if (extents[i] < extents[axleIndex]) axleIndex = i
    }
    return axleIndex
}

/**
 * Derives a wheel's rotation axis and origin from a part's local-space AABB: a wheel is thin along its
 * axle and wide in the wheel plane, so the AABB's smallest extent picks out the axle direction and the
 * AABB center approximates the hub. Mirrors the assumption URDFWheelPhysics.inferWheelDimensionsFromAxle
 * already makes in the other direction (axis known, infer radius from AABB) -- here the axis is unknown,
 * so we start from geometry instead.
 *
 * Kept as the naive baseline: the AABB center only coincides with the true rotation axis when the part
 * is symmetric about that axis in the plane perpendicular to it, which real wheel/tire meshes generally
 * aren't (tread patterns, hub bosses, off-center bolt circles). See computeWheelAxisFromCircleFit for the
 * replacement that doesn't share this bias.
 */
export function computeWheelAxisFromAABB(points: THREE.Vector3[]): WheelAxis | undefined {
    if (points.length === 0) return undefined

    const bbox = new THREE.Box3().setFromPoints(points)
    const center = bbox.getCenter(new THREE.Vector3())
    const size = bbox.getSize(new THREE.Vector3())
    const axleIndex = axleIndexFromAABB(points)
    const radialExtents = [size.x, size.y, size.z].filter((_, i) => i !== axleIndex)

    return { center, axis: LOCAL_AXES[axleIndex].clone(), radius: Math.max(...radialExtents) / 2 }
}

// Minimum points to trust a circle fit -- below this a single outlier can swing the result arbitrarily.
const MIN_POINTS_FOR_CIRCLE_FIT = 12

// Fraction of points (by fit residual) dropped before refitting. Localized asymmetric detail (bolt
// bosses, stub shafts, sensor mounts fused into the wheel part) produces the largest residuals against an
// initial fit; discarding them and refitting keeps a single such feature from pulling the hub off-axis by
// itself, while leaving enough of a normal tread pattern's noise in place to still average out.
const TRIM_FRACTION = 0.15

interface Circle2D {
    cx: number
    cy: number
    radius: number
}

/**
 * Algebraic (Kasa) least-squares circle fit: minimizes sum((x^2+y^2) - 2a*x - 2b*y - c)^2, which is
 * linear in (a, b, c) unlike the true geometric distance residual. One 3x3 linear solve, no iteration --
 * accurate enough once points are already roughly clustered around a near-circular cross-section.
 */
function fitCircle2D(coords: { x: number; y: number }[]): Circle2D | undefined {
    if (coords.length < 3) return undefined

    let sx = 0
    let sy = 0
    let sxx = 0
    let syy = 0
    let sxy = 0
    let sxz = 0
    let syz = 0
    let sz = 0
    for (const { x, y } of coords) {
        const z = x * x + y * y
        sx += x
        sy += y
        sxx += x * x
        syy += y * y
        sxy += x * y
        sxz += x * z
        syz += y * z
        sz += z
    }
    const n = coords.length

    const det3 = (
        m00: number,
        m01: number,
        m02: number,
        m10: number,
        m11: number,
        m12: number,
        m20: number,
        m21: number,
        m22: number
    ) => m00 * (m11 * m22 - m12 * m21) - m01 * (m10 * m22 - m12 * m20) + m02 * (m10 * m21 - m11 * m20)

    // Solve [[sxx,sxy,sx],[sxy,syy,sy],[sx,sy,n]] * [2a,2b,c]^T = [sxz,syz,sz]^T via Cramer's rule.
    const D = det3(sxx, sxy, sx, sxy, syy, sy, sx, sy, n)
    if (Math.abs(D) < 1e-9) return undefined // degenerate (collinear/coincident points)

    const Da = det3(sxz, sxy, sx, syz, syy, sy, sz, sy, n)
    const Db = det3(sxx, sxz, sx, sxy, syz, sy, sx, sz, n)
    const Dc = det3(sxx, sxy, sxz, sxy, syy, syz, sx, sy, sz)

    const a = Da / D / 2
    const b = Db / D / 2
    const c = Dc / D

    const radiusSq = c + a * a + b * b
    if (radiusSq <= 0) return undefined

    return { cx: a, cy: b, radius: Math.sqrt(radiusSq) }
}

/**
 * Derives a wheel's rotation axis and hub origin from a part's local-space point cloud. The AABB's
 * smallest extent still picks out the axle direction -- that half of the old heuristic holds, since real
 * wheel meshes are thin along the axle and wide in the wheel plane regardless of tread/hub detail. The
 * hub origin, though, is fit as the center of a least-squares circle through the points projected into
 * the plane perpendicular to that axle, instead of the AABB center of the whole part: tread patterns, hub
 * bosses, and off-center bolt circles bias a bounding-box center but average out in a circle fit, since
 * they're distributed around (not to one side of) the true rotation axis. A single trim-and-refit pass
 * discards the worst-residual points before refitting, so one strongly asymmetric feature (e.g. a stub
 * shaft) can't pull the result off axis by itself.
 */
export function computeWheelAxisFromCircleFit(points: THREE.Vector3[]): WheelAxis | undefined {
    if (points.length < MIN_POINTS_FOR_CIRCLE_FIT) return undefined

    const axleIndex = axleIndexFromAABB(points)
    const axis = LOCAL_AXES[axleIndex].clone()
    const [uIndex, vIndex] = [0, 1, 2].filter(i => i !== axleIndex)

    const centroid = points.reduce((sum, p) => sum.add(p), new THREE.Vector3()).divideScalar(points.length)
    const toCoord = (p: THREE.Vector3) => ({
        x: p.getComponent(uIndex) - centroid.getComponent(uIndex),
        y: p.getComponent(vIndex) - centroid.getComponent(vIndex),
    })

    const coords = points.map(toCoord)
    let fit = fitCircle2D(coords)
    if (!fit) return undefined

    const residual = (coord: { x: number; y: number }, f: Circle2D) =>
        Math.abs(Math.hypot(coord.x - f.cx, coord.y - f.cy) - f.radius)

    const keepCount = Math.floor(coords.length * (1 - TRIM_FRACTION))
    if (keepCount >= MIN_POINTS_FOR_CIRCLE_FIT && keepCount < coords.length) {
        const trimmedCoords = coords
            .map(coord => ({ coord, r: residual(coord, fit!) }))
            .sort((a, b) => a.r - b.r)
            .slice(0, keepCount)
            .map(entry => entry.coord)

        const refit = fitCircle2D(trimmedCoords)
        if (refit) fit = refit
    }

    const center = centroid.clone()
    center.setComponent(uIndex, centroid.getComponent(uIndex) + fit.cx)
    center.setComponent(vIndex, centroid.getComponent(vIndex) + fit.cy)

    return { center, axis, radius: fit.radius }
}

/** Transforms a locally-derived wheel axis into another space (e.g. world space). Radius is carried
 *  through via a rim point rather than a scale factor, so it stays correct even under non-uniform scale. */
export function transformWheelAxis(local: WheelAxis, matrixWorld: THREE.Matrix4): WheelAxis {
    const center = local.center.clone().applyMatrix4(matrixWorld)

    const normalMatrix = new THREE.Matrix3().getNormalMatrix(matrixWorld)
    const axis = local.axis.clone().applyMatrix3(normalMatrix).normalize()

    const rim = local.center
        .clone()
        .addScaledVector(anyPerpendicular(local.axis), local.radius)
        .applyMatrix4(matrixWorld)
    const radius = rim.distanceTo(center)

    return { center, axis, radius }
}
