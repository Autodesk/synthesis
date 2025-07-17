import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import ScoringZoneSceneObject, { OnScoreChangedEvent } from "../../mirabuf/ScoringZoneSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import Jolt from "@azaleacolburn/jolt-physics"
import { createBodyMock } from "../mocks/jolt"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

const mockPhysicsSystem = {
    createSensor: vi.fn(),
    destroyBodyIds: vi.fn(),
    setBodyPosition: vi.fn(),
    setBodyRotation: vi.fn(),
    // This cast is fine as long as we regularly update createBodyMock() to include new methods we call on bodies in functions we're testing
    getBody: vi.fn(() => createBodyMock() as unknown as Jolt.Body),
    getBodyAssociation: vi.fn(),
    disablePhysicsForBody: vi.fn(),
    enablePhysicsForBody: vi.fn(),
    isBodyAdded: vi.fn(),
    setShape: vi.fn(),
}
const mockSceneRenderer = {
    sceneObjects: new Map(),
    createBox: vi.fn(),
    scene: {
        remove: vi.fn(),
    },
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return mockPhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
    },
}))

describe("ScoringZoneSceneObject", () => {
    const originalConsoleLog = console.log

    beforeEach(() => {
        vi.clearAllMocks()
        SimulationSystem.redScore = 0
        SimulationSystem.blueScore = 0
        console.log = vi.fn()
    })

    afterEach(() => {
        vi.clearAllMocks()
        console.log = originalConsoleLog
    })

    test("Setup creates sensor and mesh", () => {
        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        const parent = {
            fieldPreferences: {
                scoringZones: [
                    {
                        parentNode: "node1",
                        deltaTransformation: [1, 2, 3, 4],
                        alliance: "red",
                        points: 10,
                        persistentPoints: false,
                    },
                ],
            },
            mechanism: { nodeToBody: new Map([["node1", mockBodyId]]) },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const instance = new ScoringZoneSceneObject(parent, 0)
        instance.setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(mockPhysicsSystem.createSensor).toHaveBeenCalled()
    })

    test("ZoneCollision updates score", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: false, alliance: "red", points: 10 })
        // This is *ok* as long as we update createBodyMock properly
        // In the long run, we should have better ways of constructing arguments for testing
        const gpID = {} as unknown as Jolt.BodyID
        mockPhysicsSystem.getBodyAssociation = vi.fn(() => ({ isGamePiece: true, associatedBody: 0 }))
        const dispatchSpy = vi.spyOn(OnScoreChangedEvent.prototype, "dispatch")
        instance.zoneCollision(gpID)
        expect(SimulationSystem.redScore).toBe(10)
        expect(dispatchSpy).toHaveBeenCalled()
    })

    test("Dispose destroys mesh and sensor", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        const mockMesh = { geometry: { dispose: vi.fn() }, material: { dispose: vi.fn() } }
        Reflect.set(instance, "_mesh", mockMesh)
        instance.dispose()
        expect(mockPhysicsSystem.destroyBodyIds).toHaveBeenCalledWith(Reflect.get(instance, "_joltBodyId"))
        expect(mockMesh.geometry.dispose).toHaveBeenCalled()
        expect(mockMesh.material.dispose).toHaveBeenCalled()
        expect(mockSceneRenderer.scene.remove).toHaveBeenCalledWith(mockMesh)
    })
})
