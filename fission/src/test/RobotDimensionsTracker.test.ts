import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import RobotDimensionTracker from "@/systems/match_mode/RobotDimensionTracker"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import World from "@/systems/World"

interface MockDimensions {
    width: number
    height: number
    depth: number
}

interface MockRobotObject {
    _id: number | null
    id: number
    assemblyName: string
    miraType: MiraType
    getDimensions: () => MockDimensions
    getDimensionsWithoutRotation: () => MockDimensions
    setup: () => void
    update: () => void
    dispose: () => void
}

interface MockNonRobotObject {
    _id: number | null
    id: number
    assemblyName: string
    miraType: MiraType
    getDimensions: () => MockDimensions
    setup: () => void
    update: () => void
    dispose: () => void
}

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
}))

vi.mock("@/systems/match_mode/ScoreTracker", () => ({
    default: {
        robotPenalty: vi.fn(),
    },
}))

type RecursivePartial<T> = {
    [P in keyof T]?: RecursivePartial<T[P]>
}

vi.mock("@/systems/World", (): { default: RecursivePartial<typeof World> } => ({
    default: {
        getOwnRobots: vi.fn(),
    },
}))

describe("RobotDimensionTracker", () => {
    let mockRobot1: MockRobotObject
    let mockRobot2: MockRobotObject
    let mockNonRobot: MockNonRobotObject

    beforeEach(() => {
        vi.clearAllMocks()

        const tracker = RobotDimensionTracker as unknown as {
            _robotLastFramePenalty?: Map<number, boolean>
            _robotSize?: Map<number, { width: number; depth: number }>
        }
        tracker._robotLastFramePenalty?.clear()
        tracker._robotSize?.clear()

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
            _id: 3,
            id: 3,
            assemblyName: "Field",
            miraType: MiraType.FIELD,
            getDimensions: vi.fn(() => ({ height: 5.0, width: 10.0, depth: 10.0 })),
            setup: vi.fn(),
            update: vi.fn(),
            dispose: vi.fn(),
        }
        ;(World.getOwnRobots as ReturnType<typeof vi.fn>).mockReturnValue([mockRobot1, mockRobot2])
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })

    test("config values determine which dimension method is used", () => {
        RobotDimensionTracker.setConfigValues(false, 2, 15, 1.5, 15)
        RobotDimensionTracker.update()

        expect(mockRobot1.getDimensions).toHaveBeenCalled()
        expect(mockRobot1.getDimensionsWithoutRotation).not.toHaveBeenCalled()
    })

    test("should penalize robot if it exceeds max height", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5, 1.5, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 3.5, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).toHaveBeenCalledWith(mockRobot2, 5, expect.any(String))
        expect(ScoreTracker.robotPenalty).not.toHaveBeenCalledWith(mockRobot1, expect.any(Number), expect.any(String))
    })

    test("should penalize robot if it exceeds side max extension (width)", () => {
        RobotDimensionTracker.setConfigValues(true, 10, 5, 0.5, 2)

        mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })

        mockRobot2.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.matchStart()

        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.7, depth: 1.0 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).toHaveBeenCalledWith(mockRobot2, 2, expect.any(String))
        expect(ScoreTracker.robotPenalty).not.toHaveBeenCalledWith(mockRobot1, expect.any(Number), expect.any(String))
    })

    test("should penalize robot if it exceeds side max extension (depth)", () => {
        RobotDimensionTracker.setConfigValues(true, 10, 5, 0.3, 3)

        mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })
        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })

        mockRobot2.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })

        RobotDimensionTracker.matchStart()

        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.7 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).toHaveBeenCalledWith(mockRobot2, 3, expect.any(String))
        expect(ScoreTracker.robotPenalty).not.toHaveBeenCalledWith(mockRobot1, expect.any(Number), expect.any(String))
    })

    test("should not penalize a robot for side extension if initial dimensions were not recorded", () => {
        RobotDimensionTracker.setConfigValues(false, 10, 5, 0.3, 3)

        mockRobot1.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })
        mockRobot2.getDimensions = vi.fn().mockReturnValue({ height: 2.0, width: 1.0, depth: 1.2 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).not.toHaveBeenCalled()
    })

    test("should not penalize robot every frame", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5, 1.5, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update()
        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).toHaveBeenCalledTimes(1)
    })

    test("should penalize multiple robots", () => {
        RobotDimensionTracker.setConfigValues(true, 3.048, 5, 1.5, 5)

        mockRobot1.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })
        mockRobot2.getDimensionsWithoutRotation = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).toHaveBeenCalledTimes(2)
    })

    test("should not penalize robot if it is not a robot", () => {
        RobotDimensionTracker.setConfigValues(true, 3, 5, 1.5, 5)

        mockNonRobot.getDimensions = vi.fn().mockReturnValue({ height: 12, width: 1.0, depth: 1.0 })

        RobotDimensionTracker.update()

        expect(ScoreTracker.robotPenalty).not.toHaveBeenCalled()
    })
})
