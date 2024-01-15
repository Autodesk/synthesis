import RAPIER from "@dimforge/rapier3d-compat";
import * as THREE from "three";

export class Translations {
    public static vector3ToThree(v: RAPIER.Vector3): THREE.Vector3 {
        return new THREE.Vector3(v.x, v.y, v.z);
    }

    public static vector3ToRapier(v: THREE.Vector3): RAPIER.Vector3 {
        return new RAPIER.Vector3(v.x, v.y, v.z);
    }

    public static rotationToThree(q: RAPIER.Rotation): THREE.Quaternion {
        return new THREE.Quaternion(q.x, q.y, q.z, q.w);
    }

    public static rigidToMatrix4(rb: RAPIER.RigidBody): THREE.Matrix4 {
        var mat = new THREE.Matrix4();
        // console.log(this.rotationToThree(rb.rotation()));
        mat.compose(
            this.vector3ToThree(rb.translation()),
            this.rotationToThree(rb.rotation()),
            new THREE.Vector3(1.0, 1.0, 1.0)
        );
        return mat;
    }

    public static loadMeshWithRigidbody(rb: RAPIER.RigidBody, mesh: THREE.Mesh) {
        var pos = this.vector3ToThree(rb.translation());
        mesh.position.set(pos.x, pos.y, pos.z);
        mesh.rotation.setFromQuaternion(this.rotationToThree(rb.rotation()));
    }
}