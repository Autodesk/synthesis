import { test, expect, describe } from 'vitest';
import * as THREE from 'three';
import { ThreeEuler_JoltQuat, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3, _JoltQuat } from '../../../util/conversions/JoltThreeConversions';

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