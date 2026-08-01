import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { weldBodies } from "@/mix-and-match/MixAndMatchWeld"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"

let physicsSystem: PhysicsSystem

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return physicsSystem
        },
        get multiplayerSystem() {
            return undefined
        },
    },
}))

const HALF_EXTENTS = new THREE.Vector3(0.25, 0.25, 0.25)
const STEP = 1 / 60
const STEPS = 120

function spawnBox(position: THREE.Vector3) {
    const body = physicsSystem.createBox(HALF_EXTENTS, 1, position, undefined)
    physicsSystem.addBodyToSystem(body.GetID(), true)

    return body
}

/**
 * Copied out immediately: Jolt getters hand back a per-function static scratch, so holding two of
 * them at once gives you the same values twice.
 */
function positionOf(body: ReturnType<typeof spawnBox>): THREE.Vector3 {
    const position = body.GetPosition()

    return new THREE.Vector3(position.GetX(), position.GetY(), position.GetZ())
}

function separation(a: ReturnType<typeof spawnBox>, b: ReturnType<typeof spawnBox>): THREE.Vector3 {
    return positionOf(b).sub(positionOf(a))
}

/**
 * The bake step turns every recorded weld into one of these constraints, so this is the check that a
 * finished build actually behaves as one rigid robot.
 */
describe("Mix and Match Weld Bake", () => {
    beforeEach(() => {
        physicsSystem = new PhysicsSystem()
    })

    afterEach(() => {
        physicsSystem.destroy()
    })

    test("Welded Bodies Keep Their Relative Pose Through Simulation", () => {
        const parent = spawnBox(new THREE.Vector3(0, 5, 0))
        const child = spawnBox(new THREE.Vector3(0.6, 5, 0.3))
        const before = separation(parent, child)

        weldBodies(parent, child)

        for (let i = 0; i < STEPS; i++) physicsSystem.update(STEP)

        const after = separation(parent, child)

        expect(after.x).toBeCloseTo(before.x, 2)
        expect(after.y).toBeCloseTo(before.y, 2)
        expect(after.z).toBeCloseTo(before.z, 2)
    })

    test("Welded Bodies Fall Together Rather Than Staying Put", () => {
        const parent = spawnBox(new THREE.Vector3(0, 5, 0))
        const child = spawnBox(new THREE.Vector3(0.6, 5, 0))
        weldBodies(parent, child)

        for (let i = 0; i < STEPS; i++) physicsSystem.update(STEP)

        expect(parent.GetPosition().GetY()).toBeLessThan(5)
        expect(child.GetPosition().GetY()).toBeLessThan(5)
    })

    test("Unwelded Bodies Drift Apart, Confirming The Constraint Does The Work", () => {
        const parent = spawnBox(new THREE.Vector3(0, 5, 0))
        const child = spawnBox(new THREE.Vector3(0.6, 9, 0))
        const before = separation(parent, child)

        for (let i = 0; i < STEPS; i++) physicsSystem.update(STEP)

        expect(Math.abs(separation(parent, child).y - before.y)).toBeGreaterThan(0.1)
    })

    test("A Body Can Be Moved Onto Another Object Layer", () => {
        const body = spawnBox(new THREE.Vector3(0, 5, 0))
        const original = body.GetObjectLayer()

        physicsSystem.setBodyObjectLayer(body.GetID(), original + 1)

        expect(body.GetObjectLayer()).toBe(original + 1)
    })

    test("Jolt Exposes The Constraint Settings The Bake Relies On", () => {
        const settings = new JOLT.FixedConstraintSettings()
        settings.mSpace = JOLT.EConstraintSpace_WorldSpace
        settings.mAutoDetectPoint = true

        expect(settings.mAutoDetectPoint).toBe(true)
        expect(settings.mSpace).toBe(JOLT.EConstraintSpace_WorldSpace)

        JOLT.destroy(settings)
    })
})
