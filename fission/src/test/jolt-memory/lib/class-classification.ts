// Classifies classes with a nonzero live-count delta in `real-lifecycle.test.ts` into one of three
// buckets, so `diffLiveCountsFiltered` (instrumentation.ts) asserts only on the bucket that means
// something. Grown on demand, same pattern as `KNOWN_UNSEEDED_HANDOFFS`/`factories.ts`. Manual once
// per class, reused forever. Do not speculatively add classes that haven't shown a nonzero delta.
//
// Attribution method: compare `real-lifecycle.test.ts`'s 5-cycle run against a zero-cycle
// construct+destroy-only run to separate one-time constructor/teardown artifacts (delta identical
// with or without cycles) from per-cycle contributions (delta grows with cycles).
//
// Bucket definitions (exactly these three):
// - PERMANENT_SINGLETON: constructed once for the app/process lifetime, never destroyed by design.
//   Evidence required: a source comment or documented invariant, cited by file:line.
// - INTERNAL_REF_UNPROVABLE: the class's *entire* observed delta traces to object(s) whose JS
//   wrapper is, by documented ownership convention, never individually destroy()'d, via either a
//   `returnOwnership: "INTERNAL_REF"` getter or a documented `CONSUMED` handoff, so the cache entry
//   structurally cannot decrement regardless of whether the containing object is torn down.
//   Every contributing code path must have this evidence. If even one path lacks it, the class
//   stays in MUST_RETURN_TO_BASELINE.
// - MUST_RETURN_TO_BASELINE: everything else. No exceptions.
export type OwnershipBucket = "PERMANENT_SINGLETON" | "INTERNAL_REF_UNPROVABLE" | "MUST_RETURN_TO_BASELINE"

export type ClassClassification = {
    bucket: OwnershipBucket
    reason: string
}

