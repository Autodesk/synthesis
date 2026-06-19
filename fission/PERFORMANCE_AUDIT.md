# Performance Audit — Synthesis Fission

_Audit date: 2026-06-18_

## Executive Summary

The dominant performance theme is **per-frame allocation churn** — both on the JS heap (throwaway `THREE.Vector3/Matrix4/Quaternion` temporaries in hot update loops) and, more dangerously, on the **fixed-size Jolt WASM heap**, where several callback and conversion paths allocate WASM objects (`Vec3`, `Quat`, `BodyID`, `BoxShapeSettings`) every frame without ever calling `JOLT.destroy()`. The WASM leaks (PERF-001, PERF-002, PERF-006) are the most critical findings: unlike JS garbage, they are not reclaimed and can exhaust Jolt's linear memory and crash a long session. The second major theme is **React re-renders driven by the 60 Hz game loop** (PERF-013, PERF-014, PERF-015) — overlay/graph components and an unmemoized context reconcile every frame regardless of whether anything changed. Finally, the physics step count is effectively pinned at the 20-substep maximum regardless of scene complexity (PERF-004), and several large synchronous operations (mirabuf parse, WASM init) block the main thread (PERF-016, PERF-017).

Note on impact estimates: allocation counts are derived from reading the code (constructors per call × calls per frame). Frame-budget figures marked "estimate" need a profiler capture (Chrome DevTools Performance / `--enable-precise-memory-info`) to confirm; they are included to rank, not to promise specific ms.

## Findings

### [PERF-001] WASM heap leak + 4 JS allocs in `convertJoltMat44ToThreeMatrix4` — High

**File(s):** `src/util/TypeConversions.ts` (lines ~71–76)
**Category:** TypeConversions
**Impact:** ~10–30 calls/frame (once per rigid body via `updateMeshTransforms`) → at 60 Hz, ~600–1800 calls/s. Each call allocates 4 JS objects (2× `THREE.Vector3`, 1× `THREE.Quaternion`, 1× `THREE.Matrix4`) **and** 2 WASM objects (`GetTranslation()`→`Vec3`, `GetQuaternion()`→`Quat`) that are never destroyed → ~1,200–3,600 leaked WASM allocations/sec.
**Description:** This is the single hottest conversion in the app. It calls `convertJoltVec3ToThreeVector3` and `convertJoltQuatToThreeQuaternion` (each `new`-allocates), constructs a constant `new THREE.Vector3(1,1,1)` scale every call, and builds a fresh `Matrix4`. Critically, the Emscripten-returned `Vec3`/`Quat` from `m.GetTranslation()`/`m.GetQuaternion()` are WASM-heap objects requiring `JOLT.destroy()` — the sibling `convertThreeMatrix4ToJoltMat44` correctly destroys its intermediates, this one does not. The leak is unbounded over a session.
**Recommendation:** (1) Add an optional `out: THREE.Matrix4` param and write in place via `out.makeRotationFromQuaternion(q).setPosition(...)`. (2) Capture and destroy the intermediates: `const t = m.GetTranslation(); const q = m.GetQuaternion(); /* read */ JOLT.destroy(t); JOLT.destroy(q);` — or read scalars directly with `GetX/Y/Z`. (3) Hoist a module-level `const SCALE_ONE = new THREE.Vector3(1,1,1)`.

### [PERF-002] `OnContactAdded` leaks two `JOLT.BodyID` WASM objects per contact event — High

**File(s):** `src/systems/physics/PhysicsSystem.ts` (lines ~1577–1578)
**Category:** Memory/GC (WASM heap)
**Impact:** Fires per new contact pair per substep. ~10 pairs × 20 substeps × 60 Hz ≈ 12,000 orphaned WASM `BodyID` objects/sec in a busy scene; never destroyed.
**Description:** `OnContactAdded` does `new JOLT.BodyID(body1.GetID().GetIndexAndSequenceNumber())` twice, stores them in `CurrentContactData`, enqueues them, and the drain path (`_physicsEventQueue.forEach(x => x.dispatch()); _physicsEventQueue = []`) never destroys them. `OnContactPersisted` (lines ~1602–1603) already does this correctly by using `body.GetID()` directly without allocation.
**Recommendation:** Mirror `OnContactPersisted`: store `body.GetID()` directly, or store the raw `number` from `GetIndexAndSequenceNumber()` and reconstruct lazily only if a stable copy must outlive the callback. Destroy any WASM object whose lifetime crosses the callback boundary.

