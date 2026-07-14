import type Jolt from "@synthesis.adsk/jolt-physics"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import EventSystem from "@/systems/EventSystem.ts"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import ScoringZoneSceneObject from "../../mirabuf/ScoringZoneSceneObject"
import { createBodyMock } from "../mocks/jolt"
import JOLT from "@/util/loading/JoltSyncLoader"

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

    describe("checkObjectsInZone", () => {
        const createZoneWithBounding = (alliance: "red" | "blue", points: number) => {
            const parent = {} as unknown as MirabufSceneObject
            Reflect.set(parent, "fieldPreferences", {
                scoringZones: [
                    {
                        shouldPointsAccumulate: true,
                        alliance,
                        points,
                        name: "Test",
                        parentNode: undefined,
                        deltaTransformation: [],
                    },
                ],
            })
            const zone = new ScoringZoneSceneObject(parent, 0)
            zone.bounding = new JOLT.OrientedBox(
                new JOLT.Mat44().sTranslation(new JOLT.Vec3(0, 0, 0)),
                new JOLT.Vec3(1, 1, 1)
            )
            return zone
        }

        const makeField = (gpId: Jolt.BodyID) => ({
            mirabufInstance: {
                parser: {
                    rigidNodes: new Map([["gp_0", { isGamePiece: true, id: "gp_0" }]]),
                },
            },
            mechanism: {
                nodeToBody: new Map([["gp_0", gpId]]),
            },
        })

        test("scores when game piece overlaps zone", () => {
            const mockBodyId = {} as unknown as Jolt.BodyID
            mockSceneRenderer.mirabufSceneObjects.getField = vi.fn(() => makeField(mockBodyId))

            mockPhysicsSystem.getBody = vi.fn((_bodyId: Jolt.BodyID) => {
                const bodyMock = createBodyMock()
                bodyMock.GetWorldSpaceBounds = vi.fn(
                    () => new JOLT.AABox(new JOLT.Vec3(-0.2, -0.2, -0.2), new JOLT.Vec3(0.2, 0.2, 0.2))
                )

                return bodyMock as unknown as Jolt.Body
            })
            mockPhysicsSystem.getBodyAssociation = vi.fn(() => ({ robotLastInContactWith: undefined }))

            const zone = createZoneWithBounding("red", 10)
            zone["checkObjectsInZone"]()

            expect(ScoreTracker.redScore).toBe(10)
        })

        test("does not score when game piece is outside zone", () => {
            const mockBodyId = {} as unknown as Jolt.BodyID
            mockSceneRenderer.mirabufSceneObjects.getField = vi.fn(() => makeField(mockBodyId))

            mockPhysicsSystem.getBody = vi.fn((_bodyId: Jolt.BodyID) => {
                const bodyMock = createBodyMock()
                bodyMock.GetWorldSpaceBounds = vi.fn(
                    () => new JOLT.AABox(new JOLT.Vec3(10, 10, 10), new JOLT.Vec3(10.2, 10.2, 10.2))
                )

                return bodyMock as unknown as Jolt.Body
            })

            const zone = createZoneWithBounding("red", 10)
            zone["checkObjectsInZone"]()

            expect(ScoreTracker.redScore).toBe(0)
        })

        test("warns when game pieces exist but have no body IDs", () => {
            const warnSpy = vi.spyOn(console, "warn")
            const mockField = {
                mirabufInstance: {
                    parser: {
                        rigidNodes: new Map([["gp_0", { isGamePiece: true, id: "gp_0" }]]),
                    },
                },
                mechanism: {
                    nodeToBody: new Map(),
                },
            }
            mockSceneRenderer.mirabufSceneObjects.getField = vi.fn(() => mockField)

            const zone = createZoneWithBounding("red", 10)
            zone["checkObjectsInZone"]()

            expect(warnSpy).toHaveBeenCalledWith(
                expect.stringContaining("game piece nodes exist but none have body IDs")
            )
            expect(ScoreTracker.redScore).toBe(0)
        })
    })
})
