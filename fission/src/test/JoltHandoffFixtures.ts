// Minimal, standalone recipes for JoltHandoffTiming.test.ts. Adapted from the `memory-audit`
// branch's `fission/src/test/jolt-memory/lib/factories.ts` (`createMinimalPhysicsSystem`,
// `createStandaloneBoxShape`, `handoffRegistry`), retargeted at fission's actual production call
// sites -- these recipes exist to prove the exact claims JoltClassClassification.ts's manual
// entries make for RefTarget/CONSUMED classes, not to be a generic Jolt test harness.
//
// Deliberately does NOT include the memory-audit branch's "destroy immediately after handoff"
// variant (a) / uaf-probe.ts child-process machinery: that's a use-after-free/corruption check,
// squarely branch 282 (branp/282/asan-memory-test)'s job. This branch only needs to prove the
// SAFE side of the contract -- hand off, never destroy the child directly, parent teardown is
// clean -- which is exactly what backs the "INTERNAL_REF_UNPROVABLE" classification for these
// classes.
import type Jolt from "@synthesis.adsk/jolt-physics"

type JoltModule = typeof Jolt

export const MINIMAL_LAYER = 0

export type MinimalPhysicsContext = {
    joltInterface: Jolt.JoltInterface
    physicsSystem: Jolt.PhysicsSystem
    bodyInterface: Jolt.BodyInterface
    destroy: () => void
}

// Mirrors PhysicsSystem.ts's setupCollisionFiltering + JoltInterface construction. Exercises the
// ObjectLayerPairFilterTable / BroadPhaseLayerInterfaceTable / ObjectVsBroadPhaseLayerFilterTable
// CONSUMED handoff chain documented in JoltClassClassification.ts's manual entries for those three
// classes: all three are assigned into JoltSettings, consumed by `new JOLT.JoltInterface(settings)`,
// and never destroy()'d directly -- only `destroy()` is called on the settings object and,
// eventually, the JoltInterface itself.
export function createMinimalPhysicsSystem(jolt: JoltModule): MinimalPhysicsContext {
    const objectFilter = new jolt.ObjectLayerPairFilterTable(1)
    objectFilter.EnableCollision(MINIMAL_LAYER, MINIMAL_LAYER)

    // Deliberately destroy()'d, matching jolt/helloworld/HelloWorld.js's own canonical example
    // ("'BP_LAYER' has been copied into bpInterface") -- NOT matching fission's own PhysicsSystem.ts,
    // which has a `// WARNING DO NOT FREE` comment on its equivalent construction. That's a real,
    // open disagreement (see the `BroadPhaseLayer` manual entry in JoltClassClassification.ts);
    // this minimal recipe follows the canonical example since it isn't exercising fission's own
    // BroadPhaseLayer call site.
    const bpLayer = new jolt.BroadPhaseLayer(0)
    const bpInterface = new jolt.BroadPhaseLayerInterfaceTable(1, 1)
    bpInterface.MapObjectToBroadPhaseLayer(MINIMAL_LAYER, bpLayer)
    jolt.destroy(bpLayer)

    const bpFilter = new jolt.ObjectVsBroadPhaseLayerFilterTable(bpInterface, 1, objectFilter, 1)

    const settings = new jolt.JoltSettings()
    settings.mObjectLayerPairFilter = objectFilter
    settings.mBroadPhaseLayerInterface = bpInterface
    settings.mObjectVsBroadPhaseLayerFilter = bpFilter
    const joltInterface = new jolt.JoltInterface(settings)
    jolt.destroy(settings)

    const physicsSystem = joltInterface.GetPhysicsSystem()
    const bodyInterface = physicsSystem.GetBodyInterface()

    return {
        joltInterface,
        physicsSystem,
        bodyInterface,
        destroy: () => jolt.destroy(joltInterface),
    }
}

// Fresh Shape, held at refcount 1 by a defensive `AddRef()` the caller must `Release()` once a
// real owner has taken its own reference (exactly HelloWorld.js's `shape.AddRef()`/`.Release()`
// bracket around `Jolt.destroy(compound)` -- confirmed by direct reproduction that skipping this
// is a real, immediate use-after-free: `ShapeResult.Clear()` drops the ShapeResult's own internal
// `Ref<Shape>`, and without an independent reference already in place, that's what was keeping the
// shape alive. Manifested as a nondeterministic WASM trap several calls later
// (`CreateBody`: "function signature mismatch" / "null function" / "index out of bounds" /
// "memory access out of bounds", depending on what reused the freed memory) -- textbook
// heap-corruption symptom, not a logic error, reproduced and root-caused via bisection against
// jolt/helloworld/HelloWorld.js before this fix.
export function createStandaloneBoxShape(jolt: JoltModule): Jolt.Shape {
    const size = new jolt.Vec3(1, 1, 1)
    const shapeSettings = new jolt.BoxShapeSettings(size)
    jolt.destroy(size)
    const shapeResult = shapeSettings.Create()
    const shape = shapeResult.Get()
    shapeResult.Clear()
    shape.AddRef()
    jolt.destroy(shapeSettings)
    return shape
}

