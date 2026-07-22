// Per-class instance recipes for the memory-audit suite, seeded from real construction sites
// (jolt/helloworld/HelloWorld.js, fission/src). Classes with no entry are reported as
// "no factory, untested" by the suite.
import type Jolt from "@synthesis.adsk/jolt-physics"

type JoltModule = typeof Jolt

// --- Standalone factories: no parent/scene required, safe to construct+destroy in isolation. ---
// Each factory returns a freshly constructed instance the caller owns (matches the `COPY`/`new`
// convention). The test harness destroys it per the row's ownership rule.
export const factories: Record<string, (JOLT: JoltModule) => unknown> = {
    Vec3: JOLT => new JOLT.Vec3(1, 2, 3),
    RVec3: JOLT => new JOLT.RVec3(10, 20, 30),
    Vec4: JOLT => new JOLT.Vec4(1, 2, 3, 4),
    Quat: JOLT => new JOLT.Quat(0, 0, 0, 1),
    Float3: JOLT => new JOLT.Float3(1, 2, 3),
    // Not `.sIdentity()`: non-constructor binder functions returning `[Value]` types alias a
    // `static` scratch buffer in glue.cpp, reused on every call
    // (docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md's static-temp-aliased section). Only
    // `new JOLT.X(...)` heap-allocates an independently destroyable instance. `sIdentity()` would
    // hand back a non-owned alias with no per-call identity, breaking the destroy test and leak count.
    Mat44: JOLT => new JOLT.Mat44(),
    RMat44: JOLT => new JOLT.RMat44(),
    AABox: JOLT => {
        const min = new JOLT.Vec3(-1, -1, -1)
        const max = new JOLT.Vec3(1, 1, 1)
        const box = new JOLT.AABox(min, max)
        JOLT.destroy(min)
        JOLT.destroy(max)
        return box
    },
    IndexedTriangle: JOLT => new JOLT.IndexedTriangle(0, 1, 2, 0),
    PhysicsMaterial: JOLT => new JOLT.PhysicsMaterial(),
    PhysicsMaterialList: JOLT => new JOLT.PhysicsMaterialList(),
    VertexList: JOLT => new JOLT.VertexList(),
    IndexedTriangleList: JOLT => new JOLT.IndexedTriangleList(),
    BoxShapeSettings: JOLT => {
        const size = new JOLT.Vec3(1, 1, 1)
        const settings = new JOLT.BoxShapeSettings(size)
        JOLT.destroy(size)
        return settings
    },
    SphereShapeSettings: JOLT => new JOLT.SphereShapeSettings(1),
    BoxShape: JOLT => {
        const size = new JOLT.Vec3(1, 1, 1)
        const shape = new JOLT.BoxShape(size)
        JOLT.destroy(size)
        return shape
    },
    SphereShape: JOLT => new JOLT.SphereShape(1),
    StaticCompoundShapeSettings: JOLT => new JOLT.StaticCompoundShapeSettings(),
    HingeConstraintSettings: JOLT => new JOLT.HingeConstraintSettings(),
    JoltSettings: JOLT => new JOLT.JoltSettings(),
    ObjectLayerPairFilterTable: JOLT => {
        const filter = new JOLT.ObjectLayerPairFilterTable(1)
        filter.EnableCollision(0, 0)
        return filter
    },
    BroadPhaseLayerInterfaceTable: JOLT => {
        const bpLayer = new JOLT.BroadPhaseLayer(0)
        const table = new JOLT.BroadPhaseLayerInterfaceTable(1, 1)
        table.MapObjectToBroadPhaseLayer(0, bpLayer)
        JOLT.destroy(bpLayer)
        return table
    },
}

// Minimal single-layer physics world, mirroring jolt/helloworld/HelloWorld.js. Settings,
// objectFilter, bpInterface, and bpFilter are all consumed by JoltInterface's constructor,
// matching the CONSUMED category in docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md.
export const MINIMAL_LAYER = 0

export type MinimalPhysicsContext = {
    joltInterface: Jolt.JoltInterface
    physicsSystem: Jolt.PhysicsSystem
    bodyInterface: Jolt.BodyInterface
    destroy: () => void
}

export function createMinimalPhysicsSystem(JOLT: JoltModule): MinimalPhysicsContext {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(1)
    objectFilter.EnableCollision(MINIMAL_LAYER, MINIMAL_LAYER)

    const bpLayer = new JOLT.BroadPhaseLayer(0)
    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(1, 1)
    bpInterface.MapObjectToBroadPhaseLayer(MINIMAL_LAYER, bpLayer)
    JOLT.destroy(bpLayer)

    const bpFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(bpInterface, 1, objectFilter, 1)

    const settings = new JOLT.JoltSettings()
    settings.mObjectLayerPairFilter = objectFilter
    settings.mBroadPhaseLayerInterface = bpInterface
    settings.mObjectVsBroadPhaseLayerFilter = bpFilter
    const joltInterface = new JOLT.JoltInterface(settings)
    JOLT.destroy(settings)

    const physicsSystem = joltInterface.GetPhysicsSystem()
    const bodyInterface = physicsSystem.GetBodyInterface()

    return {
        joltInterface,
        physicsSystem,
        bodyInterface,
        destroy: () => JOLT.destroy(joltInterface),
    }
}

