import { describe, expect, test } from "vitest"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem, { LayerReserve } from "@/systems/physics/PhysicsSystem"
import { beforeEach } from "node:test"

describe("Mirabuf Physics Loading", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    test("Body Loading (Dozer)", async () => {
        const assembly = await MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT).then(
            x => MirabufCachingService.get(x!.id, MiraType.ROBOT)
        )
        const parser = new MirabufParser(assembly!)
        const mapping = PhysicsSystem.createBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(7)
    })

    /*
     * Multi-Joint Wheels robot contains
     * - 4 wheels (4 revolute joints)
     * - 2 additional revolute joints
     * - 2 slider joints
     * Mira File: https://synthesis.autodesk.com/api/mira/private/Multi-Joint_Wheels_v0.mira
     */
    test("Body Loading (Multi-Joint Wheels)", async () => {
        const assembly = await MirabufCachingService.cacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.get(x!.id, MiraType.ROBOT))
        const parser = new MirabufParser(assembly!)
        const mapping = PhysicsSystem.createBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(9)
    })
})
