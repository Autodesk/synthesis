// Integration exercise for DragModeSystem.ts and the raycasting path it triggers on every drag
// click (RaycastUtils.ts's rayCastForRigidBody -> PhysicsSystem.rayCast).
//
// Three known ownership hazards, all fixed at the source:
//   1. PhysicsSystem.rayCast(): `new JOLT.RRayCast(rayVec, dir)` (a COPY, jolt/JoltJS.idl) was
//      never destroy()'d on either return path, and no caller used the returned `.ray` field.
//      Fixed by destroying it internally and dropping the unused field from `RayCastHit`.
//   2. RaycastUtils.ts's rayCastForRigidBody(): the transparent-object skip loop discarded each
//      skipped iteration's `hit.point` (a fresh COPY per rayCast() call) by reassigning `hit`
//      without destroying the previous one. It also never destroy()'d the accumulated
//      `ignoredBodies` BodyIDs (`IgnoreMultipleBodiesFilter.IgnoreBody` CLONES its arg, so the
//      caller's own copy is still its to free). The final returned `hit.point` was also converted
//      with `destroy: false`, leaking that COPY too. It should have been `true` per
//      convertJoltVec3ToThreeVector3's documented rule: only STATIC_ALIAS getter results default
//      to false, and this is a genuine `new JOLT.Vec3(...)`.
//   3. DragModeSystem.ts's updateDragForce(): every per-frame `Body.AddForce`/`AddTorque`/
//      `SetAngularVelocity` call was handed a freshly-constructed Vec3 (`joltForce`, `yawRotation`,
//      `pitchRotation`, `brakingForce`, the zero-velocity literal), all CLONED args that Jolt
//      copies internally, with none of them destroy()'d afterward. stopDragging()'s own two Vec3
//      constructions were already destroyed correctly, so this was a partial omission.
// This file is the regression test for all three.
//
// `World.physicsSystem` is a real `PhysicsSystem`. `World.sceneRenderer` is a minimal stub (a real
// THREE.PerspectiveCamera for raycasting math, a stubbed `pixelToWorldSpace`, and plain objects for
// the interaction-handler hooks DragModeSystem swaps out), inert w.r.t. Jolt.
// `RaycastUtils.rayCastForRigidBody` is mocked only for the DragModeSystem-level test, so it can
// control which body gets dragged without depending on the raycasting tests above it.
import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeReadOnly } from "@/mirabuf/MirabufParser"
import { BodyAssociate } from "@/systems/physics/BodyAssociate"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCounts, patchDestroyGuard, snapshotAllLiveCounts, snapshotLiveCounts } from "../lib/instrumentation"

let activePhysicsSystem: PhysicsSystem

const mainCamera = new THREE.PerspectiveCamera(50, 1, 0.1, 1000)
mainCamera.position.set(0, 0, 10)
mainCamera.lookAt(0, 0, 0)
mainCamera.updateMatrixWorld(true)

const canvas = document.createElement("canvas")
const canvasParent = document.createElement("div")
canvasParent.appendChild(canvas)

const mockScreenInteractionHandler: {
    interactionStart?: (i: unknown) => void
    interactionMove?: (i: unknown) => void
    interactionEnd?: (i: unknown) => void
} = {}

const mockSceneRenderer = {
    mainCamera,
    scene: { add: vi.fn(), remove: vi.fn() },
    currentCameraControls: { enabled: true },
    renderer: { domElement: canvas },
    screenInteractionHandler: mockScreenInteractionHandler,
    pixelToWorldSpace: vi.fn(() => new THREE.Vector3(0, 0, 9)),
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
        get analyticsSystem() {
            return undefined
        },
    },
}))

