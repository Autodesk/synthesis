import { test, expect, describe } from "vitest"
import * as THREE from "three"
import {
    arrayThreeMatrix4,
    joltMat44ThreeMatrix4,
    mirabufTransformThreeMatrix,
    threeEulerJoltQuat,
    threeMatrix4Array,
    threeMatrix4JoltMat44,
    threeQuaternionJoltQuat,
    threeVector3JoltVec3,
    joltQuat,
} from "../../util/TypeConversions"
import { mirabuf } from "../../proto/mirabuf"
import JOLT from "../../util/loading/JoltSyncLoader"
import Jolt from "@azaleacolburn/jolt-physics"

describe("Three to Jolt Conversions", async () => {
    function compareMat(tM: THREE.Matrix4, jM: Jolt.Mat44) {
        const threeArr = tM.toArray()

        for (let c = 0; c < 4; c++) {
            const column = jM.GetColumn4(c)
            for (let r = 0; r < 4; r++) {
                expect(threeArr[c * 4 + r]).toBeCloseTo(column.GetComponent(r), 4)
            }
            JOLT.destroy(column)
        }

        const threeTranslation = new THREE.Vector3()
        const threeRotation = new THREE.Quaternion()
        const threeScale = new THREE.Vector3()
        tM.decompose(threeTranslation, threeRotation, threeScale)

        const joltTranslation = jM.GetTranslation()
        const joltRotation = jM.GetQuaternion()
        const joltScale = new JOLT.Vec3(1, 1, 1)

        expect(joltTranslation.GetX()).toBeCloseTo(threeTranslation.x, 4)
        expect(joltTranslation.GetY()).toBeCloseTo(threeTranslation.y, 4)
        expect(joltTranslation.GetZ()).toBeCloseTo(threeTranslation.z, 4)
        // JOLT.destroy(joltTranslation); // Causes error for some reason?
        expect(joltRotation.GetX()).toBeCloseTo(threeRotation.x, 4)
        expect(joltRotation.GetY()).toBeCloseTo(threeRotation.y, 4)
        expect(joltRotation.GetZ()).toBeCloseTo(threeRotation.z, 4)
        expect(joltRotation.GetW()).toBeCloseTo(threeRotation.w, 4)
        JOLT.destroy(joltRotation)
        expect(joltScale.GetX()).toBeCloseTo(threeScale.x, 4)
        expect(joltScale.GetY()).toBeCloseTo(threeScale.y, 4)
        expect(joltScale.GetZ()).toBeCloseTo(threeScale.z, 4)
        JOLT.destroy(joltScale)
    }

    test("THREE.Vector3 -> Jolt.Vec3", () => {
        const a = new THREE.Vector3(2, 4, 1)
        const joltVec = threeVector3JoltVec3(a)

        expect(joltVec.GetX()).toBe(a.x)
        expect(joltVec.GetY()).toBe(a.y)
        expect(joltVec.GetZ()).toBe(a.z)
        expect(joltVec.Length() - a.length()).toBeLessThan(0.0001)
    })

    test("THREE.Euler -> Jolt.Quat", () => {
        const a = new THREE.Euler(30, 60, 15)
        const myJoltQuat = threeEulerJoltQuat(a)
        const threeQuat = new THREE.Quaternion()
        threeQuat.setFromEuler(a)

        expect(myJoltQuat.GetX() - threeQuat.x).toBeLessThan(0.0001)
        expect(myJoltQuat.GetY() - threeQuat.y).toBeLessThan(0.0001)
        expect(myJoltQuat.GetZ() - threeQuat.z).toBeLessThan(0.0001)
        expect(myJoltQuat.GetW() - threeQuat.w).toBeLessThan(0.0001)
    })

    test("THREE.Quaternion -> Jolt.Quat", () => {
        const a = new THREE.Quaternion(0.285, 0.45, 0.237, 0.812)
        a.normalize()
        const myJoltQuat = threeQuaternionJoltQuat(a)

        expect(myJoltQuat.GetX() - a.x).toBeLessThan(0.0001)
        expect(myJoltQuat.GetY() - a.y).toBeLessThan(0.0001)
        expect(myJoltQuat.GetZ() - a.z).toBeLessThan(0.0001)
        expect(myJoltQuat.GetW() - a.w).toBeLessThan(0.0001)
    })

    test("THREE.Quaterion -> Jolt.Quat (General Func)", () => {
        const a = new THREE.Quaternion(0.285, 0.45, 0.237, 0.812)
        a.normalize()
        const myJoltQuat = joltQuat(a)

        expect(myJoltQuat.GetX() - a.x).toBeLessThan(0.0001)
        expect(myJoltQuat.GetY() - a.y).toBeLessThan(0.0001)
        expect(myJoltQuat.GetZ() - a.z).toBeLessThan(0.0001)
        expect(myJoltQuat.GetW() - a.w).toBeLessThan(0.0001)
    })

    test("THREE.Euler -> Jolt.Quat (General Func)", () => {
        const a = new THREE.Euler(30, 60, 15)
        const myJoltQuat = joltQuat(a)
        const threeQuat = new THREE.Quaternion()
        threeQuat.setFromEuler(a)

        expect(myJoltQuat.GetX() - threeQuat.x).toBeLessThan(0.0001)
        expect(myJoltQuat.GetY() - threeQuat.y).toBeLessThan(0.0001)
        expect(myJoltQuat.GetZ() - threeQuat.z).toBeLessThan(0.0001)
        expect(myJoltQuat.GetW() - threeQuat.w).toBeLessThan(0.0001)
    })

    test("undefined -> Jolt.Quat (General Func)", () => {
        const myJoltQuat = joltQuat(undefined)

        expect(myJoltQuat.GetX()).toBe(0.0)
        expect(myJoltQuat.GetY()).toBe(0.0)
        expect(myJoltQuat.GetZ()).toBe(0.0)
        expect(myJoltQuat.GetW()).toBe(1.0)
    })

    test("THREE.Matrix4 [Identity] -> Jolt.Mat44", () => {
        const threeMat = new THREE.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)

        const jMat = threeMatrix4JoltMat44(threeMat)

        compareMat(threeMat, jMat)
    })

    test("THREE.Matrix4 [+X Axis Rotation] -> Jolt.Mat44", () => {
        const threeMat = new THREE.Matrix4(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1)

        const jMat = threeMatrix4JoltMat44(threeMat)

        compareMat(threeMat, jMat)
    })

    test("THREE.Matrix4 [-X Axis Rotation] -> Jolt.Mat44", () => {
        const threeMat = new THREE.Matrix4(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1)

        const jMat = threeMatrix4JoltMat44(threeMat)

        compareMat(threeMat, jMat)
    })

    test("THREE.Matrix4 [XY Translation] -> Jolt.Mat44", () => {
        const threeMat = new THREE.Matrix4(1, 0, 0, 3, 0, 1, 0, 5, 0, 0, 1, 0, 0, 0, 0, 1)

        const jMat = threeMatrix4JoltMat44(threeMat)

        compareMat(threeMat, jMat)
    })
})

