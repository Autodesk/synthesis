import * as THREE from 'three';
import JOLT from '../loading/JoltSyncLoader';
import Jolt from '@barclah/jolt-physics';

export function _JoltQuat(a: THREE.Euler | THREE.Quaternion | undefined) {
    if (a instanceof THREE.Euler) {
        return ThreeEuler_JoltQuat(a as THREE.Euler);
    } else if (a instanceof THREE.Quaternion) {
        return ThreeQuaternion_JoltQuat(a as THREE.Quaternion);
    } else {
        return new JOLT.Quat(0, 0, 0, 1);
    }
}

export function ThreeEuler_JoltQuat(euler: THREE.Euler) {
    const quat = new THREE.Quaternion();
    quat.setFromEuler(euler);
    return ThreeQuaternion_JoltQuat(quat);
}

export function ThreeQuaternion_JoltQuat(quat: THREE.Quaternion) {
    return new JOLT.Quat(quat.x, quat.y, quat.z, quat.w);
}

export function ThreeVector3_JoltVec3(vec: THREE.Vector3) {
    return new JOLT.Vec3(vec.x, vec.y, vec.z);
}

export function JoltVec3_ThreeVector3(vec: Jolt.Vec3) {
    return new THREE.Vector3(vec.GetX(), vec.GetY(), vec.GetZ());
}

export function JoltQuat_ThreeQuaternion(quat: Jolt.Quat) {
    return new THREE.Quaternion(quat.GetX(), quat.GetY(), quat.GetZ(), quat.GetW());
}
