import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { afterEach, assert, beforeEach, describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import PhysicsSystem, { BodyAssociate, LayerReserve } from "../../systems/physics/PhysicsSystem"

describe("Physics Sanity Checks", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Convex Hull Shape (Cube)", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = PhysicsSystem.createConvexHull(points)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()

        expect(shape.GetVolume() - 1.0).toBeLessThan(0.001)
        expect(shape.GetCenterOfMass().Length()).toBe(0.0)

        shape.Release()
    })

    test("Convex Hull Shape (Tetrahedron)", () => {
        const points: Float32Array = new Float32Array([0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0])

        const shapeResult = PhysicsSystem.createConvexHull(points)

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
        const shapeResult = PhysicsSystem.createConvexHull(points, density)

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str())
        expect(shapeResult.IsValid()).toBe(true)

        const shape = shapeResult.Get()
        expect(shape.GetMassProperties().mMass).toBeCloseTo(density, 2)

        shape.Release()
    })
})

describe("Shape Creation Edge Cases", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Convex Hull Shape (Invalid Points)", () => {
        const points: Float32Array = new Float32Array([0.0, 0.0, 0.0]) // Only one point

        const shapeResult = PhysicsSystem.createConvexHull(points)

        expect(shapeResult.HasError()).toBe(true)
    })

    test("Convex Hull with Invalid Point Count", () => {
        expect(() => {
            PhysicsSystem.createConvexHull(new Float32Array([1, 2])) // Not divisible by 3
        }).toThrow("Invalid size of points: 2")
    })

    test("Convex Hull with Zero Density", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = PhysicsSystem.createConvexHull(points, 0.0)

        expect(shapeResult.IsValid()).toBe(true)
        const shape = shapeResult.Get()
        expect(shape.GetMassProperties().mMass).toBe(0.0)

        shape.Release()
    })

    test("Box with Zero Extents", () => {
        const body = PhysicsSystem.createBox(new THREE.Vector3(0, 0, 0), 1.0, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(PhysicsSystem.isBodyAdded(body.GetID())).toBe(true)
    })

    test("Box with Negative Mass", () => {
        const body = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), -1.0, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        // Body should still be created but mass should be handled appropriately
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
    })
})

describe("Body Creation and Management", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Create Static Box", () => {
        const halfExtents = new THREE.Vector3(1, 2, 3)
        const position = new THREE.Vector3(5, 10, 15)

        const body = PhysicsSystem.createBox(halfExtents, undefined, position, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(PhysicsSystem.isBodyAdded(body.GetID())).toBe(true)

        const bodyPosition = body.GetPosition()
        expect(bodyPosition.GetX()).toBeCloseTo(5, 2)
        expect(bodyPosition.GetY()).toBeCloseTo(10, 2)
        expect(bodyPosition.GetZ()).toBeCloseTo(15, 2)
    })

    test("Create Dynamic Box with Mass", () => {
        const halfExtents = new THREE.Vector3(0.5, 0.5, 0.5)
        const mass = 10.0

        const body = PhysicsSystem.createBox(halfExtents, mass, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
        expect(body.GetMotionProperties().GetInverseMass()).toBeCloseTo(1.0 / mass, 2)

        // Test that different masses produce different inverse masses
        const mass2 = 5.0
        const body2 = PhysicsSystem.createBox(halfExtents, mass2, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body2.GetID(), false)

        expect(body2.GetMotionProperties().GetInverseMass()).toBeCloseTo(1.0 / mass2, 2)
        expect(body.GetMotionProperties().GetInverseMass()).not.toBeCloseTo(
            body2.GetMotionProperties().GetInverseMass(),
            2
        )
    })

    test("Create Box with Rotation", () => {
        const halfExtents = new THREE.Vector3(1, 1, 1)
        const rotation = new THREE.Euler(Math.PI / 4, 0, 0)

        const body = PhysicsSystem.createBox(halfExtents, 1.0, undefined, rotation)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)

        expect(body).toBeDefined()
        const bodyRotation = body.GetRotation()
        expect(bodyRotation.GetW()).toBeCloseTo(Math.cos(Math.PI / 8), 2)
    })

    test("Create Body with Custom Shape", () => {
        const points: Float32Array = new Float32Array([
            0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
            0.5, -0.5, 0.5, 0.5, -0.5,
        ])

        const shapeResult = PhysicsSystem.createConvexHull(points)
        const shape = shapeResult.Get()
        const mass = 5.0

        const body = PhysicsSystem.createBody(shape, mass, undefined, undefined)

        expect(body).toBeDefined()
        expect(body.GetMotionType()).toBe(JOLT.EMotionType_Dynamic)
        expect(PhysicsSystem.isBodyAdded(body.GetID())).toBe(false) // Not added to system yet

        PhysicsSystem.addBodyToSystem(body.GetID(), true)
        expect(PhysicsSystem.isBodyAdded(body.GetID())).toBe(true)

        shape.Release()
    })
})

