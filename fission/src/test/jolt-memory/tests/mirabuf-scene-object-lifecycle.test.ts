// Integration exercise for `MirabufSceneObject` itself: the production load/unload entry point
// (constructor -> setup() -> update() -> dispose()) every real robot/field spawn/despawn goes
// through. mirabuf-lifecycle.test.ts covers the lower layer (MirabufParser +
// PhysicsSystem's createMechanismFromParser/destroyMechanism) but drives it directly, bypassing
// MirabufSceneObject, so its own Jolt handling was never exercised: `setObjectPosition`'s dozen
// or so `JOLT.destroy()` calls (moveToSpawnLocation, run from setup()), and
// `updateMeshTransforms`'s per-frame `RVec3`/position/velocity churn (run from update()).
//
// Only `World` is mocked, and only its non-physics slices (sceneRenderer, simulationSystem), spies
// with no Jolt-owning state. `World.physicsSystem` is a real `PhysicsSystem` instance, so every
// body/constraint/shape MirabufSceneObject creates or destroys is the real thing, checked against
// the same leak/destroy-guard instrumentation as the other real-lifecycle tests. `SynthesisBrain`/
// `WPILibBrain` are mocked because brain construction pulls in a separate subsystem (drivetrain
// detection, drivers/behaviors) that is its own audit surface, out of scope here.
import * as THREE from "three"
import { describe, expect, test, vi } from "vitest"
import MirabufInstance from "@/mirabuf/MirabufInstance"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"
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
    },
}))

vi.mock("@/systems/simulation/synthesis_brain/SynthesisBrain", () => ({
    default: vi.fn(() => ({ inputSchemeName: "TestScheme", clearControls: vi.fn() })),
}))

vi.mock("@/systems/simulation/wpilib_brain/WPILibBrain", () => ({
    default: vi.fn(() => ({ loadSimConfig: vi.fn() })),
}))

describe("real lifecycle: MirabufSceneObject construct/setup/update/dispose through a real PhysicsSystem", () => {
    test("robot spawn/despawn (constructor -> setup -> update x3 -> dispose) leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()

        const { assembly } = createWheeledRobotAssembly()
        const parser = new MirabufParser(assembly)
        const mirabufInstance = new MirabufInstance(parser)

        const sceneObject = new MirabufSceneObject(mirabufInstance, undefined)
        expect(sceneObject.mechanism.nodeToBody.size).toBe(2)

        sceneObject.setup()

        for (let i = 0; i < 3; i++) {
            sceneObject.update()
        }

        sceneObject.dispose()
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    // Real-world flow: a robot is loaded, the user deletes it, a different one is loaded in its
    // place, repeat, all through one long-lived `PhysicsSystem`. A per-swap leak too small to
    // catch in a single construct/dispose pair compounds linearly across swaps. Each cycle spawns
    // a distinct variant (different GUIDs/dimensions, see mirabuf-fixtures.ts) so an
    // identity-keyed cache can't silently reuse state across "different" robots.
    //
    // Same hazard as mirabuf-lifecycle.test.ts's cycling test (see its comment):
    // `createVehicleListeners`'s `VehicleCollisionTester` double-free, fixed in PhysicsSystem.ts.
    // Harder to trigger here since `setup()`/`dispose()` allocate and free far more per cycle
    // (meshes, name tag, debug-body map) than the raw PhysicsSystem path, diluting how often a
    // freed `VehicleConstraint`'s address gets reused by the next cycle. Keep this at 50, not 10:
    // a lower count passing isn't evidence the fix holds, only that this hazard needs the reuse
    // to line up.
    test("N robot swaps (construct -> setup -> update x3 -> dispose) x50 through one PhysicsSystem leaves no leak", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()

        for (let cycle = 0; cycle < 50; cycle++) {
            const { assembly } = createWheeledRobotAssembly(cycle)
            const parser = new MirabufParser(assembly)
            const mirabufInstance = new MirabufInstance(parser)

            const sceneObject = new MirabufSceneObject(mirabufInstance, undefined)
            expect(sceneObject.mechanism.nodeToBody.size).toBe(2)

            sceneObject.setup()

            for (let i = 0; i < 3; i++) {
                sceneObject.update()
            }

            sceneObject.dispose()
        }

        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
