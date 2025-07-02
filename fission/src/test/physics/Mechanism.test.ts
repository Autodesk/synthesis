import { test, expect, describe, beforeEach, vi } from "vitest"
import Mechanism, { MechanismConstraint } from "../../systems/physics/Mechanism"
import PhysicsSystem from "../../systems/physics/PhysicsSystem"
import MirabufParser from "@/mirabuf/MirabufParser"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { LayerReserve } from "../../systems/physics/PhysicsSystem"
import { RigidNodeId } from "../../mirabuf/MirabufParser"
import { mirabuf } from "../../proto/mirabuf"
import Jolt from "@azaleacolburn/jolt-physics"

// Mock Jolt types
const createMockBodyID = (index: number = 123): Jolt.BodyID =>
    ({
        GetIndexAndSequenceNumber: vi.fn(() => index),
        GetIndex: vi.fn(() => index),
        GetSequenceNumber: vi.fn(() => 1),
        IsInvalid: vi.fn(() => false),
        Equals: vi.fn(() => false),
    }) as unknown as Jolt.BodyID

const mockBodyID = createMockBodyID(123)
const mockBodyID2 = createMockBodyID(456)
const mockBodyID3 = createMockBodyID(789)

const mockConstraint = {
    Release: vi.fn(),
} as unknown as Jolt.Constraint

const mockStepListener = {
    OnPrePhysicsUpdate: vi.fn(),
    OnPostPhysicsUpdate: vi.fn(),
} as unknown as Jolt.PhysicsStepListener

const mockLayerReserve = {
    layer: 1,
    isReleased: false,
    Release: vi.fn(),
} as unknown as LayerReserve

describe("Mechanism Constructor Tests", () => {
    test("Constructor with minimal parameters", () => {
        const rootBody = "root-body-id"
        const bodyMap = new Map<string, Jolt.BodyID>()
        const controllable = true

        const mechanism = new Mechanism(rootBody, bodyMap, controllable)

        expect(mechanism.rootBody).toBe(rootBody)
        expect(mechanism.nodeToBody).toBe(bodyMap)
        expect(mechanism.controllable).toBe(controllable)
        expect(mechanism.constraints).toEqual([])
        expect(mechanism.stepListeners).toEqual([])
        expect(mechanism.ghostBodies).toEqual([])
        expect(mechanism.layerReserve).toBeUndefined()
    })

    test("Constructor with all parameters", () => {
        const rootBody = "root-body-id"
        const bodyMap = new Map<string, Jolt.BodyID>([
            ["node1", mockBodyID],
            ["node2", mockBodyID],
        ])
        const controllable = false
        const layerReserve = mockLayerReserve

        const mechanism = new Mechanism(rootBody, bodyMap, controllable, layerReserve)

        expect(mechanism.rootBody).toBe(rootBody)
        expect(mechanism.nodeToBody).toBe(bodyMap)
        expect(mechanism.controllable).toBe(controllable)
        expect(mechanism.layerReserve).toBe(layerReserve)
        expect(mechanism.constraints).toEqual([])
        expect(mechanism.stepListeners).toEqual([])
        expect(mechanism.ghostBodies).toEqual([])
    })

    test("Constructor with empty body map", () => {
        const rootBody = "empty-root"
        const bodyMap = new Map<string, Jolt.BodyID>()
        const controllable = true

        const mechanism = new Mechanism(rootBody, bodyMap, controllable)

        expect(mechanism.nodeToBody.size).toBe(0)
        expect(mechanism.GetBodyByNodeId("nonexistent")).toBeUndefined()
    })
})