describe("Body Position and Rotation Manipulation", () => {
    let body: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.setup()
        body = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Set Body Position", () => {
        const newPosition = new JOLT.RVec3(10, 20, 30)

        PhysicsSystem.setBodyPosition(body.GetID(), newPosition)

        const bodyPosition = body.GetPosition()
        expect(bodyPosition.GetX()).toBeCloseTo(10, 2)
        expect(bodyPosition.GetY()).toBeCloseTo(20, 2)
        expect(bodyPosition.GetZ()).toBeCloseTo(30, 2)

        JOLT.destroy(newPosition)
    })

    test("Set Body Rotation", () => {
        const newRotation = new JOLT.Quat(0, 0, Math.sin(Math.PI / 8), Math.cos(Math.PI / 8))

        PhysicsSystem.setBodyRotation(body.GetID(), newRotation)

        const bodyRotation = body.GetRotation()
        expect(bodyRotation.GetZ()).toBeCloseTo(Math.sin(Math.PI / 8), 2)
        expect(bodyRotation.GetW()).toBeCloseTo(Math.cos(Math.PI / 8), 2)

        JOLT.destroy(newRotation)
    })

    test("Set Body Position and Rotation", () => {
        const newPosition = new JOLT.RVec3(5, 10, 15)
        const newRotation = new JOLT.Quat(0, 0, 0, 1)

        PhysicsSystem.setBodyPositionAndRotation(body.GetID(), newPosition, newRotation)

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

        PhysicsSystem.setBodyPositionRotationAndVelocity(body.GetID(), newPosition, newRotation, linearVel, angularVel)

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
        const nonAddedBody = PhysicsSystem.createBody(
            PhysicsSystem.createConvexHull(new Float32Array([0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1])).Get(),
            1.0,
            undefined,
            undefined
        )
        const newPosition = new JOLT.RVec3(10, 20, 30)

        // Should not throw error, but also should not affect position since body is not added
        PhysicsSystem.setBodyPosition(nonAddedBody.GetID(), newPosition)
        PhysicsSystem.addBodyToSystem(nonAddedBody.GetID(), false)

        expect(nonAddedBody.GetPosition().GetX()).toBeCloseTo(0, 2)
        expect(nonAddedBody.GetPosition().GetY()).toBeCloseTo(0, 2)
        expect(nonAddedBody.GetPosition().GetZ()).toBeCloseTo(0, 2)

        JOLT.destroy(newPosition)
    })
})

describe("Physics Enable/Disable", () => {
    let body: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.setup()
        body = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), false)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Disable Physics for Body", () => {
        PhysicsSystem.disablePhysicsForBody(body.GetID())

        expect(body.IsSensor()).toBe(true)
        expect(body.IsActive()).toBe(false)
    })

    test("Enable Physics for Body", () => {
        PhysicsSystem.disablePhysicsForBody(body.GetID())
        PhysicsSystem.enablePhysicsForBody(body.GetID())

        expect(body.IsSensor()).toBe(false)
        expect(body.IsActive()).toBe(true)
    })

    test("Disable Physics on Non-Added Body", () => {
        const nonAddedBody = PhysicsSystem.createBody(
            PhysicsSystem.createConvexHull(new Float32Array([0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1])).Get(),
            1.0,
            undefined,
            undefined
        )

        // Should not throw error
        PhysicsSystem.disablePhysicsForBody(nonAddedBody.GetID())
        PhysicsSystem.enablePhysicsForBody(nonAddedBody.GetID())
    })
})