// Hands `shape` to a fresh BodyCreationSettings, creates the body, and releases the caller's own
// defensive hold (from createStandaloneBoxShape) now that the Body has its own reference.
export function createBody(jolt: JoltModule, ctx: MinimalPhysicsContext, shape: Jolt.Shape): Jolt.Body {
    const pos = new jolt.RVec3(0, 0, 0)
    const rot = new jolt.Quat(0, 0, 0, 1)
    const creationSettings = new jolt.BodyCreationSettings(shape, pos, rot, jolt.EMotionType_Static, MINIMAL_LAYER)
    jolt.destroy(pos)
    jolt.destroy(rot)
    shape.Release()
    const body = ctx.bodyInterface.CreateBody(creationSettings)
    jolt.destroy(creationSettings)
    return body
}

// Each recipe builds its own parent+child, performs the handoff, and reads the child's refcount
// immediately before and after -- same tick, no window for the object to have been freed or
// reused elsewhere -- then tears everything down itself (own class-specific cleanup order,
// since e.g. Constraint's correct teardown is RemoveConstraint, never destroy(), while others tear
// down via a parent's own destroy()). Returns the two counts for the test to assert on.
export type HandoffProof = { refCountBeforeHandoff: number; refCountAfterHandoff: number }
export type HandoffRecipe = (jolt: JoltModule) => HandoffProof

// --- RefTarget handoff recipes: each proves "the handoff call itself takes a reference, so the
// caller must never destroy() its own handle" for one JoltClassClassification.ts manual entry. ---
export const refTargetHandoffRecipes: Record<string, HandoffRecipe> = {
    // Backs the `Shape` manual entry: "It's handed to BodyCreationSettings (which AddRefs it)."
    "BodyCreationSettings(shape, ...) constructor": jolt => {
        const shape = createStandaloneBoxShape(jolt)
        const refCountBeforeHandoff = shape.GetRefCount()

        const pos = new jolt.RVec3(0, 0, 0)
        const rot = new jolt.Quat(0, 0, 0, 1)
        const creationSettings = new jolt.BodyCreationSettings(shape, pos, rot, jolt.EMotionType_Static, MINIMAL_LAYER)
        jolt.destroy(pos)
        jolt.destroy(rot)
        const refCountAfterHandoff = shape.GetRefCount()

        jolt.destroy(creationSettings) // never destroy `shape` directly -- creationSettings owned the only remaining reference
        shape.Release() // drop this recipe's own defensive hold from createStandaloneBoxShape
        return { refCountBeforeHandoff, refCountAfterHandoff }
    },

    // Backs the `BoxShape` manual entry: "hands it to PhysicsSystem.setShape, which AddRefs it
    // (BodyInterface.SetShape)", mirroring ZoneSceneObject.ts's setSensorProperties.
    "BodyInterface.SetShape": jolt => {
        const ctx = createMinimalPhysicsSystem(jolt)
        const initialShape = createStandaloneBoxShape(jolt)
        const body = createBody(jolt, ctx, initialShape)
        ctx.bodyInterface.AddBody(body.GetID(), jolt.EActivation_DontActivate)

        const newShape = createStandaloneBoxShape(jolt)
        const refCountBeforeHandoff = newShape.GetRefCount()
        ctx.bodyInterface.SetShape(body.GetID(), newShape, false, jolt.EActivation_DontActivate)
        const refCountAfterHandoff = newShape.GetRefCount()
        newShape.Release() // drop this recipe's own defensive hold -- the body now holds its own

        ctx.bodyInterface.RemoveBody(body.GetID())
        ctx.bodyInterface.DestroyBody(body.GetID())
        ctx.destroy()
        return { refCountBeforeHandoff, refCountAfterHandoff }
    },

    // Backs the `ConvexHullShapeSettings` manual entry: "AddShape([Const] ShapeSettings inShape)
    // AddRefs its sub-shape settings", from PhysicsSystem.ts's createConvexShapeSettingsFromPart.
    "CompoundShapeSettings.AddShape": jolt => {
        const points = new jolt.ArrayVec3()
        const rawPoints = [
            new jolt.Vec3(0, 0, 0),
            new jolt.Vec3(1, 0, 0),
            new jolt.Vec3(0, 1, 0),
            new jolt.Vec3(0, 0, 1),
        ]
        for (const p of rawPoints) points.push_back(p) // push_back CLONES ([Const, Ref] arg)
        for (const p of rawPoints) jolt.destroy(p)
        const subShapeSettings = new jolt.ConvexHullShapeSettings()
        subShapeSettings.mPoints = points
        jolt.destroy(points)
        const refCountBeforeHandoff = subShapeSettings.GetRefCount()

        const compoundSettings = new jolt.StaticCompoundShapeSettings()
        const pos = new jolt.Vec3(0, 0, 0)
        const rot = new jolt.Quat(0, 0, 0, 1)
        compoundSettings.AddShape(pos, rot, subShapeSettings, 0)
        jolt.destroy(pos)
        jolt.destroy(rot)
        const refCountAfterHandoff = subShapeSettings.GetRefCount()

        jolt.destroy(compoundSettings) // never destroy `subShapeSettings` directly
        return { refCountBeforeHandoff, refCountAfterHandoff }
    },

    // Backs the `WheelSettingsWV` manual entry: "`vehicleSettings.mWheels.push_back` AddRefs it."
    "VehicleConstraintSettings.mWheels.push_back": jolt => {
        const wheelSettings = new jolt.WheelSettingsWV()
        const refCountBeforeHandoff = wheelSettings.GetRefCount()

        const vehicleSettings = new jolt.VehicleConstraintSettings()
        vehicleSettings.mWheels.push_back(wheelSettings)
        const refCountAfterHandoff = wheelSettings.GetRefCount()

        jolt.destroy(vehicleSettings) // never destroy `wheelSettings` directly
        return { refCountBeforeHandoff, refCountAfterHandoff }
    },
}

