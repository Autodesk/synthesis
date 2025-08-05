import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode from "./MatchMode"

const BUFFER_HEIGHT = 0.1

class RobotDimensionTracker {
    private static _robotLastFramePenalty: Map<number, boolean> = new Map()
    private static _ignoreRotation: boolean = true
    private static _maxHeight: number = Infinity
    private static _heightPenalty: number = 0

    public static setConfigValues(ignoreRotation: boolean, maxHeight: number, heightPenalty: number) {
        this._ignoreRotation = ignoreRotation
        this._maxHeight = maxHeight
        this._heightPenalty = heightPenalty
    }

    public static update(): void {
        if (!MatchMode.getInstance().isMatchEnabled()) return

        MirabufSceneObject.getRobots().forEach(robot => {
            const dimensions = this._ignoreRotation ? robot.getDimensionsWithoutRotation() : robot.getDimensions()

            if (dimensions.height > this._maxHeight + BUFFER_HEIGHT) {
                if (!(this._robotLastFramePenalty.get(robot.id) ?? false)) {
                    SimulationSystem.robotPenalty(robot, this._heightPenalty, "Height Expansion Limit")
                }
                this._robotLastFramePenalty.set(robot.id, true)
            } else {
                this._robotLastFramePenalty.set(robot.id, false)
            }
        })
    }
}

export default RobotDimensionTracker