describe("Pause System", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Hold and Release Pause", () => {
        expect(PhysicsSystem.isPaused).toBe(false)

        PhysicsSystem.holdPause("test-pause")
        expect(PhysicsSystem.isPaused).toBe(true)

        const released = PhysicsSystem.releasePause("test-pause")
        expect(released).toBe(true)
        expect(PhysicsSystem.isPaused).toBe(false)
    })

    test("Multiple Pause References", () => {
        PhysicsSystem.holdPause("pause1")
        PhysicsSystem.holdPause("pause2")
        expect(PhysicsSystem.isPaused).toBe(true)

        PhysicsSystem.releasePause("pause1")
        expect(PhysicsSystem.isPaused).toBe(true) // Still paused due to pause2

        PhysicsSystem.releasePause("pause2")
        expect(PhysicsSystem.isPaused).toBe(false)
    })

    test("Release Non-Existent Pause", () => {
        const released = PhysicsSystem.releasePause("non-existent")
        expect(released).toBe(false)
    })

    test("Force Unpause", () => {
        PhysicsSystem.holdPause("pause1")
        PhysicsSystem.holdPause("pause2")
        expect(PhysicsSystem.isPaused).toBe(true)

        PhysicsSystem.forceUnpause()
        expect(PhysicsSystem.isPaused).toBe(false)
    })
})

describe("Raycast System", () => {
    let targetBody: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.setup()
        targetBody = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, new THREE.Vector3(0, 5, 0), undefined)
        PhysicsSystem.addBodyToSystem(targetBody.GetID(), false)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Successful Raycast Hit", () => {
        const from = new JOLT.Vec3(0, 0, 0)
        const direction = new JOLT.Vec3(0, 10, 0) // Ray pointing up

        const hit = PhysicsSystem.rayCast(from, direction)

        expect(hit).toBeDefined()
        expect(hit!.point.GetY()).toBeGreaterThan(0)
        expect(hit!.point.GetY()).toBeLessThan(6)

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })

    test("Raycast Miss", () => {
        const from = new JOLT.Vec3(10, 0, 0)
        const direction = new JOLT.Vec3(0, 5, 0) // Ray pointing up but offset

        const hit = PhysicsSystem.rayCast(from, direction)

        expect(hit).toBeUndefined()

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })

    test("Raycast with Ignored Bodies", () => {
        const from = new JOLT.Vec3(0, 0, 0)
        const direction = new JOLT.Vec3(0, 10, 0)

        const hit = PhysicsSystem.rayCast(from, direction, targetBody.GetID())

        expect(hit).toBeUndefined() // Should miss because target body is ignored

        JOLT.destroy(from)
        JOLT.destroy(direction)
    })
})

describe("Sensor Creation", () => {
    beforeEach(() => {
        PhysicsSystem.setup()
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Create Valid Sensor", () => {
        const size = new JOLT.Vec3(1, 1, 1)
        const shapeSettings = new JOLT.BoxShapeSettings(size)

        const sensorId = PhysicsSystem.createSensor(shapeSettings)

        expect(sensorId).toBeDefined()
        expect(PhysicsSystem.isBodyAdded(sensorId!)).toBe(true)

        const sensorBody = PhysicsSystem.getBody(sensorId!)
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

            const sensorId = PhysicsSystem.createSensor(shapeSettings)

            expect(sensorId).toBeUndefined()

            JOLT.destroy(shapeSettings)
        } finally {
            // Always restore console.error
            console.error = originalConsoleError
        }
    })
})

