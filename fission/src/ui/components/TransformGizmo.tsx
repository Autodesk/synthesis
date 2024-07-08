import MirabufInstance from "@/mirabuf/MirabufInstance"
import World from "@/systems/World"
import * as THREE from "three"

class TransformGizmo {
    private mode: "translate" | "rotate" | "scale"
    private mesh: THREE.Mesh

    get getMode() {
        return this.mode
    }

    set setMode(value: "translate" | "rotate" | "scale") {
        this.mode = value
        World.SceneRenderer.UpdateTransformGizmoMode(value, this.mesh)
    }

    constructor(TransformMode: "translate" | "rotate" | "scale", object?: MirabufInstance) {
        this.mode = TransformMode
        ;(this.mesh = new THREE.Mesh(new THREE.SphereGeometry(5))), new THREE.MeshBasicMaterial({ color: 0xff0000 })
        // add implementation in JIRA issue #1733 to wrap Mirabuf in Mesh which translates changes
        World.SceneRenderer.AddTransformGizmo(this.mesh, TransformMode, 5.0)
    }

    public deleteGizmo() {
        World.SceneRenderer.RemoveTransformGizmo(this.mesh)
    }
}

export default TransformGizmo
