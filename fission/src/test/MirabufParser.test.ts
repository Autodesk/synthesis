import { describe, test, expect } from "vitest";

import { mirabuf } from "../proto/mirabuf";
import MirabufParser, { RigidNodeReadOnly } from "../mirabuf/MirabufParser";
import { LoadMirabufLocal } from "../mirabuf/MirabufLoader";

describe('Mirabuf Parser Tests', () => {
    test('Generate Rigid Nodes (Dozer_v9.mira)', () => {
        const spikeMira = LoadMirabufLocal('./public/Downloadables/Mira/Robots/Dozer_v9.mira');

        const t = new MirabufParser(spikeMira);
        const rn = t.rigidNodes;

        expect(filterNonPhysicsNodes(rn, spikeMira).length).toBe(7);
    });

    test('Generate Rigid Nodes (FRC_Field_2018_v14.mira)', () => {
        const field = LoadMirabufLocal('./public/Downloadables/Mira/Fields/FRC Field 2018_v13.mira');

        const t = new MirabufParser(field);

        expect(filterNonPhysicsNodes(t.rigidNodes, field).length).toBe(34);
    });

    test('Generate Rigid Nodes (Team_2471_(2018)_v7.mira)', () => {
        const mm = LoadMirabufLocal('./public/Downloadables/Mira/Robots/Team 2471 (2018)_v7.mira');

        const t = new MirabufParser(mm);

        expect(filterNonPhysicsNodes(t.rigidNodes, mm).length).toBe(10);
    });
});

function filterNonPhysicsNodes(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly): RigidNodeReadOnly[] {
    return nodes.filter(x => {
        for (const part of x.parts) {
            const inst = mira.data!.parts!.partInstances![part]!;
            const def = mira.data!.parts!.partDefinitions![inst.partDefinitionReference!]!;
            if (def.bodies && def.bodies.length > 0) {
                return true;
            }
        }
        return false;
    });
}

// function printRigidNodeParts(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly) {
//     nodes.forEach(x => {
//         console.log(`[ ${x.name} ]:`);
//         x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`));
//         console.log('');
//     });
// }