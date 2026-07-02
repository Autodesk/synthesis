import { describe, expect, test } from "vitest"
import MirabufCachingService, { MiraType } from "../../mirabuf/MirabufLoader.ts"
import MirabufParser, { type RigidNodeReadOnly } from "../../mirabuf/MirabufParser.ts"
import type { mirabuf } from "@/proto/mirabuf"
import type { Matrix4 } from "three"

describe("Mirabuf Parser Tests", () => {
    test("Generate Rigid Nodes (Dozer)", async () => {
        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/robots/Dozer v11.mira",
            MiraType.ROBOT
        ).then(async x => ({ hash: x!.hash, asset: await MirabufCachingService.get(x!.hash) }))

        const t = new MirabufParser(spikeMira.hash, spikeMira.asset!)
        const rn = [...t.rigidNodes.values()]

        const physicsNodes = filterNonPhysicsNodes(rn, spikeMira.asset!).length
        expect(physicsNodes).toBe(7)
        expect([...t.partTreeValues.values()].length).toBe(13)
        expect([...t.partToNodeMap.values()].length).toBe(12)
        expect(await hashTransforms(t.globalTransforms)).toMatchSnapshot()
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
        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(async x => ({ hash: x!.hash, asset: await MirabufCachingService.get(x!.hash) }))

        const t = new MirabufParser(spikeMira.hash, spikeMira.asset!)
        const rn = [...t.rigidNodes.values()]
        const physicsNodes = filterNonPhysicsNodes(rn, spikeMira.asset!)

        expect(physicsNodes.length).toBe(9)
        expect([...t.partTreeValues.values()].length).toBe(12)
        expect([...t.partToNodeMap.values()].length).toBe(11)
        expect(await hashTransforms(t.globalTransforms)).toMatchSnapshot()
        expect(t.rootNode).toBe("16")
    })

    test("Generate Rigid Nodes (FRC Field 2018_v13.mira)", async () => {
        const field = await MirabufCachingService.cacheRemote(
            "/api/mira/fields/FRC Field 2018_v13.mira",
            MiraType.FIELD
        ).then(async x => ({ hash: x!.hash, asset: await MirabufCachingService.get(x!.hash) }))

        const t = new MirabufParser(field.hash, field.asset!)
        const physicsNodes = filterNonPhysicsNodes([...t.rigidNodes.values()], field.asset!)

        expect(physicsNodes.length).toBe(34)
        expect([...t.partTreeValues.values()].length).toBe(982)
        expect([...t.partToNodeMap.values()].length).toBe(981)
        expect(await hashTransforms(t.globalTransforms)).toMatchSnapshot()
        expect(t.rootNode).toBe("35merged")
    })
})

async function hashTransforms(globalTransforms: Map<string, Matrix4>): Promise<ArrayBuffer> {
    return crypto.subtle.digest(
        "SHA-1",
        new Int16Array([...globalTransforms.values()].flatMap(mat => mat.toArray()).map(n => Math.round(n * 1000)))
    )
}

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
