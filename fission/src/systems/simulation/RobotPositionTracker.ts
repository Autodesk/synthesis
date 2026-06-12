import { globalAddToast } from "@/components/GlobalUIControls.ts"
import World from "../World"

class RobotPositionTracker {
    private static _mapBoundaryY: number = -4

    public static update(): void {
        World.getOwnRobots().forEach(robot => {
            const rootNodeId = robot.getRootNodeId()
            if (!rootNodeId) {
                return
            }

            const rootBody = World.physicsSystem.getBody(rootNodeId)
            const y = rootBody.GetPosition().GetY()

            if (robot.hasPhysics() && y < this._mapBoundaryY) {
                globalAddToast("warning", "Robot fell off the map", `${robot.nameTag?.text()} - ${robot.assemblyName}`)
                robot.moveToSpawnLocation()
            }
        })
    }
}

export default RobotPositionTracker
