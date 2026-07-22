// Integration exercise: AutoTestPanel.tsx's resetBodies(), called every time a user clicks
// "Reset" after an auto-test run to restore every captured body's position/rotation/velocity.
// Previously untested against real Jolt ownership.
//
// `resetBodies` shares one `zero` Vec3 across every capture, passed as *both* the `linear` and
// `angular` arg to `PhysicsSystem.setBodyPositionRotationAndVelocity` with the default
// `destroy: true`, which destroys `linear` and `angular` after use. Since
// `linear === angular === zero`, a single call already destroys the same object twice, and a
// `captures` array of more than one body then hands that already-freed handle back in on every
// subsequent iteration (a use-after-free feeding a double-free). Vec3 isn't a RefTarget, so
// `Body.SetLinearVelocity`/`SetAngularVelocity` and `JOLT.destroy()` have no refcount to guard
// against this the way `patchDestroyGuard` catches RefTarget double-destroys: destroy()ing a
// plain Vec3 twice in this suite's Node build throws nothing and just silently double-frees. So
// this test instruments `JOLT.destroy` itself (tracking every destroyed reference) rather than
// relying on the shared `patchDestroyGuard` helper. Fixed by passing `destroy: false` and
// destroying each capture's own `pos`/`rot` explicitly plus `zero` exactly once after the loop.
//
// `World.physicsSystem` is a real `PhysicsSystem`. Two real bodies stand in for
// `captureBodies()`'s output (that function itself is a thin, ownership-safe wrapper over
// `Body.GetWorldTransform()`, STATIC_ALIAS, never destroy()'d, and fresh COPY conversions, not
// exercised separately here).
import * as THREE from "three"
import { describe, expect, test, vi } from "vitest"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"

let activePhysicsSystem: PhysicsSystem

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
    },
}))

const { resetBodies } = await import("@/ui/panels/simulation/AutoTestPanel")
type BodyCapture = Parameters<typeof resetBodies>[0][number]

describe("real lifecycle: AutoTestPanel.resetBodies through a real PhysicsSystem", () => {
    test("resetting more than one captured body never destroy()s the same object twice", () => {
        const system = new PhysicsSystem()
        activePhysicsSystem = system
        const bodyA = system.createBox(new THREE.Vector3(0.2, 0.2, 0.2), 1, new THREE.Vector3(1, 1, 0), undefined)
        system.addBodyToSystem(bodyA.GetID(), true)
        const bodyB = system.createBox(new THREE.Vector3(0.2, 0.2, 0.2), 1, new THREE.Vector3(-1, 1, 0), undefined)
        system.addBodyToSystem(bodyB.GetID(), true)

        const captures: BodyCapture[] = [
            { id: bodyA.GetID(), pos: new JOLT.RVec3(1, 2, 3), rot: new JOLT.Quat(0, 0, 0, 1) },
            { id: bodyB.GetID(), pos: new JOLT.RVec3(4, 5, 6), rot: new JOLT.Quat(0, 0, 0, 1) },
        ]

        // Tracks every reference JOLT.destroy() is called with. A plain Vec3/RVec3/Quat isn't a
        // RefTarget, so patchDestroyGuard's refcount check (elsewhere in this suite) can't see a
        // double-destroy of one, so identity tracking is the only way to catch it here.
        const originalDestroy = (JOLT as unknown as Record<string, (obj: unknown) => void>).destroy
        const destroyed: unknown[] = []
        ;(JOLT as unknown as Record<string, (obj: unknown) => void>).destroy = obj => {
            destroyed.push(obj)
            originalDestroy(obj)
        }

        try {
            resetBodies(captures)
        } finally {
            ;(JOLT as unknown as Record<string, (obj: unknown) => void>).destroy = originalDestroy
        }

        expect(new Set(destroyed).size).toBe(destroyed.length)

        system.destroy()
    })
})
