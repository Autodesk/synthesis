import type Jolt from "@azaleacolburn/jolt-physics"
import { afterEach, assert, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import ProtectedZoneSceneObject from "../../mirabuf/ProtectedZoneSceneObject"
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
        getRobots: vi.fn(),
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

vi.mock("@/systems/match_mode/ScoreTracker", () => ({
    default: {
        robotPenalty: vi.fn(),
    },
}))

vi.mock("@/systems/match_mode/MatchMode", () => ({
    MatchModeType: {
        SANDBOX: "Sandbox",
        AUTONOMOUS: "Autonomous",
        TELEOP: "Teleop",
        ENDGAME: "Endgame",
        MATCH_ENDED: "Match Ended",
    },
    default: {
        getInstance: vi.fn(() => ({
            getMatchModeType: vi.fn(() => "Teleop"),
        })),
    },
}))

type RobotsInside = "red" | "blue" | "neither" | "both"

const redBox = new JOLT.AABox(new JOLT.Vec3(0, 0, 0), new JOLT.Vec3(1, 1, 1))
const blueBox = new JOLT.AABox(new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(2, 1, 1))

const boundingConfigMap: Record<RobotsInside, Jolt.AABox> = {
    // Just `redBox` translated -0.5 along the x-axis
    red: new JOLT.AABox(new JOLT.Vec3(-0.5, 0, 0), new JOLT.Vec3(0.5, 1, 1)),
    // Just `blueBox` translated +0.5 along the x-axis
    blue: new JOLT.AABox(new JOLT.Vec3(1.5, 0, 0), new JOLT.Vec3(2.5, 1, 1)),

    neither: new JOLT.AABox(new JOLT.Vec3(-1, -1, -1), new JOLT.Vec3(-2, -2, -2)),
    both: new JOLT.AABox(new JOLT.Vec3(0, 0, 0), new JOLT.Vec3(3, 3, 3)),
}

describe("ProtectedZoneSceneObject", () => {
    let redRobot: MirabufSceneObject
    let blueRobot: MirabufSceneObject

    const createMockRobot = (alliance: string) => {
        const robot = {
            miraType: MiraType.ROBOT,
            alliance,
            getBounding: vi.fn(alliance === "red" ? () => redBox : () => blueBox),
        } as unknown as MirabufSceneObject

        return robot
    }

    const createProtectedZoneInstance = (prefs: Partial<ProtectedZonePreferences>, robots: RobotsInside) => {
        const bounding = boundingConfigMap[robots]

        const instance = new ProtectedZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "prefs", {
            activeDuring: [MatchModeType.TELEOP],
            alliance: "red",
            penaltyPoints: 5,
            contactType: ContactType.ROBOT_ENTERS,
            ...prefs,
        })
        instance.bounding = bounding

        return instance
    }

    const setupMultipleAssociations = (associations: Map<number, MirabufSceneObject>) => {
        mockPhysicsSystem.getBodyAssociation = vi.fn((bodyId: Jolt.BodyID) => {
            const id = bodyId.GetIndexAndSequenceNumber()
            const robot = associations.get(id)
            return robot ? { sceneObject: robot } : undefined
        })
    }

    beforeEach(() => {
        vi.clearAllMocks()

        redRobot = createMockRobot("red")
        blueRobot = createMockRobot("blue")

        setupMultipleAssociations(
            new Map([
                [1, redRobot],
                [2, blueRobot],
            ])
        )

        mockSceneRenderer.mirabufSceneObjects.getRobots = vi.fn(() => [redRobot, blueRobot])
    })

    afterEach(() => {
        vi.clearAllMocks()
    })

    test("ZoneCollision applies penalty to opposing robot", () => {
        const instance = createProtectedZoneInstance({}, "blue")

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("ZoneCollision does not penalize same alliance robot", () => {
        const instance = createProtectedZoneInstance(
            {
                activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME],
            },
            "red"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision does not penalize when both robots must be inside but only one robot is in the zone", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.BOTH_ROBOTS_INSIDE,
            },
            "red"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision doesn't penalize if robot is not a robot", () => {
        const fieldObject = createMockRobot("red")
        Reflect.set(fieldObject, "miraType", MiraType.FIELD)

        // const fieldBodyId = createMockBodyId(3)
        setupMultipleAssociations(new Map([[3, fieldObject]]))

        const instance = createProtectedZoneInstance({}, "red")

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty both robots inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.BOTH_ROBOTS_INSIDE,
            },
            "both"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty any robot inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.ANY_ROBOT_INSIDE,
            },
            "blue"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty blue robot inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.BLUE_ROBOT_INSIDE,
            },
            "blue"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty red robot inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.RED_ROBOT_INSIDE,
            },
            "red"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty doesn't penalize if not all robots are inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.BOTH_ROBOTS_INSIDE,
            },
            "blue"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is any and no robots are inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.ANY_ROBOT_INSIDE,
            },
            "neither"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is red and red is not inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.RED_ROBOT_INSIDE,
            },
            "blue"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is blue and blue is not inside", () => {
        const instance = createProtectedZoneInstance(
            {
                contactType: ContactType.BLUE_ROBOT_INSIDE,
            },
            "red"
        )

        instance["checkObjectsInZone"]()

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if robots are from same alliance", () => {
        const redRobot2 = createMockRobot("red")
        // const redRobot2BodyId = createMockBodyId(3)
        setupMultipleAssociations(
            new Map([
                [1, redRobot],
                [2, blueRobot],
                [3, redRobot2],
            ])
        )
        const instance = createProtectedZoneInstance({}, "both")

        instance["penalizeEnteringZone"](redRobot)
        instance["penalizeEnteringZone"](redRobot2)

        instance["handleContactPenalty"](redRobot, redRobot2)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })
})
