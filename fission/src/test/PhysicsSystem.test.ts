import { test, expect, describe, assert } from "vitest"
import PhysicsSystem, { LayerReserve } from "../systems/physics/PhysicsSystem"
import { LoadMirabufLocal } from "@/mirabuf/MirabufLoader"
import MirabufParser from "@/mirabuf/MirabufParser"
import * as THREE from "three"
import JOLT from "@/util/loading/JoltSyncLoader"

describe("Physics Sansity Checks", () => {
    test("Convex Hull Shape (Cube)", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5,
            0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const system = new PhysicsSystem()
        const shapeResult = system.CreateConvexHull(points)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()

        expect(shape.GetVolume() - 1.0).toBeLessThan(0.001)
        expect(shape.GetCenterOfMass().Length()).toBe(0.0)

        shape.Release()
        system.Destroy()
    })
    test("Convex Hull Shape (Tetrahedron)", () => {
        const points: Float32Array = new Float32Array([
            0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0,
        ])

        const system = new PhysicsSystem()
        const shapeResult = system.CreateConvexHull(points)

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
        system.Destroy()
    })
})

describe("GodMode", () => {
    test("Basic", () => {
        const system = new PhysicsSystem()
        const box = system.CreateBox(
            new THREE.Vector3(1, 1, 1),
            1,
            new THREE.Vector3(0, 0, 0),
            undefined
        )
        const [ghostObject, ghostConstraint] = system.CreateGodModeBody(
            box.GetID()
        )

        assert(system.GetBody(ghostObject.GetID()) != undefined)
        assert(system.GetBody(box.GetID()) != undefined)
        assert(ghostConstraint != undefined)
        // Check constraint after running for a few seconds
        // TODO: Make sure this is the correct way to do this
        // TODO: Figure out how to make it use substeps to check instead
        for (let i = 0; i < 30; i++) {
            system.Update(i)
        }

        assert(system.GetBody(ghostObject.GetID()) != undefined)
        assert(system.GetBody(box.GetID()) != undefined)
        assert(ghostConstraint != undefined)

        system.Destroy()
    })

    test("Position", () => {
        const system = new PhysicsSystem()
        const box = system.CreateBox(
            new THREE.Vector3(1, 1, 1),
            1,
            new THREE.Vector3(0, 0, 0),
            undefined
        )
        const [ghostObject, _ghostConstraint] = system.CreateGodModeBody(
            box.GetID()
        )
        const origPosition = ghostObject.GetPosition()
        system.SetBodyPosition(ghostObject.GetID(), new JOLT.Vec3(2, 2, 2))

        for (let i = 0; i < 30; i++) {
            system.Update(i)
        }

        const currPosition = ghostObject.GetPosition()

        assert(
            currPosition.GetX() != origPosition.GetX() ||
                currPosition.GetY() != origPosition.GetY() ||
                currPosition.GetZ() != origPosition.GetZ()
        )

        system.Destroy()
    })
})

describe("Mirabuf Physics Loading", () => {
    test("Body Loading (Dozer)", () => {
        const assembly = LoadMirabufLocal(
            "./public/Downloadables/Mira/Robots/Dozer_v9.mira"
        )
        const parser = new MirabufParser(assembly)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.CreateBodiesFromParser(
            parser,
            new LayerReserve()
        )

        expect(mapping.size).toBe(7)
    })

    test("Body Loading (Team_2471_(2018)_v7.mira)", () => {
        const assembly = LoadMirabufLocal(
            "./public/Downloadables/Mira/Robots/Team 2471 (2018)_v7.mira"
        )
        const parser = new MirabufParser(assembly)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.CreateBodiesFromParser(
            parser,
            new LayerReserve()
        )

        expect(mapping.size).toBe(10)
    })
})
