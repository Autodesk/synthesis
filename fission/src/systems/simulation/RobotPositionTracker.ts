import * as THREE from "three"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import World from "../World"
import {globalAddToast} from "@/components/GlobalUIControls.ts";

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

                // TODO: Once driver station is implemented, we should reset the robot to the driver station position
                const resetPosition = new JOLT.RVec3(0, 0.2, 0)
                const resetRotation = JOLT.Quat.prototype.sIdentity()
                const zeroVelocity = new JOLT.Vec3(0, 0, 0)

                robot.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
                    const bodyId = robot.mechanism.getBodyByNodeId(rigidNode.id)
                    if (bodyId) {
                        World.physicsSystem.setBodyPositionRotationAndVelocity(
                            bodyId,
                            resetPosition,
                            resetRotation,
                            zeroVelocity,
                            zeroVelocity,
                            true
                        )
                    }
                })

                JOLT.destroy(resetPosition)
                JOLT.destroy(resetRotation)
                JOLT.destroy(zeroVelocity)
            }
        })
    }
}

export default RobotPositionTracker