describe("Three Storage Conversion", () => {
    test("Array -> THREE.Matrix4 -> Array", () => {
        const originalArr = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]

        const threeMat = arrayThreeMatrix4(originalArr)
        const arr = threeMatrix4Array(threeMat)

        expect(arr.length).toBe(originalArr.length)
        for (let i = 0; i < arr.length; ++i) {
            expect(arr[i]).toBe(originalArr[i])
        }
    })
})

describe("Mirabuf to Three Conversions", () => {
    test("Mirabuf.Transform [Identity] -> THREE.Matrix4", () => {
        const miraMat = new mirabuf.Transform()
        miraMat.spatialMatrix = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = mirabufTransformThreeMatrix(miraMat)
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix
        const threeArr = threeMat.transpose().toArray() // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i]).toBe(miraArr[i])
        }
    })

    test("Mirabuf.Transform [+X Axis Rotation] -> THREE.Matrix4", () => {
        const miraMat = new mirabuf.Transform()
        miraMat.spatialMatrix = [1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1]

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = mirabufTransformThreeMatrix(miraMat)
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix
        const threeArr = threeMat.transpose().toArray() // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i]).toBeCloseTo(miraArr[i])
        }
    })

    test("Mirabuf.Transform [-X Axis Rotation] -> THREE.Matrix4", () => {
        const miraMat = new mirabuf.Transform()
        miraMat.spatialMatrix = [1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1]

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = mirabufTransformThreeMatrix(miraMat)
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix
        const threeArr = threeMat.transpose().toArray() // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i]).toBeCloseTo(miraArr[i])
        }
    })
})