function fakeRigidNodeAssociate(bodyId: Jolt.BodyID): RigidNodeAssociate {
    const fakeRigidNode = { id: "node", isGamePiece: false } as unknown as RigidNodeReadOnly
    // DragModeSystem.isDraggable requires miraType === ROBOT || isGamePiece, so this needs a real
    // MiraType (not an empty stub) to reach startDragging() via onInteractionStart -> findDragTarget.
    const fakeSceneObject = { miraType: MiraType.ROBOT } as unknown as MirabufSceneObject
    return new RigidNodeAssociate(fakeSceneObject, fakeRigidNode, bodyId)
}

describe("real lifecycle: PhysicsSystem.rayCast leaves no leak", () => {
    beforeEach(() => {
        activePhysicsSystem = new PhysicsSystem()
    })

    afterEach(() => {
        activePhysicsSystem.destroy()
    })

    test("a hit destroys the internal RRayCast (previously only exposed, never freed)", () => {
        const body = activePhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), undefined, new THREE.Vector3(0, 0, 0), undefined)
        activePhysicsSystem.addBodyToSystem(body.GetID(), true)

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotLiveCounts(JOLT, ["RRayCast"])

        const hit = activePhysicsSystem.rayCast(new JOLT.Vec3(0, 0, 10), new JOLT.Vec3(0, 0, -40))
        expect(hit).toBeDefined()

        const after = snapshotLiveCounts(JOLT, ["RRayCast"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCounts(before, after)).toEqual([])

        JOLT.destroy(hit!.data.mBodyID)
        JOLT.destroy(hit!.point)
    })

    test("a miss destroys the internal RRayCast (previously never freed on this path either)", () => {
        const before = snapshotLiveCounts(JOLT, ["RRayCast"])

        // Off away from PhysicsSystem's own default ground box (7.5x0.1x7.5 at y=-0.1, see its
        // constructor) so this genuinely misses everything.
        const hit = activePhysicsSystem.rayCast(new JOLT.Vec3(100, 100, 10), new JOLT.Vec3(0, 0, -40))
        expect(hit).toBeUndefined()

        const after = snapshotLiveCounts(JOLT, ["RRayCast"])
        expect(diffLiveCounts(before, after)).toEqual([])
    })
})

describe("real lifecycle: RaycastUtils.rayCastForRigidBody leaves no leak", () => {
    beforeEach(() => {
        activePhysicsSystem = new PhysicsSystem()
        mockSceneRenderer.pixelToWorldSpace.mockReturnValue(new THREE.Vector3(0, 0, 9))
    })

    afterEach(() => {
        activePhysicsSystem.destroy()
    })

    test("a direct hit destroys hit.point (previously converted with destroy: false)", async () => {
        const { rayCastForRigidBody } = await import("@/util/RaycastUtils")

        const body = activePhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), undefined, new THREE.Vector3(0, 0, 0), undefined)
        activePhysicsSystem.addBodyToSystem(body.GetID(), true)
        activePhysicsSystem.setBodyAssociation(fakeRigidNodeAssociate(body.GetID()))

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "RRayCast", "BodyID"])

        const result = rayCastForRigidBody([0, 0])
        expect(result).toBeDefined()

        const after = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "RRayCast", "BodyID"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        // `result.bodyId` is a long-lived identity handle (same convention as other session-stored
        // BodyIDs, e.g. EjectableSceneObject._gamePieceBodyId), never destroy()'d in this codebase,
        // so BodyID's delta is expected to be +1 here, not 0.
        expect(diffLiveCounts(before, after).filter(d => d.className !== "BodyID")).toEqual([])
    })

    test("a transparent object in front of the target destroys the discarded hit.point and the ignoredBodies BodyID", async () => {
        const { rayCastForRigidBody } = await import("@/util/RaycastUtils")

        // Closer to the camera (z=10) than the real target (z=0), so it's hit first and skipped
        // since its association isn't a RigidNodeAssociate, forcing the transparent-object retry
        // loop to run.
        const transparentBody = activePhysicsSystem.createBox(
            new THREE.Vector3(2, 2, 0.1),
            undefined,
            new THREE.Vector3(0, 0, 5),
            undefined
        )
        activePhysicsSystem.addBodyToSystem(transparentBody.GetID(), true)
        activePhysicsSystem.setBodyAssociation(new BodyAssociate(transparentBody.GetID()))

        const targetBody = activePhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), undefined, new THREE.Vector3(0, 0, 0), undefined)
        activePhysicsSystem.addBodyToSystem(targetBody.GetID(), true)
        activePhysicsSystem.setBodyAssociation(fakeRigidNodeAssociate(targetBody.GetID()))

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "RRayCast", "BodyID"])

        const result = rayCastForRigidBody([0, 0])
        expect(result).toBeDefined()
        expect(result?.bodyId.GetIndexAndSequenceNumber()).toBe(targetBody.GetID().GetIndexAndSequenceNumber())

        const after = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "RRayCast", "BodyID"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        // Same BodyID caveat as above for the final result's identity handle.
        expect(diffLiveCounts(before, after).filter(d => d.className !== "BodyID")).toEqual([])
    })
})

