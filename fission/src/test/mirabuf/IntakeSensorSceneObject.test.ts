import { describe, test, expect, vi, beforeEach } from "vitest"
import IntakeSensorSceneObject from "../../mirabuf/IntakeSensorSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import World from "@/systems/World"
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

describe("IntakeSensorSceneObject", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock())
    })

    test("Setup sets parentBodyId, deltaTransformation, creates sensor and adds collision listener", () => {
        const mockBodyId = {} as unknown as Jolt.BodyID
        const parent = {
            intakePreferences: { parentNode: "node1", deltaTransformation: [1, 2, 3, 4], zoneDiameter: 10 },
            mechanism: { nodeToBody: new Map([["node1", mockBodyId]]) },
            rootNodeId: "root",
            intakeActive: true,
            SetEjectable: vi.fn(),
        } as unknown as MirabufSceneObject
        const instance = new IntakeSensorSceneObject(parent)
        instance.Setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(World.PhysicsSystem.CreateSensor).toHaveBeenCalled()
        expect(World.PhysicsSystem.GetBodyAssociation).not.toBeUndefined()
    })

    test("Setup does not create sensor or add listener if intakePreferences is missing", () => {
        const parent = {
            intakePreferences: undefined,
            mechanism: { nodeToBody: new Map() },
            rootNodeId: "root",
            intakeActive: true,
            SetEjectable: vi.fn(),
        } as unknown as MirabufSceneObject
        const instance = new IntakeSensorSceneObject(parent)
        instance.Setup()
        expect(World.PhysicsSystem.CreateSensor).not.toHaveBeenCalled()
    })

    test("Update updates body position and rotation when all fields are set", () => {
        const instance = new IntakeSensorSceneObject({} as unknown as MirabufSceneObject)
        const mockBodyId = {} as unknown as Jolt.BodyID
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        Reflect.set(instance, "_parentBodyId", mockBodyId)
        Reflect.set(instance, "_deltaTransformation", {
            clone: vi.fn(() => ({ premultiply: vi.fn(() => ({ decompose: vi.fn() })) })),
        })
        Reflect.set(instance, "_visualIndicator", { position: { copy: vi.fn() }, quaternion: { copy: vi.fn() } })
        instance.Update()
        expect(World.PhysicsSystem.SetBodyPosition).toHaveBeenCalled()
        expect(World.PhysicsSystem.SetBodyRotation).toHaveBeenCalled()
    })

    test("Dispose destroys sensor body and removes collision listener", () => {
        const instance = new IntakeSensorSceneObject({} as unknown as MirabufSceneObject)
        const mockBodyId = {} as unknown as Jolt.BodyID
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        Reflect.set(instance, "_collision", vi.fn())
        instance.Dispose()
        expect(World.PhysicsSystem.DestroyBodyIds).toHaveBeenCalledWith(Reflect.get(instance, "_joltBodyId"))
        expect(World.SceneRenderer.scene.remove).not.toBeUndefined()
    })

    test("IntakeCollision calls SetEjectable when collision is with gamepiece", () => {
        const parent = {
            mechanism: { nodeToBody: new Map() },
            rootNodeId: "root",
            intakeActive: true,
            SetEjectable: vi.fn(),
        } as unknown as MirabufSceneObject
        const instance = new IntakeSensorSceneObject(parent)
        const mockBodyId = {} as unknown as Jolt.BodyID
        instance["IntakeCollision"](mockBodyId)
    })
})
