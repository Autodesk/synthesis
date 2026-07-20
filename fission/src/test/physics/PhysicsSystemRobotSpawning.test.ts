import {beforeAll, describe, expect, test, vi} from "vitest"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem, {LayerReserve} from "@/systems/physics/PhysicsSystem"
import {getMiraAssembly} from "@/test/GetAssets.ts";

describe("Mirabuf Physics Loading", () => {
    beforeAll(async () => {
        vi.spyOn(console, "warn").mockReturnValue()
        vi.spyOn(console, "log").mockReturnValue()
    })

    test("Body Loading (Dozer)", async () => {
        const assembly = await getMiraAssembly("DOZER")
        const parser = new MirabufParser(assembly!)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.createBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(7)
    })

    /*
     * Multi-Joint Wheels robot contains
     * - 4 wheels (4 revolute joints)
     * - 2 additional revolute joints
     * - 2 slider joints
     * Mira File: https://synthesis.autodesk.com/api/mira/private/Multi-Joint Wheels v0.mira
     */
    test("Body Loading (Multi-Joint Wheels)", async () => {
        const assembly = await getMiraAssembly("MULTI_JOINT")
        const parser = new MirabufParser(assembly!)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.createBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(9)
    })
})
