import type Jolt from "@synthesis.adsk/jolt-physics"
import type { DOFSpecs } from "./PhysicsSystem"
import type { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertMirabufVector3ToJoltRVec3, convertMirabufVector3ToJoltVec3 } from "@/util/TypeConversions"

type LimitSpecs = Omit<DOFSpecs, "friction" | "axis">

export function createAnchorPoint(jointInstance: mirabuf.joint.JointInstance, jointDefinition: mirabuf.joint.Joint) {
    const anchorPoint = jointDefinition.origin
        ? convertMirabufVector3ToJoltRVec3(jointDefinition.origin)
        : new JOLT.RVec3(0, 0, 0)
    // TODO: Offset transformation for robot builder.
    const jointOriginOffset = jointInstance.offset
        ? convertMirabufVector3ToJoltVec3(jointInstance.offset)
        : new JOLT.Vec3(0, 0, 0)

    anchorPoint.Add(jointOriginOffset)

    JOLT.destroy(jointOriginOffset)

    return anchorPoint
}

// Other than `maxTorque`, these controller settings are not being used as of now
// because `ArcadeDriveBehavior` goes directly to the `WheelDrivers`.
// `maxTorque` is only used as communication for `WheelDriver` to get maxAcceleration
export function createVehicleController(maxAcc: number) {
    const controllerSettings = new JOLT.WheeledVehicleControllerSettings()
    controllerSettings.mEngine.mMaxTorque = maxAcc
    controllerSettings.mTransmission.mClutchStrength = 10.0
    controllerSettings.mTransmission.mGearRatios.clear()
    controllerSettings.mTransmission.mGearRatios.push_back(2)
    controllerSettings.mTransmission.mMode = JOLT.ETransmissionMode_Auto

    return controllerSettings
}

function tryGetPerpendicular(vec: Jolt.Vec3, toCheck: Jolt.Vec3): Jolt.Vec3 | undefined {
    if (Math.abs(Math.abs(vec.Dot(toCheck)) - 1.0) < 0.0001) return undefined

    const a = vec.Dot(toCheck)
    return new JOLT.Vec3(
        toCheck.GetX() - vec.GetX() * a,
        toCheck.GetY() - vec.GetY() * a,
        toCheck.GetZ() - vec.GetZ() * a
    )
}

/**
 * @returns non-normalized vector perpendicular to vec
 */
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
    const x = miraAxis.x ?? 0
    const miraAxisX = versionNum < 5 ? -x : x
    return new JOLT.Vec3(miraAxisX, miraAxis.y ?? 0, miraAxis.z ?? 0)
}

export function setAxes(
    freedom: mirabuf.joint.IDOF,
    settings: Jolt.HingeConstraintSettings | Jolt.SliderConstraintSettings,
    versionNum?: number
) {
    const axis: Jolt.Vec3 = getAxis(freedom, versionNum)
    const constraintAxis = axis.Normalized() // static temp

    if ("mHingeAxis1" in settings) {
        settings.mHingeAxis1 = settings.mHingeAxis2 = constraintAxis // deep copying into mHingeAxis
    } else {
        settings.mSliderAxis1 = settings.mSliderAxis2 = constraintAxis
    }

    const perpendicular = getPerpendicular(constraintAxis)
    settings.mNormalAxis1 = settings.mNormalAxis2 = perpendicular.Normalized() // perpendicular.Normalized() returns a static temp

    JOLT.destroy(perpendicular)
    JOLT.destroy(constraintAxis)
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

export function createDOFSpecs(dofs: mirabuf.joint.IDOF[]): DOFSpecs[] {
    const axes = dofs
        .filter(dof => dof.axis)
        .map(dof => [convertMirabufVector3ToJoltVec3(dof.axis!), dof] as [Jolt.Vec3, mirabuf.joint.IDOF])

    const constraintSpecs: DOFSpecs[] = axes
        .filter(([_, dof]) => !dof.limits || (dof.limits.upper ?? 0) - (dof.limits.lower ?? 0) > 0.001)
        .map(([axis, dof]) => {
            return { ...dof, axis, friction: 0 } satisfies DOFSpecs
        })

    return constraintSpecs
}

export function isWheel(jDef: mirabuf.joint.Joint): boolean {
    return (jDef.info?.name !== "grounded" && (jDef.userData?.data?.wheel ?? "false") === "true") ?? false
}
