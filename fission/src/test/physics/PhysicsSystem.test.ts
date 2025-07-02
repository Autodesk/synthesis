import { test, expect, describe, assert, beforeEach, afterEach } from "vitest"
import PhysicsSystem, { LayerReserve, BodyAssociate } from "../../systems/physics/PhysicsSystem"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import * as THREE from "three"
import JOLT from "@/util/loading/JoltSyncLoader"
import Jolt from "@azaleacolburn/jolt-physics"

describe("Physics Sanity Checks", () => {
    let system: PhysicsSystem

    beforeEach(() => {
        system = new PhysicsSystem()
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Convex Hull Shape (Cube)", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = system.CreateConvexHull(points)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()

        expect(shape.GetVolume() - 1.0).toBeLessThan(0.001)
        expect(shape.GetCenterOfMass().Length()).toBe(0.0)

        shape.Release()
    })

    test("Convex Hull Shape (Tetrahedron)", () => {
        const points: Float32Array = new Float32Array([0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0])

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
    })

    test("Convex Hull Shape with Custom Density", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const density = 2.5
        const shapeResult = system.CreateConvexHull(points, density)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()
        expect(shape.GetMassProperties().mMass).toBeCloseTo(density, 2)

        shape.Release()
    })
})

describe("Shape Creation Edge Cases", () => {
    let system: PhysicsSystem

    beforeEach(() => {
        system = new PhysicsSystem()
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Convex Hull Shape (Invalid Points)", () => {
        const points: Float32Array = new Float32Array([0.0, 0.0, 0.0]) // Only one point

        const shapeResult = system.CreateConvexHull(points)

        expect(shapeResult.HasError()).toBe(true)
    })

    test("Convex Hull with Invalid Point Count", () => {
        expect(() => {
            system.CreateConvexHull(new Float32Array([1, 2])) // Not divisible by 3
        }).toThrow("Invalid size of points: 2")
    })

    test("Convex Hull with Zero Density", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = system.CreateConvexHull(points, 0.0)

        expect(shapeResult.IsValid()).toBe(true)
        const shape = shapeResult.Get()
        expect(shape.GetMassProperties().mMass).toBe(0.0)

        shape.Release()
    })

    test("Box with Zero Extents", () => {
        const body = system.CreateBox(new THREE.Vector3(0, 0, 0), 1.0, undefined, undefined)
        system.AddBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(system.IsBodyAdded(body.GetID())).toBe(true)
    })

    test("Box with Negative Mass", () => {
        const body = system.CreateBox(new THREE.Vector3(1, 1, 1), -1.0, undefined, undefined)
        system.AddBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        // Body should still be created but mass should be handled appropriately
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
    })
})

describe("Body Creation and Management", () => {
    let system: PhysicsSystem

    beforeEach(() => {
        system = new PhysicsSystem()
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Create Static Box", () => {
        const halfExtents = new THREE.Vector3(1, 2, 3)
        const position = new THREE.Vector3(5, 10, 15)

        const body = system.CreateBox(halfExtents, undefined, position, undefined)
        system.AddBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(system.IsBodyAdded(body.GetID())).toBe(true)

        const bodyPosition = body.GetPosition()
        expect(bodyPosition.GetX()).toBeCloseTo(5, 2)
        expect(bodyPosition.GetY()).toBeCloseTo(10, 2)
        expect(bodyPosition.GetZ()).toBeCloseTo(15, 2)
    })

    test("Create Dynamic Box with Mass", () => {
        const halfExtents = new THREE.Vector3(0.5, 0.5, 0.5)
        const mass = 10.0

        const body = system.CreateBox(halfExtents, mass, undefined, undefined)
        system.AddBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
        expect(body.GetMotionProperties().GetInverseMass()).toBeCloseTo(1.0 / mass, 2)

        // Test that different masses produce different inverse masses
        const mass2 = 5.0
        const body2 = system.CreateBox(halfExtents, mass2, undefined, undefined)
        system.AddBodyToSystem(body2.GetID(), false)

        expect(body2.GetMotionProperties().GetInverseMass()).toBeCloseTo(1.0 / mass2, 2)
        expect(body.GetMotionProperties().GetInverseMass()).not.toBeCloseTo(
            body2.GetMotionProperties().GetInverseMass(),
            2
        )
    })

    test("Create Box with Rotation", () => {
        const halfExtents = new THREE.Vector3(1, 1, 1)
        const rotation = new THREE.Euler(Math.PI / 4, 0, 0)

        const body = system.CreateBox(halfExtents, 1.0, undefined, rotation)
        system.AddBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        const bodyRotation = body.GetRotation()
        expect(bodyRotation.GetW()).toBeCloseTo(Math.cos(Math.PI / 8), 2)
    })

    test("Create Body with Custom Shape", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = system.CreateConvexHull(points)
        const shape = shapeResult.Get()
        const mass = 5.0

        const body = system.CreateBody(shape, mass, undefined, undefined)

        expect(body).toBeDefined()
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
        expect(system.IsBodyAdded(body.GetID())).toBe(false) // Not added to system yet

        system.AddBodyToSystem(body.GetID(), true)
        expect(system.IsBodyAdded(body.GetID())).toBe(true)

        shape.Release()
    })
})