export const CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
    // --- PERMANENT_SINGLETON ---
    // None confirmed. A BroadPhaseLayer candidate didn't survive cross-checking against the
    // generated ownership table and docs.

    // --- INTERNAL_REF_UNPROVABLE ---
    AABox: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Shape.GetLocalBounds()/Body.GetWorldSpaceBounds() (`[Value]`, non-constructor, a " +
            "STATIC_ALIAS, see docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md) are called from " +
            "PhysicsSystem.ts's wheel-radius resolution and MirabufSceneObject.ts's " +
            "postGizmoCreation. Same fixed +1 cache entry as RMat44/ShapeResult above. Confirmed " +
            "under ASan that destroy()ing either return is a bad-free, and the binder's pointer " +
            "cache only shrinks on JOLT.destroy(), never legitimate here.",
    },
    BodyInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.GetBodyInterface() returnOwnership=INTERNAL_REF (ownership-table.generated.json). " +
            "Parent _joltPhysSystem is never independently destroy()'d, per PhysicsSystem.ts:1494-1495's " +
            'comment "Don\'t destroy BodyInterface: it\'s a value member of PhysicsSystem, not a heap ' +
            'allocation, so freeing it corrupts the heap." Single call site (constructor), delta identical ' +
            "(1) with zero cycles.",
    },
    PhysicsSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.GetPhysicsSettings() returnOwnership=INTERNAL_REF. docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:500-501 confirms 'Mutate fields in place, never destroy()'. Only call site is " +
            "PhysicsSystem.ts:191-193 (constructor), delta identical (1) with zero cycles.",
    },
    PhysicsSystem: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "JoltInterface.GetPhysicsSystem() returnOwnership=INTERNAL_REF. docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:508 states 'Obtained from JoltInterface.GetPhysicsSystem() (an INTERNAL_REF). Never " +
            "destroy().' Only call site is PhysicsSystem.ts:182 (constructor), delta identical (1) with zero " +
            "cycles. JoltInterface itself IS destroyed (PhysicsSystem.ts:1496), but that frees the underlying " +
            "C++ object without ever calling JOLT.destroy() on this specific JS wrapper, so the cache entry " +
            "cannot decrement either way. Same structural blind spot as BodyInterface/PhysicsSettings above.",
    },
    MassProperties: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:136-137 explicitly documents " +
            "'BodyCreationSettings.mMassPropertiesOverride.mMass = number, COPIED (number). No Ownership " +
            "Concerns.' Only call site is PhysicsSystem.ts:342 (createBody, mass-truthy path). The " +
            "intermediate struct-member wrapper this line reads is never destroy()'d by convention, same as " +
            "other INTERNAL_REF struct-member accessors (e.g. PhysicsSettings above).",
    },
    HingeConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "SimulationSystem.ts's driver-construction dispatch (and this suite's driver-lifecycle.test.ts, " +
            "mirroring it) does `JOLT.castObject(x.primaryConstraint, JOLT.HingeConstraint)` before handing " +
            "the cast to `new HingeDriver(...)`. `JOLT.castObject` does not change ownership (docs/JOLT_" +
            "FUNCTIONS_OWNERSHIP_INVARIANTS.md:898-899: 'the result aliases the same object, do not destroy() " +
            "the cast result separately') but caches the wrapper under the *target* class, separate from the " +
            "`Constraint`-keyed cache entry the Constraint row above already accounts for. Same binder " +
            "polymorphic-return-caching quirk as the BoxShape entry above, just via castObject instead of a " +
            "polymorphic return type. The underlying object is freed via RemoveConstraint's pure refcounting " +
            "(destroyMechanism, never a direct JOLT.destroy() call, see the Constraint entry above), so " +
            "neither cache entry for it can ever decrement. Delta tracks 1:1 with HingeDriver construction " +
            "count.",
    },
    SliderConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Same `JOLT.castObject` cache-aliasing blind spot as HingeConstraint above, via " +
            "`JOLT.castObject(x.primaryConstraint, JOLT.SliderConstraint)` ahead of `new SliderDriver(...)` " +
            "(SimulationSystem.ts's driver-construction dispatch). Delta tracks 1:1 with SliderDriver " +
            "construction count.",
    },
    VehicleEngine: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "WheeledVehicleController.GetEngine() returnOwnership=INTERNAL_REF (ownership-table.generated." +
            "json). WheelDriver.ts's constructor reads `controller.GetEngine().mMaxTorque` once per " +
            "WheelDriver, an INTERNAL_REF getter's return, read once and dropped, never destroy()'d by " +
            "convention. Delta tracks 1:1 with WheelDriver construction count.",
    },
    Wheel: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraint.GetWheel(index) returnOwnership=INTERNAL_REF, declared return type `Wheel` " +
            "(ownership-table.generated.json). The call itself caches a wrapper under `Wheel` before " +
            "WheelDriver.ts's constructor even runs `JOLT.castObject(..., JOLT.WheelWV)` on it (see WheelWV " +
            "below for the second, cast-side cache entry). Never destroy()'d by convention. Delta tracks 1:1 " +
            "with WheelDriver construction count.",
    },
    WheelWV: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "WheelDriver.ts's constructor does `JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV)` " +
            "once per WheelDriver. Same castObject cache-aliasing blind spot as HingeConstraint above, " +
            "layered on top of the `Wheel` entry's own INTERNAL_REF cache entry (see Wheel above). Never " +
            "destroy()'d by convention. Delta tracks 1:1 with WheelDriver construction count.",
    },
    VehicleController: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraint.GetController() returnOwnership=INTERNAL_REF, declared return type " +
            "`VehicleController` (ownership-table.generated.json). The call itself caches a wrapper under " +
            "`VehicleController` before WheelDriver.ts's constructor casts it to `WheeledVehicleController` " +
            "(see WheeledVehicleController below). Same Wheel/WheelWV-pair pattern. Delta tracks 1:1 with " +
            "WheelDriver construction count.",
    },
    WheeledVehicleController: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "WheelDriver.ts's constructor does `JOLT.castObject(this._constraint.GetController(), JOLT." +
            "WheeledVehicleController)` once per WheelDriver. Same castObject cache-aliasing blind spot as " +
            "HingeConstraint above, layered on top of the `VehicleController` entry's own INTERNAL_REF cache " +
            "entry (see VehicleController above). Delta tracks 1:1 with WheelDriver construction count.",
    },
    MotorSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "HingeConstraint/SliderConstraint.GetMotorSettings() returnOwnership=INTERNAL_REF (docs/JOLT_" +
            "FUNCTIONS_OWNERSHIP_INVARIANTS.md:462-463: 'Mutate in place, never destroy()'). HingeDriver.ts/" +
            "SliderDriver.ts call it once in their constructor and again on every `maxAcceleration` setter " +
            "call (HingeDriver.ts:91/SliderDriver.ts:40). Each call hands back a fresh JS wrapper over the " +
            "same underlying struct member, never destroy()'d by convention. Same INTERNAL_REF struct-member-" +
            "accessor pattern as SpringSettings above.",
    },
    SpringSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "MotorSettings.mSpringSettings returnOwnership=INTERNAL_REF (docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:467-469: 'embedded spring settings ... Do not destroy() the SpringSettings'). " +
            "HingeDriver.ts:125/SliderDriver.ts:71 each read `motorSettings.mSpringSettings` once per driver " +
            "construction to set .mFrequency/.mDamping before writing it back. Same INTERNAL_REF struct-" +
            "member-accessor pattern as MassProperties/PhysicsSettings above, a fresh JS wrapper never " +
            "destroy()'d by convention. Delta tracks 1:1 with driver construction count (one HingeDriver + one " +
            "SliderDriver per Mechanism in driver-lifecycle.test.ts).",
    },
    ObjectLayerPairFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:455-460: 'CONSUMED once assigned to " +
            "JoltSettings.mObjectLayerPairFilter ... Do not destroy() after hand-off' and :386-390 confirms " +
            "JoltInterface's destructor frees it. Only constructed once, in setupCollisionFiltering " +
            "(PhysicsSystem.ts:1842). Delta identical (1) with zero cycles.",
    },
    BroadPhaseLayerInterfaceTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:383-390: assigned to " +
            "JoltSettings.mBroadPhaseLayerInterface, CONSUMED, freed by JoltInterface's destructor, JS must not " +
            "destroy() after hand-off. Only constructed once (PhysicsSystem.ts:1869). Delta identical (1) with " +
            "zero cycles.",
    },
    ObjectVsBroadPhaseLayerFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:464-472: assigned to " +
            "JoltSettings.mObjectVsBroadPhaseLayerFilter, CONSUMED, freed by JoltInterface's destructor. Only " +
            "constructed once (PhysicsSystem.ts:1879). Delta identical (1) with zero cycles.",
    },
    ObjectLayerPairFilter: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same ObjectLayerPairFilterTable instance above (same CONSUMED " +
            "hand-off, same doc citation, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:459-460). Delta identical " +
            "(1) with zero cycles, tracking ObjectLayerPairFilterTable 1:1.",
    },
    BroadPhaseLayerInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same BroadPhaseLayerInterfaceTable instance above (same CONSUMED " +
            "hand-off, same doc citation, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:383-390). Delta identical " +
            "(1) with zero cycles, tracking BroadPhaseLayerInterfaceTable 1:1.",
    },
    Body: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Body is `[NoDelete]` (docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:88-89: 'bodies are owned by " +
            "Jolt's body manager and are created/destroyed only through BodyInterface. Never destroy() a " +
            "Body.'). Confirmed: `JOLT.destroy()` on a Body, whether obtained from " +
            "`BodyInterface.CreateBody()` or a contact-listener's wrapPointer'd callback arg, throws 'Cannot " +
            "destroy object. (Did you create it yourself?)'. The observed delta tracks the number of distinct " +
            "heap addresses the WASM allocator ever handed out for a Body across the run, not a leak: " +
            "create/destroy/create cycles on the same freed slot are a cache *hit* (confirmed: the " +
            "JS wrapper returned by a later CreateBody() at a reused address is === the earlier one), so this " +
            "only grows on a genuinely new address, and there is no API that can ever clear an entry once made.",
    },
    ArrayVec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ConvexHullShapeSettings.mPoints is a `[Value] attribute ArrayVec3` (jolt/JoltJS.idl:1040). Each " +
            "read of the getter (PhysicsSystem.ts:1168, createConvexShapeSettingsFromPart, once per convex " +
            "part) hands back a fresh wrapper over the settings' own internal array, never destroy()'d by " +
            "convention. Same INTERNAL_REF struct-member-accessor pattern as MassProperties/PhysicsSettings " +
            "above. Delta tracks 1:1 with the number of convex (dynamic) parts createBodiesFromParser " +
            "processes in a run (2 for this suite's synthetic chassis+wheel fixture).",
    },
    Vec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "AABox.mMin/mMax are `[Value] attribute Vec3` (jolt/JoltJS.idl:666-667). Same INTERNAL_REF " +
            "struct-member-accessor pattern as ArrayVec3/MassProperties/PhysicsSettings above, each read " +
            "hands back a fresh wrapper never destroy()'d by convention. Call sites on the wheel-radius path " +
            "read both fields per wheel joint: resolveWheelRadii's inferWheelRadius (URDFWheelPhysics.ts:37, " +
            "non-URDF branch) and createWheelConstraint's own bounds destructuring (PhysicsSystem.ts, " +
            "non-URDF wheelDimensions branch). Two other real (fixable) contributors found alongside this: " +
            "WheelSettings.mPosition (jolt/JoltJS.idl:3557) not destroy()ing the Vec3 handed to it, and " +
            "StaticCompoundShapeSettings.AddShape's `translation`/`rotation` args (Vec3/Quat) wrongly believed " +
            "CONSUMED when the IDL (jolt/JoltJS.idl:1097) says `[Const, Ref]` (borrowed). Both call sites now " +
            "destroy() their handles (see createWheelConstraint/createBodiesFromParser's constructPartDefinition). " +
            "A third contributor: RMat44.GetTranslation() (convertJoltMat44ToThreeMatrix4, TypeConversions.ts, " +
            "called from MirabufSceneObject.ts's updateMeshTransforms per rigid node per frame) aliases a scratch " +
            "buffer rather than returning a fresh copy. Confirmed: destroy()ing it crashes the real " +
            "browser WASM build ('memory access out of bounds', TypeConversions.test.ts's compareMat already " +
            "worked around this). Never destroy()'d.",
    },
    Quat: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "RMat44.GetQuaternion() (convertJoltMat44ToThreeMatrix4, TypeConversions.ts, called from " +
            "MirabufSceneObject.ts's updateMeshTransforms per rigid node per frame) aliases the same kind of " +
            "scratch buffer as GetTranslation() above rather than returning a fresh `[Value]` copy. Confirmed: " +
            "destroy()ing the copy convertJoltMat44ToThreeMatrix4 obtains internally, while " +
            "TypeConversions.test.ts's compareMat independently re-fetches and destroys its own " +
            "jM.GetQuaternion() call on the same RMat44, is a double-free of that shared scratch ('memory access " +
            "out of bounds'/'index out of bounds', real browser WASM build only, did not reproduce under this " +
            "suite's plain-Node Jolt build). Never destroy() the copy obtained inside convertJoltMat44ToThree" +
            "Matrix4.",
    },
    Mat44: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "TwoBodyConstraint.GetConstraintToBody1Matrix()/GetConstraintToBody2Matrix() (`[Value]`, " +
            "non-constructor, a STATIC_ALIAS) are called from SynthesisBrain.ts's " +
            "createSkidSteerDriveBehavior (left/right wheel split detection). Same fixed +1 cache-entry " +
            "pattern as RMat44 below: the first call in a test run adds exactly one entry to the Mat44 " +
            "cache that persists for the process's lifetime, never legitimate to destroy() (would free a " +
            "non-heap scratch address every later GetConstraintToBody1Matrix() call still relies on being " +
            "intact, confirmed by synthesis-brain-lifecycle.test.ts, which read the same return twice in a " +
            "row and asserted the values agreed).",
    },
    RMat44: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Body.GetWorldTransform() (`[Value]`, non-constructor, a STATIC_ALIAS, see " +
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md) is called from ZoneSceneObject.ts's " +
            "setup()/update() and MirabufSceneObject.ts's updateMeshTransforms. The binder's pointer " +
            "cache only removes an entry on JOLT.destroy() (never legitimate here, confirmed under ASan " +
            "that destroy()ing it is a real bad-free), so the first call in a test run adds exactly one " +
            "entry to the RMat44 cache that persists for the process's lifetime. A fixed +1, not an " +
            "unbounded leak, and delta stays 1 regardless of how many times GetWorldTransform() is called " +
            "afterward, since every later call reuses the same static pointer already in the cache.",
    },
    RVec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ContactListenerJS.OnContactValidate's `baseOffset` is `JOLT.wrapPointer(inBaseOffsetPtr, " +
            "JOLT.RVec3)` (PhysicsSystem.ts:setUpContactListener), per jolt/JoltJS.h:626-632, this aliases " +
            "the physics engine's own transient call-stack argument, never a heap allocation JS owns (see " +
            "releaseContactEventPayload's comment for why it must never be destroy()'d). Each real contact-" +
            "validate callback during simulation (this suite's mirabuf-lifecycle.test.ts overlaps a wheel and " +
            "chassis body on purpose) wraps a distinct address, so the RVec3 cache grows by one per callback " +
            "and can never shrink without a destroy() that would corrupt memory.",
    },
    Shape: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createBodiesFromParser's compound Shape (from ShapeResult.Get()) is correctly never raw-destroy()'d. " +
            "It's handed to BodyCreationSettings (which AddRefs it), and the ShapeResult wrapper around it is " +
            "only ever `.Clear()`'d (never destroy()'d, see the ShapeResult entry below and " +
            "[[jolt_refcounted_destroy_danger]]/handoff-timing.test.ts's variant (b)). " +
            "The Body then owns the only remaining reference and frees it correctly via refcounting when the " +
            "Body itself is destroyed, but per instrumentation.ts's documented cache-aliasing note (also " +
            "integration.test.ts's second test), a RefTarget freed via pure C++ refcounting (never passed to " +
            "JOLT.destroy() itself) never has its JS wrapper cache entry removed. Delta tracks 1:1 with the " +
            "number of bodies createBodiesFromParser creates (2 for this suite's synthetic fixture).",
    },
    BoxShape: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ZoneSceneObject.ts's setSensorProperties builds a fresh BoxShape (via `new " +
            "JOLT.BoxShapeSettings(...).Create().Get()`) and hands it to PhysicsSystem.setShape, which " +
            "AddRefs it (BodyInterface.SetShape). Same pattern as the Shape entry above, just cached " +
            "under the constructor's exact subtype instead of the declared `Shape` return type (the " +
            "binder's polymorphic-return caching quirk, see the codegen plan notes). Correctly never " +
            "raw-destroy()'d, freed via refcounting when the body's shape is next replaced or the body " +
            "itself is destroyed. Delta tracks 1:1 with the number of setSensorProperties calls " +
            "(setup + update = 2 for this suite's fixture).",
    },
    ShapeResult: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ShapeSettings.Create() (`[Value]`, non-constructor, a STATIC_ALIAS, see " +
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md) is called throughout PhysicsSystem.ts/" +
            "ZoneSceneObject.ts to build colliders/sensors. Confirmed under ASan that JOLT.destroy()ing the " +
            "returned ShapeResult is a bad-free. `.Clear()` (a real method, drops its internal `Ref<Shape>`) " +
            "is the correct cleanup and is used everywhere instead. Same fixed +1 cache entry as RMat44 above. " +
            "The binder's pointer cache only shrinks on JOLT.destroy(), never legitimate here.",
    },
    ConvexHullShapeSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "CompoundShapeSettings.AddShape([Const] ShapeSettings inShape) AddRefs its sub-shape settings " +
            "(confirmed: GetRefCount() goes 0->1 across the call, jolt/JoltJS.idl:1055) and holds " +
            "that reference until the compound settings object itself is destroyed, at which point it's " +
            "correctly freed via refcounting (confirmed: reading GetRefCount() on the sub-shape " +
            "settings after JOLT.destroy()ing the already-existing compoundShapeSettings in " +
            "createBodiesFromParser returns garbage, i.e. it's already gone). Explicitly destroy()ing it " +
            "ourselves beforehand, which looked like the obvious fix for this class's delta, is a live " +
            "double-free, not a leak fix (same hazard as [[jolt_refcounted_destroy_danger]]). Same cache-" +
            "aliasing blind spot as Shape above. Delta tracks 1:1 with convex parts processed.",
    },
    Constraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "*ConstraintSettings.Create() hands back a Constraint at refcount 0 (confirmed). " +
            "PhysicsSystem.AddConstraint takes the only real reference (0->1), and RemoveConstraint releases " +
            "it and, being the sole owner, frees the underlying C++ object right there (confirmed: " +
            "GetRefCount() reads garbage immediately after RemoveConstraint once the constraint was actually " +
            "simulated). destroyMechanism/destroy() correctly call RemoveConstraint only, never destroy() " +
            "afterward (see destroyMechanism's comment, destroy()ing here is a live use-after-free, not a " +
            "leak fix, the mistake this suite's mirabuf-lifecycle.test.ts caught first). Same cache-aliasing " +
            "blind spot as Shape/ConvexHullShapeSettings above: the JS wrapper cache entry for a RefTarget " +
            "freed via pure refcounting is never removed without a destroy() call that would corrupt memory.",
    },
    VehicleConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraint implements the same Constraint lifecycle (jolt/JoltJS.idl:3490-3500) as the " +
            "Constraint entry above: identical AddConstraint/RemoveConstraint refcounting and the same JS " +
            "wrapper cache-aliasing blind spot once RemoveConstraint frees it as the sole owner. WheelDriver.ts's " +
            "constructor adds a second, independent contributor to this same class's cache: " +
            "`JOLT.castObject(x.primaryConstraint, JOLT.VehicleConstraint)` in SimulationSystem.ts's driver-" +
            "construction dispatch (driver-lifecycle.test.ts) caches its own wrapper under `VehicleConstraint` " +
            "too. Same castObject cache-aliasing blind spot as HingeConstraint above, on top of the " +
            "refcounting one this entry already covers.",
    },
    BodyLockInterfaceLocking: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.GetBodyLockInterface() returnOwnership=INTERNAL_REF (docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:511), same pattern as BodyInterface/PhysicsSettings above. Only call site is " +
            "PhysicsSystem.ts's getBody() (used throughout createJointsFromParser/resolveWheelRadii). Every " +
            "call returns the same underlying address (a single object owned by PhysicsSystem for its whole " +
            "lifetime), so repeat calls are cache hits. Delta stays at 1 regardless of call count.",
    },
    CollideShapeResult: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Same call site and reasoning as RVec3 above: ContactListenerJS.OnContactValidate's " +
            "`collisionResult` is `JOLT.wrapPointer(inCollisionResultPtr, JOLT.CollideShapeResult)`, a " +
            "`[Const, Ref] CollideShapeResult` (jolt/JoltJS.idl:2185) aliasing the physics engine's own " +
            "transient call-stack argument, never destroy()'d, never heap-owned by JS.",
    },
    ContactManifold: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ContactListenerJS.OnContactAdded/OnContactPersisted's `manifold` is `JOLT.wrapPointer(manifoldPtr, " +
            "JOLT.ContactManifold)`, a `[Const, Ref] ContactManifold` (jolt/JoltJS.idl:2186-2187) aliasing the " +
            "physics engine's own transient call-stack argument, same reasoning as RVec3/CollideShapeResult above.",
    },
    ContactSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ContactListenerJS.OnContactAdded/OnContactPersisted's `settings` is `JOLT.wrapPointer(settingsPtr, " +
            "JOLT.ContactSettings)`, a `[Ref] ContactSettings` (jolt/JoltJS.idl:2186-2187), an INOUT parameter " +
            "the physics engine reads back after the callback returns. Same transient-alias reasoning as above.",
    },
    ArrayFloat: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleController's `controllerSettings.mTransmission.mGearRatios` (a `[Value]` array " +
            "attribute) is read once per wheel joint (ConstraintSettingsUtilities.ts). Same INTERNAL_REF " +
            "struct-member-accessor pattern as ArrayVec3/mPoints above.",
    },
    ArrayVehicleAntiRollBar: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleConstraint's `vehicleSettings.mAntiRollBars.clear()` reads the `[Value]` array " +
            "attribute once per wheel joint. Same INTERNAL_REF struct-member-accessor pattern as ArrayVec3/" +
            "ArrayFloat above.",
    },
    ArrayWheelSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleConstraint's `vehicleSettings.mWheels.clear()`/`.push_back(wheelSettings)` reads the " +
            "`[Value]` array attribute once per wheel joint. Same INTERNAL_REF struct-member-accessor pattern " +
            "as ArrayVec3/ArrayFloat/ArrayVehicleAntiRollBar above.",
    },
    WheelSettingsWV: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "WheelSettings is RefTarget (jolt/JoltJS.idl:3551-3555). `vehicleSettings.mWheels.push_back` " +
            "AddRefs it (same confirmed pattern as ConvexHullShapeSettings/CompoundShapeSettings.AddShape " +
            "above), and it's correctly freed via refcounting once `vehicleSettings` itself is destroy()'d " +
            "(createVehicleConstraint), never raw-destroy()'d directly. Same cache-aliasing blind spot.",
    },
    VehicleCollisionTesterCastCylinder: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleCollisionTester is RefTarget (jolt/JoltJS.idl:3453-3457). SetVehicleCollisionTester AddRefs " +
            "it and destroyMechanism correctly Release()s the mechanism's own claim (never raw-destroy()s it, " +
            "see destroyMechanism's comment), so it's freed via refcounting once the constraint that held the " +
            "other reference is also gone. Same cache-aliasing blind spot as Shape/Constraint above.",
    },
    WheeledVehicleControllerSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "`VehicleConstraintSettings.mController` (jolt/JoltJS.idl, no `[Value]` annotation) CONSUMES the " +
            "assigned WheeledVehicleControllerSettings. Confirmed: JOLT.destroy()ing the parent " +
            "VehicleConstraintSettings already frees it, a second JOLT.destroy() on the controller settings " +
            "handle crashes with a WASM out-of-bounds access (same double-free hazard as " +
            "[[jolt_refcounted_destroy_danger]], here via plain ownership-transfer rather than refcounting). " +
            "createVehicleConstraint correctly never destroy()s it directly, and the parent vehicleSettings " +
            "destroy() (same function) does the job. Same cache-aliasing blind spot as Shape/Constraint above.",
    },
    SubShapeIDPair: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ContactListenerJS.OnContactRemoved's `shapePair` is `JOLT.wrapPointer(subShapePairPtr, " +
            "JOLT.SubShapeIDPair)`, a `[Const, Ref] SubShapeIDPair` (jolt/JoltJS.idl:2188) aliasing the " +
            "physics engine's own transient call-stack argument, same reasoning as RVec3/ContactManifold " +
            "above. Never destroy()'d even though this dispatch is synchronous (see setUpContactListener's " +
            "comment on OnContactRemoved for why sync-vs-deferred doesn't matter here).",
    },
    BodyID: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "The one real leak here, PhysicsSystem.ts's OnContactAdded handler constructing fresh, " +
            "caller-owned `new JOLT.BodyID(...)` copies (COPY ownership) and queuing them for deferred dispatch, " +
            "is fixed: `PhysicsSystem.update()` now destroy()s them once the queued event has dispatched. " +
            "The entire remaining delta traces to `Body.GetID()` (INTERNAL_REF into a live Body's internal " +
            "member, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:91-93, 'Do not destroy()'), which aliases its " +
            "parent Body's address 1:1 and inherits the exact same `[NoDelete]` structural block as Body above. " +
            "Confirmed: this class's delta now matches Body's delta exactly on every run.",
    },
    MotionProperties: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Body.GetMotionProperties() returnOwnership=INTERNAL_REF (docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:420-422: 'Obtained from Body.GetMotionProperties() (an INTERNAL_REF).'). Call site is " +
            "MirabufSceneObject.ts's updateMeshTransforms (per rigid node, per frame, only " +
            "GetInverseMass()/NONE is read). Every call aliases the same live Body's own member, never " +
            "destroy()'d by convention, same struct-member-accessor pattern as PhysicsSettings/MassProperties " +
            "above. Delta tracks 1:1 with the number of distinct dynamic bodies whose motion properties are " +
            "read across the run (2 for this suite's synthetic chassis+wheel fixture).",
    },
}

// Any nonzero-delta class not in CLASS_CLASSIFICATION is intentionally not defaulted anywhere.
// See `diffLiveCountsFiltered` in instrumentation.ts, which throws loudly instead.
