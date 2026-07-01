import type Jolt from "@azaleacolburn/jolt-physics"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import EventSystem from "@/systems/EventSystem.ts"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import ScoringZoneSceneObject from "../../mirabuf/ScoringZoneSceneObject"
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
}
const mockSceneRenderer = {
    sceneObjects: new Map(),
    createBox: vi.fn(),
    scene: {
        remove: vi.fn(),
    },
    mirabufSceneObjects: {
        getField: vi.fn(),
    },
    addObject: vi.fn(),
    removeObject: vi.fn(),
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

describe("ScoringZoneSceneObject", () => {
    const originalConsoleLog = console.log

    beforeEach(() => {
        vi.clearAllMocks()
        ScoreTracker.resetScores()
        console.log = vi.fn()
    })

    afterEach(() => {
        vi.clearAllMocks()
        console.log = originalConsoleLog
    })

    test("Setup creates mesh", () => {
        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        const parent = {
            fieldPreferences: {
                scoringZones: [
                    {
                        parentNode: "node1",
                        deltaTransformation: [1, 2, 3, 4],
                        alliance: "red",
                        points: 10,
                        shouldPointsAccumulate: true,
                    },
                ],
            },
            mechanism: { nodeToBody: new Map([["node1", mockBodyId]]) },
            rootNodeId: "node1",
        } as unknown as MirabufSceneObject

        const instance = new ScoringZoneSceneObject(parent, 0)
        instance.setup()

        expect(instance["parentBodyId"]).toBe(mockBodyId)
    })

    test("ZoneCollision updates score", () => {
        const parent = {} as unknown as MirabufSceneObject
        Reflect.set(parent, "fieldPreferences", {
            scoringZones: [{ shouldPointsAccumulate: true, alliance: "red", points: 10 }],
        })
        const instance = new ScoringZoneSceneObject(parent, 0)

        const gamePieceId = {} as unknown as Jolt.BodyID
        mockPhysicsSystem.getBodyAssociation = vi.fn(() => ({ isGamePiece: true, associatedBody: 0 }))

        const dispatchSpy = vi.fn()
        const unsubscribe = EventSystem.listen("ScoreChangedEvent", dispatchSpy)

        instance["zoneCollision"](gamePieceId)

        expect(ScoreTracker.redScore).toBe(10)
        expect(dispatchSpy).toHaveBeenCalled()

        unsubscribe()
    })

    test("Dispose destroys mesh and bounding box", () => {
        const parent = {} as unknown as MirabufSceneObject
        Reflect.set(parent, "fieldPreferences", {
            scoringZones: [{ shouldPointsAccumulate: true, alliance: "red", points: 10 }],
        })

        const zone = new ScoringZoneSceneObject(parent, 0)

        const mockBodyId = { GetIndexAndSequenceNumber: () => "id" } as unknown
        Reflect.set(zone, "joltBodyId", mockBodyId)

        const mockMesh = { geometry: { dispose: vi.fn() }, material: { dispose: vi.fn() } }
        Reflect.set(zone, "mesh", mockMesh)

        zone.dispose()

        expect(mockMesh.geometry.dispose).toHaveBeenCalled()
        expect(mockSceneRenderer.removeObject).toHaveBeenCalledWith(mockMesh)
    })
})