describe("Body Position and Rotation Manipulation", () => {
    let system: PhysicsSystem
    let body: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        body = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        system.AddBodyToSystem(body.GetID(), false)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Set Body Position", () => {
        const newPosition = new JOLT.RVec3(10, 20, 30)

        system.SetBodyPosition(body.GetID(), newPosition)

        const bodyPosition = body.GetPosition()
        expect(bodyPosition.GetX()).toBeCloseTo(10, 2)
        expect(bodyPosition.GetY()).toBeCloseTo(20, 2)
        expect(bodyPosition.GetZ()).toBeCloseTo(30, 2)

        JOLT.destroy(newPosition)
    })

    test("Set Body Rotation", () => {
        const newRotation = new JOLT.Quat(0, 0, Math.sin(Math.PI / 8), Math.cos(Math.PI / 8))

        system.SetBodyRotation(body.GetID(), newRotation)

        const bodyRotation = body.GetRotation()
        expect(bodyRotation.GetZ()).toBeCloseTo(Math.sin(Math.PI / 8), 2)
        expect(bodyRotation.GetW()).toBeCloseTo(Math.cos(Math.PI / 8), 2)

        JOLT.destroy(newRotation)
    })

    test("Set Body Position and Rotation", () => {
        const newPosition = new JOLT.RVec3(5, 10, 15)
        const newRotation = new JOLT.Quat(0, 0, 0, 1)

        system.SetBodyPositionAndRotation(body.GetID(), newPosition, newRotation)

        const bodyPosition = body.GetPosition()
        const bodyRotation = body.GetRotation()

        expect(bodyPosition.GetX()).toBeCloseTo(5, 2)
        expect(bodyPosition.GetY()).toBeCloseTo(10, 2)
        expect(bodyPosition.GetZ()).toBeCloseTo(15, 2)
        expect(bodyRotation.GetW()).toBeCloseTo(1, 2)

        JOLT.destroy(newPosition)
        JOLT.destroy(newRotation)
    })

    test("Set Body Position Rotation and Velocity", () => {
        const newPosition = new JOLT.RVec3(1, 2, 3)
        const newRotation = new JOLT.Quat(0, 0, 0, 1)
        const linearVel = new JOLT.Vec3(5, 0, 0)
        const angularVel = new JOLT.Vec3(0, 1, 0)

        system.SetBodyPositionRotationAndVelocity(body.GetID(), newPosition, newRotation, linearVel, angularVel)

        const bodyLinearVel = body.GetLinearVelocity()
        const bodyAngularVel = body.GetAngularVelocity()

        expect(bodyLinearVel.GetX()).toBeCloseTo(5, 2)
        expect(bodyAngularVel.GetY()).toBeCloseTo(1, 2)

        JOLT.destroy(newPosition)
        JOLT.destroy(newRotation)
        JOLT.destroy(linearVel)
        JOLT.destroy(angularVel)
    })

    test("Set Body Position on Non-Added Body", () => {
        const nonAddedBody = system.CreateBody(
            system.CreateConvexHull(new Float32Array([0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1])).Get(),
            1.0,
            undefined,
            undefined
        )
        const newPosition = new JOLT.RVec3(10, 20, 30)

        // Should not throw error, but also should not affect position since body is not added
        system.SetBodyPosition(nonAddedBody.GetID(), newPosition)
        system.AddBodyToSystem(nonAddedBody.GetID(), false)

        expect(nonAddedBody.GetPosition().GetX()).toBeCloseTo(0, 2)
        expect(nonAddedBody.GetPosition().GetY()).toBeCloseTo(0, 2)
        expect(nonAddedBody.GetPosition().GetZ()).toBeCloseTo(0, 2)

        JOLT.destroy(newPosition)
    })
})

describe("Physics Enable/Disable", () => {
    let system: PhysicsSystem
    let body: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        body = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        system.AddBodyToSystem(body.GetID(), false)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Disable Physics for Body", () => {
        system.DisablePhysicsForBody(body.GetID())

        expect(body.IsSensor()).toBe(true)
        expect(body.IsActive()).toBe(false)
    })

    test("Enable Physics for Body", () => {
        system.DisablePhysicsForBody(body.GetID())
        system.EnablePhysicsForBody(body.GetID())

        expect(body.IsSensor()).toBe(false)
        expect(body.IsActive()).toBe(true)
    })

    test("Disable Physics on Non-Added Body", () => {
        const nonAddedBody = system.CreateBody(
            system.CreateConvexHull(new Float32Array([0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1])).Get(),
            1.0,
            undefined,
            undefined
        )

        // Should not throw error
        system.DisablePhysicsForBody(nonAddedBody.GetID())
        system.EnablePhysicsForBody(nonAddedBody.GetID())
    })
})

