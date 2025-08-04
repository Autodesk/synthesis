import * as THREE from "three"
import World from "@/systems/World"
import SceneObject from "@/systems/scene/SceneObject"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

/**
 * Visual representation of the WPILib camera in the 3D scene
 * Shows where the camera is mounted and its field of view
 */
export class SimCameraVisualization extends SceneObject {
    private _robot: MirabufSceneObject
    private _cameraGroup: THREE.Group
    private _cameraPosition: THREE.Vector3
    private _isVisible: boolean = false

    constructor(robot: MirabufSceneObject) {
        super()
        this._robot = robot

        this._cameraPosition = new THREE.Vector3(0, 0.5, 0.2)

        this._cameraGroup = new THREE.Group()
    }

    public setup(): void {
        World.sceneRenderer.addObject(this._cameraGroup)
        console.log("[VISUAL] SimCameraVisualization added to scene")
    }

    public update(): void {
        if (!this._isVisible) return

        this.updateCameraTransform()
    }

    private updateCameraTransform() {
        if (!this._robot.mechanism.rootBody) return

        const robotBody = World.physicsSystem.getBody(
            this._robot.mechanism.nodeToBody.get(this._robot.mechanism.rootBody)!
        )

        if (!robotBody) return

        const robotPos = robotBody.GetPosition()
        const robotRot = robotBody.GetRotation()

        const robotPosition = new THREE.Vector3(robotPos.GetX(), robotPos.GetY(), robotPos.GetZ())
        const robotQuaternion = new THREE.Quaternion(robotRot.GetX(), robotRot.GetY(), robotRot.GetZ(), robotRot.GetW())

        const worldCameraPos = this._cameraPosition.clone()
        worldCameraPos.applyQuaternion(robotQuaternion)
        worldCameraPos.add(robotPosition)

        const cameraRotation = new THREE.Quaternion()
        cameraRotation.copy(robotQuaternion)
        const forwardFix = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 1, 0), Math.PI)
        const upFix = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 0, 1), Math.PI)
        cameraRotation.multiply(forwardFix).multiply(upFix)

        this._cameraGroup.position.copy(worldCameraPos)
        this._cameraGroup.quaternion.copy(cameraRotation)
    }

    public setVisible(visible: boolean) {
        this._isVisible = visible
        this._cameraGroup.visible = visible

        // if (visible) {
        //     console.log("📹 [VISUAL] Camera visualization enabled - you should see a camera model on your robot")
        // } else {
        //     console.log("📹 [VISUAL] Camera visualization disabled")
        // }
    }

    public dispose(): void {
        if (this._cameraGroup.parent) {
            this._cameraGroup.parent.remove(this._cameraGroup)
        }

        this._cameraGroup.traverse(child => {
            if (child instanceof THREE.Mesh) {
                child.geometry.dispose()
                if (Array.isArray(child.material)) {
                    child.material.forEach(material => material.dispose())
                } else {
                    child.material.dispose()
                }
            }
        })

        console.log("[VISUAL] SimCameraVisualization disposed")
    }
}
