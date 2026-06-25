// Memory Regression Tests
//
// Reproduces the three Jolt/Wasm memory-management bugs found during
// the SYNTH-109 handle-memory investigation. These tests exercise low-level
// heap invariants and can be slow, so they are intentionally excluded from the
// normal test run.
//
// To Run, rename this file to `PhysicsSystemHeap.test.ts` and run the normal
// test suite with `bun run test`
//

import * as THREE from "three"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import PhysicsSystem from "../../systems/physics/PhysicsSystem"

// Mirror the layer constants that PhysicsSystem.ts uses internally (not exported).
const LAYER_FIELD = 0
const LAYER_GENERAL_DYNAMIC = 1
const ROBOT_LAYERS = [2, 3, 4, 5, 6, 7, 8, 9]
const COUNT_OBJECT_LAYERS = 11
const COUNT_BP_LAYERS = 2 + ROBOT_LAYERS.length

function buildFilterObjects() {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS)
    objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, LAYER_GENERAL_DYNAMIC)
    objectFilter.EnableCollision(LAYER_FIELD, LAYER_GENERAL_DYNAMIC)
    ROBOT_LAYERS.forEach(l => {
        objectFilter.EnableCollision(LAYER_FIELD, l)
        objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, l)
    })
    for (let i = 0; i < ROBOT_LAYERS.length - 1; i++)
        for (let j = i + 1; j < ROBOT_LAYERS.length; j++)
            objectFilter.EnableCollision(ROBOT_LAYERS[i], ROBOT_LAYERS[j])

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BP_LAYERS)
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_FIELD, new JOLT.BroadPhaseLayer(LAYER_FIELD))
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_GENERAL_DYNAMIC, new JOLT.BroadPhaseLayer(LAYER_GENERAL_DYNAMIC))
    ROBOT_LAYERS.forEach(l => bpInterface.MapObjectToBroadPhaseLayer(l, new JOLT.BroadPhaseLayer(l)))

    const settings = new JOLT.JoltSettings()
    settings.mObjectLayerPairFilter = objectFilter
    settings.mBroadPhaseLayerInterface = bpInterface
    settings.mObjectVsBroadPhaseLayerFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(
        bpInterface,
        COUNT_BP_LAYERS,
        objectFilter,
        COUNT_OBJECT_LAYERS
    )

    return { objectFilter, bpInterface, objectVsBpFilter: settings.mObjectVsBroadPhaseLayerFilter, settings }
}

describe("PhysicsSystem memory regression", () => {
    // Bug 1
    // destroy() was calling JOLT.destroy(this._joltBodyInterface).
    //
    // IDL:  [Ref] BodyInterface GetBodyInterface()
    // "[Ref]" means the return is a reference to an interior sub-object of
    // PhysicsSystem, NOT an independently heap-allocated object. C++ source
    // confirms both mBodyInterfaceLocking and mBodyInterfaceNoLock are value
    // members embedded directly in PhysicsSystem.
    //
    // Calling JOLT.destroy() on an interior pointer passes that address to
    // dlmalloc free(), which writes free-list bookkeeping (fd/bk pointers)
    // into live memory, corrupting the heap for all subsequent allocations.
    test("GetBodyInterface returns an interior pointer of PhysicsSystem", () => {
        const { settings } = buildFilterObjects()
        const joltInterface = new JOLT.JoltInterface(settings)
        JOLT.destroy(settings)

        const physSystem = joltInterface.GetPhysicsSystem()
        const bodyInterface = physSystem.GetBodyInterface()

        const physPtr = JOLT.getPointer(physSystem)
        const bodyPtr = JOLT.getPointer(bodyInterface)

        // bodyInterface is embedded inside physSystem's allocation.
        // Its address must be strictly greater than physSystem's start address
        // and within the first kilobyte (PhysicsSystem struct fields are near the top).
        expect(bodyPtr).toBeGreaterThan(physPtr)
        expect(bodyPtr - physPtr).toBeLessThan(1024)

        // Correct teardown: only destroy joltInterface — its destructor runs
        // ~JoltInterface() → delete mPhysicsSystem, which owns bodyInterface.
        JOLT.destroy(joltInterface)
    })

    // Bug 2
    // destroy() was calling JOLT.destroy(this._joltPhysSystem.GetContactListener())
    // AFTER JOLT.destroy(this._joltInterface).
    //
    // JoltInterface destructor runs: delete mPhysicsSystem
    // After that, _joltPhysSystem is a dangling pointer.  Dereferencing it to
    // call GetContactListener() is a use-after-free.
    //
    // Fix: capture the contactListener BEFORE destroying joltInterface.
    test("contactListener must be captured before JOLT.destroy(joltInterface)", () => {
        const { settings } = buildFilterObjects()
        const joltInterface = new JOLT.JoltInterface(settings)
        JOLT.destroy(settings)

        const physSystem = joltInterface.GetPhysicsSystem()
        const contactListener = new JOLT.ContactListenerJS()
        physSystem.SetContactListener(contactListener)

        // Correct: fetch contactListener while physSystem is still live.
        const capturedListener = physSystem.GetContactListener()

        // JoltInterface destructor: delete mPhysicsSystem → physSystem is now dangling.
        JOLT.destroy(joltInterface)

        // capturedListener was fetched before the free — safe to destroy.
        JOLT.destroy(capturedListener)
    })

    // Bug 3
    // Both sensor tests were double-freeing shapeSettings.
    //
    // createSensor(shapeSettings, destroy=true) calls JOLT.destroy(shapeSettings)
    // internally on both the success and error paths.  The sensor tests then
    // called JOLT.destroy(shapeSettings) a second time after createSensor returned.
    //
    // The double-free wrote stale free-list pointers into a block that had
    // already been reused, corrupting the dlmalloc bookkeeping incrementally
    // across test cycles.  The corruption only manifested as a Wasm trap after
    // enough prior PhysicsSystem lifecycles had run (groups 1-7 of the main
    // suite set up the specific heap layout that exposed it).
    //
    // This test reproduces the cumulative failure pattern: 30 full create ->
    // sensor -> body -> GetID -> destroy cycles. If the double-free is
    // reintroduced, the Wasm heap will corrupt and GetIndexAndSequenceNumber()
    // will trap - the exact failure seen in the Body Associations tests.
    test("30 PhysicsSystem cycles with sensor creation must not corrupt body ID access", () => {
        const CYCLES = 30

        for (let i = 0; i < CYCLES; i++) {
            const system = new PhysicsSystem()

            // createSensor(destroy=true) frees shapeSettings internally.
            // Do NOT call JOLT.destroy(shapeSettings) after this returns.
            const size = new JOLT.Vec3(1, 1, 1)
            const shapeSettings = new JOLT.BoxShapeSettings(size)
            JOLT.destroy(size)
            const sensorId = system.createSensor(shapeSettings)

            expect(sensorId).toBeDefined()
            expect(system.isBodyAdded(sensorId!)).toBe(true)

            // createBox + GetIndexAndSequenceNumber was the exact call that
            // trapped in the Body Associations tests after heap corruption.
            const body = system.createBox(new THREE.Vector3(1, 1, 1), 1.0, undefined, undefined)
            expect(() => body.GetID().GetIndexAndSequenceNumber()).not.toThrow()

            system.destroy()
        }
    })
})
