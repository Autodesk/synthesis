import Jolt from "@azaleacolburn/jolt-physics"
import { DOFSpecs } from "./PhysicsSystem"
import { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"

type LimitSpecs = Omit<DOFSpecs, "friction" | "axis">

function tryGetPerpendicular(vec: Jolt.Vec3, toCheck: Jolt.Vec3): Jolt.Vec3 | undefined {
    if (Math.abs(Math.abs(vec.Dot(toCheck)) - 1.0) < 0.0001) return undefined

    const a = vec.Dot(toCheck)
    return new JOLT.Vec3(
        toCheck.GetX() - vec.GetX() * a,
        toCheck.GetY() - vec.GetY() * a,
        toCheck.GetZ() - vec.GetZ() * a
    ).Normalized()
}

export function getPerpendicular(vec: Jolt.Vec3): Jolt.Vec3 {
    const v1 = new JOLT.Vec3(0, 1, 0)
    const v2 = new JOLT.Vec3(0, 0, 1)

    const perp = tryGetPerpendicular(vec, v1) ?? tryGetPerpendicular(vec, v2)!

    JOLT.destroy(v1)
    JOLT.destroy(v2)

    return perp
}

export function getAxis(freedom: mirabuf.joint.IDOF, versionNum: number = 6): Jolt.Vec3 {
    const miraAxis = freedom.axis! as mirabuf.Vector3
    // No scaling, these are unit vectors
    const miraAxisX = (versionNum < 5 ? -miraAxis.x : miraAxis.x) ?? 0
    return new JOLT.Vec3(miraAxisX, miraAxis.y ?? 0, miraAxis.z ?? 0)
}

export function setAxes(
    freedom: mirabuf.joint.IDOF,
    settings: Jolt.HingeConstraintSettings | Jolt.SliderConstraintSettings,
    versionNum?: number
) {
    const axis = getAxis(freedom, versionNum)

    let constraintAxis: Jolt.Vec3
    if ("mHingeAxis1" in settings) {
        constraintAxis = settings.mHingeAxis1 = settings.mHingeAxis2 = axis.Normalized()
    } else {
        constraintAxis = settings.mSliderAxis1 = settings.mSliderAxis2 = axis.Normalized()
    }
    settings.mNormalAxis1 = settings.mNormalAxis2 = getPerpendicular(constraintAxis)

    JOLT.destroy(axis)
}

export function applyHingeLimits(freedom: LimitSpecs, hingeSettings: Jolt.HingeConstraintSettings) {
    // if (!freedom.limits || !freedom.limits?.upper || !freedom.limits?.lower || !freedom.value) return
    if (!freedom.limits || Math.abs((freedom.limits?.upper ?? 0) - (freedom.limits?.lower ?? 0)) <= 0.001) return

    // Some values that are meant to be exactly PI are perceived as being past it, causing unexpected behavior.
    // This safety check caps the values to be within [-PI, PI] wth minimal difference in precision.
    const piSafetyCheck = (v: number) => Math.min(3.14158, Math.max(-3.14158, v))

    const currentPos = piSafetyCheck(freedom.value ?? 0)
    const upper = piSafetyCheck(freedom.limits.upper ?? 0) - currentPos
    const lower = piSafetyCheck(freedom.limits.lower ?? 0) - currentPos

    hingeSettings.mLimitsMin = -upper
    hingeSettings.mLimitsMax = -lower
}

export function applySliderLimits(freedom: LimitSpecs, sliderConstraintSettings: Jolt.SliderConstraintSettings) {
    if (!freedom.limits || Math.abs((freedom.limits?.upper ?? 0) - (freedom.limits?.lower ?? 0)) <= 0.001) return

    const currentPos = (freedom.value ?? 0) * 0.01
    const upper = (freedom.limits!.upper ?? 0) * 0.01 - currentPos
    const lower = (freedom.limits!.lower ?? 0) * 0.01 - currentPos

    // Calculate mid point
    const midPoint = (upper + lower) / 2.0
    const halfRange = Math.abs((upper - lower) / 2.0)

    // Move the anchor points
    // NOTE
    // `Add` does modify the "self" vector
    sliderConstraintSettings.mPoint2.Add(
        // NOTE
        // `MulFloat` does not modify its arguments
        sliderConstraintSettings.mSliderAxis1.MulFloat(midPoint)
    )

    sliderConstraintSettings.mLimitsMax = halfRange
    sliderConstraintSettings.mLimitsMin = -halfRange
}
