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

type TrackerUpdateParam = Parameters<typeof RobotDimensionTracker.update>[1]

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
    DEFAULT_MAX_HEIGHT: 0,
    DEFAULT_HEIGHT_PENALTY: 2,
}))

vi.mock("@/systems/simulation/SimulationSystem", () => ({
    default: {
        robotPenalty: vi.fn(),
    },
}))

vi.mock("@/util/UnitConversions", () => ({
    convertFeetToMeters: vi.fn((feet: number) => feet * 0.3048),
}))

describe("RobotDimensionTracker", () => {
    let mockSceneRenderer: MockSceneRenderer
    let mockRobot1: MockRobotObject
    let mockRobot2: MockRobotObject
    let mockNonRobot: MockNonRobotObject

    beforeEach(() => {
        vi.clearAllMocks()

        mockMatchModeInstance.isMatchEnabled.mockReturnValue(true)

        const tracker = RobotDimensionTracker as unknown as { _robotHeightPenalties?: Map<string, number> }
        tracker._robotHeightPenalties?.clear?.()

        RobotDimensionTracker.setConfigValues(true, 1000, 0) // Very high limit to reset

        const robot1Base = Object.create(MirabufSceneObject.prototype)
        const robot2Base = Object.create(MirabufSceneObject.prototype)

        mockRobot1 = robot1Base as MockRobotObject
        mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 1.8, width: 1.0, depth: 1.0 })

        Object.defineProperty(mockRobot1, "assemblyName", {
            get: () => "Robot1",
            configurable: true,
        })
        Object.defineProperty(mockRobot1, "miraType", {
            get: () => MiraType.ROBOT,
            configurable: true,
        })

        mockRobot2 = robot2Base as MockRobotObject
        mockRobot2.getDimensions = vi.fn().mockReturnValue({ height: 3.0, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.5, width: 1.0, depth: 1.0 })

        Object.defineProperty(mockRobot2, "assemblyName", {
            get: () => "Robot2",
            configurable: true,
        })
        Object.defineProperty(mockRobot2, "miraType", {
            get: () => MiraType.ROBOT,
            configurable: true,
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

        RobotDimensionTracker.setConfigValues(true, Infinity, 0)
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })

    describe("setConfigValues", () => {
        test("should set configuration values correctly", () => {
            RobotDimensionTracker.setConfigValues(false, 6.0, 15)

            mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })

            RobotDimensionTracker.update(16, mockSceneRenderer as unknown as TrackerUpdateParam)

            expect(mockRobot1.getDimensions).toHaveBeenCalled()
            expect(mockRobot1.getDimensionsWithoutRotation).not.toHaveBeenCalled()
        })

        test("should convert feet to meters for max height", () => {
            RobotDimensionTracker.setConfigValues(true, 10.0, 5)

            mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
            mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 3.5, width: 1.0, depth: 1.0 })

            RobotDimensionTracker.update(16, mockSceneRenderer as unknown as TrackerUpdateParam)

            expect(SimulationSystem.robotPenalty).toHaveBeenCalledWith(mockRobot2, 5, "Height Expansion Limit")
            expect(SimulationSystem.robotPenalty).not.toHaveBeenCalledWith(
                mockRobot1,
                expect.any(Number),
                expect.any(String)
            )
        })
    })
})
