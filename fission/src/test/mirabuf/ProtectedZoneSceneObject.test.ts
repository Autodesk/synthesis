import type Jolt from "@azaleacolburn/jolt-physics"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import type MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import ProtectedZoneSceneObject from "../../mirabuf/ProtectedZoneSceneObject"
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

describe("ProtectedZoneSceneObject", () => {
    let redRobot: MirabufSceneObject
    let blueRobot: MirabufSceneObject
    let redRobotBodyId: Jolt.BodyID
    let blueRobotBodyId: Jolt.BodyID

    const createMockRobot = (alliance: string) =>
        ({
            miraType: MiraType.ROBOT,
            alliance,
        }) as unknown as MirabufSceneObject

    const createMockBodyId = (id: number) =>
        ({
            GetIndexAndSequenceNumber: () => id,
        }) as unknown as Jolt.BodyID

    const createProtectedZoneInstance = (prefs: Partial<ProtectedZonePreferences>) => {
        const instance = new ProtectedZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", {
            activeDuring: [MatchModeType.TELEOP],
            alliance: "red",
            penaltyPoints: 5,
            contactType: ContactType.ROBOT_ENTERS,
            ...prefs,
        })
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
        redRobotBodyId = createMockBodyId(1)
        blueRobotBodyId = createMockBodyId(2)

        setupMultipleAssociations(
            new Map([
                [1, redRobot],
                [2, blueRobot],
            ])
        )
    })

    afterEach(() => {
        vi.clearAllMocks()
    })

    test("ZoneCollision applies penalty to opposing robot", () => {
        const instance = createProtectedZoneInstance({})

        instance["zoneCollision"](blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("ZoneCollision does not penalize same alliance robot", () => {
        const instance = createProtectedZoneInstance({
            activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME],
        })

        instance["zoneCollision"](redRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision does not penalize when zone is inactive", () => {
        const instance = createProtectedZoneInstance({
            activeDuring: [MatchModeType.AUTONOMOUS],
        })

        instance["zoneCollision"](blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision does not penalize when both robots must be inside but only one robot is in the zone", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.BOTH_ROBOTS_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision doesn't penalize if robot robot is already inside", () => {
        const instance = createProtectedZoneInstance({})

        instance["zoneCollision"](blueRobotBodyId)
        instance["zoneCollision"](blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledTimes(1)
    })

    test("ZoneCollision doesn't penalize if robot is not a robot", () => {
        const fieldObject = createMockRobot("red")
        Reflect.set(fieldObject, "miraType", MiraType.FIELD)

        const fieldBodyId = createMockBodyId(3)
        setupMultipleAssociations(new Map([[3, fieldObject]]))

        const instance = createProtectedZoneInstance({})
        instance["zoneCollision"](fieldBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty both robots inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.BOTH_ROBOTS_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)
        instance["zoneCollision"](redRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty any robot inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.ANY_ROBOT_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty blue robot inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.BLUE_ROBOT_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty red robot inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.RED_ROBOT_INSIDE,
        })

        instance["zoneCollision"](redRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledExactlyOnceWith(blueRobot, 5, expect.any(String))
    })

    test("HandleContactPenalty doesn't penalize if not all robots are inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.BOTH_ROBOTS_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is any and no robots are inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.ANY_ROBOT_INSIDE,
        })

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is red and red is not inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.RED_ROBOT_INSIDE,
        })

        instance["zoneCollision"](blueRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if contact type is blue and blue is not inside", () => {
        const instance = createProtectedZoneInstance({
            contactType: ContactType.BLUE_ROBOT_INSIDE,
        })

        instance["zoneCollision"](redRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if robots are from same alliance", () => {
        const redRobot2 = createMockRobot("red")
        const redRobot2BodyId = createMockBodyId(3)
        setupMultipleAssociations(
            new Map([
                [1, redRobot],
                [2, blueRobot],
                [3, redRobot2],
            ])
        )
        const instance = createProtectedZoneInstance({})

        instance["zoneCollision"](redRobotBodyId)
        instance["zoneCollision"](redRobot2BodyId)

        instance["handleContactPenalty"](redRobotBodyId, redRobot2BodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).not.toHaveBeenCalled()
    })

    test("HandleContactPenalty doesn't penalize if collision occurs too quickly", () => {
        const instance = createProtectedZoneInstance({})

        instance["zoneCollision"](blueRobotBodyId)
        instance["zoneCollision"](redRobotBodyId)

        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)
        instance["handleContactPenalty"](redRobotBodyId, blueRobotBodyId)

        expect(vi.mocked(ScoreTracker.robotPenalty)).toHaveBeenCalledTimes(1)
    })
})
