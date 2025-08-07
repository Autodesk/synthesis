import type Jolt from "@azaleacolburn/jolt-physics"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import EjectableSceneObject from "../../mirabuf/EjectableSceneObject"
import type MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import { createBodyMock, createVec3Mock } from "../mocks/jolt"

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
}
const mockSceneRenderer = {
    filterSceneObjects: vi.fn().mockReturnValue([]),
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

describe("EjectableSceneObject", () => {
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

    test("Setup disables physics for game piece", () => {
        const mockBodyId = {} as unknown as Jolt.BodyID
        const parent = {
            ejectorPreferences: { parentNode: "node1", deltaTransformation: [1, 2, 3, 4], ejectorVelocity: 5 },
            mechanism: { nodeToBody: new Map([["node1", mockBodyId]]) },
            rootNodeId: "root",
        } as unknown as MirabufSceneObject
        const gamePieceBody = {} as unknown as Jolt.BodyID
        const instance = new EjectableSceneObject(parent, gamePieceBody)
        instance.setup()
        expect(instance["_parentBodyId"]).toBe(mockBodyId)
        expect(mockPhysicsSystem.disablePhysicsForBody).toHaveBeenCalledWith(gamePieceBody)
    })

    test("Eject sets velocities and enables physics", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_parentBodyId", {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_ejectVelocity", 1)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        mockPhysicsSystem.isBodyAdded = vi.fn(() => true)
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
        mockPhysicsSystem.getBody = vi.fn(() => bodyMock)
        expect(() => instance.eject()).not.toThrow()
    })

    test("Dispose enables physics for game piece", () => {
        const instance = new EjectableSceneObject({} as unknown as MirabufSceneObject, {} as unknown as Jolt.BodyID)
        Reflect.set(instance, "_gamePieceBodyId", {} as unknown as Jolt.BodyID)
        instance.dispose()
        expect(mockPhysicsSystem.enablePhysicsForBody).toHaveBeenCalledWith(Reflect.get(instance, "_gamePieceBodyId"))
    })
})