describe("Pause System", () => {
    let system: PhysicsSystem

    beforeEach(() => {
        system = new PhysicsSystem()
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Hold and Release Pause", () => {
        expect(system.isPaused).toBe(false)

        system.HoldPause("test-pause")
        expect(system.isPaused).toBe(true)

        const released = system.ReleasePause("test-pause")
        expect(released).toBe(true)
        expect(system.isPaused).toBe(false)
    })

    test("Multiple Pause References", () => {
        system.HoldPause("pause1")
        system.HoldPause("pause2")
        expect(system.isPaused).toBe(true)

        system.ReleasePause("pause1")
        expect(system.isPaused).toBe(true) // Still paused due to pause2

        system.ReleasePause("pause2")
        expect(system.isPaused).toBe(false)
    })

    test("Release Non-Existent Pause", () => {
        const released = system.ReleasePause("non-existent")
        expect(released).toBe(false)
    })

    test("Force Unpause", () => {
        system.HoldPause("pause1")
        system.HoldPause("pause2")
        expect(system.isPaused).toBe(true)

        system.ForceUnpause()
        expect(system.isPaused).toBe(false)
    })
})

describe("Raycast System", () => {
    let system: PhysicsSystem
    let targetBody: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        targetBody = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, new THREE.Vector3(0, 5, 0), undefined)
        system.AddBodyToSystem(targetBody.GetID(), false)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Successful Raycast Hit", () => {
        const from = new JOLT.Vec3(0, 0, 0)
        const direction = new JOLT.Vec3(0, 10, 0) // Ray pointing up

        const hit = system.RayCast(from, direction)

        expect(hit).toBeDefined()
        expect(hit!.point.GetY()).toBeGreaterThan(0)
        expect(hit!.point.GetY()).toBeLessThan(6)

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })

    test("Raycast Miss", () => {
        const from = new JOLT.Vec3(10, 0, 0)
        const direction = new JOLT.Vec3(0, 5, 0) // Ray pointing up but offset

        const hit = system.RayCast(from, direction)

        expect(hit).toBeUndefined()

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })

    test("Raycast with Ignored Bodies", () => {
        const from = new JOLT.Vec3(0, 0, 0)
        const direction = new JOLT.Vec3(0, 10, 0)

        const hit = system.RayCast(from, direction, targetBody.GetID())

        expect(hit).toBeUndefined() // Should miss because target body is ignored

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })
})

describe("Sensor Creation", () => {
    let system: PhysicsSystem

    beforeEach(() => {
        system = new PhysicsSystem()
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Create Valid Sensor", () => {
        const size = new JOLT.Vec3(1, 1, 1)
        const shapeSettings = new JOLT.BoxShapeSettings(size)

        const sensorId = system.CreateSensor(shapeSettings)

        expect(sensorId).toBeDefined()
        expect(system.IsBodyAdded(sensorId!)).toBe(true)

        const sensorBody = system.GetBody(sensorId!)
        expect(sensorBody.IsSensor()).toBe(true)

        JOLT.destroy(size)
        JOLT.destroy(shapeSettings)
    })

    test("Create Invalid Sensor", () => {
        // Temporarily suppress console.error for this test since we expect an error
        const originalConsoleError = console.error
        console.error = () => {} // Suppress error output

        try {
            // Create invalid shape settings that will fail
            const shapeSettings = new JOLT.ConvexHullShapeSettings()
            // Don't add any points - this should make it invalid

            const sensorId = system.CreateSensor(shapeSettings)

            expect(sensorId).toBeUndefined()

            JOLT.destroy(shapeSettings)
        } finally {
            // Always restore console.error
            console.error = originalConsoleError
        }
    })
})

describe("Body Associations", () => {
    let system: PhysicsSystem
    let body: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        body = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Set and Get Body Association", () => {
        const association = new BodyAssociate(body.GetID())

        system.SetBodyAssociation(association)

        const retrieved = system.GetBodyAssociation(body.GetID())
        expect(retrieved).toBe(association)
    })

    test("Remove Body Association", () => {
        const association = new BodyAssociate(body.GetID())
        system.SetBodyAssociation(association)

        system.RemoveBodyAssociation(body.GetID())

        const retrieved = system.GetBodyAssociation(body.GetID())
        expect(retrieved).toBeUndefined()
    })

    test("Get Non-Existent Association", () => {
        const retrieved = system.GetBodyAssociation(body.GetID())
        expect(retrieved).toBeUndefined()
    })
})

