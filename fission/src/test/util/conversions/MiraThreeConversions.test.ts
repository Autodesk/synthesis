import { test, expect, describe } from 'vitest';
import { mirabuf } from '../../../proto/mirabuf';
import { MirabufTransform_ThreeMatrix4 } from '../../../util/conversions/MiraThreeConversions';

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

        console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        console.debug(`Three: ${matToString(threeMat)}`);

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

        console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        console.debug(`Three: ${matToString(threeMat)}`);

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

        console.debug(`Mira: ${miraMatToString(miraMat)}`);

        const threeMat = MirabufTransform_ThreeMatrix4(miraMat);
        console.debug(`Three: ${matToString(threeMat)}`);

        const miraArr = miraMat.spatialMatrix;
        const threeArr = threeMat.transpose().toArray(); // To Array gives column major for some reason. See docs

        for (let i = 0; i < 16; i++) {
            expect(threeArr[i] - miraArr[i]).toBeLessThan(0.00001);
        }
    });
});