import SceneObject from "./SceneObject"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import InputSystem from "../input/InputSystem"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { PerspectiveCamera } from "three"
import { ThreeMatrix4_JoltMat44, ThreeQuaternion_JoltQuat } from "@/util/TypeConversions"

export type TransformGizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _mesh: THREE.Mesh
    private _parentObject: MirabufSceneObject | undefined
    private _gizmo: TransformControls

    private _mainCamera: PerspectiveCamera

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
        parentObject?: MirabufSceneObject
    ) {
        super()

        this._mesh = mesh
        this._parentObject = parentObject
        this._mainCamera = mainCamera

        this._size = size

        this._gizmo = new TransformControls(mainCamera, domElement)
        this._gizmo.setMode(mode)

        World.SceneRenderer.RegisterSceneObject(this)
    }

    public Setup(): void {
        World.SceneRenderer.AddToScene(this._mesh)
        World.SceneRenderer.AddToScene(this._gizmo)

        this._gizmo.setSpace("local")
        this._gizmo.attach(this._mesh)
        this._gizmo.addEventListener("dragging-changed", (event: { target: TransformControls; value: unknown }) => {
            const gizmoDragging = World.SceneRenderer.IsAnyGizmoDragging()
            if (!event.value && !gizmoDragging) {
                World.SceneRenderer.orbitControls.enabled = true // enable orbit controls when not dragging another transform gizmo
            } else if (!event.value && gizmoDragging) {
                World.SceneRenderer.orbitControls.enabled = false // disable orbit controls when dragging another transform gizmo
            } else {
                World.SceneRenderer.orbitControls.enabled = !event.value // disable orbit controls when dragging transform gizmo
            }

            if (event.target.mode === "translate") {
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

        if (this._parentObject !== undefined) this._parentObject.DisablePhysics()
    }
    public Update(): void {
        const mainCameraFovRadians = (Math.PI * (this._mainCamera.fov * 0.5)) / 180
        this._gizmo.setSize(
            (this._size / this._mainCamera.position.distanceTo(this.gizmo.object!.position)) *
                Math.tan(mainCameraFovRadians) *
                1.9
        )

        // updating the position of the mirabuf object
        if (this._parentObject !== undefined) {
            this._parentObject.DisablePhysics()

            if (this._gizmo.dragging) {
                this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                    World.PhysicsSystem.SetBodyPosition(
                        this._parentObject!.mechanism.GetBodyByNodeId(rn.id)!,
                        ThreeMatrix4_JoltMat44(this._mesh.matrix).GetTranslation()
                    )
                    World.PhysicsSystem.SetBodyRotation(
                        this._parentObject!.mechanism.GetBodyByNodeId(rn.id)!,
                        ThreeQuaternion_JoltQuat(this._mesh.quaternion)
                    )

                    rn.parts.forEach(part => {
                        const partTransform = this._parentObject!.mirabufInstance.parser.globalTransforms.get(part)!
                            .clone()
                            .premultiply(this._mesh.matrix)

                        const meshes = this._parentObject!.mirabufInstance.meshes.get(part) ?? []
                        meshes.forEach(([batch, id]) => batch.setMatrixAt(id, partTransform))
                    })
                })
            }
        }

        if (InputSystem.isKeyPressed("Enter")) {
            // confirming placement of the mirabuf object
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
        World.SceneRenderer.RemoveObject(this._mesh)
        World.SceneRenderer.RemoveObject(this._gizmo)
    }
}

export default GizmoSceneObject
