import { describe, test, expect } from "vitest";
import * as fs from 'fs';
import { mirabuf } from "../../proto/mirabuf";
import MirabufParser from "../../mirabuf/MirabufParser";

describe('Mirabuf Parser Tests', () => {
    test('Test Load TestCube_v1.mira', () => {
        const testCubeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/TestCube_v1.mira')));
        
        expect(testCubeMira).toBeDefined();
        expect(testCubeMira.info!.name!).toBe('TestCube v1');
    });

    test('Generate Design Hierarchy Node Values (TestCube_v1.mira)', () => {
        const testCubeMira = mirabuf.Assembly.decode(new Uint8Array(fs.readFileSync('./public/TestCube_v1.mira')));
        
        class T extends MirabufParser {
            public get partTreeValues() { return this._partTreeValues; }
        }

        const t = new T(testCubeMira);
        const vals = t.partTreeValues;

        vals.forEach((v, k) => console.debug(`${k} -> ${v}`));
    });
});