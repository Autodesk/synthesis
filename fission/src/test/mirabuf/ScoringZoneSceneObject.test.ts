import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import ScoringZoneSceneObject, { OnScoreChangedEvent } from "../../mirabuf/ScoringZoneSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import Jolt from "@azaleacolburn/jolt-physics"
import { createBodyMock } from "../mocks/jolt"

vi.mock("@/systems/World", () => ({
    default: {
        PhysicsSystem: {
            CreateSensor: vi.fn(),
            DestroyBodyIds: vi.fn(),
            SetBodyPosition: vi.fn(),
            SetBodyRotation: vi.fn(),
            GetBody: vi.fn(() => ({ GetWorldTransform: vi.fn() })),
            GetBodyAssociation: vi.fn(),
            DisablePhysicsForBody: vi.fn(),
            EnablePhysicsForBody: vi.fn(),
            IsBodyAdded: vi.fn(),
            SetShape: vi.fn(),
        },
        SceneRenderer: {
            sceneObjects: new Map(),
            CreateBox: vi.fn(),
            scene: {
                remove: vi.fn(),
            },
        },
    },
}))

describe("ScoringZoneSceneObject", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    beforeEach(() => {
        vi.clearAllMocks()
        SimulationSystem.redScore = 0
        SimulationSystem.blueScore = 0
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock() as unknown as Jolt.Body)
        vi.stubGlobal("World", {
            ...World,
            PhysicsSystem: {
                CreateSensor: vi.fn(),
                DestroyBodyIds: vi.fn(),
                SetBodyPosition: vi.fn(),
                SetBodyRotation: vi.fn(),
                GetBody: vi.fn(() => ({ GetWorldTransform: vi.fn() })),
                GetBodyAssociation: vi.fn(),
                DisablePhysicsForBody: vi.fn(),
                EnablePhysicsForBody: vi.fn(),
                IsBodyAdded: vi.fn(),
                SetShape: vi.fn(),
            },
            SceneRenderer: {
                sceneObjects: new Map(),
                CreateBox: vi.fn(),
                scene: {
                    remove: vi.fn(),
                },
            },
        })

        console.log = vi.fn()
        console.error = vi.fn()
        console.warn = vi.fn()
        console.debug = vi.fn()
    })

    afterEach(() => {
        vi.clearAllMocks()
        console.log = originalConsoleLog
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.debug = originalConsoleDebug
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
        instance.Setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(World.PhysicsSystem.CreateSensor).toHaveBeenCalled()
    })

    test("ZoneCollision updates score", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: false, alliance: "red", points: 10 })
        const gamePieceBody = {} as unknown as Jolt.BodyID
        World.PhysicsSystem.GetBodyAssociation = vi.fn(() => ({ isGamePiece: true, associatedBody: 0 }))
        const dispatchSpy = vi.spyOn(OnScoreChangedEvent.prototype, "Dispatch")
        instance["ZoneCollision"](gamePieceBody)
        expect(SimulationSystem.redScore).toBe(10)
        expect(dispatchSpy).toHaveBeenCalled()
    })

    test("Dispose destroys mesh and sensor", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        const mockMesh = { geometry: { dispose: vi.fn() }, material: { dispose: vi.fn() } }
        Reflect.set(instance, "_mesh", mockMesh)
        instance.Dispose()
        expect(World.PhysicsSystem.DestroyBodyIds).toHaveBeenCalledWith(Reflect.get(instance, "_joltBodyId"))
        expect(mockMesh.geometry.dispose).toHaveBeenCalled()
        expect(mockMesh.material.dispose).toHaveBeenCalled()
        expect(World.SceneRenderer.scene.remove).toHaveBeenCalledWith(mockMesh)
    })
})
