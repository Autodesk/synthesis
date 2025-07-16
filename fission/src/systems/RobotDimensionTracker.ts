import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "./scene/SceneRenderer"

class RobotDimensionTracker {
    // TODO add a way to track the dimensions of a robot without rotation

    public static update(deltaT: number, sceneRenderer: SceneRenderer): void {
        const robots = [...sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
            const dimensions = robot.getDimensions()

            // TODO add penalty tracking
            console.log(
                `Robot ${robot.assemblyName}: ${dimensions.width.toFixed(2)} x ${dimensions.height.toFixed(2)} x ${dimensions.depth.toFixed(2)}${this.IGNORE_ROTATION ? " (canonical)" : ""}`
            )
        })
    }
}

export default RobotDimensionTracker
