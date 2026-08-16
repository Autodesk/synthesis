// Ported from the `memory-audit` branch's `fission/src/test/jolt-memory/lib/class-classification.ts`,
// where every entry below was attributed empirically (not guessed) against a real ASan build and
// this repo's `docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md`. That doc exists in this branch too and
// still matches the concepts cited here (INTERNAL_REF, [NoDelete], CONSUMED, castObject aliasing),
// spot-checked while porting this table, but the exact file:line citations were captured on the
// memory-audit branch's snapshot of the app code and may have drifted since -- treat them as
// pointers to re-verify, not as guaranteed-current line numbers.
//
// Classifies classes with a nonzero live-count delta into one of three buckets, so
// `diffLiveCountsFiltered` (`JoltLeakDetection.ts`) asserts only on the bucket that means
// something. Grown on demand: when the automatic per-test check (`JoltLeakDetectionSetup.ts`)
// throws on an unclassified class, that is a prompt to go read the actual call path (same
// discipline every entry below was built with) and add an entry -- not to guess one in. Do not
// speculatively add classes that haven't shown a nonzero delta.
//
// A big caveat inherited as-is from the original design: this only checks "is this class known,"
// not "is this exact delta size/call-site expected." Once a class is classified, ANY future
// nonzero delta for it -- including from a real, unrelated leak in different code -- is silently
// accepted. It is not a substitute for reading a new leak's cause before reusing an existing entry.
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

// Split in two, as of the `jolt-ownership` codegen pipeline (`fission/scripts/jolt-ownership/`):
//
// - `GENERATED_CLASS_CLASSIFICATION` (imported below) is machine-checked: every entry's reason
//   cites the exact generated glue.cpp line (ground truth for what the binder's C++ actually does,
//   compiled from `jolt/JoltJS.idl` via `webidl_binder.py`) and the fission/src production call
//   site(s) that exercise it. A class only lands there if EVERY production call site producing an
//   instance of it is mechanically provable non-owned. Regenerate with
//   `bun run jolt-ownership:generate` when `jolt/JoltJS.idl` or fission's Jolt call sites change --
//   see `fission/scripts/jolt-ownership/README.md`.
// - `MANUAL_CLASS_CLASSIFICATION` below is everything the tool can't prove, most commonly because
//   the class is `RefTarget`-derived (has `AddRef`/`Release`/`GetRefCount`): a RefTarget's
//   `return self->Method();` in glue.cpp is textually identical whether that method is an
//   innocuous accessor or a delegate into Jolt's own `Create()` that allocates a fresh refcounted
//   object under the hood (confirmed for `*ConstraintSettings.Create()` specifically -- see the
//   `Constraint` entry below) -- proving those needs the actual `AddRef`/`Release` call graph, not
//   just glue.cpp's return-statement shape, which the tool doesn't attempt. Also covers a few
//   classes the tool's call-site scanner doesn't look at yet (`[Value] attribute` field access like
//   `.mWheels`/`.mSpringSettings` is a plain property read in the AST, not a call expression) and
//   the genuinely-conflicting-evidence case (`BroadPhaseLayer`).
import { GENERATED_CLASS_CLASSIFICATION } from "@/test/JoltClassClassification.generated"

