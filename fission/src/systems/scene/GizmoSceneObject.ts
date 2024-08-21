import SceneObject from "./SceneObject"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import InputSystem from "../input/InputSystem"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Object3D, PerspectiveCamera } from "three"

export type GizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _gizmo: TransformControls
    private _obj: Object3D
    private _parentObject: MirabufSceneObject | undefined

    private _mainCamera: PerspectiveCamera

    private _size: number

    public get gizmo() {
        return this._gizmo
    }

    public get obj() {
        return this._obj
    }

    public get isDragging() {
        return this._gizmo.dragging
    }

    public constructor(mesh: THREE.Mesh, mode: GizmoMode, size: number, parentObject?: MirabufSceneObject) {
        super()

        this._obj = mesh
        this._parentObject = parentObject
        this._mainCamera = World.SceneRenderer.mainCamera

        this._size = size

        this._gizmo = new TransformControls(World.SceneRenderer.mainCamera, World.SceneRenderer.renderer.domElement)
        this._gizmo.setMode(mode)

        World.SceneRenderer.RegisterSceneObject(this)
    }

    public Setup(): void {
        // adding the mesh and gizmo to the scene
        World.SceneRenderer.AddObject(this._obj)
        World.SceneRenderer.AddObject(this._gizmo)

        // forcing the gizmo to rotate and transform with the object
        this._gizmo.setSpace("local")
        this._gizmo.attach(this._obj)

        this._gizmo.addEventListener("dragging-changed", (event: { target: TransformControls; value: unknown }) => {
            // disable orbit controls when dragging the transform gizmo
            const gizmoDragging = World.SceneRenderer.IsAnyGizmoDragging()
            if (!event.value && !gizmoDragging) {
                World.SceneRenderer.orbitControls.enabled = true // enable orbit controls when not dragging another transform gizmo
            } else if (!event.value && gizmoDragging) {
                World.SceneRenderer.orbitControls.enabled = false // disable orbit controls when dragging another transform gizmo
            } else {
                World.SceneRenderer.orbitControls.enabled = !event.value // disable orbit controls when dragging transform gizmo
            }

            if (event.target.mode === "translate") {
                // disable other gizmos when translating
                Array.from(World.SceneRenderer.sceneObjects.values())
                    .filter(obj => obj instanceof GizmoSceneObject)
                    .forEach(obj => {
                        if (obj.gizmo.object === event.target.object && obj.gizmo.mode !== "translate") {
                            obj.gizmo.dragging = false
                            obj.gizmo.enabled = !event.value
                            return
                        }
                    })
            } else if (
                event.target.mode === "scale" &&
                (InputSystem.isKeyPressed("ShiftRight") || InputSystem.isKeyPressed("ShiftLeft"))
            ) {
                // scale uniformly if shift is pressed
                event.target.axis = "XYZE"
            } else if (event.target.mode === "rotate") {
                // scale on all axes
                Array.from(World.SceneRenderer.sceneObjects.values())
                    .filter(obj => obj instanceof GizmoSceneObject)
                    .forEach(obj => {
                        if (
                            obj.gizmo.mode === "scale" &&
                            event.target !== obj.gizmo &&
                            obj.gizmo.object === event.target.object
                        ) {
                            obj.gizmo.dragging = false
                            obj.gizmo.enabled = !event.value
                            return
                        }
                    })
            }
        })
    }

    public Update(): void {
        // updating the size of the gizmo based on the distance from the camera
        const mainCameraFovRadians = (Math.PI * (this._mainCamera.fov * 0.5)) / 180
        this._gizmo.setSize(
            (this._size / this._mainCamera.position.distanceTo(this.gizmo.object!.position)) *
                Math.tan(mainCameraFovRadians) *
                1.9
        )

        // creating enter key and escape key event listeners
        if (InputSystem.isKeyPressed("Enter") && this._parentObject) {
            // confirming placement of object
            if (this._parentObject !== undefined) this._parentObject.EnablePhysics()
            World.SceneRenderer.RemoveSceneObject(this.id)
            return
        } else if (InputSystem.isKeyPressed("Escape") && this._parentObject) {
            // cancelling the creation of the mirabuf scene object
            World.SceneRenderer.RemoveSceneObject(this._parentObject.id)
            World.SceneRenderer.RemoveSceneObject(this.id)
            return
        }
    }

    public Dispose(): void {
        this._gizmo.detach()
        if (this._parentObject) this._parentObject.RemoveGizmo()
        World.SceneRenderer.RemoveObject(this._obj)
        World.SceneRenderer.RemoveObject(this._gizmo)
    }

    /** changes the mode of the gizmo */
    public SetMode(mode: GizmoMode) {
        this._gizmo.setMode(mode)
    }
}

export default GizmoSceneObject
