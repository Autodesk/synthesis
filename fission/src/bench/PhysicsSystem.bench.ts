import * as THREE from "three"
import { bench, describe } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import PhysicsSystem, { STANDARD_SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem"
import MirabufParser from "@/mirabuf/MirabufParser"
import { getMiraAssembly } from "@/test/GetAssets"

function addFloor(system: PhysicsSystem) {
    const floor = system.createBox(new THREE.Vector3(50, 0.5, 50), undefined, new THREE.Vector3(0, -0.5, 0), undefined)
    system.addBodyToSystem(floor.GetID(), false)
}

// --- Simulation step systems ---

const emptySystem = new PhysicsSystem()
addFloor(emptySystem)

const eightBodySystem = new PhysicsSystem()
addFloor(eightBodySystem)
for (let i = 0; i < 8; i++) {
    const body = eightBodySystem.createBox(
        new THREE.Vector3(0.25, 0.25, 0.25),
        1.0,
        new THREE.Vector3((i % 4) * 0.6, Math.floor(i / 4) + 0.5, 0),
        undefined
    )
    eightBodySystem.addBodyToSystem(body.GetID(), true)
}

const fiftyBodySystem = new PhysicsSystem()
addFloor(fiftyBodySystem)
for (let i = 0; i < 50; i++) {
    const body = fiftyBodySystem.createBox(
        new THREE.Vector3(0.25, 0.25, 0.25),
        0.5,
        new THREE.Vector3((i % 10) * 0.6, Math.floor(i / 10) * 0.6 + 0.5, 0),
        undefined
    )
    fiftyBodySystem.addBodyToSystem(body.GetID(), true)
}

// --- Raycast system ---
// Pass destroy=false on each rayCast call so the Vec3s survive across iterations.

const raycastSystem = new PhysicsSystem()
const rayTarget = raycastSystem.createBox(
    new THREE.Vector3(1, 1, 1),
    undefined,
    new THREE.Vector3(0, 5, 0),
    undefined
)
raycastSystem.addBodyToSystem(rayTarget.GetID(), false)
const RAY_FROM = new JOLT.Vec3(0, 0, 0)
const RAY_HIT_DIR = new JOLT.Vec3(0, 10, 0)
const RAY_MISS_DIR = new JOLT.Vec3(100, 0, 0)

// --- Body creation system ---

const creationSystem = new PhysicsSystem()
const HALF_EXTENTS = new THREE.Vector3(0.5, 0.5, 0.5)
const CUBE_HULL_POINTS = new Float32Array([
    0.5, -0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, -0.5, -0.5,
    0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5,
])

// --- Assembly spawn systems (async: need real mirabuf assemblies) ---

const [dozerAssembly, multiJointAssembly] = await Promise.all([
    getMiraAssembly("DOZER"),
    getMiraAssembly("MULTI_JOINT"),
])
const dozerParser = new MirabufParser(dozerAssembly!)
const multiJointParser = new MirabufParser(multiJointAssembly!)

// Separate systems per spawn bench so their internal state stays independent.
const spawnSystemDozer = new PhysicsSystem()
const spawnSystemMultiJoint = new PhysicsSystem()

// ────────────────────────────────────────────────────────────────────────────

describe("PhysicsSystem — simulation step", () => {
    bench("update — empty world", () => {
        emptySystem.update(STANDARD_SIMULATION_PERIOD)
    })

    bench("update — 8 dynamic bodies", () => {
        eightBodySystem.update(STANDARD_SIMULATION_PERIOD)
    })

    bench("update — 50 dynamic bodies", () => {
        fiftyBodySystem.update(STANDARD_SIMULATION_PERIOD)
    })
})

describe("PhysicsSystem — body creation", () => {
    bench("createBox + add + destroy", () => {
        const body = creationSystem.createBox(HALF_EXTENTS, 1.0, undefined, undefined)
        creationSystem.addBodyToSystem(body.GetID(), false)
        creationSystem.destroyBodies(body)
    })

    bench("createConvexHull — 8-point cube", () => {
        const result = creationSystem.createConvexHull(CUBE_HULL_POINTS)
        if (result.IsValid()) result.Get().Release()
    })
})

describe("PhysicsSystem — raycast", () => {
    bench("rayCast — hit", () => {
        raycastSystem.rayCast(RAY_FROM, RAY_HIT_DIR, false)
    })

    bench("rayCast — miss", () => {
        raycastSystem.rayCast(RAY_FROM, RAY_MISS_DIR, false)
    })
})

describe("PhysicsSystem — assembly spawn", () => {
    // createMechanismFromParser allocates a LayerReserve (one of 8 robot slots).
    // destroyMechanism removes bodies and constraints but does not release the slot,
    // so we do it manually to keep the pool from exhausting across iterations.
    bench("spawn + destroy Dozer (7 bodies, 6 joints)", () => {
        const mech = spawnSystemDozer.createMechanismFromParser(dozerParser)
        spawnSystemDozer.destroyMechanism(mech)
        mech.layerReserve?.release()
    })

    bench("spawn + destroy Multi-Joint Wheels (9 bodies, 8 joints)", () => {
        const mech = spawnSystemMultiJoint.createMechanismFromParser(multiJointParser)
        spawnSystemMultiJoint.destroyMechanism(mech)
        mech.layerReserve?.release()
    })
})
