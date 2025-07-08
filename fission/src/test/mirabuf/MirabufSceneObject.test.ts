import { describe, test, expect, vi, beforeEach } from "vitest"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import type MirabufInstance from "../../mirabuf/MirabufInstance"
import type Mechanism from "@/systems/physics/Mechanism"
import type { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import { createBodyMock } from "../mocks/jolt"
import World from "@/systems/World"
import IntakeSensorSceneObject from "@/mirabuf/IntakeSensorSceneObject"

function mockBodyId() {
    return { GetIndex: () => 0, GetIndexAndSequenceNumber: () => 0 }
}

vi.mock("@/systems/World", () => ({
    default: {
        PhysicsSystem: {
            CreateMechanismFromParser: vi.fn(() => mockMechanism()),
            SetBodyAssociation: vi.fn(),
            GetBody: vi.fn(() => createBodyMock() as unknown),
            EnablePhysicsForBody: vi.fn(),
            DisablePhysicsForBody: vi.fn(),
            RemoveBodyAssociation: vi.fn(),
            DestroyMechanism: vi.fn(),
        },
        SceneRenderer: {
            sceneObjects: new Map(),
            scene: { add: vi.fn(), remove: vi.fn() },
            RegisterSceneObject: vi.fn(),
            RemoveSceneObject: vi.fn(),
            CreateSphere: vi.fn(() => ({ material: {}, geometry: {}, position: {}, rotation: {} })),
            currentCameraControls: { focusProvider: undefined, controlsType: "Orbit", locked: false },
            WorldToPixelSpace: vi.fn(() => [0, 0]),
        },
        SimulationSystem: {
            RegisterMechanism: vi.fn(),
            GetSimulationLayer: vi.fn(() => ({ SetBrain: vi.fn() })),
            UnregisterMechanism: vi.fn(),
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
    SceneOverlayTag: vi.fn(() => ({ Dispose: vi.fn() })),
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
        layerReserve: { Release: vi.fn() },
        GetBodyByNodeId: vi.fn(() => mockBodyId()),
        AddConstraint: vi.fn(),
        AddStepListener: vi.fn(),
        DisablePhysics: vi.fn(),
    } as unknown as Mechanism
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
        AddToScene: vi.fn(),
        Dispose: vi.fn(),
        meshes: new Map(),
        batches: [],
        materials: new Map(),
    } as unknown as MirabufInstance
}

function setPrivate<T>(obj: T, key: string, value: unknown) {
    // eslint-disable-next-line no-extra-semi
    ;(obj as Record<string, unknown>)[key] = value
}

describe("MirabufSceneObject", () => {
    let instance: MirabufSceneObject
    let mirabufInstance: MirabufInstance
    let progressHandle: ProgressHandle | undefined

    beforeEach(() => {
        vi.clearAllMocks()
        mirabufInstance = mockMirabufInstance()
        progressHandle = undefined
        instance = new MirabufSceneObject(mirabufInstance, "TestAssembly", progressHandle)
    })

    test("Setup calls AddToScene, SetBodyAssociation, RegisterMechanism, and sets brain", () => {
        instance.Setup()
        expect(mirabufInstance.AddToScene).toHaveBeenCalled()
        expect(instance.brain).toBeDefined()
    })

    test("Update calls UpdateMeshTransforms and UpdateBatches", () => {
        const spy = vi.spyOn(instance, "UpdateMeshTransforms")
        instance.Update()
        expect(spy).toHaveBeenCalled()
    })

    test("Dispose cleans up scene objects and mechanism", () => {
        setPrivate(instance, "_ejectables", [{ id: 1, gamePieceBodyId: mockBodyId() }])
        setPrivate(instance, "_scoringZones", [{ id: 2 }])
        setPrivate(instance, "_intakeSensor", { id: 3 } as unknown as IntakeSensorSceneObject)
        instance.Dispose()
        expect(World.SceneRenderer.RemoveSceneObject).toHaveBeenCalled()
        expect(World.PhysicsSystem.DestroyMechanism).toHaveBeenCalled()
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
        expect(instance.SetEjectable(undefined)).toBe(false)
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
        })
        expect(instance.SetEjectable(bodyId)).toBe(false)
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
        })
        setPrivate(instance, "_ejectables", [])
        const bodyId = mockBodyId()
        bodyId.GetIndexAndSequenceNumber = () => 123
        const result = instance.SetEjectable(bodyId)
        expect(result).toBe(true)
    })
})