### [PERF-003] Per-vertex WASM allocation + 3 boundary crossings in shape builders — High

**File(s):** `src/systems/physics/PhysicsSystem.ts` (lines ~1119–1128 convex, ~1172–1175 concave)
**Category:** TypeConversions / Asset Loading
**Impact:** Body loading crosses JS↔WASM 3× per vertex (allocate `Vec3`/`Float3`, `push_back`, `destroy`). A 5,000–50,000-vertex robot → 15,000–150,000 round-trips at load time; the concave path allocates a *second* redundant `new JOLT.Vec3(vert)` per vertex purely for bounds checking.
**Description:** `createConvexShapeSettingsFromPart` loops every 3 floats calling `convertMirabufFloatToArrJoltVec3` then `points.push_back` then `JOLT.destroy`. `createConcaveShapeSettingsFromPart` adds `new JOLT.Vec3(vert)` immediately after creating a `Float3`, just to feed `updateMinMaxBounds`, which only needs three scalars.
**Recommendation:** For convex hulls, batch-write vertices into the `mPoints` buffer via a typed-array view instead of per-vertex `push_back`. For concave bounds, read `vert`'s X/Y/Z scalars from the existing `Float3` instead of allocating a second `Vec3`. Reuse one preallocated scratch `Vec3` and destroy once.

### [PERF-004] Physics substeps effectively pinned at 20 regardless of scene complexity — High

**File(s):** `src/systems/physics/PhysicsSystem.ts` (line ~1327)
**Category:** Physics
**Impact:** Always runs ~20 substeps (1,200 narrow-phase iterations/sec) even with zero dynamic bodies; an empty scene pays the same as a full 6-robot match.
**Description:** `MIN_SUBSTEPS = 12`, `MAX_SUBSTEPS = 20`. The formula `floor((lastDeltaT / STANDARD_SIMULATION_PERIOD) * STANDARD_SUB_STEPS)` normalizes to exactly 20 at 60 Hz, so the MIN clamp never *reduces* work — it only caps reduction when frame rate exceeds 60 Hz. Effective count is always `STANDARD_SUB_STEPS`.
**Recommendation:** Lower `MIN_SUBSTEPS` to 4–6 (sufficient for 60 Hz robotics without thin-body tunneling) and make the floor scene-aware (few/no dynamic bodies → fewer substeps). Reserve 12–20 for high-speed wheel-constraint cases. Optionally expose a quality slider. **Profile first** to confirm contact stability at lower counts.

### [PERF-005] `OnContactValidate` queues an event for every candidate pair but never filters — High

**File(s):** `src/systems/physics/PhysicsSystem.ts` (lines ~1621–1635)
**Category:** EventSystem
**Impact:** Jolt invokes this for every potential pair per substep (up to 20×N/frame). Each call allocates an `OnContactValidateData`, calls `EventSystem.create()`, and pushes to the queue — then the callback unconditionally returns `AcceptAllContactsForThisBodyPair`. Pure overhead if nothing consumes `OnContactValidateEvent`.
**Description:** The callback does no actual filtering, so registering it forces a WASM→JS round-trip + JS allocation + array push for every broad-phase candidate, all discarded.
**Recommendation:** If no subscriber acts on `OnContactValidateEvent`, remove the `OnContactValidate` registration entirely (leave it unimplemented in `ContactListenerJS`). Highest leverage-per-line change in the contact subsystem. If filtering is ever needed, decide inline and return immediately without queuing.

### [PERF-006] `ScoringZoneSceneObject.update()` recreates and leaks WASM shape objects every frame — High

