import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import type IntakeSensorSceneObject from "@/mirabuf/IntakeSensorSceneObject"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufParser from "@/mirabuf/MirabufParser"
import type Mechanism from "@/systems/physics/Mechanism"
import type { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import type MirabufInstance from "../../mirabuf/MirabufInstance"
import MirabufInstanceClass from "../../mirabuf/MirabufInstance"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import { createBodyMock } from "../mocks/jolt"

const mockPhysicsSystem = {
    createMechanismFromParser: vi.fn(() => mockMechanism()),
    setBodyAssociation: vi.fn(),
    getBody: vi.fn(() => createBodyMock() as unknown),
    enablePhysicsForBody: vi.fn(),
    disablePhysicsForBody: vi.fn(),
    removeBodyAssociation: vi.fn(),
    destroyMechanism: vi.fn(),
    setBodyPosition: vi.fn(),
    setBodyRotation: vi.fn(),
    setShape: vi.fn(),
    createSensor: vi.fn(),
}
const mockSceneRenderer = {
    sceneObjects: new Map(),
    scene: { add: vi.fn(), remove: vi.fn() },
    registerSceneObject: vi.fn(),
    removeSceneObject: vi.fn(),
    createSphere: vi.fn(() => ({ material: {}, geometry: {}, position: {}, rotation: {} })),
    currentCameraControls: { focusProvider: undefined, controlsType: "Orbit", locked: false },
    worldToPixelSpace: vi.fn(() => [0, 0]),
    createToonMaterial: vi.fn(() => ({ color: 0x123456 })),
    setupMaterial: vi.fn(),
    mirabufSceneObjects: {
        getField: vi.fn(),
    },
}
const mockSimulationSystem = {
    registerMechanism: vi.fn(),
    getSimulationLayer: vi.fn(() => ({ setBrain: vi.fn() })),
    unregisterMechanism: vi.fn(),
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return mockPhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
        get simulationSystem() {
            return mockSimulationSystem
        },
    },
}))

vi.mock("@/systems/preferences/PreferencesSystem", () => ({
    default: {
        getRobotPreferences: vi.fn(() => ({
            intake: { deltaTransformation: [1], zoneDiameter: 1, parentNode: "n", showZoneAlways: false, maxPieces: 1 },
            ejector: { deltaTransformation: [1], ejectorVelocity: 1, parentNode: "n", ejectOrder: "FIFO" },
            simConfig: undefined,
        })),
        getFieldPreferences: vi.fn(() => ({ defaultSpawnLocation: [0, 1, 0], scoringZones: [] })),
        getGlobalPreference: vi.fn(() => false),
        addPreferenceEventListener: vi.fn(() => () => {}),
        setRobotPreferences: vi.fn(),
        savePreferences: vi.fn(),
    },
}))

vi.mock("@/ui/components/SceneOverlayEvents", () => ({
    SceneOverlayTag: vi.fn(() => ({ dispose: vi.fn() })),
}))

vi.mock("@/systems/simulation/synthesis_brain/SynthesisBrain", () => ({
    default: vi.fn(() => ({ inputSchemeName: "TestScheme", clearControls: vi.fn() })),
}))

vi.mock("@/systems/simulation/wpilib_brain/WPILibBrain", () => ({
    default: vi.fn(() => ({ loadSimConfig: vi.fn() })),
}))

function mockMechanism(): Mechanism {
    return {
        rootBody: "root",
        nodeToBody: new Map([["root", mockBodyId()]]),
        constraints: [],
        stepListeners: [],
        controllable: true,
        ghostBodies: [],
        layerReserve: { release: vi.fn() },
        getBodyByNodeId: vi.fn(() => mockBodyId()),
        addConstraint: vi.fn(),
        addStepListener: vi.fn(),
        disablePhysics: vi.fn(),
    } as unknown as Mechanism
}

function mockBodyId() {
    return { GetIndex: () => 0, GetIndexAndSequenceNumber: () => 0 }
}

function mockMirabufInstance(): MirabufInstance {
    return {
        parser: {
            assembly: { dynamic: true, info: { name: "TestAssembly" } },
            rootNode: "root",
            rigidNodes: new Map([
                ["root", { id: "root", parts: new Set(), isDynamic: true, isGamePiece: false, mass: 1 }],
            ]),
            globalTransforms: new Map(),
        },
        addToScene: vi.fn(),
        dispose: vi.fn(),
        meshes: new Map(),
        batches: [],
        materials: new Map(),
    } as unknown as MirabufInstance
}

