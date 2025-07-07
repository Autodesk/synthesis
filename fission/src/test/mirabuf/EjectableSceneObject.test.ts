import { describe, test, expect, vi, beforeEach } from "vitest"
import EjectableSceneObject from "../../mirabuf/EjectableSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import Jolt from "@azaleacolburn/jolt-physics"
import World from "@/systems/World"
import { createVec3Mock, createBodyMock } from '../mocks/jolt'

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

describe("EjectableSceneObject", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock() as any)
        // Mock PhysicsSystem
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

    test("Setup disables physics for game piece", () => {
        const mockBodyId = {} as unknown as Jolt.BodyID
        const parent = {
            ejectorPreferences: { parentNode: "node1", deltaTransformation: [1, 2, 3, 4], ejectorVelocity: 5 },
            mechanism: { nodeToBody: new Map([["node1", mockBodyId]]) },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const gamePieceBody = {} as unknown as Jolt.BodyID
        const instance = new EjectableSceneObject(parent, gamePieceBody)
        instance.Setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(World.PhysicsSystem.DisablePhysicsForBody).toHaveBeenCalledWith(gamePieceBody)
    })

    test("Eject sets velocities and enables physics", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_ejectVelocity", 1)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        ;(World.PhysicsSystem.IsBodyAdded as any).mockReturnValue(true)
        const quatMock = {
            GetX: vi.fn(() => 0),
            GetY: vi.fn(() => 0),
            GetZ: vi.fn(() => 0),
            GetW: vi.fn(() => 1),
        }
        const rotationMock = {
            ...quatMock,
            set: vi.fn(),
            clone: vi.fn(() => ({ ...quatMock })),
        }
        const bodyMock = {
            GetWorldTransform: vi.fn(() => ({
                GetTranslation: vi.fn(() => createVec3Mock()),
                GetQuaternion: vi.fn(() => quatMock),
            })),
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => quatMock),
            GetCenterOfMassTransform: vi.fn(() => ({
                GetTranslation: vi.fn(() => createVec3Mock()),
                GetQuaternion: vi.fn(() => quatMock),
            })),
            GetRotation: vi.fn(() => rotationMock),
            GetLinearVelocity: vi.fn(() => createVec3Mock()),
            SetLinearVelocity: vi.fn(),
            SetAngularVelocity: vi.fn(),
            GetAngularVelocity: vi.fn(() => createVec3Mock()),
        } as unknown as Jolt.Body
        World.PhysicsSystem.GetBody = vi.fn(() => bodyMock)
        expect(() => instance.Eject()).not.toThrow()
    })

    test("Dispose enables physics for game piece", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        instance.Dispose()
        expect(World.PhysicsSystem.EnablePhysicsForBody).toHaveBeenCalledWith(Reflect.get(instance, "_gamePieceBodyId"))
    })
})
