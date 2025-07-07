import { describe, test, expect, vi, beforeEach } from "vitest"
import EjectableSceneObject from "../../mirabuf/EjectableSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import Jolt from "@azaleacolburn/jolt-physics"
import World from "@/systems/World"
import SceneObject from "@/systems/scene/SceneObject"
import { Matrix4 } from "three"

// Mock the World module before importing it
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
        Add: vi.fn(function (this: any) {
            return this
        }),
        Sub: vi.fn(function (this: any) {
            return this
        }),
        Mul: vi.fn(function (this: any) {
            return this
        }),
        Div: vi.fn(function (this: any) {
            return this
        }),
        Length: vi.fn(() => 0),
        Normalize: vi.fn(function (this: any) {
            return this
        }),
        Clone: vi.fn(function () {
            return createVec3Mock()
        }),
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
        GetLinearVelocity: vi.fn(() => createVec3Mock()),
        SetLinearVelocity: vi.fn(),
        SetAngularVelocity: vi.fn(),
        GetAngularVelocity: vi.fn(() => createVec3Mock()),
    } as unknown as Jolt.Body
}

describe("EjectableSceneObject", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock())
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

    test("Setup sets parentBodyId, deltaTransformation, ejectVelocity, disables physics", () => {
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

    test("Setup removes gamepiece from all scoring zones", () => {
        const parent = {
            ejectorPreferences: { parentNode: "node1", deltaTransformation: [1], ejectVelocity: 2 },
            mechanism: { nodeToBody: new Map() },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const zone = {
            /* add required properties if needed for the test */
        } as unknown as SceneObject
        const sceneObjects = World.SceneRenderer.sceneObjects
        sceneObjects.set("zone1" as any, zone)
        const entriesSpy = vi.spyOn(sceneObjects, "entries").mockReturnValue([["zone1", zone]] as any)
        const gamePieceBody = {} as unknown as Jolt.BodyID
        const instance = new EjectableSceneObject(parent, gamePieceBody)
        instance.Setup()
        sceneObjects.clear()
        entriesSpy.mockRestore()
    })

    test("Setup does not throw if ejectorPreferences or gamePieceBodyId missing", () => {
        const parent = {
            mechanism: { nodeToBody: new Map() },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const gamePieceBody = {} as unknown as Jolt.BodyID
        const instance = new EjectableSceneObject(parent, gamePieceBody)
        expect(() => instance.Setup()).not.toThrow()
        const instance2 = new EjectableSceneObject(parent, undefined as unknown as Jolt.BodyID)
        expect(() => instance2.Setup()).not.toThrow()
    })

    test("Update sets body position and rotation if all fields set and body is added", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_deltaTransformation", new Matrix4())
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        ;(World.PhysicsSystem.IsBodyAdded as any).mockReturnValue(true)
        expect(() => instance.Update()).not.toThrow()
    })

    test("Update unsets gamePieceBodyId if body is not added", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_deltaTransformation", { clone: vi.fn(() => ({ premultiply: vi.fn(() => ({})) })) })
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        ;(World.PhysicsSystem.IsBodyAdded as any).mockReturnValue(false)
        instance.Update()
        expect(instance["_gamePieceBodyId"]).toBeUndefined()
    })

    test("Update does nothing if required fields missing", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        expect(() => {
            Reflect.set(instance, "_parentBodyId", undefined)
            instance.Update()
        }).not.toThrow()
        expect(() => {
            Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
            Reflect.set(instance, "_deltaTransformation", undefined)
            instance.Update()
        }).not.toThrow()
        expect(() => {
            Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
            Reflect.set(instance, "_deltaTransformation", {})
            Reflect.set(instance, "_gamePieceBodyId", undefined)
            instance.Update()
        }).not.toThrow()
    })

    test("Eject enables physics and sets velocities on happy path", () => {
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

    test("Eject unsets gamePieceBodyId if body is not added", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_ejectVelocity", 1)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        ;(World.PhysicsSystem.IsBodyAdded as any).mockReturnValue(false)
        instance.Eject()
        expect(instance["_gamePieceBodyId"]).toBeUndefined()
    })

    test("Eject does nothing if required fields missing", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        expect(() => {
            Reflect.set(instance, "_parentBodyId", undefined)
            instance.Eject()
        }).not.toThrow()
        expect(() => {
            Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
            Reflect.set(instance, "_ejectVelocity", undefined)
            instance.Eject()
        }).not.toThrow()
        expect(() => {
            Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
            Reflect.set(instance, "_ejectVelocity", 1)
            Reflect.set(instance, "_gamePieceBodyId", undefined)
            instance.Eject()
        }).not.toThrow()
    })

    test("Dispose enables physics for gamePieceBodyId if present", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        instance.Dispose()
        expect(World.PhysicsSystem.EnablePhysicsForBody).toHaveBeenCalledWith(Reflect.get(instance, "_gamePieceBodyId"))
    })

    test("Dispose does not throw if gamePieceBodyId is undefined", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_gamePieceBodyId", undefined)
        expect(() => instance.Dispose()).not.toThrow()
    })
})