**File(s):** `src/mirabuf/ScoringZoneSceneObject.ts` (lines ~155–159; same pattern in `setup()` ~90–94)
**Category:** Memory/GC (WASM heap)
**Impact:** Per zone per frame: `new JOLT.BoxShapeSettings(new JOLT.Vec3(...))` + `shapeSettings.Create()` allocated and never destroyed. 10 zones × 60 Hz ≈ 1,200 leaked WASM objects/sec; Jolt's fixed heap can exhaust → hard crash.
**Description:** `update()` unconditionally rebuilds the box shape each frame even though field scoring zones are static (`props.scale` never changes at runtime). Neither `shapeSettings` nor the `shape` handle is passed to `JOLT.destroy()`.
**Recommendation:** Cache `props.scale` and skip `setShape` when unchanged (eliminates the work entirely for static zones). When a rebuild is genuinely needed, `JOLT.destroy(shapeSettings)` and `JOLT.destroy(shape)` after `setShape()`.

### [PERF-007] `MirabufParser.rigidNodes` getter rebuilds a Map + wrapper objects on every access — High

**File(s):** `src/mirabuf/MirabufParser.ts` (line ~67)
**Category:** Memory/GC
**Impact:** Called 6+ times/frame (`updateMeshTransforms`, `enablePhysics/disablePhysics`, `setObjectPosition`, `getDimensionsWithoutRotation`). Each call allocates a new `Map`, an intermediate array, and one `RigidNodeReadOnly` per node. 20 nodes → ~140+ allocations/frame.
**Description:** `return new Map(this._rigidNodes.map(x => [x.id, new RigidNodeReadOnly(x)]))`. `RigidNodeReadOnly` is a stateless proxy, so recreating it every call is pure waste.
**Recommendation:** Build the `Map<RigidNodeId, RigidNodeReadOnly>` once after the final filter and cache it as a private field; return it (typed `ReadonlyMap`). Invalidate only on merge/bandage operations.

### [PERF-008] `MirabufSceneObject.updateBatches()` recomputes bounding box + sphere every frame — High

**File(s):** `src/mirabuf/MirabufSceneObject.ts` (line ~572, called from `update()` ~430)
**Category:** Render Loop
**Impact:** Unconditional per-frame `computeBoundingBox()` + `computeBoundingSphere()` (each an O(vertices) traversal) for every batch. A field with 50+ batches → ~100 full-geometry traversals/frame even when nothing moved.
**Description:** Called every frame regardless of whether physics moved the assembly. Static fields never change bounds after placement; even for moving assemblies the recompute is only needed after `instanceMatrix.needsUpdate`.
**Recommendation:** Set a dirty flag in `updateNodeParts()` when any `instanceMatrix.needsUpdate` was true; only recompute bounds when the flag is set, then clear it.

### [PERF-009] `DragModeSystem.updateDragForce()` allocates a Raycaster + 5 math objects every drag frame — High

**File(s):** `src/systems/scene/DragModeSystem.ts` (lines ~503–519)
**Category:** Memory/GC
**Impact:** ~7 heap allocations/frame while dragging (≈420/sec): `new THREE.Raycaster`, `Vector2`, two `Vector3`, `Plane`, and an intersection `Vector3`. GC pauses surface as spikes during the most latency-sensitive interaction.
**Description:** All constructed fresh inside the per-frame `updateDragForce()` and discarded at end of call. Same pattern in `startDragging` (~311–330).
**Recommendation:** Promote to private readonly scratch fields (`_scratchRaycaster`, `_scratchPlane`, etc.) and reuse via `.set()`/`.copy()` each frame.

### [PERF-010] `CameraControls.update()` allocates 3 `Matrix4` + 1 `Euler` + a coords object every frame — High

**File(s):** `src/systems/scene/CameraControls.ts` (lines ~325–343)
**Category:** Memory/GC
**Impact:** 3× `Matrix4` (128 B each) + `Euler` + a `{theta,phi,r}` literal at 60 Hz ≈ 240+ objects/sec, allocated even when the camera hasn't moved.
**Description:** `CustomOrbitControls.update()` builds these unconditionally every frame.
**Recommendation:** Pre-allocate `_scratchMatrixA/B/C` and `_scratchEuler` as fields; mutate `_nextCoords` in place instead of reassigning a literal. Early-out when input deltas are zero.

### [PERF-011] `SimulationLayer.drivers` / `stimuli` getters spread Maps into new arrays per access — High

