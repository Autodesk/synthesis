// Regression test for ZoneSceneObject.ts's setSensorProperties (ZoneSceneObject.ts:124's
// `World.physicsSystem.setShape(...)` call). Other tests in this suite mock
// `physicsSystem.setShape` as a spy and only assert it was called, never checking Jolt
// ownership, so this exercises the real `PhysicsSystem` and covers the `setShape`
// signature change (destroy: boolean -> release: boolean, via PhysicsSystem.ts's
// `trackBodyShape`/`releaseBodyShape`) with a real leak/destroy-guard check across
// setup -> update (replaces the shape again) -> dispose.
//
// Only `World` is mocked: physicsSystem is a real instance, sceneRenderer is a stub
// since mesh creation is inert THREE.js/DOM state with no Jolt memory.
import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import { describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import ProtectedZoneSceneObject from "@/mirabuf/ProtectedZoneSceneObject"
import { ContactType } from "@/mirabuf/ZoneTypes"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"

let activePhysicsSystem: PhysicsSystem

const mockSceneRenderer = {
    createBox: vi.fn(() => new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1))),
    scene: {
        add: vi.fn(),
        remove: vi.fn(),
    },
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

vi.mock("@/systems/match_mode/MatchMode", () => ({
    MatchModeType: { SANDBOX: "Sandbox", AUTONOMOUS: "Autonomous", TELEOP: "Teleop", ENDGAME: "Endgame" },
    default: { getInstance: vi.fn(() => ({ getMatchModeType: vi.fn(() => "Teleop") })) },
}))

function createFakeParentAssembly(parentBodyId: Jolt.BodyID): MirabufSceneObject {
    return {
        mechanism: { nodeToBody: new Map([["root", parentBodyId]]) },
        rootNodeId: "root",
        fieldPreferences: {
            protectedZones: [
                {
                    name: "test-zone",
                    alliance: "red",
                    parentNode: undefined,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                    penaltyPoints: 5,
                    contactType: ContactType.ROBOT_ENTERS,
                    activeDuring: [],
                },
            ],
        },
    } as unknown as MirabufSceneObject
}

describe("real lifecycle: ZoneSceneObject setup/update/dispose through a real PhysicsSystem", () => {
    test("setSensorProperties's setShape handoff (setup -> update -> dispose) leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()
        const parentBody = activePhysicsSystem.createBox(
            new THREE.Vector3(0.5, 0.5, 0.5),
            1,
            new THREE.Vector3(0, 5, 0),
            undefined
        )
        activePhysicsSystem.addBodyToSystem(parentBody.GetID(), true)

        const zone = new ProtectedZoneSceneObject(createFakeParentAssembly(parentBody.GetID()), 0)
        zone.setup()
        expect(zone.joltBodyId).toBeDefined()

        // Re-derives the sensor's shape from scratch (moves the parent body first so the delta
        // transform differs), exercising setSensorProperties' setShape handoff a second time,
        // replacing the shape createDefaultSensor installed during setup().
        activePhysicsSystem.setBodyPosition(parentBody.GetID(), new JOLT.RVec3(0, 6, 0))
        zone.update()

        zone.dispose()
        activePhysicsSystem.destroyBodies(parentBody)
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
