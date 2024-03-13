import { describe, test, expect } from "vitest";
import * as fs from 'fs';
import { mirabuf } from "../../proto/mirabuf";
import MirabufParser, { RigidNode } from "../../mirabuf/MirabufParser";

describe('Mirabuf Parser Tests', () => {
    test('Test Load TestCube_v1.mira', () => {
        const testCubeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/test_mira/TestCube_v1.mira')));
        
        expect(testCubeMira).toBeDefined();
        expect(testCubeMira.info!.name!).toBe('TestCube v1');
    });

    test('Generate Rigid Nodes (TestCube_v1.mira)', () => {
        const testCubeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/test_mira/TestCube_v1.mira')));
        
        class T extends MirabufParser {
            public get partTreeValues() { return this._partTreeValues; }
            public get rigidNodes() { return this._rigidNodes; }
        }

        const t = new T(testCubeMira);
        const vals = t.partTreeValues;
        const rn = t.rigidNodes;

        expect(rn.length).toBe(1);
        printRigidNodeParts(rn, testCubeMira);
    });

    // test('Generate JSON (PhysicsSpikeTest_v1.mira)', () => {
    //     const testCubeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/test_mira/PhysicsSpikeTest_v1.mira')));
        
    //     console.log('=== START JSON ===');
    //     console.log(JSON.stringify(testCubeMira));
    //     console.log('\n=== END JSON ===');

    //     expect(true).toBe(true);
    // });

    test('Generate Rigid Nodes (PhysicsSpikeTest_v1.mira)', () => {
        const spikeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/test_mira/PhysicsSpikeTest_v1.mira')));
        
        class T extends MirabufParser {
            public get partTreeValues() { return this._partTreeValues; }
            public get rigidNodes() { return this._rigidNodes; }
        }

        const t = new T(spikeMira);
        const vals = t.partTreeValues;
        const rn = t.rigidNodes;

        expect(rn.length).toBe(4);
        printRigidNodeParts(rn, spikeMira);
    });
});

function printRigidNodeParts(nodes: RigidNode[], mira: mirabuf.Assembly) {
    nodes.forEach(x => {
        console.log(`[ ${x.name} ]:`);
        x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`));
        console.log('');
    });
}