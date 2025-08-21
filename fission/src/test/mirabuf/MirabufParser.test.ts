import { describe, expect, test } from "vitest"
import MirabufCachingService, { MiraType } from "../../mirabuf/MirabufLoader.ts"
import MirabufParser, { type RigidNodeReadOnly } from "../../mirabuf/MirabufParser.ts"
import { mirabuf } from "../../proto/mirabuf"

describe("Mirabuf Parser Tests", () => {
    test("Generate Rigid Nodes (Dozer_v9.mira)", async () => {
        const expectedTransforms = [
            1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, -2.6645352591003757e-15, 0, -1, 0, 0, 1, 0, 0, 1, 0,
            -2.6645352591003757e-15, 0, -0.19047861099243166, 0, -0.6847509002685547, 1, -2.6645352591003757e-15, 0, -1,
            0, 0, 1, 0, 0, 1, 0, -2.6645352591003757e-15, 0, -0.19047861099243166, 0, -0.6847509002685547, 1,
            -2.6645352591003757e-15, 0, -1, 0, 0, 1, 0, 0, 1, 0, -2.6645352591003757e-15, 0, -0.19047861099243166, 0,
            -0.6847509002685547, 1, 2.220446049250313e-16, 1, -6.123234262925859e-17, 0, -1, 2.220446049250313e-16,
            2.4674707145810578e-15, 0, 2.4674707145810578e-15, 6.12323426292582e-17, 1, 0, 0.09802138328552246,
            0.025399999618530275, -0.4117508697509766, 1, 2.220446049250313e-16, -1, -6.123234262925859e-17, 0, 1,
            2.220446049250313e-16, -2.4674707145810578e-15, 0, 2.4674707145810578e-15, -6.12323426292582e-17, 1, 0,
            -0.4789786148071289, 0.025399999618530275, -0.4117508697509766, 1, 2.220446049250313e-16, 1,
            -6.123234262925859e-17, 0, -1, 2.220446049250313e-16, 2.4674707145810578e-15, 0, 2.4674707145810578e-15,
            6.12323426292582e-17, 1, 0, 0.09802138328552246, 0.025399999618530275, -0.6847509002685547, 1,
            2.220446049250313e-16, -1, -6.123234262925859e-17, 0, 1, 2.220446049250313e-16, -2.4674707145810578e-15, 0,
            2.4674707145810578e-15, -6.12323426292582e-17, 1, 0, -0.4789786148071289, 0.025399999618530275,
            -0.6847509002685547, 1, 2.220446049250313e-16, -1, -6.123234262925859e-17, 0, 1, 2.220446049250313e-16,
            -2.4674707145810578e-15, 0, 2.4674707145810578e-15, -6.12323426292582e-17, 1, 0, -0.4789786148071289,
            0.025399999618530275, -1.0085508728027344, 1, 2.220446049250313e-16, 1, -6.123234262925859e-17, 0, -1,
            2.220446049250313e-16, 2.4674707145810578e-15, 0, 2.4674707145810578e-15, 6.12323426292582e-17, 1, 0,
            0.09802138328552246, 0.025399999618530275, -1.0085508728027344, 1, -2.6645352591003757e-15, 0, -1, 0, 0, 1,
            0, 0, 1, 0, -2.6645352591003757e-15, 0, -0.19047861099243166, 0, -0.6847509002685547, 1,
            -2.6645352591003757e-15, 0, -1, 0, 0, 1, 0, 0, 1, 0, -2.6645352591003757e-15, 0, -0.19047861099243166, 0,
            -0.6847509002685547, 1,
        ]

        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/robots/Dozer_v9.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.get(x!.id, MiraType.ROBOT))

        const t = new MirabufParser(spikeMira!)
        const rn = [...t.rigidNodes.values()]

        const physicsNodes = filterNonPhysicsNodes(rn, spikeMira!).length
        expect(physicsNodes).toBe(7)
        expect([...t.partTreeValues.values()].length).toBe(13)
        expect([...t.partToNodeMap.values()].length).toBe(12)
        expect([...t.globalTransforms.values()].flatMap(matrix => matrix.toArray())).toStrictEqual(expectedTransforms)
        expect(t.rootNode).toBe("12")
    })

    /*
     * Multi-Joint Wheels robot contains
     * - 4 wheels (4 revolute joints)
     * - 2 additional revolute joints
     * - 2 slider joints
     * Mira File: https://synthesis.autodesk.com/api/mira/private/Multi-Joint_Wheels_v0.mira
     */
    test("Generate Rigid Nodes (Multi-Joint Wheels)", async () => {
        const expectedTransforms = [
            1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1.7763568394002505e-15, -1, 0, 0, 1,
            1.7763568394002505e-15, 0, 0, 0.2675, 0.3675, 1, 1, 0, 0, 0, 0, 1.7763568394002505e-15, -1, 0, 0, 1,
            1.7763568394002505e-15, 0, 0, 0.2675, 0.3675, 1, 1, 0, 0, 0, 0, 1.7763568394002505e-15, -1, 0, 0, 1,
            1.7763568394002505e-15, 0, 0, 0.25159368515014646, 0.3675, 1, 1, 0, 0, 0, 0, 1.7763568394002505e-15, -1, 0,
            0, 1, 1.7763568394002505e-15, 0, 0, 0.2675, 0.3675, 1, 1, 0, 0, 0, 0, 1.7763568394002505e-15, -1, 0, 0, 1,
            1.7763568394002505e-15, 0, 0, 0.2675, 0.3675, 1, 0.7732124781661257, -0.6341470362685602,
            -1.1657341758564144e-15, 0, -1.6653345369377348e-16, 1.5543122344752192e-15, -1.0000000000000002, 0,
            0.6341470362685602, 0.7732124781661258, 9.992007221626409e-16, 0, -0.035183987617492675, 0.2640233612060547,
            0.3675, 1, 0.7732124781661258, -0.6341470362685601, -1.0547118733938985e-15, 0, -6.308850805844422e-17,
            1.5053687278135902e-15, -1.0000000000000002, 0, 0.6341470362685601, 0.7732124781661259,
            1.026956297778269e-15, 0, -0.0351839876174927, 0.2640233612060547, 0.3675, 1, 1, 5.551115123125783e-17,
            -1.5660009058798645e-16, 0, -5.541350988171935e-17, 1.4990741093153677e-15, -1.0000000000000002, 0, 0, 1,
            1.4566032141052243e-15, 0, 0.024999999825429893, 0.2424999971276784, 0.3674999999999999, 1, 1,
            5.551115123125783e-17, -1.5660009058798645e-16, 0, -5.541350988171935e-17, 1.4990741093153677e-15,
            -1.0000000000000002, 0, 0, 1, 1.4566032141052243e-15, 0, -0.02499999988244097, 0.2424999976714384,
            0.3675000000000001, 1, 1, 5.551115123125783e-17, -1.5660009058798645e-16, 0, -5.541350988171935e-17,
            1.4990741093153677e-15, -1.0000000000000002, 0, 0, 1, 1.4566032141052243e-15, 0, 0.024999999825429893,
            0.2424999971276784, 0.3674999999999998, 1,
        ]
        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.get(x!.id, MiraType.ROBOT))

        const t = new MirabufParser(spikeMira!)
        const rn = [...t.rigidNodes.values()]
        const physicsNodes = filterNonPhysicsNodes(rn, spikeMira!)

        expect(physicsNodes.length).toBe(9)
        expect([...t.partTreeValues.values()].length).toBe(12)
        expect([...t.partToNodeMap.values()].length).toBe(11)
        expect([...t.globalTransforms.values()].flatMap(matrix => matrix.toArray())).toStrictEqual(expectedTransforms) //
        expect(t.rootNode).toBe("16")
    })

    test("Generate Rigid Nodes (FRC Field 2018_v13.mira)", async () => {
        const field = await MirabufCachingService.cacheRemote(
            "/api/mira/Fields/FRC Field 2018_v13.mira",
            MiraType.FIELD
        ).then(x => MirabufCachingService.get(x!.id, MiraType.FIELD))

        const t = new MirabufParser(field!)
        const physicsNodes = filterNonPhysicsNodes([...t.rigidNodes.values()], field!)
        const transformsSum = 1954.8213339462916

        expect(physicsNodes.length).toBe(34)
        expect([...t.partTreeValues.values()].length).toBe(982)
        expect([...t.partToNodeMap.values()].length).toBe(981)
        expect([...t.globalTransforms.values()].length).toBe(981) //
        expect([...t.globalTransforms.values()].flatMap(mat => mat.toArray()).reduce((acc, c) => acc + c, 0)).toBe(
            transformsSum
        )
        expect(t.rootNode).toBe("35merged")
    })
})

function filterNonPhysicsNodes(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly): RigidNodeReadOnly[] {
    return nodes.filter(x => {
        for (const part of x.parts) {
            const inst = mira.data!.parts!.partInstances![part]!
            const def = mira.data!.parts!.partDefinitions![inst.partDefinitionReference!]!
            if (def.bodies && def.bodies.length > 0) {
                return true
            }
        }
        return false
    })
}

// function printRigidNodeParts(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly) {
//     nodes.forEach(x => {
//         console.log(`[ ${x.name} ]:`);
//         x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`));
//         console.log('');
//     });
// }