describe("Mechanism Constraint Management", () => {
    let mechanism: Mechanism
    let mockConstraint1: MechanismConstraint
    let mockConstraint2: MechanismConstraint

    beforeEach(() => {
        mechanism = new Mechanism("root", new Map(), true)

        mockConstraint1 = {
            parentBody: mockBodyID,
            childBody: mockBodyID,
            primaryConstraint: mockConstraint,
            maxVelocity: 30.0,
            extraConstraints: [],
            extraBodies: [],
        }

        mockConstraint2 = {
            parentBody: mockBodyID,
            childBody: mockBodyID,
            primaryConstraint: mockConstraint,
            maxVelocity: 15.0,
            info: { name: "test-joint" } as mirabuf.IInfo,
            extraConstraints: [mockConstraint],
            extraBodies: [mockBodyID],
        }
    })

    test("Add single constraint", () => {
        expect(mechanism.constraints).toHaveLength(0)

        mechanism.AddConstraint(mockConstraint1)

        expect(mechanism.constraints).toHaveLength(1)
        expect(mechanism.constraints[0]).toBe(mockConstraint1)
    })

    test("Constraint with info and extra components", () => {
        mechanism.AddConstraint(mockConstraint2)

        const addedConstraint = mechanism.constraints[0]
        expect(addedConstraint.info?.name).toBe("test-joint")
        expect(addedConstraint.extraConstraints).toHaveLength(1)
        expect(addedConstraint.extraBodies).toHaveLength(1)
        expect(addedConstraint.maxVelocity).toBe(15.0)
    })

    test("Add multiple constraints", () => {
        mechanism.AddConstraint(mockConstraint1)
        mechanism.AddConstraint(mockConstraint2)

        expect(mechanism.constraints).toHaveLength(2)
        expect(mechanism.constraints[0]).toBe(mockConstraint1)
        expect(mechanism.constraints[1]).toBe(mockConstraint2)
    })
})

describe("Mechanism Step Listener Management", () => {
    let mechanism: Mechanism

    beforeEach(() => {
        mechanism = new Mechanism("root", new Map(), true)
    })

    test("Add single step listener", () => {
        expect(mechanism.stepListeners).toHaveLength(0)

        mechanism.AddStepListener(mockStepListener)

        expect(mechanism.stepListeners).toHaveLength(1)
        expect(mechanism.stepListeners[0]).toBe(mockStepListener)
    })

    test("Add multiple step listeners", () => {
        const listener1 = { ...mockStepListener }
        const listener2 = { ...mockStepListener }

        mechanism.AddStepListener(listener1)
        mechanism.AddStepListener(listener2)

        expect(mechanism.stepListeners).toHaveLength(2)
        expect(mechanism.stepListeners[0]).toBe(listener1)
        expect(mechanism.stepListeners[1]).toBe(listener2)
    })
})

describe("Mechanism Body Node Mapping", () => {
    let mechanism: Mechanism
    let bodyMap: Map<RigidNodeId, Jolt.BodyID>

    beforeEach(() => {
        bodyMap = new Map([
            ["node1", mockBodyID],
            ["node2", mockBodyID2],
            ["node3", mockBodyID3],
        ])
        mechanism = new Mechanism("root", bodyMap, true)
    })

    test("Get existing body by node ID", () => {
        const body = mechanism.GetBodyByNodeId("node1")
        expect(body).toBe(mockBodyID)
    })

    test("Get non-existing body by node ID", () => {
        const body = mechanism.GetBodyByNodeId("nonexistent")
        expect(body).toBeUndefined()
    })

    test("Get all mapped bodies", () => {
        expect(mechanism.GetBodyByNodeId("node1")).toBeDefined()
        expect(mechanism.GetBodyByNodeId("node2")).toBeDefined()
        expect(mechanism.GetBodyByNodeId("node3")).toBeDefined()
    })

    test("Body map reference integrity", () => {
        // Verify that the mechanism holds a reference to the same map
        expect(mechanism.nodeToBody).toBe(bodyMap)

        // Adding to the original map should affect the mechanism
        const newBodyID = createMockBodyID(999)
        bodyMap.set("node4", newBodyID)

        expect(mechanism.GetBodyByNodeId("node4")).toBe(newBodyID)
    })
})

describe("Mechanism Properties and State", () => {
    test("Root body property", () => {
        const rootBody = "test-root-body"
        const mechanism = new Mechanism(rootBody, new Map(), true)

        expect(mechanism.rootBody).toBe(rootBody)
    })

    test("Controllable property - true", () => {
        const mechanism = new Mechanism("root", new Map(), true)
        expect(mechanism.controllable).toBe(true)
    })

    test("Controllable property - false", () => {
        const mechanism = new Mechanism("root", new Map(), false)
        expect(mechanism.controllable).toBe(false)
    })

    test("Layer reserve property", () => {
        const mechanism1 = new Mechanism("root", new Map(), true)
        expect(mechanism1.layerReserve).toBeUndefined()

        const mechanism2 = new Mechanism("root", new Map(), true, mockLayerReserve)
        expect(mechanism2.layerReserve).toBe(mockLayerReserve)
    })

    test("Ghost bodies array initialization", () => {
        const mechanism = new Mechanism("root", new Map(), true)

        expect(mechanism.ghostBodies).toEqual([])
        expect(Array.isArray(mechanism.ghostBodies)).toBe(true)
    })

    test("Ghost bodies can be modified", () => {
        const mechanism = new Mechanism("root", new Map(), true)

        mechanism.ghostBodies.push(mockBodyID)
        expect(mechanism.ghostBodies).toHaveLength(1)
        expect(mechanism.ghostBodies[0]).toBe(mockBodyID)
    })
})