describe("Layer Reserve System", () => {
    test("Layer Reserve Creation", () => {
        const reserve = new LayerReserve()

        // Layer numbers are 2-9 are reserved for robots
        expect(reserve.layer).toBeGreaterThanOrEqual(2)
        expect(reserve.layer).toBeLessThanOrEqual(9)
        expect(reserve.isReleased).toBe(false)
    })

    test("Layer Reserve Release", () => {
        const reserve = new LayerReserve()
        const originalLayer = reserve.layer

        reserve.Release()

        expect(reserve.isReleased).toBe(true)
        expect(reserve.layer).toBe(originalLayer) // Layer number should remain the same
    })

    test("Multiple Layer Reserve Release", () => {
        const reserve = new LayerReserve()

        reserve.Release()
        reserve.Release() // Should not cause issues

        expect(reserve.isReleased).toBe(true)
    })

    test("Layer Reserve Uniqueness", () => {
        const reserve1 = new LayerReserve()
        const reserve2 = new LayerReserve()

        expect(reserve1.layer).not.toBe(reserve2.layer)

        reserve1.Release()
        reserve2.Release()
    })
})

describe("Body Cleanup", () => {
    let system: PhysicsSystem
    let body1: Jolt.Body
    let body2: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        body1 = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        body2 = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        system.AddBodyToSystem(body1.GetID(), false)
        system.AddBodyToSystem(body2.GetID(), false)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Destroy Single Body", () => {
        const bodyId = body1.GetID()

        expect(system.IsBodyAdded(bodyId)).toBe(true)

        system.DestroyBodies(body1)

        expect(system.IsBodyAdded(bodyId)).toBe(false)
    })

    test("Destroy Multiple Bodies", () => {
        system.DestroyBodies(body1, body2)

        expect(system.IsBodyAdded(body1.GetID())).toBe(false)
        expect(system.IsBodyAdded(body2.GetID())).toBe(false)
    })

    test("Destroy Bodies by ID", () => {
        const id1 = body1.GetID()
        const id2 = body2.GetID()

        system.DestroyBodyIds(id1, id2)

        expect(system.IsBodyAdded(id1)).toBe(false)
        expect(system.IsBodyAdded(id2)).toBe(false)
    })
})

describe("Update Loop", () => {
    let system: PhysicsSystem
    let body: Jolt.Body

    beforeEach(() => {
        system = new PhysicsSystem()
        body = system.CreateBox(new THREE.Vector3(1, 1, 1), 1.0, new THREE.Vector3(0, 10, 0), undefined)
        system.AddBodyToSystem(body.GetID(), true)
    })

    afterEach(() => {
        system.Destroy()
    })

    test("Update with Normal Delta Time", () => {
        const initialPos = body.GetPosition()
        const initialPosition = new JOLT.RVec3(initialPos.GetX(), initialPos.GetY(), initialPos.GetZ())

        // Run several update steps
        for (let i = 0; i < 10; i++) {
            system.Update(1 / 60) // 60 FPS
        }

        const finalPosition = body.GetPosition()

        // Body should have fallen due to gravity
        expect(finalPosition.GetY()).toBeLessThan(initialPosition.GetY())

        JOLT.destroy(initialPosition)
    })

    test("Update While Paused", () => {
        const initialPos = body.GetPosition()
        const initialPosition = new JOLT.RVec3(initialPos.GetX(), initialPos.GetY(), initialPos.GetZ())

        system.HoldPause("test-pause")

        // Run update steps while paused
        for (let i = 0; i < 10; i++) {
            system.Update(1 / 60)
        }

        const finalPosition = body.GetPosition()

        // Body should not have moved while paused
        expect(finalPosition.GetY()).toBeCloseTo(initialPosition.GetY(), 2)

        JOLT.destroy(initialPosition)
    })

    test("Update with Large Delta Time", () => {
        // Should not crash or cause issues
        system.Update(10.0) // Very large delta time

        expect(body.GetPosition().GetY()).toBeLessThan(10)
    })

    test("Update with Very Small Delta Time", () => {
        // Should not crash or cause issues
        system.Update(0.001) // Very small delta time

        expect(body.GetPosition().GetY()).toBeLessThanOrEqual(10)
    })
})

describe("Mirabuf Physics Loading", () => {
    test("Body Loading (Dozer)", async () => {
        const assembly = await MirabufCachingService.CacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT).then(
            x => MirabufCachingService.Get(x!.id, MiraType.ROBOT)
        )
        const parser = new MirabufParser(assembly!)
        const physSystem = new PhysicsSystem()
        const mapping = physSystem.CreateBodiesFromParser(parser, new LayerReserve())

        expect(mapping.size).toBe(7)
    })
})
