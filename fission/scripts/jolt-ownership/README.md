# jolt-ownership

Generates the machine-checked subset of `fission/src/test/JoltClassClassification.ts`
(`JoltClassClassification.generated.ts`) instead of hand-attributing it against
`docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md`. Run with:

```sh
bun run jolt-ownership:generate
```

Dev-time only, not part of any test run — re-run manually when `jolt/JoltJS.idl` or fission's Jolt
call sites change. Requires a local Emscripten install (only for `webidl_binder.py`, no full wasm
build) via `EMSCRIPTEN_ROOT` or `EMSDK`.

## Pipeline

1. **`generate-glue.sh`** — runs `webidl_binder.py` on `jolt/JoltJS.idl`, producing `.generated/glue.cpp`.
   This is the actual compiler output the JS/wasm build ships, i.e. ground truth for what each
   binding really does — not a re-derivation of the binder's codegen rules from the IDL's
   `[Value]`/`[Ref]`/`[Const, Ref]` tags by hand (which is exactly the kind of manual attribution
   that produced a stale doc bug fixed on `branp/291/static-alias-doc-corrections`).
2. **`parse-glue.mjs`** — parses every `emscripten_bind_<Class>_<Method>_<N>` function body into a
   return-ownership category (`COPY` / `STATIC_ALIAS` / `INTERNAL_REF` / `ALIASES_THIS` / `NONE`),
   matching the literal C++ shapes documented in `docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md`.
   Also records, per class: whether it has no generated `__destroy__` binding at all (`[NoDelete]`
   in the IDL — `JOLT.destroy()` throws unconditionally, no call-site reasoning needed) and whether
   it's `RefTarget`-derived (exposes `AddRef`/`Release`/`GetRefCount`) — see the caveat below.
   Emits `.generated/function-ownership.json`.
3. **`scan-callsites.mjs`** — walks `fission/src` (production code only, not `src/test/**`) with
   the TypeScript compiler API for `new JOLT.X(...)` constructions, `obj.Method(...)` calls
   resolved against the ownership table, and `JOLT.castObject`/`JOLT.wrapPointer`/`JOLT.destroy`
   call sites. Emits `.generated/call-sites.json`.
4. **`generate-classification.mjs`** — joins the two: a class is provably safe only if *every*
   production call site producing an instance of it is mechanically non-owned. Emits
   `.generated/classification.generated.json` (`safe` + `disqualified`, each with full evidence).
5. **`emit-classification.mjs`** — renders the `safe` set as real, committed TypeScript
   (`fission/src/test/JoltClassClassification.generated.ts`), each entry's reason citing the exact
   glue.cpp line and fission/src call site(s).

## The RefTarget caveat (why this doesn't replace manual review entirely)

A `RefTarget`-derived class's `return self->Method();` in glue.cpp is textually identical whether
that method is an innocuous accessor or a delegate into Jolt's own `Create()` that allocates a
fresh refcounted object under the hood — confirmed concretely for `*ConstraintSettings.Create()`,
which the parser correctly reads as `INTERNAL_REF` (`return self->Create(*inBody1, *inBody2);`) but
which docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md documents as one of the two genuine exceptions
that actually allocates. Proving those safe needs Jolt's own `AddRef`/`Release` call graph, not
glue.cpp's return-statement shape — out of scope for this tool. `generate-classification.mjs`
excludes every `RefTarget`-derived class's plain method-return evidence for exactly this reason;
`castObject`/`wrapPointer` alias evidence is unaffected (those never allocate regardless of the
target class's refcounting model). These classes stay hand-attested in `JoltClassClassification.ts`.

## Known gaps (scoped out, not silently wrong)

- **`[Value] attribute` field reads** (`.mWheels`, `.mSpringSettings`, `.mPoints`, ...) are plain
  property accesses in the AST, not call expressions — `scan-callsites.mjs` doesn't scan these yet.
  Affected classes (`ArrayVec3`, `ArrayFloat`, `ArrayVehicleAntiRollBar`, `ArrayWheelSettings`,
  `SpringSettings`, `ObjectLayerPairFilter`, `BroadPhaseLayerInterface`, `RayCastResult`) stay
  manually classified.
- **`CONSUMED`-argument tracing**: `parse-glue.mjs` does detect the CONSUMED setter shape
  (`self->member = arg;`, no dereference), but `scan-callsites.mjs` doesn't yet trace a
  constructor's result flowing into a CONSUMED-taking setter argument. Affects
  `ObjectLayerPairFilterTable`, `BroadPhaseLayerInterfaceTable`, `ObjectVsBroadPhaseLayerFilterTable`,
  `WheeledVehicleControllerSettings` — stay manually classified, though the underlying CONSUMED fact
  is already mechanically confirmed in `function-ownership.json`.
- **Class-level granularity ceiling**: `Vec3`/`Quat`/`Mat44`/`RVec3`/`BodyID` mix a real,
  unavoidable non-owned contributor (a `STATIC_ALIAS`/`wrapPointer` return) with genuine, legitimate
  `new JOLT.X(...)` constructions elsewhere in production code. The leak harness only diffs whole-
  class cache counts, so there's no way to say "ignore the +1 from function F, but still catch a
  forgotten destroy() on an unrelated instance" without a harness change — these stay manually
  classified, and the imprecision is real, not a tooling gap.
