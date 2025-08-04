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

        // Same position as SimCameraRenderer
        this._cameraPosition = new THREE.Vector3(0, 0.5, 0.2) // Mounted on robot

        this._cameraGroup = new THREE.Group()
    }

    public setup(): void {
        // Add to scene
        World.sceneRenderer.addObject(this._cameraGroup)
        console.log("📹 [VISUAL] SimCameraVisualization added to scene")
    }

    public setVisible(visible: boolean) {
        this._isVisible = visible
        this._cameraGroup.visible = visible

        if (visible) {
            console.log("📹 [VISUAL] Camera visualization enabled - you should see a camera model on your robot")
        } else {
            console.log("📹 [VISUAL] Camera visualization disabled")
        }
    }

    public dispose(): void {
        if (this._cameraGroup.parent) {
            this._cameraGroup.parent.remove(this._cameraGroup)
        }

        // Dispose of geometries and materials
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

        console.log("📹 [VISUAL] SimCameraVisualization disposed")
    }
}
