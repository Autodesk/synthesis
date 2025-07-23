import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import ProtectedZoneSceneObject from "../../mirabuf/ProtectedZoneSceneObject"
import MirabufSceneObject from "../../mirabuf/MirabufSceneObject"
import Jolt from "@azaleacolburn/jolt-physics"
import { createBodyMock } from "../mocks/jolt"
import { MatchModeType } from "@/systems/MatchMode"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

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

vi.mock("@/systems/simulation/SimulationSystem", () => ({
    default: {
        robotPenalty: vi.fn(),
    },
}))

vi.mock("@/systems/MatchMode", () => ({
    default: {
        getInstance: vi.fn(() => ({
            getMatchModeType: vi.fn(() => "TELEOP"),
        })),
    },
    MatchModeType: {
        AUTONOMOUS: "AUTONOMOUS",
        TELEOP: "TELEOP", 
        ENDGAME: "ENDGAME",
    },
}))

describe("ProtectedZoneSceneObject", () => {
    const createMockRobot = (alliance: string) => ({
        miraType: MiraType.ROBOT,
        alliance,
    } as unknown as MirabufSceneObject)

    const createMockBodyId = (id: string = "robot-id") => ({
        GetIndexAndSequenceNumber: () => id
    } as unknown as Jolt.BodyID)

    const createProtectedZoneInstance = (prefs: any) => {
        const instance = new ProtectedZoneSceneObject({} as unknown as MirabufSceneObject, 0)
        Reflect.set(instance, "_prefs", {
            activeDuring: [MatchModeType.TELEOP],
            alliance: "red",
            penaltyPoints: 5,
            requireRobotContact: false,
            ...prefs
        })
        return instance
    }

    const setupMockAssociation = (robot: MirabufSceneObject) => {
        mockPhysicsSystem.getBodyAssociation = vi.fn(() => ({ 
            sceneObject: robot 
        }))
    }

    beforeEach(() => {
        vi.clearAllMocks()
    })

    afterEach(() => {
        vi.clearAllMocks()
    })

    test("ZoneCollision applies penalty to opposing robot", () => {
        const instance = createProtectedZoneInstance({})
        const robotBodyId = createMockBodyId()
        const mockRobot = createMockRobot("blue")

        setupMockAssociation(mockRobot)
        instance["zoneCollision"](robotBodyId)

        expect(vi.mocked(SimulationSystem.robotPenalty)).toHaveBeenCalledWith(
            mockRobot,
            5,
            expect.any(String)
        )
    })

    test("ZoneCollision does not penalize same alliance robot", () => {
        const instance = createProtectedZoneInstance({
            activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME]
        })
        const robotBodyId = createMockBodyId()
        const mockRobot = createMockRobot("red")

        setupMockAssociation(mockRobot)
        instance["zoneCollision"](robotBodyId)

        expect(vi.mocked(SimulationSystem.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision does not penalize when zone is inactive", () => {
        const instance = createProtectedZoneInstance({
            activeDuring: [MatchModeType.AUTONOMOUS]
        })
        const robotBodyId = createMockBodyId()
        const mockRobot = createMockRobot("blue")

        setupMockAssociation(mockRobot)
        instance["zoneCollision"](robotBodyId)

        expect(vi.mocked(SimulationSystem.robotPenalty)).not.toHaveBeenCalled()
    })

    test("ZoneCollision does not penalize when requireRobotContact is true and only one robot is in the zone", () => {
        const instance = createProtectedZoneInstance({
            requireRobotContact: true
        })
        const robotBodyId = createMockBodyId()
        const mockRobot = createMockRobot("blue")

        setupMockAssociation(mockRobot)
        instance["zoneCollision"](robotBodyId)

        expect(vi.mocked(SimulationSystem.robotPenalty)).not.toHaveBeenCalled()
    })
})
