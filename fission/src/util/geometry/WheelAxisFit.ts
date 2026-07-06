import * as THREE from "three"

/** A wheel's rotation axis and pivot origin, in the local space of whatever point set they were derived from. */
export interface WheelAxis {
    center: THREE.Vector3
    axis: THREE.Vector3
}

const LOCAL_AXES = [new THREE.Vector3(1, 0, 0), new THREE.Vector3(0, 1, 0), new THREE.Vector3(0, 0, 1)]

/**
 * Derives a wheel's rotation axis and origin from a part's local-space AABB: a wheel is thin along its
 * axle and wide in the wheel plane, so the AABB's smallest extent picks out the axle direction and the
 * AABB center approximates the hub. Mirrors the assumption URDFWheelPhysics.inferWheelDimensionsFromAxle
 * already makes in the other direction (axis known, infer radius from AABB) -- here the axis is unknown,
 * so we start from geometry instead.
 */
export function computeWheelAxisFromAABB(points: THREE.Vector3[]): WheelAxis | undefined {
    if (points.length === 0) return undefined

    const bbox = new THREE.Box3().setFromPoints(points)
    const size = bbox.getSize(new THREE.Vector3())
    const center = bbox.getCenter(new THREE.Vector3())

    const extents = [size.x, size.y, size.z]
    let axleIndex = 0
    for (let i = 1; i < 3; i++) {
        if (extents[i] < extents[axleIndex]) axleIndex = i
    }

    return { center, axis: LOCAL_AXES[axleIndex].clone() }
}

/** Transforms a locally-derived wheel axis into another space (e.g. world space) using a rigid/uniform-scale matrix. */
export function transformWheelAxis(local: WheelAxis, matrixWorld: THREE.Matrix4): WheelAxis {
    const center = local.center.clone().applyMatrix4(matrixWorld)

    const normalMatrix = new THREE.Matrix3().getNormalMatrix(matrixWorld)
    const axis = local.axis.clone().applyMatrix3(normalMatrix).normalize()

    return { center, axis }
}
