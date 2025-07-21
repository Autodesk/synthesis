import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "../scene/SceneRenderer"
import MatchMode from "./MatchMode"

class RobotDimensionTracker {
    private static readonly IGNORE_ROTATION = true

    public static update(_deltaT: number, sceneRenderer: SceneRenderer): void {
        if (!MatchMode.getInstance().isMatchEnabled()) return

        const robots = [...sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
            const dimensions = this.IGNORE_ROTATION ? robot.getDimensionsWithoutRotation() : robot.getDimensions()

            // TODO add penalty tracking
            if (this.IGNORE_ROTATION) {
                console.log(
                    `Robot ${robot.assemblyName}: ${dimensions.width.toFixed(2)} x ${dimensions.height.toFixed(2)} x ${dimensions.depth.toFixed(2)} (unrotated)`
                )
            } else {
                console.log(
                    `Robot ${robot.assemblyName}: ${dimensions.width.toFixed(2)} x ${dimensions.height.toFixed(2)} x ${dimensions.depth.toFixed(2)} (rotated)`
                )
            }
        })
    }
}

export default RobotDimensionTracker
