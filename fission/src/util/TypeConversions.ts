import type Jolt from "@synthesis.adsk/jolt-physics"
import type { RgbaColor } from "react-colorful"
import * as THREE from "three"
import type { mirabuf } from "../proto/mirabuf"
import JOLT from "./loading/JoltSyncLoader"

export function convertThreeToJoltQuat(a: THREE.Euler | THREE.Quaternion | undefined) {
    if (a instanceof THREE.Euler) {
        return convertThreeEulerToJoltQuat(a as THREE.Euler)
    } else if (a instanceof THREE.Quaternion) {
        return convertThreeQuaternionToJoltQuat(a as THREE.Quaternion)
    } else {
        return new JOLT.Quat(0, 0, 0, 1)
    }
}

export function convertArrayToThreeMatrix4(arr: number[]) {
    // DO NOT ask me why retrieving and setting the same EXACT data is done is two DIFFERENT majors
    // biome-ignore-start format: We would prefer to visualize this as a matrix
    return new THREE.Matrix4(
        arr[0], arr[4], arr[8], arr[12],
        arr[1], arr[5], arr[9], arr[13],
        arr[2], arr[6], arr[10], arr[14],
        arr[3], arr[7], arr[11], arr[15]
    )
    // biome-ignore-end format: We would prefer to visualize this as a matrix
}

export function convertThreeMatrix4ToArray(mat: THREE.Matrix4) {
    return mat.elements
}

export function convertThreeEulerToJoltQuat(euler: THREE.Euler) {
    const quat = new THREE.Quaternion()
    quat.setFromEuler(euler)
    return convertThreeQuaternionToJoltQuat(quat)
}

export function convertThreeQuaternionToJoltQuat(quat: THREE.Quaternion) {
    return new JOLT.Quat(quat.x, quat.y, quat.z, quat.w)
}

export function convertThreeVector3ToJoltVec3(vec: THREE.Vector3) {
    return new JOLT.Vec3(vec.x, vec.y, vec.z)
}

export function convertThreeVector3ToJoltRVec3(vec: THREE.Vector3) {
    return new JOLT.RVec3(vec.x, vec.y, vec.z)
}

export function convertThreeMatrix4ToJoltMat44(m: THREE.Matrix4) {
    const jMat = new JOLT.Mat44()
    const threeArr = m.toArray()
    for (let c = 0; c < 4; c++) {
        const column = new JOLT.Vec4(threeArr[4 * c + 0], threeArr[4 * c + 1], threeArr[4 * c + 2], threeArr[4 * c + 3])
        jMat.SetColumn4(c, column)
        JOLT.destroy(column)
    }

    return jMat
}

export function convertJoltVec3ToThreeVector3(vec: Jolt.Vec3 | Jolt.RVec3, destroy: boolean = true) {
    const [x, y, z] = [vec.GetX(), vec.GetY(), vec.GetZ()]
    if (destroy) JOLT.destroy(vec)

    return new THREE.Vector3(x, y, z)
}

export function convertJoltQuatToThreeQuaternion(quat: Jolt.Quat, destroy: boolean = false) {
    const [x, y, z, w] = [quat.GetX(), quat.GetY(), quat.GetZ(), quat.GetW()]
    if (destroy) JOLT.destroy(quat)

    return new THREE.Quaternion(x, y, z, w)
}

export function convertJoltMat44ToThreeMatrix4(m: Jolt.RMat44, destroy: boolean = false): THREE.Matrix4 {
    const [t, q] = [m.GetTranslation(), m.GetQuaternion()]

    const mat = new THREE.Matrix4().compose(
        convertJoltVec3ToThreeVector3(t, false),
        convertJoltQuatToThreeQuaternion(q, false),
        new THREE.Vector3(1, 1, 1)
    )

    if (destroy) JOLT.destroy(m)

    return mat
}

export function convertJoltVec3ToJoltRVec3(vec: Jolt.Vec3, destroy: boolean = true): Jolt.RVec3 {
    const [x, y, z] = [vec.GetX(), vec.GetY(), vec.GetZ()]
    if (destroy) JOLT.destroy(vec)

    return new JOLT.RVec3(x, y, z)
}

export function convertJoltRVec3ToJoltVec3(vec: Jolt.RVec3, destroy: boolean = true): Jolt.Vec3 {
    const [x, y, z] = [vec.GetX(), vec.GetY(), vec.GetZ()]
    if (destroy) JOLT.destroy(vec)

    return new JOLT.Vec3(x, y, z)
}

export function convertMirabufTransformToThreeMatrix(m: mirabuf.ITransform): THREE.Matrix4 {
    const arr = m.spatialMatrix!
    const pos = new THREE.Vector3(arr[3] * 0.01, arr[7] * 0.01, arr[11] * 0.01)
    const mat = new THREE.Matrix4().fromArray(arr)
    const onlyRotation = new THREE.Matrix4().extractRotation(mat).transpose()
    const quat = new THREE.Quaternion().setFromRotationMatrix(onlyRotation)
    return new THREE.Matrix4().compose(pos, quat, new THREE.Vector3(1, 1, 1))
}

export function convertMirabufVector3ToJoltVec3(v: mirabuf.IVector3): Jolt.Vec3 {
    return new JOLT.Vec3(v.x! / 100.0, v.y! / 100.0, v.z! / 100.0)
}

export function convertMirabufVector3ToJoltRVec3(v: mirabuf.IVector3): Jolt.RVec3 {
    return new JOLT.RVec3(v.x! / 100.0, v.y! / 100.0, v.z! / 100.0)
}

export function convertMirabufFloatToArrJoltVec3(v: number[], offsetIndex: number): Jolt.Vec3 {
    return new JOLT.Vec3(v[offsetIndex] / 100.0, v[offsetIndex + 1] / 100.0, v[offsetIndex + 2] / 100.0)
}

export function convertMirabufFloatToArrJoltFloat3(v: number[], offsetIndex: number): Jolt.Float3 {
    return new JOLT.Float3(v[offsetIndex] / 100.0, v[offsetIndex + 1] / 100.0, v[offsetIndex + 2] / 100.0)
}

export function convertReactRgbaColorToThreeColor(color: RgbaColor) {
    return new THREE.Color(Math.floor(color.r / 255), Math.floor(color.g / 255), Math.floor(color.b / 255))
}

export function convertAxisAlignedToOrientedBoundingBox(aabb: Jolt.AABox): Jolt.OrientedBox {
    const center = aabb.GetCenter()
    const halfExtent = aabb.GetExtent()
    const transform = new JOLT.Mat44().sTranslation(center)

    return new JOLT.OrientedBox(transform, halfExtent)
}
