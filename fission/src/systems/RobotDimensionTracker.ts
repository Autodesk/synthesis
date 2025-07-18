import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import SceneRenderer from "./scene/SceneRenderer"
import * as THREE from "three"
import World from "./World"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"

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

        const inverseRotation = new THREE.Matrix4().makeRotationFromQuaternion(rootRotation.clone().invert())

        const unrotatedBox = new THREE.Box3()

        const assembly = robot.mirabufInstance.parser.assembly
        const partInstances = assembly.data?.parts?.partInstances
        const partDefinitions = assembly.data?.parts?.partDefinitions

        if (!partInstances || !partDefinitions) {
            console.warn("Could not access assembly part data, using fallback method")
            return robot.getDimensions()
        }

        robot.mirabufInstance.parser.rigidNodes.forEach(rigidNode => {
            const bodyId = robot.mechanism.getBodyByNodeId(rigidNode.id)
            if (!bodyId) return

            const body = World.physicsSystem.getBody(bodyId)
            const bodyTransform = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())

            rigidNode.parts.forEach(partId => {
                const partDefinition = partDefinitions[partInstances[partId].partDefinitionReference!]
                if (!partDefinition.bodies) return
                const partGlobalTransform = robot.mirabufInstance.parser.globalTransforms.get(partId)
                if (!partGlobalTransform) return

                const relativeTransform = partGlobalTransform.clone()
                const currentPartTransform = relativeTransform.premultiply(bodyTransform)

                partDefinition.bodies.forEach(body => {
                    const mesh = body.triangleMesh?.mesh
                    if (!mesh?.verts || mesh.verts.length === 0) return

                    for (let i = 0; i < mesh.verts.length; i += 3) {
                        const vertex = new THREE.Vector3(
                            mesh.verts[i] / 100.0,
                            mesh.verts[i + 1] / 100.0,
                            mesh.verts[i + 2] / 100.0
                        )

                        vertex.applyMatrix4(currentPartTransform)

                        vertex.sub(rootPosition).applyMatrix4(inverseRotation).add(rootPosition)

                        unrotatedBox.expandByPoint(vertex)
                    }
                })
            })
        })

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
