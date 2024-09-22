import * as THREE from "three"
import SceneObject from "./SceneObject"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import InputSystem from "../input/InputSystem"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Object3D, PerspectiveCamera } from "three"
import { ThreeQuaternion_JoltQuat, JoltMat44_ThreeMatrix4, ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import { RigidNodeId } from "@/mirabuf/MirabufParser"

export type GizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _gizmo: TransformControls
    private _obj: Object3D
    private _forceUpdate: boolean = false

    private _parentObject: MirabufSceneObject | undefined
    private _relativeTransformations?: Map<RigidNodeId, THREE.Matrix4>

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
        mode: GizmoMode,
        size: number,
        obj?: THREE.Mesh,
        parentObject?: MirabufSceneObject,
        postGizmoCreation?: (gizmo: GizmoSceneObject) => void,
    ) {
        super()

        this._obj = obj ?? new THREE.Mesh()
        this._parentObject = parentObject
        this._mainCamera = World.SceneRenderer.mainCamera

        this._size = size

        this._gizmo = new TransformControls(World.SceneRenderer.mainCamera, World.SceneRenderer.renderer.domElement)
        this._gizmo.setMode(mode)

        World.SceneRenderer.RegisterGizmoSceneObject(this)

        postGizmoCreation?.(this)
        
        if (this._parentObject) {
            this._relativeTransformations = new Map<RigidNodeId, THREE.Matrix4>()
            const gizmoTransformInv = this._obj.matrix.clone().invert()

            /** Due to the limited math functionality exposed to JS for Jolt, we need everything in ThreeJS. */
            this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                const jBodyId = this._parentObject!.mechanism.GetBodyByNodeId(rn.id)
                if (!jBodyId) return
    
                const worldTransform = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(jBodyId).GetWorldTransform())
                const relativeTransform = worldTransform.premultiply(gizmoTransformInv)
                this._relativeTransformations!.set(rn.id, relativeTransform)
            })
        }
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
            if (this.isDragging || this._forceUpdate) {
                this._forceUpdate = false
                this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                    this.UpdateNodeTransform(rn.id)
                })
                this._parentObject.UpdateMeshTransforms()
            }
        }
    }

    public Dispose(): void {
        this._gizmo.detach()
        this._parentObject?.EnablePhysics()
        World.SceneRenderer.RemoveObject(this._obj)
        World.SceneRenderer.RemoveObject(this._gizmo)

        this._relativeTransformations?.clear()
    }

    /** changes the mode of the gizmo */
    public SetMode(mode: GizmoMode) {
        this._gizmo.setMode(mode)
    }

    /**
     * Updates a given node to follow the gizmo.
     * 
     * @param rnId Target node to update.
     */
    public UpdateNodeTransform(rnId: RigidNodeId) {
        if (!this._parentObject || !this._relativeTransformations || !this._relativeTransformations.has(rnId)) return

        const jBodyId = this._parentObject.mechanism.GetBodyByNodeId(rnId)
        if (!jBodyId) return

        const relativeTransform = this._relativeTransformations.get(rnId)!
        const worldTransform = relativeTransform.clone().premultiply(this._obj.matrix)
        const position = new THREE.Vector3(0,0,0)
        const rotation = new THREE.Quaternion(0,0,0,1)
        worldTransform.decompose(position, rotation, new THREE.Vector3(1,1,1))

        World.PhysicsSystem.SetBodyPositionAndRotation(
            jBodyId,
            ThreeVector3_JoltVec3(position),
            ThreeQuaternion_JoltQuat(rotation),
        )
    }

    /**
     * Updates the gizmos location.
     * 
     * @param gizmoTransformation Transform for the gizmo to take on.
     */
    public SetTransform(gizmoTransformation: THREE.Matrix4) {
        // Super hacky, prolly has something to do with how the transform controls update the attached object.
        const position = new THREE.Vector3(0,0,0)
        const rotation = new THREE.Quaternion(0,0,0,1)
        const scale = new THREE.Vector3(1,1,1)
        gizmoTransformation.decompose(position, rotation, scale)
        this._obj.matrix.compose(position, rotation, scale)

        this._obj.position.setFromMatrixPosition(gizmoTransformation)
        this._obj.rotation.setFromRotationMatrix(gizmoTransformation)

        this._forceUpdate = true
    }

    public SetRotation(rotation: THREE.Quaternion) {
        const position = new THREE.Vector3(0,0,0)
        const scale = new THREE.Vector3(1,1,1)
        this._obj.matrix.decompose(position, new THREE.Quaternion(0,0,0,1), scale)
        this._obj.matrix.compose(position, rotation, scale)

        this._obj.rotation.setFromQuaternion(rotation)

        this._forceUpdate = true
    }

    /** @return true if gizmo is attached to mirabufSceneObject */
    public HasParent(): boolean {
        return this._parentObject !== undefined
    }
}

export default GizmoSceneObject