**File(s):** `src/systems/simulation/SimulationSystem.ts` (lines ~70–74)
**Category:** Memory/GC
**Impact:** `[...this._drivers.values()]` + `[...this._stimuli.values()]` → 2 fresh arrays per layer per access. If any per-frame caller (behavior/React panel) reads these, that's 2×N allocations/frame.
**Description:** `update()` itself iterates the Maps directly, so the spread exists only for external callers — but the getter shape invites per-frame allocation.
**Recommendation:** Return `this._drivers.values()` (an iterator) or a readonly view; or maintain a parallel array mutated on add/remove and return it by reference.

### [PERF-012] `SimGeneric.set` sends 9 Worker `postMessage`s + 9 event dispatches per frame per robot — High

**File(s):** `src/systems/simulation/wpilib_brain/sim/SimGeneric.ts` (lines ~89–101); callers `SimGyro.ts`, `SimAccel.ts`
**Category:** Worker
**Impact:** `SimGyroInput.update()` calls `set` 6× (3 angles + 3 rates), `SimAccelInput.update()` 3× — each call does a structured-clone `postMessage` to the WPILib Worker **and** an `EventSystem.dispatch`. That's ~9 IPC round-trips + 9 dispatches/frame/robot, each carrying a single field.
**Description:** `set` was built for single-field mutation; calling it in sequence for related fields multiplies IPC and React-update scheduling that could be one message + one dispatch.
**Recommendation:** Add `setMany(simType, device, fields)` that writes all fields, sends one `postMessage`, and dispatches one `SimMapUpdateEvent`. Refactor gyro/accel `update()` to call it once per frame.

### [PERF-013] `UIProvider` context value is a new object every render → all consumers re-render — High

