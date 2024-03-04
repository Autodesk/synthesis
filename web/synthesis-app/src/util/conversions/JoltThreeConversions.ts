import * as THREE from 'three';
import JOLT from '../loading/JoltAsyncLoader';

export function _JoltQuat(a: THREE.Euler | THREE.Quaternion | undefined) {
    if (<THREE.Euler>a) {
        return ThreeEuler_JoltQuat(a as THREE.Euler);
    } else if (<THREE.Quaternion>a) {
        return ThreeQuaternion_JoltQuat(a as THREE.Quaternion);
    } else {
        return new JOLT!.Quat(0, 0, 0, 1);
    }
}

export function ThreeEuler_JoltQuat(euler: THREE.Euler) {
    var quat = new THREE.Quaternion();
    quat.setFromEuler(euler);
    return ThreeQuaternion_JoltQuat(quat);
}

export function ThreeQuaternion_JoltQuat(quat: THREE.Quaternion) {
    return new JOLT!.Quat(quat.x, quat.y, quat.z, quat.w);
}

export function ThreeVector3_JoltVec3(vec: THREE.Vector3) {
    return new JOLT!.Vec3(vec.x, vec.y, vec.z);
}