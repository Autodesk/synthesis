import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "../scene/SceneRenderer"
import MatchMode from "./MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import { convertFeetToMeters } from "@/util/UnitConversions"

const PENALTY_COOLDOWN = 1000

class RobotDimensionTracker {
    private static _robotHeightPenalties: Map<number, number> = new Map()
    private static _ignoreRotation: boolean = true
    private static _maxHeight: number = Infinity
    private static _heightPenalty: number = 0

    public static setConfigValues(ignoreRotation: boolean, maxHeight: number, heightPenalty: number) {
        this._ignoreRotation = ignoreRotation
        this._maxHeight = convertFeetToMeters(maxHeight)
        this._heightPenalty = heightPenalty
    }

    public static update(sceneRenderer: SceneRenderer): void {
        if (!MatchMode.getInstance().isMatchEnabled()) return

        const robots = [...sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
            const dimensions = this._ignoreRotation ? robot.getDimensionsWithoutRotation() : robot.getDimensions()

            if (dimensions.height > this._maxHeight) {
                if ((this._robotHeightPenalties.get(robot.id) ?? 0) < Date.now() - PENALTY_COOLDOWN) {
                    this._robotHeightPenalties.set(robot.id, Date.now() + this._heightPenalty)
                    SimulationSystem.robotPenalty(robot, this._heightPenalty, "Height Expansion Limit")
                }
            }
        })
    }
}

export default RobotDimensionTracker