function setPrivate<T>(obj: T, key: string, value: unknown) {
    ;(obj as Record<string, unknown>)[key] = value
}

describe("MirabufSceneObject", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    let instance: MirabufSceneObject
    let mirabufInstance: MirabufInstance
    let progressHandle: ProgressHandle | undefined

    beforeEach(() => {
        vi.clearAllMocks()
        mirabufInstance = mockMirabufInstance()
        progressHandle = undefined
        instance = new MirabufSceneObject(mirabufInstance, "TestAssembly", progressHandle)

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

    test("Setup calls AddToScene, SetBodyAssociation, RegisterMechanism, and sets brain", () => {
        instance.setup()
        expect(mirabufInstance.addToScene).toHaveBeenCalled()
        expect(instance.brain).toBeDefined()
    })

    test("Update calls UpdateMeshTransforms and UpdateBatches", () => {
        const spy = vi.spyOn(instance, "updateMeshTransforms")
        instance.update()
        expect(spy).toHaveBeenCalled()
    })

    test("Dispose cleans up scene objects and mechanism", () => {
        setPrivate(instance, "_ejectables", [{ id: 1, gamePieceBodyId: mockBodyId() }])
        setPrivate(instance, "_scoringZones", [{ id: 2 }])
        setPrivate(instance, "_intakeSensor", { id: 3 } as unknown as IntakeSensorSceneObject)
        instance.dispose()
        expect(mockSceneRenderer.removeSceneObject).toHaveBeenCalled()
        expect(mockPhysicsSystem.destroyMechanism).toHaveBeenCalled()
    })

    test("activeEjectables returns correct body IDs", () => {
        setPrivate(instance, "_ejectables", [
            { gamePieceBodyId: 42 } as { gamePieceBodyId: number },
            { gamePieceBodyId: 99 } as { gamePieceBodyId: number },
        ])
        expect(instance.activeEjectables).toEqual([42, 99])
    })

    test("SetEjectable returns false if not configured or max reached", () => {
        const bodyId = mockBodyId()
        expect(instance.setEjectable(undefined)).toBe(false)
        setPrivate(instance, "_ejectorPreferences", {
            parentNode: "n",
            deltaTransformation: [1],
            ejectorVelocity: 1,
            ejectOrder: "FIFO",
        })
        setPrivate(instance, "_intakePreferences", {
            parentNode: "n",
            deltaTransformation: [1],
            zoneDiameter: 1,
            showZoneAlways: false,
            maxPieces: 0,
            animationDuration: 0.5,
        })
        expect(instance.setEjectable(bodyId)).toBe(false)
    })

    test("SetEjectable returns true and registers ejectable if valid", () => {
        setPrivate(instance, "_ejectorPreferences", {
            parentNode: "n",
            deltaTransformation: [1],
            ejectorVelocity: 1,
            ejectOrder: "FIFO",
        })
        setPrivate(instance, "_intakePreferences", {
            parentNode: "n",
            deltaTransformation: [1],
            zoneDiameter: 1,
            showZoneAlways: false,
            maxPieces: 2,
            animationDuration: 0.5,
        })
        setPrivate(instance, "_ejectables", [])
        const bodyId = mockBodyId()
        bodyId.GetIndexAndSequenceNumber = () => 123
        const result = instance.setEjectable(bodyId)
        expect(result).toBe(true)
    })
})

describe("MirabufSceneObject - Real Systems Integration", () => {
    test("getDimensions returns proper values for Dozer robot", async context => {
        const cacheInfo = await MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT)

        if (!cacheInfo) {
            context.skip()
        }

        const assembly = await MirabufCachingService.get(cacheInfo!.hash)

        if (!assembly) {
            context.skip()
        }

        const parser = new MirabufParser(assembly!)
        const mirabufInstance = new MirabufInstanceClass(parser)

        mirabufInstance.batches.forEach(batch => {
            batch.computeBoundingBox()
        })

        const dozerSceneObject = new MirabufSceneObject(mirabufInstance, "Dozer_v9", undefined)

        const originalDimensions = dozerSceneObject.getDimensions()

        expect(originalDimensions.width).toBeCloseTo(0.84, 0)
        expect(originalDimensions.height).toBeCloseTo(0.48, 0)
        expect(originalDimensions.depth).toBeCloseTo(0.9, 0)
    })
})