// --- Constraint lifecycle: a distinct shape from the recipes above, because the correct teardown
// after handoff is an explicit *removal* (PhysicsSystem.RemoveConstraint), never JOLT.destroy() --
// per the `Constraint` manual entry, RemoveConstraint frees the underlying object as sole owner,
// so destroy()ing the JS handle afterward would be a live use-after-free (out of scope here, see
// branch 282). Backs both `Constraint` and (via the same AddConstraint/RemoveConstraint mechanism
// on the shared base class) `VehicleConstraint`'s manual entries.
export function constraintHandoffProof(jolt: JoltModule): HandoffProof {
    const ctx = createMinimalPhysicsSystem(jolt)
    const shape1 = createStandaloneBoxShape(jolt)
    const shape2 = createStandaloneBoxShape(jolt)
    const body1 = createBody(jolt, ctx, shape1)
    const body2 = createBody(jolt, ctx, shape2)
    const constraintSettings = new jolt.HingeConstraintSettings()
    const constraint = constraintSettings.Create(body1, body2)
    jolt.destroy(constraintSettings)

    const refCountBeforeHandoff = constraint.GetRefCount()
    ctx.physicsSystem.AddConstraint(constraint)
    const refCountAfterHandoff = constraint.GetRefCount()

    ctx.physicsSystem.RemoveConstraint(constraint) // correct release -- never JOLT.destroy(constraint)
    ctx.bodyInterface.DestroyBody(body1.GetID())
    ctx.bodyInterface.DestroyBody(body2.GetID())
    ctx.destroy()
    return { refCountBeforeHandoff, refCountAfterHandoff }
}

// Backs the `WheeledVehicleControllerSettings` manual entry: "`VehicleConstraintSettings.mController`
// ... CONSUMES the assigned WheeledVehicleControllerSettings", freed only by the parent's own
// destroy(), never independently. No refcount involved (plain pointer store, not AddRef-based),
// so this just proves the parent's own teardown doesn't crash.
export function controllerSettingsConsumptionIsClean(jolt: JoltModule): void {
    const controllerSettings = new jolt.WheeledVehicleControllerSettings()
    const vehicleConstraintSettings = new jolt.VehicleConstraintSettings()
    vehicleConstraintSettings.mController = controllerSettings
    jolt.destroy(vehicleConstraintSettings) // never destroy `controllerSettings` directly
}
