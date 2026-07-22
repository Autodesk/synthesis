// Integration exercise for MessageHandlers.ts's `handlePeerUpdate`, the multiplayer per-frame body
// sync path, previously untested against real Jolt ownership.
//
// Two real leaks found:
//   1. `linearVelocity`/`angularVelocity` (`new JOLT.Vec3(...)`) were passed to
//      `Body.SetLinearVelocity`/`SetAngularVelocity`. Per jolt/JoltJS.idl these args are cloned, so
//      Jolt copies the value and the caller keeps ownership, but the copies were never destroyed.
//      Every successful body update leaked two Vec3s. `position`/`rotation` were fine because
//      `setBodyPosition`/`setBodyRotation` destroy their own args.
//   2. When `World.physicsSystem.getBody(bodyId)` returns undefined (mapped body not added, e.g.
//      torn down locally before the next remote update), the function returned early before the
//      four freshly-constructed Vec3/RVec3/Quat were destroyed, leaking all four per skipped
//      message.
// Fixed in MessageHandlers.ts: destroy linearVelocity/angularVelocity after the Set* calls, and
// destroy all four before the early return.
//
// `World.physicsSystem` is a real `PhysicsSystem`. `World.sceneRenderer`/`World.multiplayerSystem`/
// `SynthesisBrain`/`WPILibBrain` are minimal stubs, inert w.r.t. Jolt, just enough shape for
// `handlePeerUpdate`'s `instanceof MirabufSceneObject` check and scene-object/body-id lookups to
// resolve to the real objects under test.
import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import { describe, expect, test, vi } from "vitest"
import MirabufInstance from "@/mirabuf/MirabufInstance"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import type { LocalSceneObjectId, RemoteSceneObjectId, UpdateObjectData } from "@/systems/multiplayer/types"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCounts, patchDestroyGuard, snapshotLiveCounts } from "../lib/instrumentation"
import { createWheeledRobotAssembly } from "../lib/mirabuf-fixtures"

let activePhysicsSystem: PhysicsSystem

const mockSceneRenderer = {
    scene: { add: vi.fn(), remove: vi.fn() },
    sceneObjects: new Map(),
    registerSceneObject: vi.fn(),
    removeSceneObject: vi.fn(),
    createSphere: vi.fn(() => new THREE.Mesh(new THREE.SphereGeometry(0.05))),
    createToonMaterial: vi.fn(() => new THREE.MeshStandardMaterial()),
    setupMaterial: vi.fn(),
    worldToPixelSpace: vi.fn(() => [0, 0]),
    currentCameraControls: { focusProvider: undefined, controlsType: "Target" },
    mirabufSceneObjects: {
        getField: vi.fn(() => undefined),
    },
}

const mockSimulationSystem = {
    registerMechanism: vi.fn(),
    unregisterMechanism: vi.fn(),
    getSimulationLayer: vi.fn(() => ({ setBrain: vi.fn() })),
}

const PEER_ID = "peer-1"
const mockMultiplayerSystem = {
    _clientToBodyMap: new Map<string, Map<number, Jolt.BodyID>>(),
    convertSceneObjectId: vi.fn(() => 1 as LocalSceneObjectId),
    // MirabufSceneObject.setup() -> loadPreferences() -> savePreferences() schedules a
    // setTimeout(sendPreferences), which broadcasts once multiplayerSystem is truthy. Inert w.r.t.
    // Jolt, stubbed only so the deferred call doesn't throw after the test finishes.
    broadcast: vi.fn(),
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
        get simulationSystem() {
            return mockSimulationSystem
        },
        get multiplayerSystem() {
            return mockMultiplayerSystem
        },
    },
}))

vi.mock("@/systems/simulation/synthesis_brain/SynthesisBrain", () => ({
    default: vi.fn(() => ({ inputSchemeName: "TestScheme", clearControls: vi.fn() })),
}))

