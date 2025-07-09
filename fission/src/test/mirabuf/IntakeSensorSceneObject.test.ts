import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import IntakeSensorSceneObject from "../../mirabuf/IntakeSensorSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import World from "@/systems/World"
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

describe("IntakeSensorSceneObject", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    beforeEach(() => {
        vi.clearAllMocks()
        World.PhysicsSystem.GetBody = vi.fn((_bodyId: Jolt.BodyID) => createBodyMock() as unknown as Jolt.Body)

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

    test("Setup creates sensor", () => {
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
        expect(World.PhysicsSystem.GetBodyAssociation).toBeDefined()
    })

    test("Update sets body position/rotation", () => {
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

    test("Dispose destroys sensor", () => {
        const instance = new IntakeSensorSceneObject({} as unknown as MirabufSceneObject)
        const mockBodyId = {} as unknown as Jolt.BodyID
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        Reflect.set(instance, "_collision", vi.fn())
        instance.Dispose()
        expect(World.PhysicsSystem.DestroyBodyIds).toHaveBeenCalledWith(Reflect.get(instance, "_joltBodyId"))
        expect(World.SceneRenderer.scene.remove).toBeDefined()
    })
})
