// Exercises URDFWheelPhysics.ts's inferURDFAutoWheelBasis() via its caller,
// PhysicsSystem.ts's createWheelConstraint(), the URDF-import wheel path (other
// fixtures in this suite are non-URDF, so `urdfWheelBasis` is otherwise always
// undefined).
//
// inferURDFAutoWheelBasis() returns a `WheelBasis` of four fresh `new
// JOLT.Vec3(...)` (COPY) vectors. createWheelConstraint() copies each into a
// `[Value]` attribute (`WheelSettingsWV.mWheelForward/mWheelUp/
// mSuspensionDirection/mSteeringAxis`, and inside createVehicleConstraint,
// `VehicleConstraintSettings.mForward/mUp`), none of which take ownership
// (jolt/JoltJS.idl:3559-3562/3480-3481), same as `wheelSettings.mPosition` above.
// All four need destroy() once createVehicleConstraint (the last reader) returns.
//
// URDFWheelPhysics.ts (getShapeExtents/inferWheelDimensionsFromAxle/
// inferWheelRadius/inferURDFAutoWheelBasis) holds no other Jolt ownership: every
// AABox/Vec3 it reads is caller-owned and never destroyed here, since it's pure
// geometry math.
//
// createWheelConstraint is called directly (it's public), bracketed around a
// second call so the measured Vec3 count excludes the first call's STATIC_ALIAS
// first-touch artifacts (AABox.mMin/mMax, Shape.GetLocalBounds()), which would
// otherwise dwarf the 4-Vec3-per-call leak. Vec3 is also globally
// INTERNAL_REF_UNPROVABLE in class-classification.ts for an unrelated reason
// (see ejectable-intake-lifecycle.test.ts), so the filtered leak-checker alone
// would miss a regression here.
import { describe, expect, test } from "vitest"
import * as THREE from "three"
import { mirabuf } from "@/proto/mirabuf"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCounts, patchDestroyGuard, snapshotLiveCounts } from "../lib/instrumentation"

function urdfWheelJoint(): [mirabuf.joint.JointInstance, mirabuf.joint.Joint] {
    const jointDefinition = new mirabuf.joint.Joint({
        info: { name: "WheelJoint" },
        jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
        origin: { x: 30, y: -10, z: 0 },
        rotational: {
            rotationalFreedom: {
                axis: { x: 0, y: 0, z: 1 },
            },
        },
        userData: { data: { wheel: "true" } },
    })
    const jointInstance = new mirabuf.joint.JointInstance({
        info: { name: "WheelJoint" },
        offset: { x: 0, y: 0, z: 0 },
    })
    return [jointInstance, jointDefinition]
}

describe("real lifecycle: PhysicsSystem.createWheelConstraint (URDF import) leaves no leak", () => {
    test("a second createWheelConstraint call adds no net Vec3 beyond the first call's fixed one-time cost", () => {
        const system = new PhysicsSystem()

        const chassis = system.createBox(new THREE.Vector3(0.5, 0.5, 0.5), 1, new THREE.Vector3(0, 1, 0), undefined)
        system.addBodyToSystem(chassis.GetID(), true)
        const wheel1 = system.createBox(new THREE.Vector3(0.1, 0.1, 0.1), 0.05, new THREE.Vector3(1, 1, 0), undefined)
        system.addBodyToSystem(wheel1.GetID(), true)
        const wheel2 = system.createBox(new THREE.Vector3(0.1, 0.1, 0.1), 0.05, new THREE.Vector3(-1, 1, 0), undefined)
        system.addBodyToSystem(wheel2.GetID(), true)

        // Warm-up call: primes STATIC_ALIAS Vec3 first-touch artifacts (AABox.mMin/mMax,
        // Shape.GetLocalBounds(), etc.) so the measured call below isolates just the
        // URDF-basis leak.
        const [jointInstance, jointDefinition] = urdfWheelJoint()
        system.createWheelConstraint(jointInstance, jointDefinition, 1.5, chassis, wheel1, 6, true)

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotLiveCounts(JOLT, ["Vec3"])

        system.createWheelConstraint(jointInstance, jointDefinition, 1.5, chassis, wheel2, 6, true)

        const after = snapshotLiveCounts(JOLT, ["Vec3"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCounts(before, after)).toEqual([])

        system.destroy()
    })
})