describe("Mechanism Methods", () => {
    let mechanism: Mechanism

    beforeEach(() => {
        mechanism = new Mechanism("root", new Map(), true)
    })

    test("DisablePhysics method exists and is callable", () => {
        // The method currently has no implementation, but should be callable
        expect(() => mechanism.DisablePhysics()).not.toThrow()
    })
})

describe("Mechanism Integration Tests", () => {
    test("Complete mechanism setup", () => {
        const rootBody = "integration-root"
        const bodyMap = new Map([
            ["wheel1", mockBodyID],
            ["wheel2", mockBodyID],
            ["chassis", mockBodyID],
        ])
        const layerReserve = mockLayerReserve

        const mechanism = new Mechanism(rootBody, bodyMap, true, layerReserve)

        // Add constraints
        const wheelConstraint: MechanismConstraint = {
            parentBody: mockBodyID,
            childBody: mockBodyID,
            primaryConstraint: mockConstraint,
            maxVelocity: 50.0,
            extraConstraints: [],
            extraBodies: [],
        }

        mechanism.AddConstraint(wheelConstraint)
        mechanism.AddStepListener(mockStepListener)
        mechanism.ghostBodies.push(mockBodyID)

        // Verify all components are properly set
        expect(mechanism.rootBody).toBe(rootBody)
        expect(mechanism.nodeToBody.size).toBe(3)
        expect(mechanism.constraints).toHaveLength(1)
        expect(mechanism.stepListeners).toHaveLength(1)
        expect(mechanism.ghostBodies).toHaveLength(1)
        expect(mechanism.layerReserve).toBe(layerReserve)
        expect(mechanism.controllable).toBe(true)

        // Test body retrieval
        expect(mechanism.GetBodyByNodeId("wheel1")).toBeDefined()
        expect(mechanism.GetBodyByNodeId("nonexistent")).toBeUndefined()
    })

    test("Mechanism with complex constraint setup", () => {
        const mechanism = new Mechanism("root", new Map(), false)

        const constraints: MechanismConstraint[] = [
            {
                parentBody: mockBodyID,
                childBody: mockBodyID,
                primaryConstraint: mockConstraint,
                maxVelocity: 10.0,
                info: { name: "Revolute 8" } as mirabuf.IInfo,
                extraConstraints: [],
                extraBodies: [],
            },
            {
                parentBody: mockBodyID,
                childBody: mockBodyID,
                primaryConstraint: mockConstraint,
                maxVelocity: 20.0,
                extraConstraints: [mockConstraint, mockConstraint],
                extraBodies: [mockBodyID, mockBodyID],
            },
        ]

        constraints.forEach(constraint => mechanism.AddConstraint(constraint))

        expect(mechanism.constraints).toHaveLength(2)
        expect(mechanism.constraints[0].info?.name).toBe("Revolute 8")
        expect(mechanism.constraints[1].extraConstraints).toHaveLength(2)
        expect(mechanism.constraints[1].extraBodies).toHaveLength(2)
    })
})

describe("Mirabuf Mechanism Creation", () => {
    let physSystem: PhysicsSystem

    beforeEach(() => {
        physSystem = new PhysicsSystem()
    })

    test("Body Loading (Dozer)", async () => {
        const assembly = await MirabufCachingService.CacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT).then(
            x => MirabufCachingService.Get(x!.id, MiraType.ROBOT)
        )
        const parser = new MirabufParser(assembly!)

        const mechanism = physSystem.CreateMechanismFromParser(parser)

        expect(mechanism).toBeDefined()
        expect(mechanism.controllable).toBe(true)
        expect(mechanism.constraints.length).toBe(12)
    })

    test("Body Loading (Mutli-Joint Robot)", async () => {
        const assembly = await MirabufCachingService.CacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.Get(x!.id, MiraType.ROBOT))
        const parser = new MirabufParser(assembly!)

        const mechanism = physSystem.CreateMechanismFromParser(parser)

        expect(mechanism).toBeDefined()
        expect(mechanism.controllable).toBe(true)
        expect(mechanism.constraints.length).toBe(12)
    })
})
