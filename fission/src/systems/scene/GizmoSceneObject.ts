import type { Object3D, PerspectiveCamera } from "three"
import * as THREE from "three"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import {
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
} from "@/util/TypeConversions"
import InputSystem from "../input/InputSystem"
import World from "../World"
import SceneObject from "./SceneObject"

export type GizmoMode = "translate" | "rotate" | "scale"

class GizmoSceneObject extends SceneObject {
    private _gizmo: TransformControls
    private _obj: Object3D
    private _forceUpdate: boolean = false

    private _parentObject?: MirabufSceneObject
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
        postGizmoCreation?: (gizmo: GizmoSceneObject) => void
    ) {
        super()

        this._obj = obj ?? new THREE.Mesh()
        this._parentObject = parentObject
        this._mainCamera = World.sceneRenderer.mainCamera

        this._size = size

        this._gizmo = new TransformControls(World.sceneRenderer.mainCamera, World.sceneRenderer.renderer.domElement)
        this._gizmo.setMode(mode)

        World.sceneRenderer.registerGizmoSceneObject(this)

        postGizmoCreation?.(this)

        if (this._parentObject) {
            this._relativeTransformations = new Map<RigidNodeId, THREE.Matrix4>()
            const gizmoTransformInv = this._obj.matrix.clone().invert()

            /** Due to the limited math functionality exposed to JS for Jolt, we need everything in ThreeJS. */
            this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                const jBodyId = this._parentObject!.mechanism.getBodyByNodeId(rn.id)
                if (!jBodyId) return

                const worldTransform = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(jBodyId).GetWorldTransform()
                )
                const relativeTransform = worldTransform.premultiply(gizmoTransformInv)
                this._relativeTransformations!.set(rn.id, relativeTransform)
            })
        }
    }

    public setup(): void {
        // adding the mesh and gizmo to the scene
        World.sceneRenderer.addObject(this._obj)
        World.sceneRenderer.addObject(this._gizmo.getHelper())

        // forcing the gizmo to rotate and transform with the object
        this._gizmo.setSpace("local")
        this._gizmo.attach(this._obj)

        this._gizmo.addEventListener("dragging-changed", (event: { target: TransformControls; value: unknown }) => {
            // disable orbit controls when dragging the transform gizmo
            const gizmoDragging = World.sceneRenderer.isAnyGizmoDragging()
            World.sceneRenderer.currentCameraControls.enabled = !event.value && !gizmoDragging

            const isShift = InputSystem.isKeyPressed("ShiftRight") || InputSystem.isKeyPressed("ShiftLeft")
            const isAlt = InputSystem.isKeyPressed("AltRight") || InputSystem.isKeyPressed("AltLeft")

            switch (event.target.mode) {
                case "translate": {
                    // snap if alt is pressed
                    event.target.translationSnap = isAlt ? 0.1 : null

                    // disable other gizmos when translating
                    const gizmos = [...World.sceneRenderer.gizmosOnMirabuf.values()]
                    gizmos.forEach(obj => {
                        if (obj.gizmo.object === event.target.object && obj.gizmo.mode !== "translate") {
                            obj.gizmo.dragging = false
                            obj.gizmo.enabled = !event.value
                            return
                        }
                    })
                    break
                }
                case "rotate": {
                    // snap if alt is pressed
                    event.target.rotationSnap = isAlt ? Math.PI * (1.0 / 12.0) : null

                    // disable scale gizmos added to the same object
                    const gizmos = [...World.sceneRenderer.gizmosOnMirabuf.values()]
                    gizmos.forEach(obj => {
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
                    break
                }
                case "scale": {
                    // snap if alt is pressed
                    event.target.setScaleSnap(isAlt ? 0.1 : null)

                    // scale uniformly if shift is pressed
                    if (isShift) {
                        event.target.axis = "XYZE"
                    }

                    break
                }
                default: {
                    console.error("Invalid gizmo state")
                    break
                }
            }
        })
    }

    public update(): void {
        this._gizmo.getHelper().updateMatrixWorld()

        if (!this.gizmo.object) {
            console.error("No object added to gizmo")
            return
        }

        // updating the size of the gizmo based on the distance from the camera
        const mainCameraFovRadians = (Math.PI * (this._mainCamera.fov * 0.5)) / 180
        this._gizmo.setSize(
            (this._size / this._mainCamera.position.distanceTo(this.gizmo.object!.position)) *
                Math.tan(mainCameraFovRadians) *
                1.9
        )

        /** Translating the obj changes to the mirabuf scene object */
        if (this._parentObject) {
            this._parentObject.disablePhysics()
            if (this.isDragging || this._forceUpdate) {
                this._forceUpdate = false
                this._parentObject.mirabufInstance.parser.rigidNodes.forEach(rn => {
                    this.updateNodeTransform(rn.id)
                })
                this._parentObject.updateMeshTransforms()
            }
        }
    }

    public dispose(): void {
        this._gizmo.detach()
        this._parentObject?.enablePhysics()
        World.sceneRenderer.removeObject(this._obj)
        World.sceneRenderer.removeObject(this._gizmo.getHelper())

        this._relativeTransformations?.clear()
    }

    /** changes the mode of the gizmo */
    public setMode(mode: GizmoMode) {
        this._gizmo.setMode(mode)
    }

    /**
     * Updates a given node to follow the gizmo.
     *
     * @param rnId Target node to update.
     */
    public updateNodeTransform(rnId: RigidNodeId) {
        if (!this._parentObject || !this._relativeTransformations || !this._relativeTransformations.has(rnId)) return

        const jBodyId = this._parentObject.mechanism.getBodyByNodeId(rnId)
        if (!jBodyId) return

        const relativeTransform = this._relativeTransformations.get(rnId)!
        const worldTransform = relativeTransform.clone().premultiply(this._obj.matrix)
        const position = new THREE.Vector3(0, 0, 0)
        const rotation = new THREE.Quaternion(0, 0, 0, 1)
        worldTransform.decompose(position, rotation, new THREE.Vector3(1, 1, 1))

        World.physicsSystem.setBodyPositionAndRotation(
            jBodyId,
            convertThreeVector3ToJoltRVec3(position),
            convertThreeQuaternionToJoltQuat(rotation)
        )
    }

    /**
     * Updates the gizmos location.
     *
     * @param gizmoTransformation Transform for the gizmo to take on.
     */
    public setTransform(gizmoTransformation: THREE.Matrix4) {
        // Super hacky, prolly has something to do with how the transform controls update the attached object.
        const position = new THREE.Vector3(0, 0, 0)
        const rotation = new THREE.Quaternion(0, 0, 0, 1)
        const scale = new THREE.Vector3(1, 1, 1)
        gizmoTransformation.decompose(position, rotation, scale)
        this._obj.matrix.compose(position, rotation, scale)

        this._obj.position.setFromMatrixPosition(gizmoTransformation)
        this._obj.rotation.setFromRotationMatrix(gizmoTransformation)

        this._forceUpdate = true
    }

    public setRotation(rotation: THREE.Quaternion) {
        const position = new THREE.Vector3(0, 0, 0)
        const scale = new THREE.Vector3(1, 1, 1)
        this._obj.matrix.decompose(position, new THREE.Quaternion(0, 0, 0, 1), scale)
        this._obj.matrix.compose(position, rotation, scale)

        this._obj.rotation.setFromQuaternion(rotation)

        this._forceUpdate = true
    }

    /** @return true if gizmo is attached to mirabufSceneObject */
    public hasParent(): boolean {
        return this._parentObject !== undefined
    }
}

export default GizmoSceneObject