**File(s):** `src/ui/UIProvider.tsx` (lines ~259–274)
**Category:** React
**Impact:** Every `openModal`/`openPanel`/`closeModal`/`closePanel` state change creates a new `value={{...}}` object, re-rendering every `useUIContext()` consumer (MainHUD, UIRenderer, all open panels). `openModal` also captures `modal` in its deps, cascading further.
**Description:** Unlike `StateProvider` (correctly `useMemo`'d), this provider's value is an unmemoized object literal.
**Recommendation:** `useMemo` the value with proper deps; ideally split stable setter callbacks from volatile `modal`/`panels` state into separate contexts so setter-only consumers don't re-render on state changes.

### [PERF-014] `BespokeGraph` re-renders the full SVG tree at rAF rate (~60 Hz) — High

**File(s):** `src/ui/components/BespokeGraph.tsx` (lines ~487–527)
**Category:** Render Loop / React
**Impact:** A `requestAnimationFrame` loop calls `forceRenderer()` unconditionally every frame, reconciling every node/edge/junction and recomputing path strings every 16 ms even when the graph is unchanged. The `comps` `useMemo` is keyed on `[graph]` only, so it can't short-circuit, and child comps (`EdgeComp`/`NodeComp`/…) aren't memoized.
**Description:** Continuous reconciliation of an SVG graph that changes only on topology edits.
**Recommendation:** Drive redraws from data changes (callback/event from the `Graph` class on topology change) instead of rAF; if a live mode is needed, gate on a `dirty` flag. Wrap child comps in `React.memo` and move the `[...values()]` spreads out of the per-render path.

### [PERF-015] `SceneOverlay` rebuilds all overlay JSX on every physics frame — High

**File(s):** `src/ui/components/SceneOverlay.tsx` (line ~53); dispatched from `src/systems/scene/SceneRenderer.ts` (line ~249)
**Category:** Render Loop / React
**Impact:** `SceneRenderer.update()` dispatches `SceneOverlayUpdateEvent` every frame; the handler runs `updateComponents()`, re-running the reducer and reconstructing all overlay `<div>`s + inline style objects each frame regardless of movement.
**Description:** Per-frame React reconciliation for tag overlays whose positions usually don't change frame to frame.
**Recommendation:** Cache last-rendered tag positions; only dispatch/update when a tag moved beyond a pixel threshold. Better: apply positions imperatively via CSS `transform` on pre-rendered DOM nodes, bypassing React for the per-frame update.

### [PERF-016] Mirabuf decompress + protobuf decode + parser construction block the main thread — Medium

**File(s):** `src/mirabuf/MirabufLoader.ts` (line ~489), `src/mirabuf/MirabufSceneObject.ts` (line ~1095)
**Category:** Worker / Asset Loading
**Impact:** `Pako.ungzip` (synchronous, multi-MB) + `mirabuf.Assembly.decode()` (synchronous protobuf walk) + `MirabufParser` construction (`generateTreeValues`, `loadGlobalTransforms`, `initializeRigidGroups`, `generateRigidNodeGraph`, all O(parts×joints)) run on the main thread. A ~500-part / ~50-joint robot can stall render + input for an estimated 200–600 ms (needs profiler confirmation).
**Description:** `assemblyFromBuffer` is also called twice (once in `cacheRemote` to read the name, once on load), none of it yielding to the event loop.
**Recommendation:** Move ungzip + decode + parser construction into a Worker, transferring the `ArrayBuffer`; return the `RigidNode` graph + `globalTransforms` as a typed-array pack / transferable. Mirrors the existing async-load pattern. Also dedupe the double `assemblyFromBuffer`.

### [PERF-017] `JoltSyncLoader` top-level `await` suspends the entire import graph on WASM init — Medium

**File(s):** `src/util/loading/JoltSyncLoader.ts` (line ~8)
**Category:** Asset Loading
**Impact:** `await j.default()` at module top level makes every transitive importer (14 files: TypeConversions, PhysicsSystem, MirabufSceneObject, SimulationSystem, all drivers) an async module suspended until the >1 MB WASM binary is fetched, compiled, and instantiated — an estimated 100–500 ms before React shell UI can render.
**Description:** Top-level await propagates async-ness through the whole static graph, blocking UI that doesn't need Jolt yet.
**Recommendation:** Remove the top-level await; use the existing `joltInit` Promise from `JoltAsyncLoader` as an explicit gate before physics-dependent code, and null-check synchronous access with a descriptive error if called pre-init.

### [PERF-018] Gyro/accel sensors make redundant per-axis WASM calls + allocate THREE temporaries every frame — Medium

**File(s):** `src/systems/simulation/wpilib_brain/sim/SimGyro.ts` (~70–94), `src/systems/simulation/wpilib_brain/sim/SimAccel.ts` (~52–68)
**Category:** Physics / TypeConversions
**Impact:** `SimGyroInput.update()` calls `GetRotation()`/`GetAngularVelocity()` once per axis → up to 9 WASM crossings/frame where 2 would do. `SimAccelInput.update()` allocates a `Quaternion` + `Matrix4` + `Vector3` each frame and leaks the WASM `Quat`/`Vec3` from `GetRotation()`/`GetLinearVelocity()` (no `JOLT.destroy`).
**Description:** Same root cause as PERF-001 — non-reuse conversions and re-querying the same WASM body multiple times.
**Recommendation:** Fetch `GetRotation()`/`GetAngularVelocity()` once per `update()` and pass into the per-axis helpers (9 crossings → 2). Hoist `_rot`/`_vel`/`_mat` scratch fields and write in place; destroy WASM intermediates.

### [PERF-019] `MirabufInstance` double-copies vertex/normal buffers and uses `.at()` in the hot loop — Medium

**File(s):** `src/mirabuf/MirabufInstance.ts` (lines ~76–108)
**Category:** Asset Loading
**Impact:** `transformVerts` fills `newVerts`, then `transformGeometry` wraps it in `new THREE.BufferAttribute(new Float32Array(newVerts), 3)` — a second copy; same for normals. A 200-body × ~1k-vert robot → several redundant MB of intermediate `Float32Array` allocated at load. `mesh.verts!.at(i)` does bounds-checking per call inside the loop.
**Recommendation:** Have `transformVerts`/`transformNorms` write directly into a pre-sized `Float32Array` passed straight to `BufferAttribute` (no re-wrap). Replace `.at(i)` with `[i]`. Scale in place if the decoder already yields a `Float32Array`.

### [PERF-020] `bodyToMiraSceneObject` does an O(robots×nodes) Map-spread scan on every contact callback — Medium

**File(s):** `src/systems/physics/PhysicsSystem.ts` (lines ~1527–1531)
**Category:** Physics
**Impact:** `OnContactAdded` calls `isClient(body1)` + `isClient(body2)`, each invoking `bodyToMiraSceneObject`, which does `[...obj.mechanism.nodeToBody].some(...)` per robot — spreading the full node→body Map into a temp array each call. ~200 contact-callback invocations/frame × 2 bodies × R robots → thousands of map-spreads/sec.
**Description:** A reverse lookup already exists: `_bodyAssociations: Map<JoltBodyIndexAndSequence, BodyAssociate>`. The current scan also compares `BodyID` with `==` on WASM pointers (fragile).
**Recommendation:** Extend `BodyAssociate` to reference its owning `MirabufSceneObject`, populate in `createBodiesFromParser`, and replace the scan with `this._bodyAssociations.get(body.GetID().GetIndexAndSequenceNumber())` — O(1), zero allocations.

### [PERF-021] `RobotPositionTracker.update()` does a full matrix decompose per robot for a Y-coordinate check — Medium

**File(s):** `src/systems/simulation/RobotPositionTracker.ts` (lines ~17–22)
**Category:** Memory/GC
**Impact:** `convertJoltMat44ToThreeMatrix4` (1 Matrix4 + 2 Vector3 + 1 Quaternion) then `decompose` into 3 more objects = ~7 THREE allocations/robot/frame — to read only the Y position for an out-of-bounds check.
**Recommendation:** Replace with `rootBody.GetPosition().GetY()` (single WASM getter, no allocations). Keep the full path only where the whole transform is actually consumed, with reused scratch fields.

### [PERF-022] `EjectableSceneObject.update()` allocates ~8 transform temporaries per held piece per frame — Medium

**File(s):** `src/mirabuf/EjectableSceneObject.ts` (lines ~96–152)
**Category:** Render Loop
**Impact:** Per active ejectable: 3× `Vector3`, 2× `Quaternion`, 2× matrix `.clone()`, 1× `Matrix4` → ~960 short-lived objects/sec with 2 ejectables, during the animation window.
**Recommendation:** Pre-allocate scratch fields (`_scratchVec3a/b`, `_scratchQuat`, `_scratchMat4a/b`) and reuse via `set()`/`copy()`/`identity()`.

### [PERF-023] `InputSystem.getInput` does an O(N) `.find()` over scheme inputs on every poll — Medium

**File(s):** `src/systems/input/InputSystem.ts` (line ~183)
**Category:** EventSystem
**Impact:** `targetScheme?.inputs.find(i => i.inputName == name)` per input query; ~6–10 inputs × 6 robots → 36–60 linear scans/frame at 60 Hz.
**Recommendation:** Build a `Map<InputName, Input>` when a scheme is assigned (`setBrainIndexSchemeMapping`) and look up O(1).

### [PERF-024] `GizmoSceneObject.updateNodeTransform()` allocates 3 math objects per rigid node per drag frame — Medium

**File(s):** `src/systems/scene/GizmoSceneObject.ts` (lines ~215–218)
**Category:** Memory/GC
**Impact:** Per node: `new Vector3` + `new Quaternion` + `new Vector3(1,1,1)` as decompose buffers, called in a `forEach` over all nodes each frame while dragging. 30 nodes × 60 Hz ≈ 5,400 allocations/sec.
**Recommendation:** Hoist `_scratchPos`/`_scratchQuat`/`_scratchScale` fields and reuse.

### [PERF-025] `filterSceneObjects` spreads the whole scene-object Map into an array every frame — Medium

**File(s):** `src/systems/scene/SceneRenderer.ts` (lines ~62–64); called from `CameraControls.validateFocusProvider` (line ~190, run every `update()`)
**Category:** Render Loop
**Impact:** `[...this._sceneObjects.values()].filter(...)` → O(N) iteration + temp array each frame; with a field + 6 robots that's hundreds of objects scanned at 60 Hz, plus a discarded array.
**Recommendation:** Maintain a dedicated `Set`/array of mirabuf scene objects updated on register/remove; gate `validateFocusProvider` behind a dirty flag so it runs only when the set changes.

### [PERF-026] Skybox fragment shader evaluates 4D simplex noise on every background pixel for a static gradient — Medium

**File(s):** `src/shaders/fragment.glsl` (lines ~32–33)
**Category:** Render Loop
**Impact:** `snoise(vec4)` (≈60–100 ALU ops) runs for every background fragment (~2M at 1080p) every frame — ~160M shader ops/frame — for a gradient whose driving uniforms (`rColor/gColor/bColor`) are never updated (the update path is commented out in `SceneRenderer.ts` ~496–503). The `w` component is constant `1.0`, so it's a 3D call dressed as 4D.
**Recommendation:** Bake the skybox to a cube/render target once at startup and sample it; or replace with a cheap `vPosition.y` gradient. At minimum drop to `snoise(vec3)`.

### [PERF-027] `EventSystem` per-frame queue realloc + `CustomEvent` (DOM) wrappers on the hot contact path — Medium

**File(s):** `src/systems/physics/PhysicsSystem.ts` (line ~1371), `src/systems/EventSystem.ts` (line ~93)
**Category:** EventSystem
**Impact:** `this._physicsEventQueue = []` abandons the array to GC each frame; each entry is a full DOM `CustomEvent` (via `EventSystem.create()`) dispatched through `window`. `OnContactPersisted` produces one per persistent pair per step → O(contacts) DOM-event objects/frame. The multiplayer path `.filter()`s the queue twice (two more temp arrays).
**Recommendation:** Reuse the array with `.length = 0`. For same-thread contact callbacks, bypass the DOM-event wrapper with a lightweight `Map<key, Set<listener>>` pub/sub to avoid `CustomEvent` + `window.dispatchEvent` overhead.

## Summary Table

| ID       | Title                                                       | Category       | Severity |
| -------- | --------------------------------------------------------- | -------------- | -------- |
| PERF-001 | WASM leak + JS allocs in `convertJoltMat44ToThreeMatrix4` | TypeConversions| High     |
| PERF-002 | `OnContactAdded` leaks `JOLT.BodyID` per contact          | Memory/GC      | High     |
| PERF-003 | Per-vertex WASM alloc/crossings in shape builders         | TypeConversions| High     |
| PERF-004 | Substeps pinned at 20 regardless of scene                 | Physics        | High     |
| PERF-005 | `OnContactValidate` queues every pair, never filters      | EventSystem    | High     |
| PERF-006 | `ScoringZone.update` leaks WASM shape every frame         | Memory/GC      | High     |
| PERF-007 | `rigidNodes` getter rebuilds Map+wrappers per call        | Memory/GC      | High     |
| PERF-008 | `updateBatches` recomputes bounds every frame             | Render Loop    | High     |
| PERF-009 | `DragModeSystem` allocates Raycaster+5 objs/drag frame    | Memory/GC      | High     |
| PERF-010 | `CameraControls.update` allocates Matrix4/Euler/frame     | Memory/GC      | High     |
| PERF-011 | `SimulationLayer` drivers/stimuli spread getters          | Memory/GC      | High     |
| PERF-012 | `SimGeneric.set` 9 postMessage+dispatch/frame/robot       | Worker         | High     |
| PERF-013 | `UIProvider` context value unmemoized                     | React          | High     |
| PERF-014 | `BespokeGraph` re-renders SVG tree at rAF rate            | Render Loop    | High     |
| PERF-015 | `SceneOverlay` rebuilds JSX every physics frame           | Render Loop    | High     |
| PERF-016 | Mirabuf decompress/decode/parse block main thread         | Worker         | Medium   |
| PERF-017 | `JoltSyncLoader` top-level await suspends import graph     | Asset Loading  | Medium   |
| PERF-018 | Gyro/accel redundant WASM calls + temporaries             | Physics        | Medium   |
| PERF-019 | `MirabufInstance` double-copies buffers, `.at()` in loop  | Asset Loading  | Medium   |
| PERF-020 | `bodyToMiraSceneObject` O(R×N) scan per contact           | Physics        | Medium   |
| PERF-021 | `RobotPositionTracker` full decompose for Y check         | Memory/GC      | Medium   |
| PERF-022 | `EjectableSceneObject` ~8 temporaries/piece/frame         | Render Loop    | Medium   |
| PERF-023 | `InputSystem.getInput` O(N) find per poll                 | EventSystem    | Medium   |
| PERF-024 | `GizmoSceneObject` 3 allocs/node/drag frame               | Memory/GC      | Medium   |
| PERF-025 | `filterSceneObjects` spreads Map every frame              | Render Loop    | Medium   |
| PERF-026 | Skybox 4D simplex noise per pixel for static gradient     | Render Loop    | Medium   |
| PERF-027 | EventSystem queue realloc + DOM CustomEvent on contacts   | EventSystem    | Medium   |

## Quick Wins

Best effort-to-impact ratio — small, localized changes:

- **PERF-005** — Delete the `OnContactValidate` registration if unused. One line; removes up to 20×N WASM→JS round-trips + allocations per frame.
- **PERF-002** — Make `OnContactAdded` use `body.GetID()` like `OnContactPersisted` already does. Two lines; stops a 12k-objects/sec WASM leak.
- **PERF-006** — Cache `props.scale` in `ScoringZoneSceneObject.update()` and skip `setShape` when unchanged. Eliminates a per-frame WASM leak for static zones.
- **PERF-013** — Wrap the `UIProvider` value in `useMemo`. One change; stops a global re-render wave on every modal/panel toggle.
- **PERF-007** — Cache the `rigidNodes` `ReadonlyMap` instead of rebuilding it per access. Removes ~140 allocations/frame per robot.
- **PERF-021** — Swap `RobotPositionTracker`'s decompose for `GetPosition().GetY()`. Removes ~7 allocations/robot/frame.

## Deferred / Out of Scope

These were observed but are lower priority or need confirmation before action:

- **WASM-leak sweep (cross-cutting):** PERF-001/002/006/018 are instances of one pattern — Emscripten value-returns (`GetTranslation`, `GetQuaternion`, `GetRotation`, `GetLinearVelocity`, `Vec3`/`Quat`/`BodyID` constructors) not paired with `JOLT.destroy()`. Worth a dedicated audit of all `JOLT.`/`.Get*()` call sites with a leak-counter harness (instrument the allocator) rather than fixing file by file.
- **`PerformanceMonitor` interval leak** (`PerformanceMonitor.ts` ~14/47): `setInterval` never cleared; `destroy()` is an empty stub, leaking one 15 s interval per `initWorld()`/`destroyWorld()` cycle. Low runtime cost but a correctness/leak issue; store and `clearInterval` the handle.
- **`World.time()` timing harness** (`World.ts` ~141–164): uses `Date.now()` (1 ms resolution can't resolve sub-ms subsystems) and allocates 5 closures/frame. Switch to `performance.now()` and inline timing. Low impact; mainly affects the accuracy of the profiler the team itself relies on.
- **`Lazy.getValue()` falsy guard** (`Lazy.ts` ~13): `if (!this._value)` re-initializes for falsy `T` (`0`, `false`, `""`). Not triggered by the current `Lazy<Worker>` usage, so no live perf impact — flagged as a latent correctness bug for future numeric/boolean uses.
- **Driver property write-through caches** (`HingeDriver.ts`/`SliderDriver.ts`): `targetAngle`/`targetPosition` setters call `GetLimitsMin/Max()` across WASM on each write; `maxForce` getter calls `GetMotorSettings()` per read. Limits are fixed at construction — cache them. Low per-call cost; only matters if these are set per frame in WPILib mode.
- **`getBody` lock-interface caching** (`PhysicsSystem.ts` ~1313): `GetBodyLockInterface()` is reconstructed per call rather than cached like `_joltBodyInterface`. Minor; relevant only when the event queue is large.
- **`SimulationLayer` JSON.stringify Map keys** (`SimulationSystem.ts` ~88–111): construction-time only today, but fragile (V8 key-ordering) and an allocation risk if any caller stringifies a `DriverID` per frame. Switch to a stable `` `${type}:${guid}` `` key function.
- **Substep reduction (PERF-004) needs profiling** before shipping: verify contact stability (no tunneling/jitter on wheels and thin field elements) at the lower floor across the supported field/robot set.
