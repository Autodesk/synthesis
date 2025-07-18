import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "../scene/SceneRenderer"
import * as THREE from "three"
import World from "../World"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import JOLT from "@/util/loading/JoltSyncLoader"

class RobotDimensionTracker {
    private static readonly IGNORE_ROTATION = true

    public static update(_deltaT: number, sceneRenderer: SceneRenderer): void {
        const robots = [...sceneRenderer.sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => obj instanceof MirabufSceneObject && obj.miraType === MiraType.ROBOT
        )

        robots.forEach(robot => {
            const dimensions = this.IGNORE_ROTATION ? this.getUnrotatedDimensions(robot) : robot.getDimensions()

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

    /**
     * Calculates the robot's dimensions as if it had no rotation applied.
     * This removes the robot's root rotation to get the true size constraints.
     */
    private static getUnrotatedDimensions(robot: MirabufSceneObject): { width: number; height: number; depth: number } {
        const rootNodeId = robot.getRootNodeId()
        if (!rootNodeId) {
            console.warn("No root node found for robot, using regular dimensions")
            return robot.getDimensions()
        }

        const rootBody = World.physicsSystem.getBody(rootNodeId)
        const rootTransform = convertJoltMat44ToThreeMatrix4(rootBody.GetWorldTransform())

        const rootPosition = new THREE.Vector3()
        const rootRotation = new THREE.Quaternion()
        const rootScale = new THREE.Vector3()
        rootTransform.decompose(rootPosition, rootRotation, rootScale)

        // Create inverse rotation matrix to "undo" the robot's rotation
        const inverseRotation = new THREE.Matrix4().makeRotationFromQuaternion(rootRotation.clone().invert())

        const unrotatedBox = new THREE.Box3()

        robot.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
            const bodyId = robot.mechanism.getBodyByNodeId(rigidNode.id)
            if (!bodyId) return

            const body = World.physicsSystem.getBody(bodyId)
            const bodyTransform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())

            const shape = body.GetShape()
            const scale = new JOLT.Vec3(1, 1, 1)
            const triangleContext = new JOLT.ShapeGetTriangles(
                shape,
                JOLT.AABox.prototype.sBiggest(),
                shape.GetCenterOfMass(),
                JOLT.Quat.prototype.sIdentity(),
                scale
            )

            try {
                const vertices = new Float32Array(
                    JOLT.HEAP32.buffer,
                    triangleContext.GetVerticesData(),
                    triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT
                )

                for (let i = 0; i < vertices.length; i += 3) {
                    const vertex = new THREE.Vector3(vertices[i], vertices[i + 1], vertices[i + 2])

                    vertex.applyMatrix4(bodyTransform).applyMatrix4(inverseRotation)

                    unrotatedBox.expandByPoint(vertex)
                }
            } finally {
                JOLT.destroy(triangleContext)
                JOLT.destroy(scale)
            }
        })

        // Fallback if no vertices were processed
        if (unrotatedBox.isEmpty()) {
            console.warn("Could not process physics shapes, using regular dimensions")
            return robot.getDimensions()
        }

        const unrotatedSize = new THREE.Vector3()
        unrotatedBox.getSize(unrotatedSize)

        return {
            width: unrotatedSize.x,
            height: unrotatedSize.y,
            depth: unrotatedSize.z,
        }
    }
}

export default RobotDimensionTracker