const MANUAL_CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
    // --- PERMANENT_SINGLETON ---
    // None confirmed. The memory-audit branch's own audit tried and failed to confirm a
    // BroadPhaseLayer candidate here; this branch's own attempt at it (see that entry under
    // INTERNAL_REF_UNPROVABLE below) is also inconclusive -- read that entry in full before
    // trusting it either way.

    // --- INTERNAL_REF_UNPROVABLE ---
    SpringSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "MotorSettings.mSpringSettings returnOwnership=INTERNAL_REF (docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md: 'embedded spring settings ... Do not destroy() the SpringSettings'). " +
            "HingeDriver.ts/SliderDriver.ts each read `motorSettings.mSpringSettings` once per driver " +
            "construction to set .mFrequency/.mDamping before writing it back. Same INTERNAL_REF struct-" +
            "member-accessor pattern as MassProperties/PhysicsSettings above, a fresh JS wrapper never " +
            "destroy()'d by convention. Delta tracks 1:1 with driver construction count (one HingeDriver + " +
            "one SliderDriver per Mechanism). NOT YET covered by the codegen pipeline: `.mSpringSettings` " +
            "is a `[Value] attribute` field read, a plain property access in the AST rather than a call " +
            "expression, which `fission/scripts/jolt-ownership/scan-callsites.mjs` doesn't scan yet.",
    },
    ObjectLayerPairFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md: 'CONSUMED once assigned to " +
            "JoltSettings.mObjectLayerPairFilter ... Do not destroy() after hand-off', and confirms " +
            "JoltInterface's destructor frees it. Only constructed once, in setupCollisionFiltering. Delta " +
            "identical (1) with zero cycles. The codegen pipeline's glue.cpp parser does detect this exact " +
            "CONSUMED setter shape (`self->mObjectLayerPairFilter = arg0;`, no dereference -- see " +
            "`parse-glue.mjs`'s `setterOwnership` heuristic), but the call-site scanner doesn't yet trace " +
            "constructor-argument flow into a CONSUMED-taking setter, so this class isn't auto-classified " +
            "yet -- a scoped, known extension, not a disagreement.",
    },
    BroadPhaseLayerInterfaceTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md: assigned to " +
            "JoltSettings.mBroadPhaseLayerInterface, CONSUMED, freed by JoltInterface's destructor, JS must " +
            "not destroy() after hand-off. Only constructed once. Delta identical (1) with zero cycles. " +
            "Same not-yet-wired CONSUMED-argument-tracing gap as ObjectLayerPairFilterTable above.",
    },
    ObjectVsBroadPhaseLayerFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md: assigned to " +
            "JoltSettings.mObjectVsBroadPhaseLayerFilter, CONSUMED, freed by JoltInterface's destructor. " +
            "Only constructed once per PhysicsSystem. Delta identical (1) with zero cycles -- reproduced " +
            "on this branch's own leak run (delta of exactly 1 per PhysicsSystem instance across every " +
            "test), matching this entry's original attribution. Same not-yet-wired CONSUMED-argument-" +
            "tracing gap as ObjectLayerPairFilterTable above.",
    },
    ObjectLayerPairFilter: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same ObjectLayerPairFilterTable instance above (same CONSUMED " +
            "hand-off). Delta identical (1) with zero cycles, tracking ObjectLayerPairFilterTable 1:1.",
    },
    BroadPhaseLayerInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same BroadPhaseLayerInterfaceTable instance above (same CONSUMED " +
            "hand-off). Delta identical (1) with zero cycles, tracking BroadPhaseLayerInterfaceTable 1:1.",
    },
    ArrayVec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ConvexHullShapeSettings.mPoints is a `[Value] attribute ArrayVec3`. Each read of the getter " +
            "(PhysicsSystem.ts's createConvexShapeSettingsFromPart, once per convex part) hands back a " +
            "fresh wrapper over the settings' own internal array, never destroy()'d by convention. Same " +
            "INTERNAL_REF struct-member-accessor pattern as MassProperties/PhysicsSettings above. NOT YET " +
            "covered by the codegen pipeline (plain property read, see SpringSettings above).",
    },
    Vec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "AABox.mMin/mMax are `[Value] attribute Vec3`. Same INTERNAL_REF struct-member-accessor " +
            "pattern as ArrayVec3/MassProperties/PhysicsSettings above, each read hands back a fresh " +
            "wrapper never destroy()'d by convention. Call sites on the wheel-radius path read both fields " +
            "per wheel joint. A separate, real contributor confirmed on the memory-audit branch: " +
            "RMat44.GetTranslation() (TypeConversions.ts) aliases a scratch buffer rather than returning a " +
            "fresh copy -- destroy()ing it crashes the real browser WASM build ('memory access out of " +
            "bounds'). Never destroy()'d. NOT machine-classified: the codegen pipeline's call-site scan " +
            "confirms fission/src also has plenty of genuine `new JOLT.Vec3(...)` constructions meant to be " +
            "constructed-and-destroyed (temporaries throughout DragModeSystem.ts, MeshCreation.ts, etc.) --" +
            " mixed in with the same STATIC_ALIAS contributors this entry describes. Classifying the whole " +
            "class here (as this entry already does) means a genuinely leaked, forgotten-to-destroy owned " +
            "Vec3 would be silently accepted too; the tool correctly refuses to call that provably safe. " +
            "Fixing this for real needs a coarser-than-class-level check the leak harness doesn't have " +
            "today, not a smarter classification -- left as a known, pre-existing imprecision, not " +
            "something this pass regressed.",
    },
    Quat: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "RMat44.GetQuaternion() (TypeConversions.ts, called from MirabufSceneObject.ts's " +
            "updateMeshTransforms per rigid node per frame) aliases the same kind of scratch buffer as " +
            "GetTranslation() above rather than returning a fresh `[Value]` copy. Confirmed on the " +
            "memory-audit branch: destroy()ing the copy while another call site independently re-fetches " +
            "and destroys its own GetQuaternion() call on the same RMat44 is a double-free of that shared " +
            "scratch, real browser WASM build only. Never destroy() the copy obtained inside " +
            "convertJoltMat44ToThreeMatrix4. Same not-provable-at-class-level situation as Vec3 above -- " +
            "genuine `new JOLT.Quat(...)` constructions coexist with the STATIC_ALIAS contributors.",
    },
    Mat44: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "TwoBodyConstraint.GetConstraintToBody1Matrix()/GetConstraintToBody2Matrix() (`[Value]`, " +
            "non-constructor, a STATIC_ALIAS) are called from SynthesisBrain.ts's " +
            "createSkidSteerDriveBehavior (left/right wheel split detection). Same fixed +1 cache-entry " +
            "pattern as RMat44 below: the first call in a run adds exactly one entry to the Mat44 cache " +
            "that persists for the process's lifetime, never legitimate to destroy() (would free a " +
            "non-heap scratch address every later call still relies on being intact). Same not-provable-" +
            "at-class-level situation as Vec3 above -- genuine `new JOLT.Mat44(...)` constructions coexist.",
    },
    RVec3: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ContactListenerJS.OnContactValidate's `baseOffset` is `JOLT.wrapPointer(inBaseOffsetPtr, " +
            "JOLT.RVec3)` (PhysicsSystem.ts's contact-listener setup), aliasing the physics engine's own " +
            "transient call-stack argument, never a heap allocation JS owns. Each real contact-validate " +
            "callback during simulation wraps a distinct address, so the RVec3 cache grows by one per " +
            "callback and can never shrink without a destroy() that would corrupt memory. Same not-" +
            "provable-at-class-level situation as Vec3 above -- genuine `new JOLT.RVec3(...)` " +
            "constructions coexist with this wrapPointer contributor.",
    },
    Shape: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createBodiesFromParser's compound Shape (from ShapeResult.Get()) is correctly never " +
            "raw-destroy()'d. It's handed to BodyCreationSettings (which AddRefs it), and the ShapeResult " +
            "wrapper around it is only ever `.Clear()`'d (never destroy()'d, see the ShapeResult entry " +
            "below and [[jolt_refcounted_destroy_danger]]). The Body then owns the only remaining " +
            "reference and frees it correctly via refcounting when the Body itself is destroyed, but per " +
            "this file's own header note on the binder's cache-aliasing blind spot, a RefTarget freed via " +
            "pure C++ refcounting (never passed to JOLT.destroy() itself) never has its JS wrapper cache " +
            "entry removed. Delta tracks 1:1 with the number of bodies createBodiesFromParser creates. " +
            "Confirmed NOT mechanically provable: `Shape` is RefTarget-derived (AddRef/Release/GetRefCount " +
            "all present in glue.cpp), so its `return self->Method();`-shaped INTERNAL_REF returns are " +
            "textually indistinguishable from a delegate into a genuine allocation -- proving this needs " +
            "Jolt's own AddRef/Release call graph, which the codegen pipeline doesn't model. Correctly " +
            "excluded from the generated table for exactly this reason.",
    },
    BoxShape: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ZoneSceneObject.ts's setSensorProperties builds a fresh BoxShape (via `new " +
            "JOLT.BoxShapeSettings(...).Create().Get()`) and hands it to PhysicsSystem.setShape, which " +
            "AddRefs it (BodyInterface.SetShape). Same pattern as the Shape entry above, just cached under " +
            "the constructor's exact subtype instead of the declared `Shape` return type (the binder's " +
            "polymorphic-return caching quirk). Correctly never raw-destroy()'d, freed via refcounting " +
            "when the body's shape is next replaced or the body itself is destroyed. Same RefTarget " +
            "unprovability as Shape above.",
    },
    ConvexHullShapeSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "CompoundShapeSettings.AddShape([Const] ShapeSettings inShape) AddRefs its sub-shape settings " +
            "(confirmed on the memory-audit branch: GetRefCount() goes 0->1 across the call) and holds " +
            "that reference until the compound settings object itself is destroyed, at which point it's " +
            "correctly freed via refcounting. Explicitly destroy()ing a sub-shape settings object oneself " +
            "beforehand, which looks like the obvious fix for this class's delta, is a live double-free, " +
            "not a leak fix (same hazard as [[jolt_refcounted_destroy_danger]]). Same cache-aliasing " +
            "blind spot as Shape above. NOTE: this class also has a separate, real, still-open leak in " +
            "this branch's own `PhysicsSystem.createConvexHull()` (a standalone `new " +
            "JOLT.ConvexHullShapeSettings()` that is never destroy()'d or handed to anything that takes " +
            "ownership) -- classifying the class here silences that leak's signal too, per this file's " +
            "header caveat. Do not treat a nonzero ConvexHullShapeSettings delta as automatically fine; " +
            "the real leak still needs a fix, just not via destroy()ing it inside CompoundShapeSettings-" +
            "owned call paths (confirmed that specific fix corrupts the WASM heap). Same RefTarget " +
            "unprovability as Shape above -- correctly excluded from the generated table.",
    },
    Constraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "*ConstraintSettings.Create() hands back a Constraint at refcount 0 (confirmed on the " +
            "memory-audit branch). PhysicsSystem.AddConstraint takes the only real reference (0->1), and " +
            "RemoveConstraint releases it and, being the sole owner, frees the underlying C++ object right " +
            "there. destroyMechanism/destroy() correctly call RemoveConstraint only, never destroy() " +
            "afterward (destroy()ing here is a live use-after-free, not a leak fix). Same cache-aliasing " +
            "blind spot as Shape/ConvexHullShapeSettings above: the JS wrapper cache entry for a RefTarget " +
            "freed via pure refcounting is never removed without a destroy() call that would corrupt " +
            "memory. Confirmed NOT mechanically provable, concretely: the codegen pipeline's glue.cpp " +
            "parser classifies every `*ConstraintSettings.Create()` as INTERNAL_REF (`return " +
            "self->Create(*inBody1, *inBody2);`), but per docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md this " +
            "is one of the two genuine exceptions where that call shape hides a real fresh heap allocation " +
            "-- exactly the ambiguity `Constraint` being RefTarget-derived predicts. Correctly excluded.",
    },
    VehicleConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraint implements the same Constraint lifecycle as the Constraint entry above: " +
            "identical AddConstraint/RemoveConstraint refcounting and the same JS wrapper cache-aliasing " +
            "blind spot once RemoveConstraint frees it as the sole owner. WheelDriver.ts's constructor " +
            "adds a second, independent contributor to this same class's cache: `JOLT.castObject(x." +
            "primaryConstraint, JOLT.VehicleConstraint)` in SimulationSystem.ts's driver-construction " +
            "dispatch. Same castObject cache-aliasing blind spot as HingeConstraint above, on top of the " +
            "refcounting one this entry already covers. Same RefTarget unprovability as Constraint above.",
    },
    WheelSettingsWV: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "WheelSettings is RefTarget. `vehicleSettings.mWheels.push_back` AddRefs it (same confirmed " +
            "pattern as ConvexHullShapeSettings/CompoundShapeSettings.AddShape above), and it's correctly " +
            "freed via refcounting once `vehicleSettings` itself is destroy()'d, never raw-destroy()'d " +
            "directly. Same cache-aliasing blind spot. Same RefTarget unprovability as Shape above.",
    },
    VehicleCollisionTesterCastCylinder: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleCollisionTester is RefTarget. SetVehicleCollisionTester AddRefs it and destroyMechanism " +
            "correctly Release()s the mechanism's own claim (never raw-destroy()s it), so it's freed via " +
            "refcounting once the constraint that held the other reference is also gone. Same " +
            "cache-aliasing blind spot as Shape/Constraint above. Same RefTarget unprovability.",
    },
    WheeledVehicleControllerSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "`VehicleConstraintSettings.mController` (no `[Value]` annotation) CONSUMES the assigned " +
            "WheeledVehicleControllerSettings. Confirmed on the memory-audit branch: JOLT.destroy()ing the " +
            "parent VehicleConstraintSettings already frees it, a second JOLT.destroy() on the controller " +
            "settings handle crashes with a WASM out-of-bounds access. createVehicleConstraint correctly " +
            "never destroy()s it directly, and the parent vehicleSettings destroy() (same function) does " +
            "the job. Same cache-aliasing blind spot as Shape/Constraint above. The codegen pipeline does " +
            "detect `VehicleConstraintSettings.set_mController` as a CONSUMED setter (raw pointer store, " +
            "no dereference), same not-yet-wired constructor-argument-tracing gap as " +
            "ObjectLayerPairFilterTable above.",
    },
    ArrayFloat: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleController's `controllerSettings.mTransmission.mGearRatios` (a `[Value]` array " +
            "attribute) is read once per wheel joint. Same INTERNAL_REF struct-member-accessor pattern as " +
            "ArrayVec3/mPoints above. NOT YET covered by the codegen pipeline (plain property read).",
    },
    ArrayVehicleAntiRollBar: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleConstraint's `vehicleSettings.mAntiRollBars.clear()` reads the `[Value]` array " +
            "attribute once per wheel joint. Same INTERNAL_REF struct-member-accessor pattern as " +
            "ArrayVec3/ArrayFloat above. NOT YET covered by the codegen pipeline (plain property read).",
    },
    ArrayWheelSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "createVehicleConstraint's `vehicleSettings.mWheels.clear()`/`.push_back(wheelSettings)` reads " +
            "the `[Value]` array attribute once per wheel joint. Same INTERNAL_REF struct-member-accessor " +
            "pattern as ArrayVec3/ArrayFloat/ArrayVehicleAntiRollBar above. NOT YET covered by the codegen " +
            "pipeline (plain property read).",
    },
    BodyID: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "The one real leak here (on the memory-audit branch), PhysicsSystem.ts's OnContactAdded " +
            "handler constructing fresh, caller-owned `new JOLT.BodyID(...)` copies (COPY ownership) and " +
            "queuing them for deferred dispatch, was fixed there by destroy()ing them once the queued " +
            "event dispatches. The remaining delta traces to `Body.GetID()` (INTERNAL_REF into a live " +
            "Body's internal member, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md: 'Do not destroy()'), " +
            "which aliases its parent Body's address 1:1 and inherits the exact same `[NoDelete]` " +
            "structural block as Body above. NOT machine-classified: fission/src genuinely constructs " +
            "owned `new JOLT.BodyID(...)` copies (the fixed leak this entry describes), so the codegen " +
            "pipeline correctly refuses to call the whole class safe -- same class-level-granularity " +
            "limitation as Vec3/Quat/Mat44/RVec3 above, not a disagreement.",
    },
    RayCastResult: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md: 'Accessed as collector.mHit (an INTERNAL_REF " +
            "inside the collector). Do not destroy().' Call site: PhysicsSystem.ts's rayCast() reads " +
            "`collector.mHit` on a hit. Never destroy()'d by convention. NOT YET covered by the codegen " +
            "pipeline (plain property read).",
    },
    BroadPhaseLayer: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.ts's setupCollisionFiltering constructs one BroadPhaseLayer per object layer " +
            "(`new JOLT.BroadPhaseLayer(...)`, delta of exactly 2 + ROBOT_LAYERS.length per PhysicsSystem, " +
            "confirmed never decrementing on system.destroy() in isolation) and hands each one to " +
            "`bpInterface.MapObjectToBroadPhaseLayer(...)`. The evidence here is genuinely conflicting, " +
            "not a clean case like the entries above -- written down in full rather than picking a side: " +
            "(1) docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md generically describes that argument as " +
            "CLONED ('value copied into the table, caller frees'), implying destroy() right after the " +
            "call is correct; (2) PhysicsSystem.ts's own source has an explicit `// WARNING\\n// DO NOT " +
            "FREE` comment directly above these constructions, contradicting the doc; (3) empirically " +
            "adding those destroy() calls and running this repo's full physics/scene/mirabuf test suite " +
            "(real robot/field assets, raycasts, collision filtering, ejectables, mechanism spawning) " +
            "against the plain (non-ASan) build produced zero change in pass/fail results -- no crash, no " +
            "behavior change. That is real signal but NOT proof: a plain build can silently tolerate " +
            "memory corruption that an ASan build would catch (this is the whole reason this suite's ASan " +
            "mode exists), so it does not settle the disagreement between (1) and (2). Classified here " +
            "(matching current, unchanged app behavior) rather than flagged as MUST_RETURN_TO_BASELINE, " +
            "since production code was left as-is pending an ASan-based re-check -- this entry documents " +
            "an open question, not a confirmed blind spot like the others in this file. Do not cite this " +
            "entry as proof the DO NOT FREE comment is safe to remove, and do not copy this reasoning to " +
            "justify skipping destroy() on a *different* BroadPhaseLayer call site. Correctly excluded " +
            "from the generated table: a real `new JOLT.BroadPhaseLayer(...)` construction with genuinely " +
            "disputed destroy()-safety can't be mechanically proven either way.",
    },
    ContactListenerJS: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md states fission 'destroy()s the ContactListenerJS " +
            "it originally created at teardown' -- not trusted at face value, verified directly instead: " +
            "an isolated single `new PhysicsSystem(); system.destroy()` cycle (no other test state) still " +
            "leaves the ContactListenerJS cache at 1, not 0, confirmed via a throwaway diagnostic test. " +
            "PhysicsSystem.ts's destroy() does call `JOLT.destroy(contactListener)` where `contactListener " +
            "= this._joltPhysSystem.GetContactListener()` -- the destroy() call genuinely happens, so this " +
            "is not a missing-teardown bug. Most likely explanation: `GetContactListener()`'s wrapPointer " +
            "resolves to a different cache slot than the one `new JOLT.ContactListenerJS()` inserted at " +
            "construction (setUpContactListener), so destroy() clears a slot that was never the leaked " +
            "one. Same category of binder cache-aliasing blind spot as the castObject-pair entries above " +
            "(HingeConstraint/WheelWV/etc.), just via wrapPointer instead of castObject. NOT machine-" +
            "classified: fission/src's own `new JOLT.ContactListenerJS()` construction is a genuine COPY " +
            "call site, so the codegen pipeline correctly refuses to call this class safe outright -- this " +
            "entry's actual claim (the destroy() call happens but clears the wrong cache slot) is a " +
            "wrapPointer/construction cache-identity question the tool doesn't model.",
    },

    BodyIDMemRef: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "ArrayBodyID.data() is INTERNAL_REF in generated glue.cpp (`return self->data();`, glue.cpp: " +
            "ArrayBodyID's data() binding) -- a raw pointer into the array's own backing storage, not a " +
            "heap allocation. PhysicsSystem.ts's createBodiesFromParser calls `newBodies.data()` / " +
            "`newInactiveBodies.data()` right before AddBodiesPrepare/AddBodiesFinalize and never " +
            "destroy()s the result (correct -- the array itself is destroy()'d instead, right after). NOT " +
            "machine-classified: `jolt-ownership:generate`'s call-site scanner doesn't resolve `.data()` " +
            "back to this specific class today (confirmed via `bun run jolt-ownership:generate`'s own " +
            "'not seen by the ownership tool at all' hint), same class of scanner gap as the [Value] " +
            "attribute reads above, just for a plain method return instead of a field read.",
    },
    // biome-ignore lint/style/useNamingConvention: matches the Jolt-generated class name verbatim
    BodyInterface_AddState: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "BodyInterface.AddBodiesPrepare() is INTERNAL_REF in generated glue.cpp (confirmed via " +
            "`jolt-ownership:generate`'s function-ownership.json: category INTERNAL_REF) -- Jolt's real " +
            "`BodyInterface::AddState` is an opaque handle into the body array being added, not a caller-" +
            "owned heap allocation. PhysicsSystem.ts's createBodiesFromParser passes the result straight " +
            "into AddBodiesFinalize and never destroy()s it, which is correct: there is nothing to free. " +
            "NOT machine-classified: emitted by the tool as a fresh, unseen class (no prior JS-side " +
            "wrapper existed for it to disqualify), so it never reaches the generated-safe table; same " +
            "category as BodyIDMemRef above.",
    },
    MassProperties: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "BodyCreationSettings.mMassPropertiesOverride is a `[Value] attribute MassProperties` " +
            "(jolt/JoltJS.idl:2726). PhysicsSystem.ts reads it 3 times (createBodiesFromParser, " +
            "createBallGamePiece-style overrides, and the ejectable-mass-override path) to set `.mMass`. " +
            "Same INTERNAL_REF struct-member-accessor pattern as SpringSettings/ArrayFloat above: each read " +
            "hands back a fresh wrapper over the parent settings' own embedded member, never destroy()'d " +
            "by convention. NOT YET covered by the codegen pipeline (plain property read, see SpringSettings " +
            "above).",
    },
    VehicleEngineSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraintSettings.mEngine is a `[Value] attribute VehicleEngineSettings` " +
            "(jolt/JoltJS.idl:3669). ConstraintSettingsUtilities.ts's createVehicleController reads " +
            "`controllerSettings.mEngine.mMaxTorque = maxAcc` once per wheeled-vehicle constraint. Same " +
            "INTERNAL_REF struct-member-accessor pattern as SpringSettings/MassProperties above. NOT YET " +
            "covered by the codegen pipeline (plain property read).",
    },
    VehicleTransmissionSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "VehicleConstraintSettings.mTransmission is a `[Value] attribute VehicleTransmissionSettings` " +
            "(jolt/JoltJS.idl:3670). ConstraintSettingsUtilities.ts's createVehicleController reads " +
            "`controllerSettings.mTransmission` 4 times (mClutchStrength, mGearRatios.clear/push_back, " +
            "mMode) per wheeled-vehicle constraint -- each read is a fresh accessor call. Same INTERNAL_REF " +
            "struct-member-accessor pattern as VehicleEngineSettings above. NOT YET covered by the codegen " +
            "pipeline (plain property read).",
    },
    Vec4: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "`Mat44.GetColumn4()` and `RMat44.GetColumn4()` are each independently STATIC_ALIAS in " +
            "generated glue.cpp (`static Vec4 temp; return (temp = self->GetColumn4(inCol), &temp);`, two " +
            "distinct static locals, one per class). TypeConversions.test.ts's two `compareMat` helpers " +
            "(one per describe block) each call `jM.GetColumn4(c)` in a loop and never destroy() the " +
            "result -- correct, since destroy()ing a static scratch corrupts it for every later call. Same " +
            "fixed '+1 the first time this exact static address is ever touched, never grows again' " +
            "pattern already established for RMat44/Mat44/AABox above; two distinct call sites (Mat44's " +
            "and RMat44's overloads) means two distinct +1s, matching the observed delta of exactly 2 " +
            "across the whole TypeConversions.test.ts run. NOT machine-classified: `new JOLT.Vec4(...)` " +
            "also genuinely appears at src/util/TypeConversions.ts:54 (a correctly construct-and-destroy'd " +
            "column temp, see convertThreeMatrix4ToJoltMat44), so the tool correctly refuses to call the " +
            "whole class safe -- same class-level-granularity limitation as Vec3/Quat/Mat44/RVec3 above.",
    },
    MeshShapeSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.ts's createConcaveShapeSettingsFromPart builds a `new JOLT.MeshShapeSettings()` " +
            "per concave part and hands it to `compoundShapeSettings.AddShape(...)`, which AddRefs it (same " +
            "confirmed pattern as the ConvexHullShapeSettings entry above) and holds that reference until " +
            "the compound settings object itself is destroyed. Correctly never raw-destroy()'d at the " +
            "per-part call site -- doing so would double-free the same way ConvexHullShapeSettings would. " +
            "Same RefTarget cache-aliasing blind spot as Shape/ConvexHullShapeSettings above, confirmed via " +
            "`jolt-ownership:generate`'s hint (`isRefTarget: true`).",
    },
    VertexList: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "MeshShapeSettings.mTriangleVertices is a `[Value] attribute VertexList` (jolt/JoltJS.idl:1174). " +
            "createConcaveShapeSettingsFromPart's per-vertex loop calls `settings.mTriangleVertices." +
            "push_back(vert)` once per vertex (plus a few more reads for `.size()`), each a fresh accessor " +
            "call over the settings' own embedded member -- same INTERNAL_REF struct-member-accessor " +
            "pattern as ArrayVec3/ArrayFloat above, which is why the delta scales with vertex count instead " +
            "of staying fixed. The separate `settings.mTriangleVertices = new JOLT.VertexList()` assignment " +
            "at the top of the function is a genuine CLONED-argument temp (glue.cpp's setter does " +
            "`self->mTriangleVertices = *arg0`) that is never destroy()'d -- a real, small (one wrapper per " +
            "call), currently-unfixed leak layered on top of the much larger INTERNAL_REF signal; left as-is " +
            "here since fixing it does not change this class's classification (still has genuine INTERNAL_REF " +
            "contributors that can never return to baseline).",
    },
    IndexedTriangleList: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "MeshShapeSettings.mIndexedTriangles is a `[Value] attribute IndexedTriangleList` " +
            "(jolt/JoltJS.idl:1175). Same INTERNAL_REF struct-member-accessor pattern as VertexList above: " +
            "createConcaveShapeSettingsFromPart's per-triangle loop and `.size()` reads dominate the delta. " +
            "Same unfixed-but-immaterial CLONED-temp leak at the initial `settings.mIndexedTriangles = new " +
            "JOLT.IndexedTriangleList()` assignment as VertexList above.",
    },
    PhysicsMaterialList: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "MeshShapeSettings.mMaterials is a `[Value] attribute PhysicsMaterialList` (jolt/JoltJS.idl:1176). " +
            "Same INTERNAL_REF struct-member-accessor pattern as VertexList/IndexedTriangleList above, via " +
            "the `settings.mMaterials.push_back(material)` accessor call. Same unfixed-but-immaterial " +
            "CLONED-temp leak at the initial `settings.mMaterials = new JOLT.PhysicsMaterialList()` " +
            "assignment as VertexList above.",
    },
    PhysicsMaterial: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsMaterialList.push_back(inMaterial) AddRefs its argument (`self->push_back(inMaterial)` " +
            "onto an `Array<RefConst<PhysicsMaterial>>`, confirmed via `isRefTarget: true` in the ownership " +
            "hints). createConcaveShapeSettingsFromPart's single `new JOLT.PhysicsMaterial()` per part is " +
            "handed to `settings.mMaterials.push_back(material)` and correctly never destroy()'d afterward " +
            "-- same RefTarget cache-aliasing blind spot as WheelSettingsWV/ConvexHullShapeSettings above, " +
            "freed via refcounting once the owning MeshShapeSettings (and in turn the compound shape) is " +
            "torn down.",
    },
    SphereShape: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "IntakeSensorSceneObject.ts/PhysicsSystem.ts's `new JOLT.SphereShapeSettings(radius)` is handed " +
            "to `createSensor`, which calls `shapeSettings.Create()` (STATIC_ALIAS ShapeResult) then " +
            "`.Get()` to obtain the live Shape (cached here under its concrete subtype, SphereShape, same " +
            "binder polymorphic-return-caching quirk as BoxShape above) and passes it into `createBody`, " +
            "which hands it to BodyCreationSettings (AddRefs it). Correctly never raw-destroy()'d; freed via " +
            "refcounting once the sensor body is destroyed. Same RefTarget cache-aliasing blind spot as " +
            "Shape/BoxShape above.",
    },
    SphereShapeSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.ts's createBodiesFromParser builds a `new JOLT.SphereShapeSettings(radius)` for " +
            "the sphericity-collapse path and hands it straight to `new JOLT.RotatedTranslatedShapeSettings" +
            "(center, identityRotation, sphereSettings)` (glue.cpp: `return new " +
            "RotatedTranslatedShapeSettings(*inPosition, *inRotation, inShape);` -- `inShape` passed as a " +
            "raw pointer, not dereferenced, i.e. AddRef'd/held, not copied). `offsetSettings` (the " +
            "RotatedTranslatedShapeSettings) is destroy()'d right after `.Create()`, which correctly frees " +
            "`sphereSettings` via refcounting as its sole remaining owner -- `sphereSettings` itself is " +
            "correctly never raw-destroy()'d directly (would double-free). Same RefTarget cache-aliasing " +
            "blind spot as ConvexHullShapeSettings/WheelSettingsWV above, confirmed via " +
            "`jolt-ownership:generate`'s hint (`isRefTarget: true`).",
    },

    // --- Investigated but deliberately left unclassified (real candidates, not blind spots) ---
    // - RRayCast: PhysicsSystem.ts's rayCast() does `const ray = new JOLT.RRayCast(rayVec, dir)` and
    //   never destroy()s it -- not on the miss path (dropped on return undefined), and no caller of
    //   rayCast() destroys the `.ray` it hands back on a hit either (checked RaycastUtils.ts,
    //   PokerPanel.tsx, SelectButton.tsx). docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md's RRayCast
    //   section describes it as a plain caller-constructed COPY (`new RRayCast(...)`), not a binder
    //   cache-aliasing blind spot like the entries above -- this looks like a real, fixable leak, not
    //   something to classify away. Left unclassified on purpose so the automatic check keeps
    //   flagging it until it's actually fixed and verified (see the BroadPhaseLayer entry above for why
    //   "looks obviously right" still needs empirical verification, not just doc-reading, before
    //   touching PhysicsSystem.ts).
    // - JoltInterface: a single isolated `new PhysicsSystem(); system.destroy()` cycle DOES return this
    //   class's count to baseline (verified via a throwaway diagnostic test, unlike ContactListenerJS
    //   above) -- `JOLT.destroy(this._joltInterface)` genuinely clears its cache entry in isolation. The
    //   persistent +1-per-test delta seen in this branch's full leak run must come from some other test
    //   or code path (one that constructs a PhysicsSystem-adjacent JoltInterface without reaching that
    //   destroy() call, or throws before it), not a general defect in this class's teardown. Left
    //   unclassified because the actual source hasn't been found yet, not because it's assumed benign.
    // - RotatedTranslatedShapeSettings: not documented anywhere in docs/JOLT_FUNCTIONS_OWNERSHIP_
    //   INVARIANTS.md at all, and memory-audit's table predates whatever call site in this branch
    //   constructs it. No evidence either way yet.
    // - VehicleConstraintStepListener: production code IS correct -- PhysicsSystem.ts's destroyMechanism
    //   calls `RemoveStepListener` then `JOLT.destroy()` on every listener in `mech.stepListeners`
    //   (see the comment directly above that call), and Mechanism.ts pushes every
    //   `createVehicleListeners` result into that array, so a real dispose() cycle returns this class to
    //   baseline. The nonzero deltas seen in this branch's leak run (MirabufSceneObject.test.ts,
    //   ContactEvent.test.ts, Mechanism.test.ts) all come from integration tests that build a wheeled-
    //   vehicle mechanism/scene object and never call `.dispose()` on it before the test ends -- a test-
    //   teardown gap in those test files, not a call-site bug. Left unclassified so the check keeps
    //   surfacing it as a prompt to add teardown, rather than silently accepting a real future leak in
    //   this class. Same underlying pattern likely explains a chunk of ContactManifold/RVec3/OrientedBox-
    //   adjacent deltas seen in other undisposed integration tests -- not audited exhaustively here.
}

export const CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
    ...GENERATED_CLASS_CLASSIFICATION,
    ...MANUAL_CLASS_CLASSIFICATION,
}

// Any nonzero-delta class not in CLASS_CLASSIFICATION is intentionally not defaulted anywhere.
// See `diffLiveCountsFiltered` in `JoltLeakDetection.ts`, which throws loudly instead.
