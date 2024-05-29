import * as THREE from 'three';
import JOLT from './loading/JoltSyncLoader';
import Jolt from '@barclah/jolt-physics';
import { mirabuf } from "../proto/mirabuf";

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

export function ThreeMatrix4_JoltMat44(m: THREE.Matrix4) {
    const jMat = new JOLT.Mat44();
    const threeArr = m.toArray();
    for (let c = 0; c < 4; c++) {
        const column = new JOLT.Vec4(
            threeArr[4 * c + 0],
            threeArr[4 * c + 1],
            threeArr[4 * c + 2],
            threeArr[4 * c + 3]
        );
        jMat.SetColumn4(c, column);
        JOLT.destroy(column);
    }

    return jMat;
}

export function JoltVec3_ThreeVector3(vec: Jolt.Vec3 | Jolt.RVec3) {
    return new THREE.Vector3(vec.GetX(), vec.GetY(), vec.GetZ());
}

export function JoltQuat_ThreeQuaternion(quat: Jolt.Quat) {
    return new THREE.Quaternion(quat.GetX(), quat.GetY(), quat.GetZ(), quat.GetW());
}

export function JoltMat44_ThreeMatrix4(m: Jolt.RMat44): THREE.Matrix4 {
    return new THREE.Matrix4().compose(
        JoltVec3_ThreeVector3(m.GetTranslation()),
        JoltQuat_ThreeQuaternion(m.GetQuaternion()),
        new THREE.Vector3(1, 1, 1)
    );
}

export function MirabufTransform_ThreeMatrix4(m: mirabuf.ITransform): THREE.Matrix4 {
    const arr = m.spatialMatrix!;
    const pos = new THREE.Vector3(arr[3] * 0.01, arr[7] * 0.01, arr[11] * 0.01);
    const mat = new THREE.Matrix4().fromArray(arr);
    const onlyRotation = new THREE.Matrix4().extractRotation(mat).transpose();
    const quat = new THREE.Quaternion().setFromRotationMatrix(onlyRotation);
    return new THREE.Matrix4().compose(pos, quat, new THREE.Vector3(1, 1, 1));
}

export function MirabufVector3_ThreeVector3(v: mirabuf.Vector3): THREE.Vector3 {
    return new THREE.Vector3(v.x / 100.0, v.y / 100.0, v.z / 100.0);
}

export function MirabufVector3_JoltVec3(v: mirabuf.Vector3): Jolt.Vec3 {
    return new JOLT.Vec3(v.x / 100.0, v.y / 100.0, v.z / 100.0);
}

export function MirabufFloatArr_JoltVec3(v: number[], offsetIndex: number): Jolt.Vec3 {
    return new JOLT.Vec3(v[offsetIndex] / 100.0, v[offsetIndex + 1] / 100.0, v[offsetIndex + 2] / 100.0);
}

export function MirabufFloatArr_JoltVec3Arr(v: number[]): Jolt.Vec3[] {
    const arr = [];
    for (let i = 0; i < v.length; i += 3) {
        arr.push(MirabufFloatArr_JoltVec3(v, i));
    }
    return arr;
}