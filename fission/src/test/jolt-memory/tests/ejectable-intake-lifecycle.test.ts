// Integration exercise for EjectableSceneObject.ts/IntakeSensorSceneObject.ts: gameplay-triggered
// shape/body creation and per-frame `setBodyPosition`/`setBodyRotation` calls
// (EjectableSceneObject.test.ts/IntakeSensorSceneObject.test.ts fully mock
// `physicsSystem.setBodyPosition`/`setBodyRotation`/`createSensor` as spies, asserting only call
// arguments, never Jolt ownership).
//
// `PhysicsSystem.ts`'s `setBodyPosition`/`setBodyRotation`/`setBodyPositionAndRotation`/
// `setBodyPositionRotationAndVelocity` all early-return `if (!this.isBodyAdded(id))` before the
// `if (destroy) JOLT.destroy(...)` call, so a caller-owned `RVec3`/`Quat`/`Vec3` (from
// `convertThreeVector3ToJoltRVec3`/`convertThreeQuaternionToJoltQuat`, a fresh `new JOLT.___(...)`)
// leaks whenever the target body isn't added. `EjectableSceneObject.update()`/`eject()` both
// independently guard with their own `isBodyAdded` check first, so they never hit it.
// `IntakeSensorSceneObject.update()` calls `setBodyPosition`/`setBodyRotation` every frame with no
// such guard, so any future removal of the sensor body without clearing `_joltBodyId` first would
// silently leak every frame after. Fixed at the source: all four `PhysicsSystem.ts` methods now
// destroy their args on the early-return path too. This file's last test is a direct regression
// for that fix, and the two lifecycle tests above it cover the actual call sites this audit
// targeted.
//
// Only `World.sceneRenderer`/`EventSystem` are left real (inert w.r.t. Jolt: a plain event bus and
// unused mesh/scene stubs). `World.physicsSystem` is a real `PhysicsSystem` instance, so every
// body/shape either class creates or destroys is the real thing, checked with the same leak/
// destroy-guard instrumentation as other real-lifecycle tests.
import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import { describe, expect, test, vi } from "vitest"
import EjectableSceneObject from "@/mirabuf/EjectableSceneObject"
import IntakeSensorSceneObject from "@/mirabuf/IntakeSensorSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertThreeQuaternionToJoltQuat, convertThreeVector3ToJoltRVec3 } from "@/util/TypeConversions"
import {
    diffLiveCounts,
    diffLiveCountsFiltered,
    patchDestroyGuard,
    snapshotAllLiveCounts,
    snapshotLiveCounts,
} from "../lib/instrumentation"

let activePhysicsSystem: PhysicsSystem

const mockSceneRenderer = {
    filterSceneObjects: vi.fn(() => []),
    scene: { add: vi.fn(), remove: vi.fn() },
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
    },
}))

const IDENTITY_TRANSFORM = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

function createFakeEjectorAssembly(chassisBodyId: Jolt.BodyID): MirabufSceneObject {
    return {
        mechanism: { nodeToBody: new Map([["root", chassisBodyId]]) },
        rootNodeId: "root",
        ejectorPreferences: {
            deltaTransformation: IDENTITY_TRANSFORM,
            ejectorVelocity: 2,
            parentNode: undefined,
            ejectOrder: "FIFO",
        },
    } as unknown as MirabufSceneObject
}

function createFakeIntakeAssembly(chassisBodyId: Jolt.BodyID): MirabufSceneObject {
    return {
        mechanism: { nodeToBody: new Map([["root", chassisBodyId]]) },
        rootNodeId: "root",
        intakeActive: false,
        setEjectable: vi.fn(),
        intakePreferences: {
            deltaTransformation: IDENTITY_TRANSFORM,
            zoneDiameter: 0.5,
            parentNode: undefined,
            showZoneAlways: false,
            maxPieces: 1,
            animationDuration: 0.5,
        },
    } as unknown as MirabufSceneObject
}

