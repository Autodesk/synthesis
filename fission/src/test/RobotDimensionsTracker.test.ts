import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import RobotDimensionTracker from "@/systems/match_mode/RobotDimensionTracker"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

interface MockDimensions {
    width: number
    height: number
    depth: number
}

interface MockRobotObject {
    assemblyName: string
    miraType: MiraType
    getDimensions: () => MockDimensions
    getDimensionsWithoutRotation: () => MockDimensions
}

interface MockNonRobotObject {
    assemblyName: string
    miraType: MiraType
    getDimensions: () => MockDimensions
}

interface MockSceneRenderer {
    sceneObjects: Map<string, MockRobotObject | MockNonRobotObject>
}

type TrackerUpdateParam = Parameters<typeof RobotDimensionTracker.update>[0]

const mockMatchModeInstance = {
    isMatchEnabled: vi.fn(() => true),
}

vi.mock("@/systems/match_mode/MatchMode", () => ({
    default: {
        getInstance: vi.fn(() => mockMatchModeInstance),
    },
    MatchModeType: {
        SANDBOX: 0,
        AUTONOMOUS: 1,
        TELEOP: 2,
        MATCH_ENDED: 3,
    },
    DEFAULT_AUTONOMOUS_TIME: 15,
    DEFAULT_TELEOP_TIME: 135,
    DEFAULT_ENDGAME_TIME: 20,
    DEFAULT_IGNORE_ROTATION: true,
    DEFAULT_MAX_HEIGHT: Infinity,
    DEFAULT_HEIGHT_PENALTY: 2,
}))

vi.mock("@/systems/simulation/SimulationSystem", () => ({
    default: {
        robotPenalty: vi.fn(),
    },
}))

describe("RobotDimensionTracker", () => {
    let mockSceneRenderer: MockSceneRenderer
    let mockRobot1: MockRobotObject
    let mockRobot2: MockRobotObject
    let mockNonRobot: MockNonRobotObject

    beforeEach(() => {
        vi.clearAllMocks()

        const tracker = RobotDimensionTracker as unknown as { _robotLastFramePenalty?: Map<number, boolean> }
        tracker._robotLastFramePenalty?.clear()

        const robot1Base = Object.create(MirabufSceneObject.prototype)
        const robot2Base = Object.create(MirabufSceneObject.prototype)

        mockRobot1 = robot1Base as MockRobotObject
        mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 1.8, width: 1.0, depth: 1.0 })

        Object.defineProperty(mockRobot1, "miraType", {
            get: () => MiraType.ROBOT,
        })

        Object.defineProperty(mockRobot1, "id", {
            get: () => 1,
        })

        mockRobot2 = robot2Base as MockRobotObject
        mockRobot2.getDimensions = vi.fn().mockReturnValue({ height: 3.0, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.5, width: 1.0, depth: 1.0 })

        Object.defineProperty(mockRobot2, "miraType", {
            get: () => MiraType.ROBOT,
        })

        Object.defineProperty(mockRobot2, "id", {
            get: () => 2,
        })

        mockNonRobot = {
            assemblyName: "Field",
            miraType: MiraType.FIELD,
            getDimensions: vi.fn(() => ({ height: 5.0, width: 10.0, depth: 10.0 })),
        }

        mockSceneRenderer = {
            sceneObjects: new Map([
                ["robot1", mockRobot1],
                ["robot2", mockRobot2],
                ["field", mockNonRobot],
            ]),
        }
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })

    test("config values determine which dimension method is used", () => {
        RobotDimensionTracker.setConfigValues(false, 2, 15)
        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)

        expect(mockRobot1.getDimensions).toHaveBeenCalled()
        expect(mockRobot1.getDimensionsWithoutRotation).not.toHaveBeenCalled()
    })

    test("should penalize robot if it exceeds max height", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 3.5, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)

        expect(SimulationSystem.robotPenalty).toHaveBeenCalledWith(mockRobot2, 5, expect.any(String))
        expect(SimulationSystem.robotPenalty).not.toHaveBeenCalledWith(
            mockRobot1,
            expect.any(Number),
            expect.any(String)
        )
    })

    test("should not penalize robot every frame", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)
        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)

        expect(SimulationSystem.robotPenalty).toHaveBeenCalledTimes(1)
    })

    test("should penalize multiple robots", () => {
        RobotDimensionTracker.setConfigValues(true, 3.048, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)

        expect(SimulationSystem.robotPenalty).toHaveBeenCalledTimes(2)
    })

    test("should not penalize robot if it is not a robot", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5)

        mockNonRobot.getDimensions = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update(mockSceneRenderer as unknown as TrackerUpdateParam)

        expect(SimulationSystem.robotPenalty).not.toHaveBeenCalled()
    })
})
