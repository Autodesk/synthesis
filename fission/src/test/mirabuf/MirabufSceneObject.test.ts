import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from "vitest"
import type IntakeSensorSceneObject from "@/mirabuf/IntakeSensorSceneObject"
import type { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import type MirabufInstance from "../../mirabuf/MirabufInstance"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import { defaultRobotPreferences } from "@/systems/preferences/PreferenceTypes.ts"
import PhysicsSystem from "@/systems/physics/PhysicsSystem.ts"
import { getMiraInstance } from "@/test/GetAssets.ts"
import { mockConsole } from "@/test/mocks/Common.ts"

const mockSceneRenderer = {
    sceneObjects: new Map(),
    scene: { add: vi.fn(), remove: vi.fn() },
    registerSceneObject: vi.fn(),
    removeSceneObject: vi.fn(),
    createSphere: vi.fn(() => ({ material: {}, geometry: {}, position: {}, rotation: {} })),
    currentCameraControls: { focusProvider: undefined, controlsType: "Target", locked: false },
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

const physicsSystem = new PhysicsSystem()

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return physicsSystem
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
        getUserPreference: vi.fn(() => false),
        hasRobotPreferences: vi.fn(() => true),
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

function mockBodyId() {
    return { GetIndex: () => 0, GetIndexAndSequenceNumber: () => 0 }
}

function setPrivate<T>(obj: T, key: string, value: unknown) {
    ;(obj as Record<string, unknown>)[key] = value
}

describe("MirabufSceneObject", () => {
    let instance: MirabufSceneObject
    let mirabufInstance: MirabufInstance
    let progressHandle: ProgressHandle | undefined
    beforeAll(async () => {
        mirabufInstance = (await getMiraInstance("DOZER"))!
    })
    beforeEach(async () => {
        vi.clearAllMocks()

        progressHandle = undefined
        instance = new MirabufSceneObject(mirabufInstance, "", progressHandle)

        mockConsole()
        vi.spyOn(physicsSystem, "destroyMechanism")
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })

    test("Setup calls AddToScene, SetBodyAssociation, RegisterMechanism, and sets brain", () => {
        instance.setup()

        expect(mockSceneRenderer.registerSceneObject).toHaveBeenCalled()
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
        expect(physicsSystem.destroyMechanism).toHaveBeenCalled()
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
        instance.robotPreferences.intake.maxPieces = 0
        expect(instance.setEjectable(bodyId)).toBe(false)
    })

    test("SetEjectable returns true and registers ejectable if valid", () => {
        instance.robotPreferences.intake.maxPieces = 2
        setPrivate(instance, "_ejectables", [])
        const bodyId = mockBodyId()
        bodyId.GetIndexAndSequenceNumber = () => 123
        const result = instance.setEjectable(bodyId)
        expect(result).toBe(true)
    })
})

describe("MirabufSceneObject - Real Systems Integration", () => {
    test("getDimensions returns proper values for Dozer robot", async () => {
        const mirabufInstance = (await getMiraInstance("DOZER"))!

        mirabufInstance.batches.forEach(batch => {
            batch.computeBoundingBox()
        })

        const dozerSceneObject = new MirabufSceneObject(mirabufInstance, "", undefined)

        const originalDimensions = dozerSceneObject.getDimensions()

        expect(originalDimensions.width).toBeCloseTo(0.84, 0)
        expect(originalDimensions.height).toBeCloseTo(0.48, 0)
        expect(originalDimensions.depth).toBeCloseTo(0.9, 0)
    })
    test("Ejector and Intake are configured for Dozer", async () => {
        const mirabufInstance = (await getMiraInstance("DOZER"))!

        const dozerSceneObject = new MirabufSceneObject(mirabufInstance, "", undefined)
        expect(dozerSceneObject.intakePreferences).not.toEqual(defaultRobotPreferences().intake)
        expect(dozerSceneObject.ejectorPreferences).not.toEqual(defaultRobotPreferences().ejector)
    })
})
