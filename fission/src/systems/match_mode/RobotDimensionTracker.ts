import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type SceneRenderer from "../scene/SceneRenderer"
import MatchMode from "./MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

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

    public static update(sceneRenderer: SceneRenderer): void {
        if (!MatchMode.getInstance().isMatchEnabled()) return

        const robots = [...sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
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
