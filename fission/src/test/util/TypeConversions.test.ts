import { test, expect, describe } from 'vitest';
import * as THREE from 'three';
import { MirabufTransform_ThreeMatrix4, ThreeEuler_JoltQuat, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3, _JoltQuat } from '../../util/TypeConversions';
import { mirabuf } from '../../proto/mirabuf';

describe('ThreeJS to Jolt Conversions', async () => {
    test('THREE.Vector3 -> JOLT.Vec3', () => {
        const a = new THREE.Vector3(2, 4, 1);
        const joltVec = ThreeVector3_JoltVec3(a);

        expect(joltVec.GetX()).toBe(a.x);
        expect(joltVec.GetY()).toBe(a.y);
        expect(joltVec.GetZ()).toBe(a.z);
        expect(joltVec.Length() - a.length()).toBeLessThan(0.0001);
    });
    test('THREE.Euler -> JOLT.Quat', () => {
        const a = new THREE.Euler(30, 60, 15);
        const joltQuat = ThreeEuler_JoltQuat(a);
        const threeQuat = new THREE.Quaternion();
        threeQuat.setFromEuler(a);

        expect(joltQuat.GetX() - threeQuat.x).toBeLessThan(0.0001);
        expect(joltQuat.GetY() - threeQuat.y).toBeLessThan(0.0001);
        expect(joltQuat.GetZ() - threeQuat.z).toBeLessThan(0.0001);
        expect(joltQuat.GetW() - threeQuat.w).toBeLessThan(0.0001);
    });
    test('THREE.Quaternion -> JOLT.Quat', () => {
        const a = new THREE.Quaternion(0.285, 0.450, 0.237, 0.812);
        a.normalize();
        const joltQuat = ThreeQuaternion_JoltQuat(a);

        expect(joltQuat.GetX() - a.x).toBeLessThan(0.0001);
        expect(joltQuat.GetY() - a.y).toBeLessThan(0.0001);
        expect(joltQuat.GetZ() - a.z).toBeLessThan(0.0001);
        expect(joltQuat.GetW() - a.w).toBeLessThan(0.0001);
    });
    test('THREE.Quaterion -> JOLT.Quat (General Func)', () => {
        const a = new THREE.Quaternion(0.285, 0.450, 0.237, 0.812);
        a.normalize();
        const joltQuat = _JoltQuat(a);
        
        expect(joltQuat.GetX() - a.x).toBeLessThan(0.0001);
        expect(joltQuat.GetY() - a.y).toBeLessThan(0.0001);
        expect(joltQuat.GetZ() - a.z).toBeLessThan(0.0001);
        expect(joltQuat.GetW() - a.w).toBeLessThan(0.0001);
    });
    test('THREE.Euler -> JOLT.Quat (General Func)', () => {
        const a = new THREE.Euler(30, 60, 15);
        const joltQuat = _JoltQuat(a);
        const threeQuat = new THREE.Quaternion();
        threeQuat.setFromEuler(a);

        expect(joltQuat.GetX() - threeQuat.x).toBeLessThan(0.0001);
        expect(joltQuat.GetY() - threeQuat.y).toBeLessThan(0.0001);
        expect(joltQuat.GetZ() - threeQuat.z).toBeLessThan(0.0001);
        expect(joltQuat.GetW() - threeQuat.w).toBeLessThan(0.0001);
    });
    test('undefined -> JOLT.Quat (General Func)', () => {
        const joltQuat = _JoltQuat(undefined);
        
        expect(joltQuat.GetX()).toBe(0.0);
        expect(joltQuat.GetY()).toBe(0.0);
        expect(joltQuat.GetZ()).toBe(0.0);
        expect(joltQuat.GetW()).toBe(1.0);
    });
});

function matToString(mat: THREE.Matrix4) {
    const arr = mat.toArray();
    return `[\n${arr[0].toFixed(4)}, ${arr[4].toFixed(4)}, ${arr[8].toFixed(4)}, ${arr[12].toFixed(4)},\n`
        + `${arr[1].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[13].toFixed(4)},\n`
        + `${arr[2].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[14].toFixed(4)},\n`
        + `${arr[3].toFixed(4)}, ${arr[7].toFixed(4)}, ${arr[11].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

function miraMatToString(mat: mirabuf.ITransform) {
    const arr = mat.spatialMatrix!;
    return `[\n${arr[0].toFixed(4)}, ${arr[1].toFixed(4)}, ${arr[2].toFixed(4)}, ${arr[3].toFixed(4)},\n`
        + `${arr[4].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[7].toFixed(4)},\n`
        + `${arr[8].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[11].toFixed(4)},\n`
        + `${arr[12].toFixed(4)}, ${arr[13].toFixed(4)}, ${arr[14].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

describe('Mirabuf Conversion Tests', () => {
    test('Matrix | Identity', () => {
        const miraMat = new mirabuf.Transform();
        miraMat.spatialMatrix = [
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        ];

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix;
        const threeArr = threeMat.transpose().toArray(); // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i]).toBe(miraArr[i]);
        }
    });

    test('Matrix | Rotation -> +X', () => {
        const miraMat = new mirabuf.Transform();
        miraMat.spatialMatrix = [
            1, 0, 0, 0,
            0, 0, 1, 0,
            0,-1, 0, 0,
            0, 0, 0, 1
        ];

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix;
        const threeArr = threeMat.transpose().toArray(); // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i] - miraArr[i]).toBeLessThan(0.00001);
        }
    });

    test('Matrix | Rotation -> -X', () => {
        const miraMat = new mirabuf.Transform();
        miraMat.spatialMatrix = [
            1, 0, 0, 0,
            0, 0,-1, 0,
            0, 1, 0, 0,
            0, 0, 0, 1
        ];

        // console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        // console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix;
        const threeArr = threeMat.transpose().toArray(); // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i] - miraArr[i]).toBeLessThan(0.00001);
        }
    });
});