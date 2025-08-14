import * as THREE from "three"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
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
            const rootTransform = convertJoltMat44ToThreeMatrix4(rootBody.GetWorldTransform())

            const rootPosition = new THREE.Vector3()
            const rootRotation = new THREE.Quaternion()
            const rootScale = new THREE.Vector3()
            rootTransform.decompose(rootPosition, rootRotation, rootScale)

            if (robot.hasPhysics() && rootPosition.y < this._mapBoundaryY) {
                globalAddToast("warning", "Robot fell off the map", `${robot.nameTag?.text()} - ${robot.assemblyName}`)

                robot.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
                    const bodyId = robot.mechanism.getBodyByNodeId(rigidNode.id)
                    if (bodyId) {
                        robot.moveToSpawnLocation()
                    }
                })
            }
        })
    }
}

export default RobotPositionTracker
