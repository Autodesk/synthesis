// Classifies the classes that showed a nonzero live-count delta in `real-lifecycle.test.ts` into
// one of three buckets, so `diffLiveCountsFiltered` (instrumentation.ts) can assert only on the
// bucket that actually means something. Grown on demand, same pattern as
// `KNOWN_UNSEEDED_HANDOFFS`/`factories.ts` — manual once per class, reused forever. Do not
// speculatively add classes that haven't shown a nonzero delta in a real run.
//
// Every entry was attributed empirically: `real-lifecycle.test.ts`'s 5-cycle run was compared
// against a zero-cycle construct+destroy-only run of the same `PhysicsSystem` (temporary
// diagnostic, not checked in) to separate one-time constructor/teardown artifacts (delta identical
// with or without cycles) from per-cycle contributions (delta grows with cycles). That distinction
// drove several of the calls below — see each reason.
//
// Bucket definitions (exactly these three):
// - PERMANENT_SINGLETON: constructed once for the app/process lifetime, never destroyed by design.
//   Evidence required: a source comment or documented invariant, cited by file:line.
// - INTERNAL_REF_UNPROVABLE: the class's *entire* observed delta traces to object(s) whose JS
//   wrapper is, by documented ownership convention, never individually destroy()'d — either a
//   `returnOwnership: "INTERNAL_REF"` getter, or a documented `CONSUMED` handoff — where the cache
//   entry structurally cannot decrement regardless of whether the containing object is torn down.
//   Every contributing code path must have this evidence; if even one path lacks it, the class
//   stays in MUST_RETURN_TO_BASELINE.
// - MUST_RETURN_TO_BASELINE: everything else. No exceptions.
export type OwnershipBucket = "PERMANENT_SINGLETON" | "INTERNAL_REF_UNPROVABLE" | "MUST_RETURN_TO_BASELINE"

export type ClassClassification = {
    bucket: OwnershipBucket
    reason: string
}

export const CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
    // --- PERMANENT_SINGLETON ---
    // (none confirmed — see BroadPhaseLayer below, which looked like a candidate but the comment
    // backing it doesn't survive cross-checking against the generated ownership table / doc.)

    // --- INTERNAL_REF_UNPROVABLE ---
    BodyInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.GetBodyInterface() returnOwnership=INTERNAL_REF (ownership-table.generated.json); " +
            "parent _joltPhysSystem is never independently destroy()'d — PhysicsSystem.ts:1494-1495 comment " +
            '"Don\'t destroy BodyInterface: it\'s a value member of PhysicsSystem, not a heap allocation, so ' +
            'freeing it corrupts the heap." Single call site (constructor); delta identical (1) with zero cycles.',
    },
    PhysicsSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "PhysicsSystem.GetPhysicsSettings() returnOwnership=INTERNAL_REF; docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:500-501 confirms 'Mutate fields in place; never destroy()'. Only call sites are " +
            "PhysicsSystem.ts:191-193 (constructor); delta identical (1) with zero cycles.",
    },
    PhysicsSystem: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "JoltInterface.GetPhysicsSystem() returnOwnership=INTERNAL_REF; docs/JOLT_FUNCTIONS_OWNERSHIP_" +
            "INVARIANTS.md:508 states 'Obtained from JoltInterface.GetPhysicsSystem() (an INTERNAL_REF). Never " +
            "destroy().' Only call site is PhysicsSystem.ts:182 (constructor); delta identical (1) with zero " +
            "cycles. JoltInterface itself IS destroyed (PhysicsSystem.ts:1496), but that frees the underlying " +
            "C++ object without ever calling JOLT.destroy() on this specific JS wrapper, so the cache entry " +
            "cannot decrement either way — same structural blind spot as BodyInterface/PhysicsSettings above.",
    },
    MassProperties: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:136-137 explicitly documents " +
            "'BodyCreationSettings.mMassPropertiesOverride.mMass = number — COPIED (number). No Ownership " +
            "Concerns.' Only call site is PhysicsSystem.ts:342 (createBody, mass-truthy path); the intermediate " +
            "struct-member wrapper this line reads is never destroy()'d by convention, same as other " +
            "INTERNAL_REF struct-member accessors (e.g. PhysicsSettings above).",
    },
    ObjectLayerPairFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:455-460: 'CONSUMED once assigned to " +
            "JoltSettings.mObjectLayerPairFilter ... Do not destroy() after hand-off' and :386-390 confirms " +
            "JoltInterface's destructor frees it. Only constructed once, in setupCollisionFiltering " +
            "(PhysicsSystem.ts:1842); delta identical (1) with zero cycles.",
    },
    BroadPhaseLayerInterfaceTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:383-390: assigned to " +
            "JoltSettings.mBroadPhaseLayerInterface, CONSUMED, freed by JoltInterface's destructor, JS must not " +
            "destroy() after hand-off. Only constructed once (PhysicsSystem.ts:1869); delta identical (1) with " +
            "zero cycles.",
    },
    ObjectVsBroadPhaseLayerFilterTable: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:464-472: assigned to " +
            "JoltSettings.mObjectVsBroadPhaseLayerFilter, CONSUMED, freed by JoltInterface's destructor. Only " +
            "constructed once (PhysicsSystem.ts:1879); delta identical (1) with zero cycles.",
    },
    ObjectLayerPairFilter: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same ObjectLayerPairFilterTable instance above (same CONSUMED " +
            "hand-off, same doc citation, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:459-460); delta identical " +
            "(1) with zero cycles, tracking ObjectLayerPairFilterTable 1:1.",
    },
    BroadPhaseLayerInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Base-class cache view of the same BroadPhaseLayerInterfaceTable instance above (same CONSUMED " +
            "hand-off, same doc citation, docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:383-390); delta identical " +
            "(1) with zero cycles, tracking BroadPhaseLayerInterfaceTable 1:1.",
    },
    Body: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "Body is `[NoDelete]` (docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:88-89: 'bodies are owned by " +
            "Jolt's body manager and are created/destroyed only through BodyInterface. Never destroy() a " +
            "Body.'). Confirmed empirically: `JOLT.destroy()` on a Body — whether obtained from " +
            "`BodyInterface.CreateBody()` or a contact-listener's wrapPointer'd callback arg — throws 'Cannot " +
            "destroy object. (Did you create it yourself?)'. The observed delta tracks the number of distinct " +
            "heap addresses the WASM allocator ever handed out for a Body across the run, not a leak: " +
            "create/destroy/create cycles on the same freed slot are a cache *hit* (confirmed empirically — the " +
            "JS wrapper returned by a later CreateBody() at a reused address is === the earlier one), so this " +
            "only grows on a genuinely new address, and there is no API that can ever clear an entry once made.",
    },
    BodyID: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason:
            "The one real leak here — PhysicsSystem.ts's OnContactAdded handler constructing fresh, " +
            "caller-owned `new JOLT.BodyID(...)` copies (COPY ownership) and queuing them for deferred dispatch " +
            "— is fixed: `PhysicsSystem.update()` now destroy()s them once the queued event has dispatched. " +
            "The entire remaining delta traces to `Body.GetID()` (INTERNAL_REF into a live Body's internal " +
            "member; docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md:91-93, 'Do not destroy()'), which aliases its " +
            "parent Body's address 1:1 and inherits the exact same `[NoDelete]` structural block as Body above " +
            "— confirmed empirically: this class's delta now matches Body's delta exactly on every run.",
    },
}

// Any nonzero-delta class not in CLASS_CLASSIFICATION is intentionally NOT defaulted anywhere —
// see `diffLiveCountsFiltered` in instrumentation.ts, which throws loudly instead.
