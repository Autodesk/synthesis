import { mirabuf } from "../../proto/mirabuf";
import * as THREE from 'three';

export function MirabufTransform_ThreeMatrix4(m: mirabuf.ITransform): THREE.Matrix4 {
    const arr = m.spatialMatrix!;
    const pos = new THREE.Vector3(arr[3] * 0.01, arr[7] * 0.01, arr[11] * 0.01);
    const mat = new THREE.Matrix4().fromArray(arr);
    const onlyRotation = new THREE.Matrix4().extractRotation(mat); // .transpose();
    const quat = new THREE.Quaternion().setFromRotationMatrix(onlyRotation);
    return new THREE.Matrix4().compose(pos, quat, new THREE.Vector3(1, 1, 1));
}