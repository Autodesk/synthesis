# Jolt memory

Tests every use of `@synthesis.adsk/jolt-physics` (Jolt's WASM binding, compiled via classic
Emscripten `webidl_binder.py`, not embind) against the real WASM build — not a mock — to catch
memory-ownership bugs: leaked objects, use-after-free, double-free, and destroying handles that
alias a shared scratch buffer instead of a heap allocation. See
`docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md` and `docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md` for
the ownership rules this suite checks against.

Nothing here is imported by app code. This directory only exists to be run by its own vitest
project (`jolt-memory`, defined in `fission/vite.config.ts`), separate from the normal
browser/jsdom test project.

## Layout

```
jolt-memory/
  tests/    every *.test.ts — the actual test suites (see below)
  lib/      shared infrastructure the tests import: leak detection, factories, fixtures
  codegen/  parse-idl.ts + its generated (gitignored) ownership-table.generated.json
  setup.ts, global-setup.ts   vitest lifecycle hooks (see their own file headers)
```

## Running it

```sh
bun run test:jolt-memory                # plain build — leak/handoff/logic checks, no setup required
JOLT_ASAN_DIST=<path> bun run test:jolt-memory   # + corruption checks (bad-free/UAF), needs an ASan build
```

The plain run uses the prebuilt npm package (`@synthesis.adsk/jolt-physics`, already in
`node_modules`) — nothing to install beyond `bun install`. It catches leaks (via the pointer-cache
diffing in `lib/instrumentation.ts`) and confirms the intentional-UAF probes in
`tests/handoff-timing.test.ts` at least *look* wrong (garbage refcount or a thrown error), but a
plain build has no way to hard-fail on memory corruption that happens to not crash.