vi.mock("@/systems/simulation/wpilib_brain/WPILibBrain", () => ({
    default: vi.fn(() => ({ loadSimConfig: vi.fn() })),
}))

// Imported after the mocks above so MessageHandlers.ts's own `import World from "../World"` (and
// MirabufSceneObject's) resolve to the mocked module.
const { peerMessageHandlers } = await import("@/systems/multiplayer/MessageHandlers")

function buildSceneObject(): MirabufSceneObject {
    const { assembly } = createWheeledRobotAssembly()
    const parser = new MirabufParser(assembly)
    const mirabufInstance = new MirabufInstance(parser)
    const sceneObject = new MirabufSceneObject(mirabufInstance, undefined)
    sceneObject.id = 1
    sceneObject.setup()
    mockSceneRenderer.sceneObjects.set(sceneObject.id, sceneObject)
    return sceneObject
}

function bodyUpdateData(remoteBodyId: number): UpdateObjectData {
    return {
        sceneObjectKey: 1 as RemoteSceneObjectId,
        gamePiecesControlled: [],
        bodies: [
            {
                bodyId: remoteBodyId,
                linearVelocityStr: JSON.stringify({ x: 1, y: 2, z: 3 }),
                angularVelocityStr: JSON.stringify({ x: 0.1, y: 0.2, z: 0.3 }),
                positionStr: JSON.stringify({ x: 0, y: 1, z: 0 }),
                rotationStr: JSON.stringify({ x: 0, y: 0, z: 0, w: 1 }),
            },
        ],
    }
}

describe("real lifecycle: MessageHandlers.handlePeerUpdate through a real PhysicsSystem", () => {
    test("body update for an added body destroys linearVelocity/angularVelocity (Body.SetLinearVelocity/SetAngularVelocity CLONE, don't consume)", () => {
        activePhysicsSystem = new PhysicsSystem()
        const sceneObject = buildSceneObject()
        const chassisBodyId = [...sceneObject.mechanism.nodeToBody.values()][0]

        mockMultiplayerSystem._clientToBodyMap.set(PEER_ID, new Map([[42, chassisBodyId]]))

        const guard = patchDestroyGuard(JOLT)
        // Vec3/RVec3/Quat are already globally classified INTERNAL_REF_UNPROVABLE in
        // class-classification.ts for an unrelated reason (ContactListenerJS's transient
        // baseOffset alias). That's per-class, not per-call-site, so the filtered leak checker
        // would silently forgive a genuine leak of these classes from a different, buggy call
        // site. No physics step runs here (no contact callback fires), so a real delta can only
        // come from handlePeerUpdate itself.
        const before = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "Quat"])

        peerMessageHandlers.update([bodyUpdateData(42)], PEER_ID, 1)

        const after = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "Quat"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCounts(before, after)).toEqual([])

        activePhysicsSystem.destroy()
    })

    test("body update for a body that isn't added destroys all four constructed handles before returning", () => {
        activePhysicsSystem = new PhysicsSystem()
        const sceneObject = buildSceneObject()

        // Created but never added to the physics system, so
        // World.physicsSystem.getBody(unaddedBodyId) resolves to undefined, hitting the
        // early-return branch that used to skip destroy() on all four handles.
        const unaddedBody = activePhysicsSystem.createBox(new THREE.Vector3(0.1, 0.1, 0.1), 0.1, undefined, undefined)
        mockMultiplayerSystem._clientToBodyMap.set(PEER_ID, new Map([[43, unaddedBody.GetID()]]))

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "Quat"])

        peerMessageHandlers.update([bodyUpdateData(43)], PEER_ID, 2)

        const after = snapshotLiveCounts(JOLT, ["Vec3", "RVec3", "Quat"])
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCounts(before, after)).toEqual([])

        activePhysicsSystem.destroyBodies(unaddedBody)
        activePhysicsSystem.destroy()
        void sceneObject
    })
})
