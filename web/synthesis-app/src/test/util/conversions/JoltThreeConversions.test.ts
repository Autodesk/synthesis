import { test, expect, describe } from '@jest/globals';
import { joltInit } from '../../../util/loading/JoltAsyncLoader';
import * as THREE from 'three';
import { ThreeVector3_JoltVec3 } from '../../../util/conversions/JoltThreeConversions';

describe('ThreeJS to Jolt Conversions', async () => {
    await joltInit;
    test('THREE.Vector3 -> JOLT.Vec3', () => {
        var a = new THREE.Vector3(2, 4, 1);
        var joltVec = ThreeVector3_JoltVec3(a);

        expect(joltVec.GetX()).toBe(a.x);
        expect(joltVec.GetY()).toBe(a.y);
        expect(joltVec.GetZ()).toBe(a.z);
        expect(joltVec.Length() - a.length()).toBeLessThan(0.0001);
    });
});