describe("real lifecycle: DragModeSystem per-frame force/torque application leaves no leak", () => {
    beforeEach(() => {
        activePhysicsSystem = new PhysicsSystem()
    })

    afterEach(() => {
        activePhysicsSystem.destroy()
        vi.doUnmock("@/util/RaycastUtils")
        vi.resetModules()
    })

    test("startDragging -> update() x5 (moving then braking) -> stopDragging -> destroy leaves no leak", async () => {
        const body = activePhysicsSystem.createBox(new THREE.Vector3(0.3, 0.3, 0.3), 2, new THREE.Vector3(2, 0, 0), undefined)
        activePhysicsSystem.addBodyToSystem(body.GetID(), true)
        activePhysicsSystem.setBodyAssociation(fakeRigidNodeAssociate(body.GetID()))

        vi.doMock("@/util/RaycastUtils", () => ({
            rayCastForRigidBody: () => ({
                bodyId: body.GetID(),
                hitPoint: new THREE.Vector3(2, 0, 0),
                association: fakeRigidNodeAssociate(body.GetID()),
            }),
        }))
        const { default: DragModeSystem } = await import("@/systems/scene/DragModeSystem")
        const { PRIMARY_MOUSE_INTERACTION } = await import("@/systems/scene/ScreenInteractionHandler")

        const dragSystem = new DragModeSystem()
        dragSystem.enabled = true

        mockScreenInteractionHandler.interactionStart?.({
            interactionType: PRIMARY_MOUSE_INTERACTION,
            position: [500, 500],
        })

        // One warm-up frame, snapshotted before it and discarded. Body.GetPosition/GetRotation/
        // GetLinearVelocity are STATIC_ALIAS scratch buffers whose JS wrapper cache gets a one-time,
        // permanent +1 on first call in the process (same fixed-cost pattern as RMat44/AABox
        // elsewhere, not a leak), and this is this file's first caller of all three. Absorbing that
        // cost here, rather than filtering Vec3/RVec3/Quat by class like other tests, matters
        // because those classes are also globally whitelisted in class-classification.ts for an
        // unrelated reason (see ejectable-intake-lifecycle.test.ts). Filtering them would forgive a
        // genuine regression of the AddForce/AddTorque/SetAngularVelocity leak this test exists to
        // catch.
        dragSystem.update(1 / 60)

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        // Remaining frames land in updateDragForce's "moving toward target" branch (mouse target
        // differs from the body's position), then fall into the "braking" branch once close enough.
        // Both are exercised across these 4 frames without a real physics step, since no simulation
        // tick moves the body out from under the drag target math.
        for (let i = 0; i < 4; i++) {
            dragSystem.update(1 / 60)
        }

        mockScreenInteractionHandler.interactionEnd?.({ interactionType: PRIMARY_MOUSE_INTERACTION })
        dragSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCounts(before, after)).toEqual([])
    })
})
