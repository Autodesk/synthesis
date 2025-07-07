import { describe, test, expect, vi, beforeEach } from "vitest"
import ScoringZoneSceneObject, { OnScoreChangedEvent } from "../../mirabuf/ScoringZoneSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import Jolt from "@azaleacolburn/jolt-physics"

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

function createVec3Mock() {
    return {
        GetX: vi.fn(() => 0),
        GetY: vi.fn(() => 0),
        GetZ: vi.fn(() => 0),
    }
}

function createQuatMock() {
    return {
        GetX: vi.fn(() => 0),
        GetY: vi.fn(() => 0),
        GetZ: vi.fn(() => 0),
        GetW: vi.fn(() => 1),
    }
}

function createBodyMock(): Jolt.Body {
    return {
        GetWorldTransform: vi.fn(() => ({
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => createQuatMock()),
        })),
        GetTranslation: vi.fn(() => createVec3Mock()),
        GetQuaternion: vi.fn(() => createQuatMock()),
        GetCenterOfMassTransform: vi.fn(() => ({
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => createQuatMock()),
        })),
        GetRotation: vi.fn(() => ({})),
        GetID: vi.fn(),
        IsActive: vi.fn(),
        IsRigidBody: vi.fn(),
        IsSoftBody: vi.fn(),
        IsStatic: vi.fn(),
        IsKinematic: vi.fn(),
        IsDynamic: vi.fn(),
        CanBeKinematicOrDynamic: vi.fn(),
        GetBodyType: vi.fn(),
        GetMotionType: vi.fn(),
        SetIsSensor: vi.fn(),
        IsSensor: vi.fn(),
        SetUserData: vi.fn(),
        GetUserData: vi.fn(),
    } as unknown as Jolt.Body
}

describe("ScoringZoneSceneObject", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        SimulationSystem.redScore = 0
        SimulationSystem.blueScore = 0
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock())
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
    })

    test("Setup sets parentBodyId, creates sensor, mesh, adds collision listeners", () => {
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

    test("Setup does not create sensor or add listeners if preferences are missing", () => {
        const parent = {
            fieldPreferences: undefined,
            mechanism: { nodeToBody: new Map() },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const instance = new ScoringZoneSceneObject(parent, 0)
        instance.Setup()
        expect(World.PhysicsSystem.CreateSensor).not.toHaveBeenCalled()
    })

    test("Update sets body position, rotation, and shape when all fields are set", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        Reflect.set(instance, "_parentBodyId", mockBodyId)
        Reflect.set(instance, "_deltaTransformation", {
            clone: vi.fn(() => ({ premultiply: vi.fn(() => ({ decompose: vi.fn() })) })),
        })
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        Reflect.set(instance, "_prefs", { alliance: "red", points: 10, persistentPoints: false })
        Reflect.set(instance, "_mesh", {
            position: { set: vi.fn() },
            rotation: { setFromQuaternion: vi.fn() },
            scale: { set: vi.fn() },
            material: {},
        })
        instance.Update()
        expect(World.PhysicsSystem.SetBodyPosition).toHaveBeenCalled()
        expect(World.PhysicsSystem.SetBodyRotation).toHaveBeenCalled()
        expect(World.PhysicsSystem.SetShape).toHaveBeenCalled()
    })

    test("Dispose destroys sensor body and cleans up mesh", () => {
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

    test("ZoneCollision adds gamepiece to contacted list when persistentPoints is true", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: true })
        const gamePieceBody = {} as unknown as Jolt.BodyID
        World.PhysicsSystem.GetBodyAssociation = vi.fn(() => ({ isGamePiece: true, associatedBody: 0 }))
        instance["ZoneCollision"](gamePieceBody)
        expect(instance["_gpContacted"]).toContain(gamePieceBody)
    })

    test("ZoneCollision updates red score when persistentPoints is false and alliance is red", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: false, alliance: "red", points: 10 })
        const gamePieceBody = {} as unknown as Jolt.BodyID
        World.PhysicsSystem.GetBodyAssociation = vi.fn(() => ({ isGamePiece: true, associatedBody: 0 }))
        const dispatchSpy = vi.spyOn(OnScoreChangedEvent.prototype, "Dispatch")
        instance["ZoneCollision"](gamePieceBody)
        expect(SimulationSystem.redScore).toBe(10)
        expect(dispatchSpy).toHaveBeenCalled()
    })

    test("ZoneCollision does not update score when collision is not with gamepiece", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: false, alliance: "red", points: 10 })
        const nonGamePieceBody = {} as unknown as Jolt.BodyID
        World.PhysicsSystem.GetBodyAssociation = vi.fn(() => ({ isGamePiece: false, associatedBody: 0 }))
        instance["ZoneCollision"](nonGamePieceBody)
        expect(SimulationSystem.redScore).toBe(0)
    })

    test("RemoveGamepiece removes gamepiece from contacted list when persistentPoints is true", () => {
        const instance = new ScoringZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", { persistentPoints: true })
        const mockBody = {
            GetIndex: vi.fn(() => 1),
            GetSequenceNumber: vi.fn(() => 1),
            GetIndexAndSequenceNumber: vi.fn(() => 1),
        }
        Reflect.set(instance, "_gpContacted", [mockBody])
        ScoringZoneSceneObject.RemoveGamepiece(instance, mockBody)
        expect(instance["_gpContacted"]).not.toContain(mockBody)
    })

    test("OnScoreChangedEvent dispatches event to window", () => {
        const dispatchSpy = vi.spyOn(window, "dispatchEvent")
        const event = new OnScoreChangedEvent(10, 20)
        event.Dispatch()
        expect(dispatchSpy).toHaveBeenCalledWith(event)
    })
})
