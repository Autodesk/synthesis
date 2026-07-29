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

// A disc's spread along its axle is much smaller than across it. Requiring this ratio between the
// middle and smallest principal variances rejects clouds that aren't wheel-like (cubes, spheres),
// which have no meaningful principal axle to recover.
const MIN_DISC_VARIANCE_RATIO = 1.5

// Jacobi rotations needed to diagonalize a symmetric 3x3; converges well before this in practice.
const JACOBI_SWEEPS = 12

/**
 * Eigenvectors of a symmetric 3x3 matrix by cyclic Jacobi rotations, ascending by eigenvalue.
 *
 * https://en.wikipedia.org/wiki/Jacobi_eigenvalue_algorithm
 */
function symmetricEigenvectors(m: THREE.Matrix3): { values: number[]; vectors: THREE.Vector3[] } {
    const a = m.clone().elements.slice()
    const at = (r: number, c: number) => a[c * 3 + r]
    const set = (r: number, c: number, v: number) => {
        a[c * 3 + r] = v
    }
    const v = new THREE.Matrix3().identity()

    for (let sweep = 0; sweep < JACOBI_SWEEPS; sweep++) {
        for (const [p, q] of [
            [0, 1],
            [0, 2],
            [1, 2],
        ]) {
            const apq = at(p, q)
            if (Math.abs(apq) < 1e-14) continue

            // Rotation that zeroes the (p, q) entry.
            const theta = 0.5 * Math.atan2(2 * apq, at(q, q) - at(p, p))
            const c = Math.cos(theta)
            const s = Math.sin(theta)

            for (let k = 0; k < 3; k++) {
                const akp = at(k, p)
                const akq = at(k, q)
                set(k, p, c * akp - s * akq)
                set(k, q, s * akp + c * akq)
            }
            for (let k = 0; k < 3; k++) {
                const apk = at(p, k)
                const aqk = at(q, k)
                set(p, k, c * apk - s * aqk)
                set(q, k, s * apk + c * aqk)
            }

            const e = v.elements
            for (let k = 0; k < 3; k++) {
                const vkp = e[p * 3 + k]
                const vkq = e[q * 3 + k]
                e[p * 3 + k] = c * vkp - s * vkq
                e[q * 3 + k] = s * vkp + c * vkq
            }
        }
    }

    const e = v.elements
    const columns = [0, 1, 2].map(i => ({
        value: at(i, i),
        vector: new THREE.Vector3(e[i * 3], e[i * 3 + 1], e[i * 3 + 2]).normalize(),
    }))
    columns.sort((x, y) => x.value - y.value)

    return { values: columns.map(c => c.value), vectors: columns.map(c => c.vector) }
}

/**
 * The wheel's axle as the point cloud's least-spread principal axis.
 *
 * Onshape/URDF exports bake each part's assembly orientation into its mesh, so a swerve module
 * parked at 45 degrees has a 45-degree axle in its own local space. Snapping the axle to the
 * nearest local axis (see {@link axleIndexFromAABB}) would then be up to 45 degrees out, which
 * makes the module steer and roll in the wrong direction.
 *
 * @returns the unit axle, or undefined when the cloud is not disc-like enough to trust.
 */
export function computeWheelAxleFromPCA(points: THREE.Vector3[]): THREE.Vector3 | undefined {
    if (points.length < 3) return undefined

    const centroid = points.reduce((sum, p) => sum.add(p), new THREE.Vector3()).divideScalar(points.length)

    let xx = 0
    let yy = 0
    let zz = 0
    let xy = 0
    let xz = 0
    let yz = 0
    for (const p of points) {
        const dx = p.x - centroid.x
        const dy = p.y - centroid.y
        const dz = p.z - centroid.z
        xx += dx * dx
        yy += dy * dy
        zz += dz * dz
        xy += dx * dy
        xz += dx * dz
        yz += dy * dz
    }

    const n = points.length
    // biome-ignore format: covariance matrix layout
    const covariance = new THREE.Matrix3().set(
        xx / n, xy / n, xz / n,
        xy / n, yy / n, yz / n,
        xz / n, yz / n, zz / n
    )

    const { values, vectors } = symmetricEigenvectors(covariance)
    if (values[0] <= 0) return undefined
    if (values[1] < values[0] * MIN_DISC_VARIANCE_RATIO) return undefined

    const axle = vectors[0]
    // Canonical sign so the same wheel always reports the same axle direction.
    const dominant =
        Math.abs(axle.x) >= Math.abs(axle.y) && Math.abs(axle.x) >= Math.abs(axle.z)
            ? axle.x
            : Math.abs(axle.y) >= Math.abs(axle.z)
              ? axle.y
              : axle.z
    return dominant < 0 ? axle.multiplyScalar(-1) : axle
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

    // PCA recovers the true axle even when the wheel is not axis-aligned in its own local space;
    // the AABB fallback only ever reports a local axis.
    const axis = computeWheelAxleFromPCA(points) ?? LOCAL_AXES[axleIndexFromAABB(points)].clone()
    const u = anyPerpendicular(axis)
    const v = axis.clone().cross(u).normalize()

    const centroid = points.reduce((sum, p) => sum.add(p), new THREE.Vector3()).divideScalar(points.length)

    // Width along the true axle, not an AABB extent: a 45-degree axle's AABB extent is the wheel's
    // silhouette, which for this drivetrain reads ~2.5x the real tread width.
    let axialMin = Infinity
    let axialMax = -Infinity
    for (const p of points) {
        const axial = p.clone().sub(centroid).dot(axis)
        if (axial < axialMin) axialMin = axial
        if (axial > axialMax) axialMax = axial
    }
    const width = axialMax - axialMin

    const toCoord = (p: THREE.Vector3) => {
        const d = p.clone().sub(centroid)
        return { x: d.dot(u), y: d.dot(v) }
    }

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

    const center = centroid.clone().addScaledVector(u, fit.cx).addScaledVector(v, fit.cy)

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
