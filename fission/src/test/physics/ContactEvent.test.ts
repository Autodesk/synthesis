import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    OnContactAddedEvent,
    OnContactPersistedEvent,
    OnContactRemovedEvent,
    OnContactValidateEvent,
} from "../../systems/physics/ContactEvents"
import PhysicsSystem from "../../systems/physics/PhysicsSystem"

describe("Contact Event Integration Tests", () => {
    let physicsSystem: PhysicsSystem
    let groundBody: Jolt.Body
    let fallingBody: Jolt.Body

    // Event tracking variables
    let contactAddedEvents: OnContactAddedEvent[] = []
    let contactPersistedEvents: OnContactPersistedEvent[] = []
    let contactRemovedEvents: OnContactRemovedEvent[] = []
    let contactValidateEvents: OnContactValidateEvent[] = []

    // Event listeners
    const onContactAdded = (e: OnContactAddedEvent) => {
        contactAddedEvents.push(e)
    }

    const onContactPersisted = (e: OnContactPersistedEvent) => {
        contactPersistedEvents.push(e)
    }

    const onContactRemoved = (e: OnContactRemovedEvent) => {
        contactRemovedEvents.push(e)
    }

    const onContactValidate = (e: OnContactValidateEvent) => {
        contactValidateEvents.push(e)
    }

    beforeEach(() => {
        // Clear event arrays
        contactAddedEvents = []
        contactPersistedEvents = []
        contactRemovedEvents = []
        contactValidateEvents = []

        // Set up physics system
        physicsSystem = new PhysicsSystem()

        // Create a static ground body
        groundBody = physicsSystem.createBox(
            new THREE.Vector3(10, 0.5, 10), // Large flat ground
            undefined, // No mass (static)
            new THREE.Vector3(0, -1, 0), // Position below origin
            undefined // No rotation
        )
        physicsSystem.addBodyToSystem(groundBody.GetID(), false)

        // Create a dynamic falling body
        fallingBody = physicsSystem.createBox(
            new THREE.Vector3(1, 1, 1), // 1x1x1 cube
            1.0, // 1kg mass
            new THREE.Vector3(0, 10, 0), // Start 10 units above ground
            undefined // No rotation
        )
        physicsSystem.addBodyToSystem(fallingBody.GetID(), true)

        // Add event listeners
        OnContactAddedEvent.addListener(onContactAdded)
        OnContactPersistedEvent.addListener(onContactPersisted)
        OnContactRemovedEvent.addListener(onContactRemoved)
        OnContactValidateEvent.addListener(onContactValidate)
    })

    afterEach(() => {
        // Remove event listeners
        OnContactAddedEvent.removeListener(onContactAdded)
        OnContactPersistedEvent.removeListener(onContactPersisted)
        OnContactRemovedEvent.removeListener(onContactRemoved)
        OnContactValidateEvent.removeListener(onContactValidate)

        // Clean up physics system
        physicsSystem.destroy()
    })

    test("Falling body actually moves downward", async () => {
        const initialPosition = fallingBody.GetPosition()
        const initialY = initialPosition.GetY()

        // Run simulation for a bit
        for (let i = 0; i < 60; i++) {
            // 1 second at 60 FPS
            physicsSystem.update(1 / 60)
        }

        const finalPosition = fallingBody.GetPosition()
        const finalY = finalPosition.GetY()

        // Body should have fallen due to gravity
        expect(finalY).toBeLessThan(initialY)
    })

    test("Contact events are fired when objects collide", async () => {
        // Verify initial state - no contacts yet
        expect(contactAddedEvents).toHaveLength(0)
        expect(contactPersistedEvents).toHaveLength(0)
        expect(contactRemovedEvents).toHaveLength(0)

        // Run physics simulation until collision occurs
        let simulationSteps = 0
        const maxSteps = 300 // Prevent infinite loop, usually takes 82 steps
        const deltaTime = 1 / 60 // 60 FPS

        while (simulationSteps < maxSteps && contactAddedEvents.length === 0) {
            physicsSystem.update(deltaTime)
            simulationSteps++
        }

        // Verify that collision occurred and contact added event was fired
        expect(contactAddedEvents.length).toBeGreaterThan(0)
        expect(simulationSteps).toBeLessThan(maxSteps) // Should not hit max steps

        // Verify the contact data is valid
        const contactEvent = contactAddedEvents[0]
        expect(contactEvent.message.body1).toBeDefined()
        expect(contactEvent.message.body2).toBeDefined()
        expect(contactEvent.message.manifold).toBeDefined()
        expect(contactEvent.message.settings).toBeDefined()

        // The main test is that we got a contact event - this proves collision detection works
        expect(contactEvent.type).toBe("OnContactAddedEvent")
    })

    test("Contact persisted events are fired for ongoing collisions", async () => {
        // Run simulation until collision starts
        let simulationSteps = 0
        const maxSteps = 300
        const deltaTime = 1 / 60

        // Wait for initial contact (Usually around 82 steps)
        while (simulationSteps < maxSteps && contactAddedEvents.length === 0) {
            physicsSystem.update(deltaTime)
            simulationSteps++
        }

        expect(contactAddedEvents.length).toBeGreaterThan(0)

        // Continue simulation to get persisted events
        const additionalSteps = 30 // Run for 0.5 seconds after contact
        for (let i = 0; i < additionalSteps; i++) {
            physicsSystem.update(deltaTime)
        }

        // Should have persisted contact events since the box is resting on ground
        expect(contactPersistedEvents.length).toBeGreaterThan(0)

        // Verify persisted event data
        const persistedEvent = contactPersistedEvents[0]
        expect(persistedEvent.message.body1).toBeDefined()
        expect(persistedEvent.message.body2).toBeDefined()
        expect(persistedEvent.message.manifold).toBeDefined()
        expect(persistedEvent.message.settings).toBeDefined()
    })

    test("Multiple collisions generate multiple contact events", async () => {
        // Create a second falling body
        const secondFallingBody = physicsSystem.createBox(
            new THREE.Vector3(1, 1, 1),
            1.0,
            new THREE.Vector3(5, 15, 0), // Different X position, higher up
            undefined
        )
        physicsSystem.addBodyToSystem(secondFallingBody.GetID(), true)

        // Run simulation until both bodies collide with ground
        let simulationSteps = 0
        const maxSteps = 300
        const deltaTime = 1 / 60

        while (simulationSteps < maxSteps) {
            physicsSystem.update(deltaTime)
            simulationSteps++

            // Wait until we have at least 2 contact events (both bodies hit ground)
            if (contactAddedEvents.length >= 2) {
                break
            }
        }

        expect(contactAddedEvents.length).toBeGreaterThanOrEqual(2)

        // Clean up the additional body
        physicsSystem.destroyBodies(secondFallingBody)
    })

    test("Contact removed events are fired when objects stop colliding", async () => {
        // Run simulation until both bodies collide with ground
        let simulationSteps = 0
        const maxSteps = 300
        const deltaTime = 1 / 60

        while (simulationSteps < maxSteps && contactAddedEvents.length === 0) {
            physicsSystem.update(deltaTime)
            simulationSteps++
        }

        expect(contactAddedEvents.length).toBeGreaterThan(0)

        fallingBody.AddForce(new JOLT.Vec3(100, 100, 0))

        // Run simulation for a bit longer
        const additionalSteps = 30
        for (let i = 0; i < additionalSteps; i++) {
            physicsSystem.update(deltaTime)
        }

        expect(contactRemovedEvents.length).toBeGreaterThan(0)
    })

    test("Contact validate events are fired when objects are colliding", async () => {
        // Run simulation until collision occurs
        let simulationSteps = 0
        const maxSteps = 300 // Prevent infinite loop, usually takes 82 steps
        const deltaTime = 1 / 60 // 60 FPS

        while (simulationSteps < maxSteps && contactValidateEvents.length === 0) {
            physicsSystem.update(deltaTime)
            simulationSteps++
        }

        expect(contactValidateEvents.length).toBeGreaterThan(0)
    })
})