describe("Jolt to Three Conversions", () => {
    function compareMat(jM: Jolt.RMat44, tM: THREE.Matrix4) {
        const threeArr = tM.toArray()

        for (let c = 0; c < 4; c++) {
            const column = jM.GetColumn4(c)
            for (let r = 0; r < 4; r++) {
                expect(threeArr[c * 4 + r]).toBeCloseTo(column.GetComponent(r), 4)
            }
            JOLT.destroy(column)
        }

        const threeTranslation = new THREE.Vector3()
        const threeRotation = new THREE.Quaternion()
        const threeScale = new THREE.Vector3()
        tM.decompose(threeTranslation, threeRotation, threeScale)

        const joltTranslation = jM.GetTranslation()
        const joltRotation = jM.GetQuaternion()
        const joltScale = new JOLT.Vec3(1, 1, 1)

        expect(threeTranslation.x).toBeCloseTo(joltTranslation.GetX(), 4)
        expect(threeTranslation.y).toBeCloseTo(joltTranslation.GetY(), 4)
        expect(threeTranslation.z).toBeCloseTo(joltTranslation.GetZ(), 4)
        // JOLT.destroy(joltTranslation); // Causes error for some reason?
        expect(threeRotation.x).toBeCloseTo(joltRotation.GetX(), 4)
        expect(threeRotation.y).toBeCloseTo(joltRotation.GetY(), 4)
        expect(threeRotation.z).toBeCloseTo(joltRotation.GetZ(), 4)
        expect(threeRotation.w).toBeCloseTo(joltRotation.GetW(), 4)
        JOLT.destroy(joltRotation)
        expect(threeScale.x).toBeCloseTo(joltScale.GetX(), 4)
        expect(threeScale.y).toBeCloseTo(joltScale.GetY(), 4)
        expect(threeScale.z).toBeCloseTo(joltScale.GetZ(), 4)
        JOLT.destroy(joltScale)
    }

    test("Jolt.Mat44 [Identity] -> THREE.Matrix4", () => {
        const tmp = new JOLT.RMat44()
        const joltMat = tmp.sIdentity()
        const threeMat = joltMat44ThreeMatrix4(joltMat)

        compareMat(joltMat, threeMat)

        JOLT.destroy(joltMat)
    })

    test("Jolt.Mat44 [+X Axis Rotation] -> THREE.Matrix4", () => {
        const joltMat = new JOLT.RMat44()
        const c0 = new JOLT.Vec4(1, 0, 0, 0)
        const c1 = new JOLT.Vec4(0, 0, -1, 0)
        const c2 = new JOLT.Vec4(0, 1, 0, 0)
        const c3 = new JOLT.Vec4(0, 0, 0, 1)
        joltMat.SetColumn4(0, c0)
        joltMat.SetColumn4(1, c1)
        joltMat.SetColumn4(2, c2)
        joltMat.SetColumn4(3, c3)
        JOLT.destroy(c0)
        JOLT.destroy(c1)
        JOLT.destroy(c2)
        JOLT.destroy(c3)

        const threeMat = joltMat44ThreeMatrix4(joltMat)

        compareMat(joltMat, threeMat)

        JOLT.destroy(joltMat)
    })

    test("Jolt.Mat44 [-X Axis Rotation] -> THREE.Matrix4", () => {
        const joltMat = new JOLT.RMat44()
        const c0 = new JOLT.Vec4(1, 0, 0, 0)
        const c1 = new JOLT.Vec4(0, 0, 1, 0)
        const c2 = new JOLT.Vec4(0, -1, 0, 0)
        const c3 = new JOLT.Vec4(0, 0, 0, 1)
        joltMat.SetColumn4(0, c0)
        joltMat.SetColumn4(1, c1)
        joltMat.SetColumn4(2, c2)
        joltMat.SetColumn4(3, c3)
        JOLT.destroy(c0)
        JOLT.destroy(c1)
        JOLT.destroy(c2)
        JOLT.destroy(c3)

        const threeMat = joltMat44ThreeMatrix4(joltMat)

        compareMat(joltMat, threeMat)

        JOLT.destroy(joltMat)
    })

    test("Jolt.Mat44 [X,Y Translation] -> THREE.Matrix4", () => {
        const joltMat = new JOLT.RMat44()
        const c0 = new JOLT.Vec4(1, 0, 0, 0)
        const c1 = new JOLT.Vec4(0, 1, 0, 0)
        const c2 = new JOLT.Vec4(0, 0, 1, 0)
        const c3 = new JOLT.Vec4(5, 3, 0, 1)
        joltMat.SetColumn4(0, c0)
        joltMat.SetColumn4(1, c1)
        joltMat.SetColumn4(2, c2)
        joltMat.SetColumn4(3, c3)
        JOLT.destroy(c0)
        JOLT.destroy(c1)
        JOLT.destroy(c2)
        JOLT.destroy(c3)

        const threeMat = joltMat44ThreeMatrix4(joltMat)

        compareMat(joltMat, threeMat)

        JOLT.destroy(joltMat)
    })
})
