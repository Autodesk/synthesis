import * as THREE from "three"
import SceneObject from "./SceneObject"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import InputSystem from "../input/InputSystem"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Object3D, PerspectiveCamera } from "three"
import { ThreeMatrix4_JoltMat44, ThreeQuaternion_JoltQuat } from "@/util/TypeConversions"
import { RigidNodeReadOnly } from "@/mirabuf/MirabufParser"

export type GizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _gizmo: TransformControls
    private _obj: Object3D
    private _parentObject: MirabufSceneObject | undefined

    private _mainCamera: PerspectiveCamera

    private _size: number

    /** @returns the instance of the transform gizmo itself */
    public get gizmo() {
        return this._gizmo
    }

    /** @returns Object3D that is attached to transform gizmo */
    public get obj() {
        return this._obj
    }

    /** @returns true if gizmo is currently being dragged */
    public get isDragging() {
        return this._gizmo.dragging
    }

    /** @returns the id of the parent scene object */
    public get parentObjectId() {
        return this._parentObject?.id
    }

    public constructor(
        obj: THREE.Mesh = new THREE.Mesh(
            new THREE.SphereGeometry(0.2),
            new THREE.MeshBasicMaterial({ color: 0x00ff00 })
        ),
        mode: GizmoMode,
        size: number,
        parentObject?: MirabufSceneObject
    ) {
        super()

        this._obj = obj
        this._parentObject = parentObject
        this._mainCamera = World.SceneRenderer.mainCamera

        this._size = size

        this._gizmo = new TransformControls(World.SceneRenderer.mainCamera, World.SceneRenderer.renderer.domElement)
        this._gizmo.setMode(mode)

        World.SceneRenderer.RegisterGizmoSceneObject(this)
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
        this._gizmo.updateMatrixWorld()

        // updating the size of the gizmo based on the distance from the camera
        const mainCameraFovRadians = (Math.PI * (this._mainCamera.fov * 0.5)) / 180
        this._gizmo.setSize(
            (this._size / this._mainCamera.position.distanceTo(this.gizmo.object!.position)) *
                Math.tan(mainCameraFovRadians) *
                1.9
        )

        /** Translating the obj changes to the mirabuf scene object */
        if (this._parentObject) {
            this._parentObject.DisablePhysics()
            if (this.isDragging) {
                this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                    this.UpdateBodyPositionAndRotation(rn)
                    this._parentObject?.UpdateNodeParts(rn, this.obj.matrix)
                })
            }
        }

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
        this._parentObject?.EnablePhysics()
        World.SceneRenderer.RemoveObject(this._obj)
        World.SceneRenderer.RemoveObject(this._gizmo)
    }

    /** changes the mode of the gizmo */
    public SetMode(mode: GizmoMode) {
        this._gizmo.setMode(mode)
    }

    /** updates body position and rotation for each body from the parent mirabuf */
    public UpdateBodyPositionAndRotation(rn: RigidNodeReadOnly) {
        if (!this._parentObject) return
        World.PhysicsSystem.SetBodyPosition(
            this._parentObject.mechanism.GetBodyByNodeId(rn.id)!,
            ThreeMatrix4_JoltMat44(this._obj.matrix).GetTranslation()
        )
        World.PhysicsSystem.SetBodyRotation(
            this._parentObject.mechanism.GetBodyByNodeId(rn.id)!,
            ThreeQuaternion_JoltQuat(this._obj.quaternion)
        )
    }

    /**  */
    public UpdateGizmoObjectPositionAndRotation(gizmoTransformation: THREE.Matrix4) {
        this._obj.position.setFromMatrixPosition(gizmoTransformation)
        this._obj.quaternion.setFromRotationMatrix(gizmoTransformation)
    }

    /** @return true if gizmo is attached to mirabufSceneObject */
    public HasParent(): boolean {
        return this._parentObject !== undefined
    }
}

export default GizmoSceneObject
