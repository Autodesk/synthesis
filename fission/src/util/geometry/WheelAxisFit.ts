import * as THREE from "three"

/** A wheel's rotation axis, pivot origin, radius, and axle-direction width. */
export interface WheelAxis {
    center: THREE.Vector3
    axis: THREE.Vector3
    radius: number
    width: number
}

/** Any unit vector perpendicular to axis. */
function anyPerpendicular(axis: THREE.Vector3): THREE.Vector3 {
    const helper = Math.abs(axis.x) < 0.9 ? new THREE.Vector3(1, 0, 0) : new THREE.Vector3(0, 1, 0)
    return helper.cross(axis).normalize()
}

const LOCAL_AXES = [new THREE.Vector3(1, 0, 0), new THREE.Vector3(0, 1, 0), new THREE.Vector3(0, 0, 1)]

/** Index of the AABB's smallest extent -- the axle direction. */
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

/** Derives a wheel's rotation axis and origin from its AABB. Naive baseline; see computeWheelAxisFromCircleFit. */
export function computeWheelAxisFromAABB(points: THREE.Vector3[]): WheelAxis | undefined {
    if (points.length === 0) return undefined

    const bbox = new THREE.Box3().setFromPoints(points)
    const center = bbox.getCenter(new THREE.Vector3())
    const size = bbox.getSize(new THREE.Vector3())
    const axleIndex = axleIndexFromAABB(points)
    const extents = [size.x, size.y, size.z]
    const radialExtents = extents.filter((_, i) => i !== axleIndex)

    return {
        center,
        axis: LOCAL_AXES[axleIndex].clone(),
        radius: Math.max(...radialExtents) / 2,
        width: extents[axleIndex],
    }
}

// Minimum points to trust a circle fit.
const MIN_POINTS_FOR_CIRCLE_FIT = 12

// Percentile (not max) of radial distance used as outer-envelope radius.
const OUTER_RADIUS_PERCENTILE = 0.95

// Fraction of points (by fit residual) dropped before refitting.
const TRIM_FRACTION = 0.15

interface Circle2D {
    cx: number
    cy: number
    radius: number
}

/** Algebraic (Kasa) least-squares circle fit via one 3x3 linear solve. */
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

    // Cramer's rule.
    const D = det3(sxx, sxy, sx, sxy, syy, sy, sx, sy, n)
    if (Math.abs(D) < 1e-9) return undefined // degenerate

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

/** Derives a wheel's rotation axis, hub origin, and outer radius from a local-space point cloud via a trimmed circle fit. */
export function computeWheelAxisFromCircleFit(points: THREE.Vector3[]): WheelAxis | undefined {
    if (points.length < MIN_POINTS_FOR_CIRCLE_FIT) return undefined

    const axleIndex = axleIndexFromAABB(points)
    const axis = LOCAL_AXES[axleIndex].clone()
    const [uIndex, vIndex] = [0, 1, 2].filter(i => i !== axleIndex)
    const width = new THREE.Box3().setFromPoints(points).getSize(new THREE.Vector3()).getComponent(axleIndex)

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

    // Outer envelope of the untrimmed cloud, not the fit's own averaged radius.
    const outerDistances = coords.map(coord => Math.hypot(coord.x - fit.cx, coord.y - fit.cy)).sort((a, b) => a - b)
    const radius = outerDistances[Math.floor(outerDistances.length * OUTER_RADIUS_PERCENTILE)]

    return { center, axis, radius, width }
}

/** Transforms a wheel axis into another space; radius/width carried through via offset points for non-uniform scale. */
export function transformWheelAxis(local: WheelAxis, matrixWorld: THREE.Matrix4): WheelAxis {
    const center = local.center.clone().applyMatrix4(matrixWorld)

    const normalMatrix = new THREE.Matrix3().getNormalMatrix(matrixWorld)
    const axis = local.axis.clone().applyMatrix3(normalMatrix).normalize()

    const rim = local.center
        .clone()
        .addScaledVector(anyPerpendicular(local.axis), local.radius)
        .applyMatrix4(matrixWorld)
    const radius = rim.distanceTo(center)

    const edge = local.center
        .clone()
        .addScaledVector(local.axis, local.width / 2)
        .applyMatrix4(matrixWorld)
    const width = edge.distanceTo(center) * 2

    return { center, axis, radius, width }
}
