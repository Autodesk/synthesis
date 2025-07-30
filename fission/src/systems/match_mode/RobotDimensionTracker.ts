import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "../scene/SceneRenderer"
import MatchMode from "./MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import World from "@/systems/World"

const BUFFER_HEIGHT = 0.1
const SIDE_BUFFER = 0.1

class RobotDimensionTracker {
    private static _robotLastFramePenalty: Map<number, boolean> = new Map()
    private static _ignoreRotation: boolean = true
    private static _maxHeight: number = Infinity
    private static _heightLimitPenalty: number = 0
    private static _sideExtensionPenalty: number = 0
    private static _robotSize: Map<number, { width: number; depth: number }> = new Map()
    private static _sideMaxExtension: number = 0

    public static setConfigValues(
        ignoreRotation: boolean,
        maxHeight: number,
        heightLimitPenalty: number,
        sideExtensionPenalty: number,
        sideMaxExtension: number
    ) {
        this._ignoreRotation = ignoreRotation
        this._maxHeight = maxHeight
        this._heightLimitPenalty = heightLimitPenalty
        this._sideExtensionPenalty = sideExtensionPenalty
        this._sideMaxExtension = sideMaxExtension
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
                    SimulationSystem.robotPenalty(robot, this._heightLimitPenalty, "Height Expansion Limit")
                }
                this._robotLastFramePenalty.set(robot.id, true)
                return
            }

            const startingRobotSize = this._robotSize.get(robot.id) ?? { width: Infinity, depth: Infinity }
            if (
                dimensions.width > startingRobotSize.width + this._sideMaxExtension + SIDE_BUFFER ||
                dimensions.depth > startingRobotSize.depth + this._sideMaxExtension + SIDE_BUFFER
            ) {
                if (!(this._robotLastFramePenalty.get(robot.id) ?? false)) {
                    SimulationSystem.robotPenalty(robot, this._sideExtensionPenalty, "Side Expansion Limit")
                }
                this._robotLastFramePenalty.set(robot.id, true)
                return
            }

            this._robotLastFramePenalty.set(robot.id, false)
        })
    }

    public static matchStart(): void {
        this._robotSize.clear()
        this._robotLastFramePenalty.clear()

        const robots = [...World.sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
            this._robotSize.set(robot.id, robot.getDimensions())
        })
    }
}

export default RobotDimensionTracker
