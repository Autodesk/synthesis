import * as THREE from "three"

import World from "@/systems/World"
import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import { ThreeMatrix4_JoltMat44, ThreeQuaternion_JoltQuat } from "@/util/TypeConversions"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { RigidNodeReadOnly } from "@/mirabuf/MirabufParser"

class TransformGizmos {
    private _mesh: THREE.Mesh
    private _gizmos: TransformControls[] = []
    private _isActive: boolean = true

    get mesh() {
        return this._mesh
    }

    get isActive() {
        return this._isActive
    }

    constructor(object: THREE.Mesh) {
        this._mesh = object
    }

    /**
     *
     * @returns Whether or not any of the gizmos are being currently dragged by the user
     */
    public isBeingDragged() {
        return this._gizmos.some(gizmo => gizmo.dragging)
    }

    /**
     * Adds mesh to scene
     */
    public AddMeshToScene() {
        World.SceneRenderer.scene.add(this._mesh)
    }

    /**
     * Creates a gizmo for the mesh
     *
     * @param mode The type of gizmo to create
     */
    public CreateGizmo(mode: "translate" | "rotate" | "scale", size: number = 1.5) {
        const gizmo = World.SceneRenderer.AddTransformGizmo(this._mesh, mode, size)
        this._gizmos.push(gizmo)
    }

    /**
     * Removes the gizmos from the scene
     */
    public RemoveGizmos() {
        World.SceneRenderer.RemoveTransformGizmos(this._mesh)
        World.SceneRenderer.RemoveObject(this._mesh)
        this._isActive = false
    }

    /**
     * Updates the position and rotation of the gizmos to match the mesh's position
     *
     * @param obj The MirabufSceneObject that the gizmos are attached to
     * @param rn The RigidNode that are being updated
     */
    public UpdateMirabufPositioning(obj: MirabufSceneObject, rn: RigidNodeReadOnly) {
        World.PhysicsSystem.SetBodyPosition(
            obj.mechanism.GetBodyByNodeId(rn.id)!,
            ThreeMatrix4_JoltMat44(this.mesh.matrix).GetTranslation()
        ) // updating the position of the Jolt body
        World.PhysicsSystem.SetBodyRotation(
            obj.mechanism.GetBodyByNodeId(rn.id)!,
            ThreeQuaternion_JoltQuat(this.mesh.quaternion)
        ) // updating the rotation of the Jolt body

        rn.parts.forEach(part => {
            const partTransform = obj.mirabufInstance.parser.globalTransforms
                .get(part)!
                .clone()
                .premultiply(this.mesh.matrix)
            obj.mirabufInstance.meshes.get(part)!.forEach(mesh => {
                // iterating through each mesh and updating their position and rotation
                mesh.position.setFromMatrixPosition(partTransform)
                mesh.rotation.setFromRotationMatrix(partTransform)
            })
        })
    }
}

export default TransformGizmos
