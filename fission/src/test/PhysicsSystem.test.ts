import { test, expect, describe, assert } from "vitest"
import PhysicsSystem, { LayerReserve } from "../systems/physics/PhysicsSystem"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"

describe("Physics Sanity Checks", () => {
    test("Convex Hull Shape (Cube)", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const system = new PhysicsSystem()
        const shapeResult = system.createConvexHull(points)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()

        expect(shape.GetVolume() - 1.0).toBeLessThan(0.001)
        expect(shape.GetCenterOfMass().Length()).toBe(0.0)

        shape.Release()
        system.destroy()
    })
    test("Convex Hull Shape (Tetrahedron)", () => {
        const points: Float32Array = new Float32Array([0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0])

        const system = new PhysicsSystem()
        const shapeResult = system.createConvexHull(points)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()
        const bounds = shape.GetLocalBounds()
        const boxSize = bounds.mMax.Sub(bounds.mMin)

        expect(boxSize.GetX() - 1.0).toBeLessThan(0.001)
        expect(boxSize.GetY() - 1.0).toBeLessThan(0.001)
        expect(boxSize.GetZ() - 1.0).toBeLessThan(0.001)
        expect(shape.GetVolume() - 1.0 / 6.0).toBeLessThan(0.001)
        expect(shape.GetMassProperties().mMass - 6.0).toBeLessThan(0.001)

        shape.Release()
        system.destroy()
    })
})

describe("Mirabuf Physics Loading", () => {
    test("Body Loading (Dozer)", async () => {
        const assembly = await MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT).then(
            x => MirabufCachingService.get(x!.id, MiraType.ROBOT)
        )
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
     * CAD Model: https://share.autodesk.com/:u:/r/sites/Synthesis/Shared%20Documents/General/Synthesis%20Fields%20and%20Robots/Multi-Joint%20Wheels.f3d?csf=1&web=1&e=5fpTB8
     * Mira FIle: https://synthesis.autodesk.com/api/mira/private/Multi-Joint_Wheels_v0.mira
     */
    test("Body Loading (Multi-Joint Wheels)", async () => {
        const assembly = await MirabufCachingService.CacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.Get(x!.id, MiraType.ROBOT))
        const parser = new MirabufParser(assembly!)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.CreateBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(9)
    })
})