describe("real lifecycle: EjectableSceneObject/IntakeSensorSceneObject through a real PhysicsSystem", () => {
    test("EjectableSceneObject (setup -> update x3 -> eject -> dispose) leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()
        const chassis = activePhysicsSystem.createBox(
            new THREE.Vector3(0.5, 0.5, 0.5),
            1,
            new THREE.Vector3(0, 1, 0),
            undefined
        )
        activePhysicsSystem.addBodyToSystem(chassis.GetID(), true)

        const gamePiece = activePhysicsSystem.createBox(
            new THREE.Vector3(0.1, 0.1, 0.1),
            0.05,
            new THREE.Vector3(0, 2, 0),
            undefined
        )
        activePhysicsSystem.addBodyToSystem(gamePiece.GetID(), true)

        const ejectable = new EjectableSceneObject(createFakeEjectorAssembly(chassis.GetID()), gamePiece.GetID())
        ejectable.setup()

        for (let i = 0; i < 3; i++) {
            ejectable.update()
        }

        ejectable.eject()
        ejectable.dispose()

        activePhysicsSystem.destroyBodies(chassis, gamePiece)
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    test("IntakeSensorSceneObject (setup -> update x3 -> dispose) leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()
        const chassis = activePhysicsSystem.createBox(
            new THREE.Vector3(0.5, 0.5, 0.5),
            1,
            new THREE.Vector3(0, 1, 0),
            undefined
        )
        activePhysicsSystem.addBodyToSystem(chassis.GetID(), true)

        const intake = new IntakeSensorSceneObject(createFakeIntakeAssembly(chassis.GetID()))
        intake.setup()

        for (let i = 0; i < 3; i++) {
            // Moves the parent body first so the sensor's per-frame setBodyPosition/setBodyRotation
            // (IntakeSensorSceneObject.ts's update(), no isBodyAdded guard) recomputes a fresh
            // transform each call instead of repeating the same one.
            activePhysicsSystem.setBodyPosition(chassis.GetID(), new JOLT.RVec3(0, 1 + i, 0))
            intake.update()
        }

        intake.dispose()
        activePhysicsSystem.destroyBodies(chassis)
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    // Direct regression for the bug this audit found: PhysicsSystem.ts's
    // setBodyPosition/setBodyRotation/setBodyPositionAndRotation/setBodyPositionRotationAndVelocity
    // all `if (!this.isBodyAdded(id)) return`'d before their `if (destroy) JOLT.destroy(...)`,
    // leaking the caller-owned RVec3/Quat/Vec3 whenever the target body wasn't added. Every one of
    // these calls now destroys its args on the early-return path too.
    // Deliberately uses the unfiltered per-class counter (`diffLiveCounts`/`snapshotLiveCounts`),
    // not `diffLiveCountsFiltered`/`snapshotAllLiveCounts`: `RVec3`/`Quat`/`Vec3` are already
    // globally classified `INTERNAL_REF_UNPROVABLE` in class-classification.ts for an unrelated
    // reason (ContactListenerJS.OnContactValidate's transient `baseOffset` wrapPointer alias).
    // That classification is per-class, not per-call-site, so `diffLiveCountsFiltered` would
    // silently forgive a genuine leak of these exact classes from a different, buggy call site.
    // No physics step runs in this test (no contact callback fires), so a real delta here can only
    // come from the four calls below.
    test("setBodyPosition/setBodyRotation family destroy their args even when the body isn't added", () => {
        activePhysicsSystem = new PhysicsSystem()
        const body = activePhysicsSystem.createBox(new THREE.Vector3(0.5, 0.5, 0.5), 1, undefined, undefined)
        // Never added to the physics system, so isBodyAdded(body.GetID()) is false for every call
        // below, exactly the branch that used to skip destroy().
        const id = body.GetID()

        const before = snapshotLiveCounts(JOLT, ["RVec3", "Quat", "Vec3"])

        activePhysicsSystem.setBodyPosition(id, convertThreeVector3ToJoltRVec3(new THREE.Vector3(1, 2, 3)))
        activePhysicsSystem.setBodyRotation(id, convertThreeQuaternionToJoltQuat(new THREE.Quaternion(0, 0, 0, 1)))
        activePhysicsSystem.setBodyPositionAndRotation(
            id,
            convertThreeVector3ToJoltRVec3(new THREE.Vector3(4, 5, 6)),
            convertThreeQuaternionToJoltQuat(new THREE.Quaternion(0, 0, 0, 1))
        )
        activePhysicsSystem.setBodyPositionRotationAndVelocity(
            id,
            convertThreeVector3ToJoltRVec3(new THREE.Vector3(7, 8, 9)),
            convertThreeQuaternionToJoltQuat(new THREE.Quaternion(0, 0, 0, 1)),
            new JOLT.Vec3(0, 0, 0),
            new JOLT.Vec3(0, 0, 0)
        )

        const after = snapshotLiveCounts(JOLT, ["RVec3", "Quat", "Vec3"])

        expect(diffLiveCounts(before, after)).toEqual([])

        activePhysicsSystem.destroyBodies(body)
        activePhysicsSystem.destroy()
    })
})