The ASan run additionally builds Jolt from source with `-fsanitize=address`
(`jolt/CMakeLists.txt`'s `ENABLE_ASAN` option) and points `JOLT_ASAN_DIST` at the resulting
`jolt-physics.debug.wasm-compat.js`. This is what actually proves a bad-free/UAF is real instead of
"didn't crash this time." Building it requires EMSDK (version pinned in
`.github/workflows/JoltMemoryAudit.yml`); locally, the easiest path is the `emscripten/emsdk`
Docker image plus a JDK 21 (the bundled `google-closure-compiler` needs it — Docker's default is
usually older) — see that workflow file for the exact build command
(`sh build.sh Debug -DENABLE_ASAN=ON -DBUILD_WASM_COMPAT_ONLY=ON`). CI builds and runs this
automatically on changes to `jolt/**` or this directory; most contributors will never need to do it
by hand.

The codegen table (`lib/ownership-table.generated.json`) regenerates automatically before every run
via `global-setup.ts` — gitignored, never committed. To regenerate it by hand after an IDL change:

```sh
bun run jolt-memory:codegen
```

### A note on memory while running the ASan build

The ASan-instrumented WASM module is far heavier (redzones around every allocation) than the plain
build. Running the *entire* suite with default vitest concurrency can use a lot of memory. If that's
a problem, cap concurrency or isolate to one process:

```sh
JOLT_ASAN_DIST=<path> bunx vitest --project jolt-memory run --pool=forks --poolOptions.forks.maxForks=1 --poolOptions.forks.singleFork=true
```

Targeting a single file (`... run src/test/jolt-memory/tests/<file>.test.ts`) is also much cheaper
than the full suite when iterating on one bug.

## What's actually here

### `lib/` — shared infrastructure (not tests themselves)

- **`instrumentation.ts`** — the leak/violation detectors every test uses:
  - `countLive`/`snapshotAllLiveCounts` read the binder's own per-class pointer cache
    (`JOLT.getCache(JOLT.ClassName)`) directly — this is the ground truth for "is this object still
    considered live," and works on any build, no ASan needed.
  - `expectNoLeaks` / `diffLiveCountsFiltered` snapshot before/after and assert the counts return to
    baseline, filtering out deltas already documented as legitimate in `class-classification.ts`.
  - `patchDestroyGuard` wraps `JOLT.destroy` and flags any `RefTarget` (has `GetRefCount`) destroyed
    while its refcount is still `>1` — a synchronous heuristic for the handoff-timing bug class,
    independent of ASan.
- **`class-classification.ts`** — hand-maintained allow-list for classes whose live-count
  legitimately never returns to zero (structural `INTERNAL_REF`s, `[NoDelete]` types, binder
  cache-aliasing blind spots — each entry cites the specific code path and why). Every entry was
  attributed empirically, not guessed; see the file's own header for the bucket definitions. **This
  only checks "is this class known," not "is this exact delta size expected"** — once a class is
  classified, any future delta for it is silently accepted. It is not a substitute for actually
  reading a new leak's cause before adding an entry.
- **`factories.ts`** — hand-written, minimal construction recipes: `factories` (one entry per
  standalone-constructible class) and `handoffRegistry` (named `RefTarget` handoff call sites —
  parent + child + how to hand off — driving `tests/handoff-timing.test.ts`). Classes/handoffs with
  no entry are simply untested by the generic/handoff templates, not silently assumed safe.
  `KNOWN_UNSEEDED_HANDOFFS` lists handoffs flagged by the codegen as real and dangerous-shaped but
  not yet given a recipe.
- **`mirabuf-fixtures.ts`** — hand-built, in-memory `mirabuf.Assembly` fixtures (no `.mira` file is
  checked in, and this project runs with no network) for the real-lifecycle tests that need to drive
  `MirabufParser`/`createBodiesFromParser` with a genuinely valid assembly rather than a loose mock.
- **`uaf-probe.ts`** — standalone script (spawned as a child process by
  `tests/handoff-timing.test.ts`, never imported directly). Confirmed empirically: under ASan, a
  real UAF's `abort()` is a *catchable* JS exception, not a killed process — it poisons the shared
  WASM module for the rest of whatever shares that module afterward. Running each intentional-UAF
  probe in its own process contains that blast radius to just the one probe.
- **`ownership-table.generated.json`** — gitignored, regenerated by `codegen/parse-idl.ts` (see
  below) before every test run. Lives here rather than in `codegen/` because `instrumentation.ts`
  (its only real consumer) is the file that imports it.

### `codegen/parse-idl.ts`

Parses the full Jolt IDL surface (337 interfaces in `jolt/JoltJS.idl` + 8 in
`jolt/JoltJS-DebugRenderer.idl`) into a per-method ownership table (`returnOwnership`,
`isRefTarget`, `isHandoffCandidate`, …) written to `lib/ownership-table.generated.json`, including
the ~587 methods whose `[Value]` return is a `STATIC_ALIAS` (aliases a glue.cpp scratch buffer,
never a heap allocation — see the ownership docs). **This table is a reference/audit artifact, not
a live test-generation input** — no test file reads it to decide what to run; `lib/factories.ts`'s
hand-written registries are what actually drive `tests/generic.test.ts`/`tests/handoff-timing.test.ts`.
Treat it as documentation to spot-check against, and as raw material when deciding what to seed
into `factories.ts` next.

### `setup.ts` / `global-setup.ts`

Vitest lifecycle hooks, not test logic — see each file's own header comment for why it exists.
`setup.ts` (`setupFiles`, runs per test file) shims jsdom/Node gaps app code needs (broken Node
`localStorage`, no `Worker`, no dev server for relative-URL `fetch`s). `global-setup.ts`
(`globalSetup`, runs once before any worker starts) regenerates the codegen table so it's never
stale.

### `tests/` — generic/mechanical tests (broad, shallow — binder-level correctness)

- **`generic.test.ts`** — for every class in `factories.ts`, construct then destroy once, assert the
  live-count returns to baseline. Plus two meta-tests proving the leak detector itself actually
  detects a leak and tracks concurrent instances correctly.
- **`fuzz.test.ts`** — the same factory-backed classes, constructed/destroyed in randomized order
  (seeded PRNG, not `fast-check`'s built-in shrinking usage beyond generating the permutation) to
  catch order-dependent leaks a straight-line test wouldn't. Lowest-leverage file here: these
  factories are almost all independent objects, so shuffling order mostly re-covers what
  `generic.test.ts` already checks.
- **`handoff-timing.test.ts`** — the primary thing this whole suite exists to catch: for every
  `handoffRegistry` entry, three variants — (a) hand off then immediately destroy the caller's own
  handle (expected UAF, run via `uaf-probe.ts` in a subprocess), (b) hand off and never destroy
  (expected clean — the parent owns it now), (c) hand off, explicitly remove, then destroy (expected
  clean). Only as good as `handoffRegistry`'s 3 entries — most real handoff sites in the app are not
  yet registered here.
- **`integration.test.ts`** — one deeper test on a hand-rolled minimal physics world (via
  `factories.ts`, not `PhysicsSystem.ts` — that file transitively touches browser-only globals),
  covering a create/simulate/teardown cycle and confirming a shape handed to `BodyCreationSettings`
  is correctly retained rather than leaked.

### `tests/` — real-lifecycle tests (narrow, deep — actual app code, actual bugs)

Each of these drives a real production class/function through vitest's `node` environment against a
real `PhysicsSystem`, with `patchDestroyGuard` + before/after `snapshotAllLiveCounts` wrapped around
it. This category has caught essentially every real bug this suite has found — see each file's own
header comment for what specifically broke and how it was fixed; treat those comments as an
investigation log, not a live status report (a file describing a bug it found is normally already
fixed and passing — check the actual test result, not the prose, for current status).

| File | Exercises |
|---|---|
| `mirabuf-lifecycle.test.ts` | `PhysicsSystem.createBodiesFromParser`/`createJointsFromParser`/`destroyMechanism` directly — the highest-traffic robot spawn/despawn path |
| `mirabuf-scene-object-lifecycle.test.ts` | `MirabufSceneObject` itself: constructor → `setup()` → `update()` × 3 → `dispose()`, including `moveToSpawnLocation` and per-frame `updateMeshTransforms` |
| `zone-scene-object-lifecycle.test.ts` | `ZoneSceneObject.setSensorProperties`'s `setShape` handoff across setup → update (shape replaced) → dispose |
| `real-lifecycle.test.ts` | `PhysicsSystem` create/simulate/`setShape`/teardown through the real app class directly |
| `driver-lifecycle.test.ts` | `HingeDriver`/`SliderDriver`/`WheelDriver.update()` (both control modes), `worldAnchor`/`worldAxis` — the per-tick control path for every live-controlled joint |
| `simulation-layer-lifecycle.test.ts` | `SimulationLayer`'s constructor and `update()` driver/stimulus loops — the counterpart to `driver-lifecycle.test.ts` |
| `drag-mode-lifecycle.test.ts` | `DragModeSystem.ts` and the raycast path it triggers (`RaycastUtils.rayCastForRigidBody` → `PhysicsSystem.rayCast`) |
| `ejectable-intake-lifecycle.test.ts` | `EjectableSceneObject.ts`/`IntakeSensorSceneObject.ts` shape/body creation and per-frame position/rotation calls |
| `message-handlers-lifecycle.test.ts` | `MessageHandlers.handlePeerUpdate` — the multiplayer per-frame remote-body sync path |
| `synthesis-brain-lifecycle.test.ts` | `SynthesisBrain.configure()`'s drivetrain-detection pass and `applyUnstickForce()` |
| `urdf-wheel-physics-lifecycle.test.ts` | `URDFWheelPhysics.inferURDFAutoWheelBasis()` via `PhysicsSystem.createWheelConstraint()` — the URDF-import wheel path, otherwise never hit by non-URDF fixtures |
| `auto-test-panel-lifecycle.test.ts` | `AutoTestPanel.tsx`'s `resetBodies()` — the reset-button handler after an auto-test run |

## Dependencies

- **Runtime**: `@synthesis.adsk/jolt-physics` (prebuilt npm package, plain build) or a local ASan
  build of `jolt/` (via EMSDK) for corruption checks. These are independent artifacts that happen to
  usually be in sync, not the same thing.
- **Test runner**: vitest 3.x, `node` environment (not jsdom's DOM APIs, though `setup.ts` shims a
  few Node/jsdom gaps app code needs). Runs as its own `test.projects` entry in
  `fission/vite.config.ts`, excluded from the normal browser project.
- **`fast-check`**: only used by `fuzz.test.ts`, for generating random permutations.
- **`bun`**: used to spawn `uaf-probe.ts` as a subprocess from `handoff-timing.test.ts`, and to
  regenerate the codegen table via `global-setup.ts` (assumes `bun` is on `PATH`, matching how this
  whole project is normally run).

## When to add a test where

- **New class you can construct standalone, no parent needed** → add a recipe to `lib/factories.ts`'s
  `factories` map. Picked up automatically by `tests/generic.test.ts` (and `tests/fuzz.test.ts`).
- **New `RefTarget` handoff call site** (anything matching `push_back`/`AddX`/`SetX` taking a
  `RefTarget` argument — check `ownership-table.generated.json`'s `isHandoffCandidate` flag, or just
  grep the IDL) → add a recipe to `factories.ts`'s `handoffRegistry`. Picked up automatically by
  `handoff-timing.test.ts`'s three variants. This is the highest-leverage place to add coverage for
  the bug class this suite was originally built around — most real handoff sites in the app are
  still only in `KNOWN_UNSEEDED_HANDOFFS` or not listed at all.
- **A production code path that touches Jolt and doesn't have a real-lifecycle test yet** (check the
  table above for what's covered) → add a new `tests/*-lifecycle.test.ts` file, same shape as the
  existing ones: real class/function, real `PhysicsSystem`, `patchDestroyGuard` +
  `snapshotAllLiveCounts` wrapped around it. This is where bugs actually get found — prefer this
  over stretching the generic templates to reach further than they're suited for.
- **A `class-classification.ts` failure on a class you don't recognize** → don't just add an entry
  to make the test pass. Trace the actual call path first (same discipline the existing entries were
  built with — every one cites a specific file:line and, where possible, an empirical confirmation of
  *why* the delta is safe). If you can't explain it, it's probably a real leak, not a classification
  gap.
- **Something that needs ASan to prove, not just observe** (i.e., you suspect a bad-free/UAF that a
  plain build might silently "get away with") → follow `handoff-timing.test.ts`'s pattern: put the
  actual dangerous operation in its own subprocess (see `uaf-probe.ts`) rather than running it
  in-process, since a real ASan abort is catchable and will otherwise poison the shared module for
  every other test sharing that process.
