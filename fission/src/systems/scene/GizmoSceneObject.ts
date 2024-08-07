import SceneObject from "./SceneObject"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import InputSystem from "../input/InputSystem"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { PerspectiveCamera } from "three"

export type TransformGizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _mesh: THREE.Mesh
    private _parent: MirabufSceneObject | undefined
    private _gizmo: TransformControls

    private _size: number

    public get gizmo() {
        return this._gizmo
    }

    public constructor(
        mesh: THREE.Mesh,
        mode: TransformGizmoMode,
        size: number,
        mainCamera: PerspectiveCamera,
        domElement: HTMLElement,
        parent?: MirabufSceneObject
    ) {
        super()

        this._mesh = mesh
        this._parent = parent

        this._size = size

        this._gizmo = new TransformControls(mainCamera, domElement)
        this._gizmo.setMode(mode)

        World.SceneRenderer.RegisterSceneObject(this)
    }

    public Setup(): void {
        World.SceneRenderer.AddToScene(this._mesh)
        World.SceneRenderer

        this._gizmo.setSpace("local")
        this._gizmo.attach(this._mesh)
        // this._gizmo.addEventListener("dragging-changed", (event: { target: TransformControls; value: unknown }) => {
        //     const gizmoDragging = World.SceneRenderer.IsAnyGizmoDragging()
        //     if (!event.value && !gizmoDragging) {
        //         World.SceneRenderer.orbitControls.enabled = true // enable orbit controls when not dragging another transform gizmo
        //     } else if (!event.value && gizmoDragging) {
        //         World.SceneRenderer.orbitControls.enabled = false // disable orbit controls when dragging another transform gizmo
        //     } else {
        //         World.SceneRenderer.orbitControls.enabled = !event.value // disable orbit controls when dragging transform gizmo
        //     }

        //     if (event.target.mode === "translate") {
        //         Array.from(World.SceneRenderer.sceneObjects.values())
        //             .filter(obj => obj instanceof GizmoSceneObject)
        //             .forEach(obj => {
        //                 if (obj.gizmo.object === event.target.object && obj.gizmo.mode !== "translate") {
        //                     obj.gizmo.dragging = false
        //                     obj.gizmo.enabled = !event.value
        //                     return
        //                 }
        //             })
        //         this._transformControls.forEach((_size, tc) => {
        //             // disable other transform gizmos when translating
        //             if (tc.object === event.target.object && tc.mode !== "translate") {
        //                 tc.dragging = false
        //                 tc.enabled = !event.value
        //                 return
        //             }
        //         })
        //     } else if (
        //         event.target.mode === "scale" &&
        //         (InputSystem.isKeyPressed("ShiftRight") || InputSystem.isKeyPressed("ShiftLeft"))
        //     ) {
        //         // scale uniformly if shift is pressed
        //         event.target.axis = "XYZE"
        //     } else if (event.target.mode === "rotate") {
        //         // scale on all axes
        //         this._transformControls.forEach((_size, tc) => {
        //             // disable scale transform gizmo when scaling
        //             if (tc.mode === "scale" && tc !== event.target && tc.object === event.target.object) {
        //                 tc.dragging = false
        //                 tc.enabled = !event.value
        //                 return
        //             }
        //         })
        //     }
        // })
    }
    public Update(): void {
        if (InputSystem.isKeyPressed("Enter")) {
            // confirming placement of the mirabuf object
            this.Dispose()
            return
        } else if (InputSystem.isKeyPressed("Escape") && this._parent) {
            // cancelling the creation of the mirabuf scene object
            World.SceneRenderer.RemoveSceneObject(this._parent.id)
            return
        }
    }
    public Dispose(): void {
        World.SceneRenderer.RemoveSceneObject(this.id)
    }
}

export default GizmoSceneObject
