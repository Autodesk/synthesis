import type Jolt from "@azaleacolburn/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import type { mirabuf } from "@/proto/mirabuf"
import { URDF_WHEEL_TAG } from "@/urdf/URDFUserData"

export type WheelBasis = {
    forward: Jolt.Vec3
    up: Jolt.Vec3
    suspensionDirection: Jolt.Vec3
    steeringAxis: Jolt.Vec3
}

export type WheelDimensions = {
    radius: number
    width: number
}

export function isURDFWheel(jDef: mirabuf.joint.Joint): boolean {
    return jDef.userData?.data?.[URDF_WHEEL_TAG] === "true"
}

export function getShapeExtents(bounds: Jolt.AABox): [number, number, number] {
    return [
        bounds.mMax.GetX() - bounds.mMin.GetX(),
        bounds.mMax.GetY() - bounds.mMin.GetY(),
        bounds.mMax.GetZ() - bounds.mMin.GetZ(),
    ]
}

export function inferWheelDimensionsFromAxle(bounds: Jolt.AABox, axis: Jolt.Vec3): WheelDimensions {
    const extents = getShapeExtents(bounds)
    const axisAbs = [Math.abs(axis.GetX()), Math.abs(axis.GetY()), Math.abs(axis.GetZ())]
    const axleIndex = axisAbs.indexOf(Math.max(...axisAbs))
    const radialExtents = extents.filter((_, index) => index !== axleIndex)

    return {
        radius: Math.max(...radialExtents) / 2.0,
        width: extents[axleIndex],
    }
}

// Radius used for a simulated wheel before any cross-wheel unification. URDF auto-wheels infer it
// from the radial extents about the detected axle; native wheels use the vertical extent (their axle
// is horizontal). Shared by the wheel-creation path and the radius-resolution pass so they can't drift.
export function inferWheelRadius(jDef: mirabuf.joint.Joint, bounds: Jolt.AABox, axis: Jolt.Vec3): number {
    const hasHorizontalAxle = Math.abs(axis.GetX()) >= 0.5 || Math.abs(axis.GetZ()) >= 0.5
    return isURDFWheel(jDef) && hasHorizontalAxle
        ? inferWheelDimensionsFromAxle(bounds, axis).radius
        : (bounds.mMax.GetY() - bounds.mMin.GetY()) / 2.0
}

export function inferURDFAutoWheelBasis(axis: Jolt.Vec3): WheelBasis | undefined {
    const absX = Math.abs(axis.GetX())
    const absZ = Math.abs(axis.GetZ())

    if (Math.max(absX, absZ) < 0.5) return undefined

    // URDF import converts Z-up to Y-up, so wheels should have a horizontal axle.
    // Canonicalize the axle sign so all wheels on both sides share the same basis.
    const lateralX = absX >= absZ ? 1 : 0
    const lateralZ = absZ > absX ? 1 : 0

    return {
        // up x lateral gives the chassis forward axis in Synthesis/Jolt Y-up space.
        forward: new JOLT.Vec3(lateralZ, 0, -lateralX),
        up: new JOLT.Vec3(0, 1, 0),
        suspensionDirection: new JOLT.Vec3(0, -1, 0),
        steeringAxis: new JOLT.Vec3(0, 1, 0),
    }
}