describe("Body Associations", () => {
    let body: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.setup()
        body = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Set and Get Body Association", () => {
        const association = new BodyAssociate(body.GetID())

        PhysicsSystem.setBodyAssociation(association)

        const retrieved = PhysicsSystem.getBodyAssociation(body.GetID())
        expect(retrieved).toBe(association)
    })

    test("Remove Body Association", () => {
        const association = new BodyAssociate(body.GetID())
        PhysicsSystem.setBodyAssociation(association)

        PhysicsSystem.removeBodyAssociation(body.GetID())

        const retrieved = PhysicsSystem.getBodyAssociation(body.GetID())
        expect(retrieved).toBeUndefined()
    })

    test("Get Non-Existent Association", () => {
        const retrieved = PhysicsSystem.getBodyAssociation(body.GetID())
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

        reserve.release()

        expect(reserve.isReleased).toBe(true)
        expect(reserve.layer).toBe(originalLayer) // Layer number should remain the same
    })

    test("Multiple Layer Reserve Release", () => {
        const reserve = new LayerReserve()

        reserve.release()
        reserve.release() // Should not cause issues

        expect(reserve.isReleased).toBe(true)
    })

    test("Layer Reserve Uniqueness", () => {
        const reserve1 = new LayerReserve()
        const reserve2 = new LayerReserve()

        expect(reserve1.layer).not.toBe(reserve2.layer)

        reserve1.release()
        reserve2.release()
    })
})

describe("Body Cleanup", () => {
    let body1: Jolt.Body
    let body2: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.setup()
        body1 = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        body2 = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
        PhysicsSystem.addBodyToSystem(body1.GetID(), false)
        PhysicsSystem.addBodyToSystem(body2.GetID(), false)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Destroy Single Body", () => {
        const bodyId = body1.GetID()

        expect(PhysicsSystem.isBodyAdded(bodyId)).toBe(true)

        PhysicsSystem.destroyBodies(body1)

        expect(PhysicsSystem.isBodyAdded(bodyId)).toBe(false)
    })

    test("Destroy Multiple Bodies", () => {
        PhysicsSystem.destroyBodies(body1, body2)

        expect(PhysicsSystem.isBodyAdded(body1.GetID())).toBe(false)
        expect(PhysicsSystem.isBodyAdded(body2.GetID())).toBe(false)
    })

    test("Destroy Bodies by ID", () => {
        const id1 = body1.GetID()
        const id2 = body2.GetID()

        PhysicsSystem.destroyBodyIds(id1, id2)

        expect(PhysicsSystem.isBodyAdded(id1)).toBe(false)
        expect(PhysicsSystem.isBodyAdded(id2)).toBe(false)
    })
})

describe("Update Loop", () => {
    let body: Jolt.Body

    beforeEach(() => {
        PhysicsSystem.destroy()
        PhysicsSystem.setup()
        body = PhysicsSystem.createBox(new THREE.Vector3(1, 1, 1), 1.0, new THREE.Vector3(0, 10, 0), undefined)
        PhysicsSystem.addBodyToSystem(body.GetID(), true)
    })

    afterEach(() => {
        PhysicsSystem.destroy()
    })

    test("Update with Normal Delta Time", () => {
        const initialPos = body.GetPosition()
        const initialPosition = new JOLT.RVec3(initialPos.GetX(), initialPos.GetY(), initialPos.GetZ())
        // Run several update steps
        for (let i = 0; i < 10; i++) {
            PhysicsSystem.update(1 / 60) // 60 FPS
        }

        const finalPosition = body.GetPosition()

        // Body should have fallen due to gravity
        expect(finalPosition.GetY()).toBeLessThan(initialPosition.GetY())

        JOLT.destroy(initialPosition)
    })

    test("Update While Paused", () => {
        const initialPos = body.GetPosition()
        const initialPosition = new JOLT.RVec3(initialPos.GetX(), initialPos.GetY(), initialPos.GetZ())

        PhysicsSystem.holdPause("test-pause")

        // Run update steps while paused
        for (let i = 0; i < 10; i++) {
            PhysicsSystem.update(1 / 60)
        }

        const finalPosition = body.GetPosition()

        // Body should not have moved while paused
        expect(finalPosition.GetY()).toBeCloseTo(initialPosition.GetY(), 2)

        JOLT.destroy(initialPosition)
    })

    test("Update with Large Delta Time", () => {
        // Should not crash or cause issues
        PhysicsSystem.update(10) // Very large delta time

        expect(body.GetPosition().GetY()).toBeLessThan(10)
    })

    test("Update with Very Small Delta Time", () => {
        // Should not crash or cause issues
        PhysicsSystem.update(0.001) // Very small delta time

        expect(body.GetPosition().GetY()).toBeLessThanOrEqual(10)
    })
})
