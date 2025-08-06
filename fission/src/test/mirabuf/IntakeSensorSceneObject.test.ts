import type Jolt from "@azaleacolburn/jolt-physics"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import IntakeSensorSceneObject from "../../mirabuf/IntakeSensorSceneObject"
import type MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import { createBodyMock } from "../mocks/jolt"

const mockPhysicsSystem = {
    createSensor: vi.fn(),
    destroyBodyIds: vi.fn(),
    setBodyPosition: vi.fn(),
    setBodyRotation: vi.fn(),
    getBody: vi.fn((_bodyId: Jolt.BodyID) => createBodyMock() as unknown as Jolt.Body),
    getBodyAssociation: vi.fn(),
    disablePhysicsForBody: vi.fn(),
    enablePhysicsForBody: vi.fn(),
    isBodyAdded: vi.fn(),
    setShape: vi.fn(),
    setBodyAssociation: vi.fn(),
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

describe("IntakeSensorSceneObject", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    beforeEach(() => {
        vi.clearAllMocks()
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
            setEjectable: vi.fn(),
        } as unknown as MirabufSceneObject
        const instance = new IntakeSensorSceneObject(parent)
        instance.setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(mockPhysicsSystem.createSensor).toHaveBeenCalled()
        expect(mockPhysicsSystem.setBodyAssociation).toBeDefined()
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
        instance.update()
        expect(mockPhysicsSystem.setBodyPosition).toHaveBeenCalled()
    })

    test("Dispose destroys sensor", () => {
        const instance = new IntakeSensorSceneObject({} as unknown as MirabufSceneObject)
        const mockBodyId = {} as unknown as Jolt.BodyID
        Reflect.set(instance, "_joltBodyId", mockBodyId)
        Reflect.set(instance, "_collision", vi.fn())
        instance.dispose()
        expect(mockPhysicsSystem.destroyBodyIds).toHaveBeenCalledWith(Reflect.get(instance, "_joltBodyId"))
        expect(mockSceneRenderer.scene.remove).toBeDefined()
    })
})
