import * as THREE from "three"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import World from "../World"

class RobotPositionTracker {
    private static _mapBoundaryY: number = -4
    private static _offMapPenalty: number = 0

    public static update(): void {
        World.sceneRenderer.mirabufSceneObjects.getRobots().forEach(robot => {
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

            if (rootPosition.y < this._mapBoundaryY) {
                SimulationSystem.robotPenalty(robot, this._offMapPenalty, "Robot fell off the map")
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
