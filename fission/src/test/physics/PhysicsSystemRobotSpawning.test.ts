import { describe, test, expect } from "vitest"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import PhysicsSystem, { LayerReserve } from "@/systems/physics/PhysicsSystem"

describe("Mirabuf Physics Loading", () => {
    test("Body Loading (Dozer)", async () => {
        const cacheInfo = await MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT)

        if (!cacheInfo) {
            console.warn("Warning: Remote mirabuf file not accessible - cacheRemote returned undefined")
            console.warn("This may indicate network issues or API unavailability in the test environment")
            throw new Error("Failed to cache remote mirabuf file")
        }

        const assembly = await MirabufCachingService.get(cacheInfo.id, MiraType.ROBOT)

        if (!assembly) {
            console.warn(`Warning: Failed to load mirabuf assembly from cache with ID: ${cacheInfo.id}`)
            console.warn("This may indicate storage issues or fallback mechanism problems")
            throw new Error("Failed to load mirabuf assembly from cache")
        }

        const parser = new MirabufParser(assembly)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.createBodiesFromParser(parser, new LayerReserve())

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
        const cacheInfo = await MirabufCachingService.cacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        )

        if (!cacheInfo) {
            console.warn("Warning: Remote mirabuf file not accessible - cacheRemote returned undefined")
            console.warn("This may indicate network issues or API unavailability in the test environment")
            throw new Error("Failed to cache remote mirabuf file")
        }

        const assembly = await MirabufCachingService.get(cacheInfo.id, MiraType.ROBOT)

        if (!assembly) {
            console.warn(`Warning: Failed to load mirabuf assembly from cache with ID: ${cacheInfo.id}`)
            console.warn("This may indicate storage issues or fallback mechanism problems")
            throw new Error("Failed to load mirabuf assembly from cache")
        }

        const parser = new MirabufParser(assembly)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.createBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(9)
    })
})