// Builds a standalone, AddRef'd Shape (not owned by any BodyCreationSettings/Body yet), the
// canonical "child" for shape-handoff recipes below. Caller owns the returned shape's one
// reference and must Release() or hand it off exactly once.
export function createStandaloneBoxShape(JOLT: JoltModule): Jolt.Shape {
    const size = new JOLT.Vec3(1, 1, 1)
    const shapeSettings = new JOLT.BoxShapeSettings(size)
    JOLT.destroy(size)
    // `ShapeSettings.Create()` returns `[Value] ShapeResult`, a non-constructor `[Value]` return,
    // so `shapeResult` aliases glue.cpp's static scratch buffer for this bound function, not a
    // heap allocation. Never `JOLT.destroy()` it. `.Clear()` is a real, safe method call that just
    // drops the Result's internal `RefConst`, not a free.
    const shapeResult = shapeSettings.Create()
    const shape = shapeResult.Get()
    shapeResult.Clear()
    shape.AddRef()
    JOLT.destroy(shapeSettings)
    return shape
}

export function createBoxBody(JOLT: JoltModule, ctx: MinimalPhysicsContext, shape: Jolt.Shape): Jolt.Body {
    const pos = new JOLT.RVec3(0, 0, 0)
    const rot = new JOLT.Quat(0, 0, 0, 1)
    const creationSettings = new JOLT.BodyCreationSettings(shape, pos, rot, JOLT.EMotionType_Dynamic, MINIMAL_LAYER)
    JOLT.destroy(pos)
    JOLT.destroy(rot)
    const body = ctx.bodyInterface.CreateBody(creationSettings)
    JOLT.destroy(creationSettings)
    return body
}

// --- Handoff recipes: named, real RefTarget handoff call sites from the codegen table's
// isHandoffCandidate rows / attributeHandoffs. Each recipe builds a parent plus a fresh child,
// hands the child off, and exposes enough of both to drive the 3 handoff-timing test variants.
// Priority given to the codegen's flagged real sites, matching
// docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md's "not yet re-checked" list. See [[jolt_refcounted_destroy_danger]].
export type HandoffRecipe = {
    // Builds the parent object and a fresh child object about to be handed off.
    build: (JOLT: JoltModule) => { parent: unknown; child: unknown; teardown: () => void }
    // Performs the handoff call itself (e.g. `parent.push_back(child)` / `parent.SetShape(child)`).
    handoff: (JOLT: JoltModule, parent: unknown, child: unknown) => void
    // Reads back a signal that the parent actually took a reference (GetRefCount() before/after
    // handoff is asserted by the test harness around this call, not inside the recipe).
    getRefCount: (JOLT: JoltModule, child: unknown) => number
    // Optional: how to explicitly remove the child from the parent again, for handoff-timing
    // variant (c) (hand off, explicitly remove, then destroy is safe).
    explicitRemove?: (JOLT: JoltModule, parent: unknown, child: unknown) => void
}

export const handoffRegistry: Record<string, HandoffRecipe> = {
    "PhysicsMaterialList.push_back": {
        build: JOLT => {
            const parent = new JOLT.PhysicsMaterialList()
            const child = new JOLT.PhysicsMaterial()
            return { parent, child, teardown: () => JOLT.destroy(parent) }
        },
        handoff: (_JOLT, parent, child) => (parent as Jolt.PhysicsMaterialList).push_back(child as Jolt.PhysicsMaterial),
        getRefCount: (_JOLT, child) => (child as Jolt.PhysicsMaterial).GetRefCount(),
        // PhysicsMaterialList exposes no removal method in jolt/JoltJS.idl. Once added, a
        // material can only be dropped by clearing or destroying the whole list.
    },
    "BodyCreationSettings.SetShape": {
        build: JOLT => {
            const parent = new JOLT.BodyCreationSettings()
            const child = createStandaloneBoxShape(JOLT)
            return { parent, child, teardown: () => JOLT.destroy(parent) }
        },
        handoff: (_JOLT, parent, child) => (parent as Jolt.BodyCreationSettings).SetShape(child as Jolt.Shape),
        getRefCount: (_JOLT, child) => (child as Jolt.Shape).GetRefCount(),
    },
    "BodyInterface.SetShape": {
        build: JOLT => {
            const ctx = createMinimalPhysicsSystem(JOLT)
            const initialShape = createStandaloneBoxShape(JOLT)
            const body = createBoxBody(JOLT, ctx, initialShape)
            initialShape.Release()
            ctx.bodyInterface.AddBody(body.GetID(), JOLT.EActivation_DontActivate)
            const child = createStandaloneBoxShape(JOLT)
            return {
                parent: { ctx, body },
                child,
                teardown: () => {
                    ctx.bodyInterface.RemoveBody(body.GetID())
                    ctx.bodyInterface.DestroyBody(body.GetID())
                    ctx.destroy()
                },
            }
        },
        handoff: (JOLT, parent, child) => {
            const { ctx, body } = parent as { ctx: MinimalPhysicsContext; body: Jolt.Body }
            ctx.bodyInterface.SetShape(body.GetID(), child as Jolt.Shape, true, JOLT.EActivation_DontActivate)
        },
        getRefCount: (_JOLT, child) => (child as Jolt.Shape).GetRefCount(),
    },
}

// Classes/handoff rows needing a factory, not yet seeded, surfaced by the codegen report as
// "no factory, untested". Move each into `handoffRegistry` above once seeded.
export const KNOWN_UNSEEDED_HANDOFFS = [
    "VehicleConstraint.SetVehicleCollisionTester",
    "PhysicsSystem.AddConstraint",
    "CharacterVirtual.SetShape",
    "CompoundShapeSettings.AddShape",
]
