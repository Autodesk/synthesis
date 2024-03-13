import { describe, test, expect } from "vitest";

import { mirabuf } from "../../proto/mirabuf";
import MirabufParser, { RigidNodeReadOnly } from "../../mirabuf/MirabufParser";
import { LoadMirabufLocal } from "../../mirabuf/MirabufLoader";

describe('Mirabuf Parser Tests', () => {
    test('Test Load TestCube_v1.mira (Uncompressed)', () => {
        const testCubeMira = LoadMirabufLocal('./public/test_mira/TestCube_v1.mira');
        
        expect(testCubeMira).toBeDefined();
        expect(testCubeMira.info!.name!).toBe('TestCube v1');
    });

    test('Test Load FRC_Field_2018_v14.mira (Compressed)', () => {
        const field = LoadMirabufLocal('./public/test_mira/FRC_Field_2018_v14.mira');

        expect(field).toBeDefined();
        expect(field.info!.name!).toBe('FRC Field 2018 v14');
    });

    test('Generate Rigid Nodes (TestCube_v1.mira)', () => {
        const testCubeMira = LoadMirabufLocal('./public/test_mira/TestCube_v1.mira');

        const parser = new MirabufParser(testCubeMira);
        const rn = parser.rigidNodes;

        expect(rn.length).toBe(1);
        printRigidNodeParts(rn, testCubeMira);
    });

    test('Generate Rigid Nodes (PhysicsSpikeTest_v1.mira)', () => {
        const spikeMira = LoadMirabufLocal('./public/test_mira/PhysicsSpikeTest_v1.mira');

        const t = new MirabufParser(spikeMira);
        const rn = t.rigidNodes;

        expect(rn.length).toBe(4);
        printRigidNodeParts(rn, spikeMira);
    });

    test('Generate Rigid Nodes (FRC_Field_2018_v14.mira)', () => {
        const field = LoadMirabufLocal('./public/test_mira/FRC_Field_2018_v14.mira');

        const t = new MirabufParser(field);

        printRigidNodeParts(t.rigidNodes, field);
        expect(t.rigidNodes.length).toBe(34);
    });
});

function printRigidNodeParts(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly) {
    nodes.forEach(x => {
        console.log(`[ ${x.name} ]:`);
        x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`));
        console.log('');
    });
}