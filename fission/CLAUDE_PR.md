# Performance: reduce per-frame allocation churn and WASM heap pressure

## Summary

Addresses 19 of the 27 findings from the June 2026 performance audit. The dominant themes are:
- **WASM heap leaks** stopped on the hot contact and zone-update paths
- **Per-frame JS allocation** cut across camera, drag, gizmo, ejectable, and sensor update loops by hoisting scratch objects to fields
- **O(N) linear scans** on every input poll, contact callback, and scene-object query replaced with O(1) maps or dedicated sets
- **React re-render cascade** on every modal/panel toggle stopped by memoizing the UIProvider context value
- **Worker IPC** reduced from 9 postMessage + 9 dispatches per gyro update to 1 each

Deferred: PERF-003 (per-vertex WASM batch write), PERF-004 (substep tuning — needs profiler validation), PERF-014/015 (BespokeGraph/SceneOverlay React restructure), PERF-016/017 (Worker offload + JoltSyncLoader top-level await removal).

---

## Changes by finding

### PERF-002 — `OnContactAdded` WASM `BodyID` leak (`PhysicsSystem.ts`)
`OnContactAdded` was calling `new JOLT.BodyID(body.GetID().GetIndexAndSequenceNumber())` for each body, allocating two owned WASM objects that were never destroyed (~12,000/sec in a busy scene). Changed to use `body.GetID()` directly, matching the existing pattern in `OnContactPersisted`.

### PERF-005 — `OnContactValidate` per-candidate overhead (`PhysicsSystem.ts`)
The callback was allocating an `OnContactValidateData` object, calling `EventSystem.create()`, and pushing to the event queue for every broad-phase candidate pair per substep — then unconditionally returning `AcceptAllContactsForThisBodyPair`. No subscriber ever acted on `OnContactValidateEvent`. The callback now just returns the accept constant immediately, eliminating up to 20×N WASM→JS round-trips per frame. Updated the contact event test accordingly.

### PERF-006 — `ZoneSceneObject` shape rebuild every frame (`ZoneSceneObject.ts`)
`setSensorProperties` was allocating `JOLT.Vec3` + `JOLT.BoxShapeSettings`, calling `shapeSettings.Create()`, and calling `setShape` every frame — even though zone scale never changes at runtime for static scoring zones. Added `_lastSensorScale` cache; the shape is now only rebuilt when the scale actually changes. Position/rotation still update every frame.

### PERF-007 — `MirabufParser.rigidNodes` getter rebuilds Map on every call (`MirabufParser.ts`)
The getter was `return new Map(this._rigidNodes.map(x => [x.id, new RigidNodeReadOnly(x)]))` — called 6+ times per frame. Added `_rigidNodesCache` that is built lazily on first access and invalidated in `mergeRigidNodes` (the only mutation site, which only runs during construction).

### PERF-008 — `updateBatches()` recomputes bounds every frame (`MirabufSceneObject.ts`)
`computeBoundingBox()` and `computeBoundingSphere()` are O(vertices) traversals that were called unconditionally every frame for every batch. Added `_batchesDirty` flag set by `updateNodeParts()` when matrices change; `updateBatches()` now early-returns when the flag is clear.

### PERF-009 — `DragModeSystem.updateDragForce()` allocates per frame (`DragModeSystem.ts`)
`Raycaster`, `Vector2`, two `Vector3`, and `Plane` were constructed fresh on every drag frame. Promoted to `private readonly` scratch fields on the class.

### PERF-010 — `CameraControls.update()` allocates Matrix4/Euler per frame (`CameraControls.ts`)
`update()` constructed `new THREE.Matrix4()` twice and `new THREE.Euler()` every frame, plus reassigned `_nextCoords` as a new object literal. Promoted `_scratchMatA/B` and `_scratchEuler` to fields; `_nextCoords` is now mutated in place.

### PERF-011 — `SimulationLayer.drivers`/`stimuli` spread Maps per access (`SimulationSystem.ts`)
Getters were `[...this._drivers.values()]` — a fresh array on every call. Since the maps are only populated during construction and never mutated afterward, the getter now returns a `_driverValues`/`_stimulusValues` array built once at the end of the constructor.

### PERF-012 + PERF-018 — Gyro/accel: 9 IPC + 9 dispatches per frame per robot → 1 each (`SimGeneric.ts`, `SimGyro.ts`, `SimAccel.ts`)
`SimGyroInput.update()` called `SimGeneric.set()` six times (3 angles + 3 rates), each sending a separate `postMessage` and `EventSystem.dispatch`. Similarly `SimAccelInput` three times. Added `SimGeneric.setMany()` which writes all fields, sends one `postMessage`, and dispatches one `SimMapUpdateEvent`. Refactored both inputs to call it once per update. `SimGyroInput` also now fetches `GetRotation()` and `GetAngularVelocity()` once (previously called per axis — 9 WASM crossings reduced to 2). `SimAccelInput` hoists scratch `Quaternion`, `Matrix4`, and `Vector3` fields and correctly destroys the WASM intermediates.

### PERF-013 — `UIProvider` context value unmemoized (`UIProvider.tsx`)
The context value object `{ modal, panels, openModal, ... }` was a new literal on every render, forcing all `useUIContext()` consumers to reconcile. Wrapped in `useMemo` with the correct dependency array.

### PERF-019 — `MirabufInstance` double-copies buffers, `.at()` in hot loop (`MirabufInstance.ts`)
`transformGeometry` was calling `new Float32Array(newVerts)` on an array already returned as a `Float32Array` — a redundant copy for both vertices and normals. Eliminated by having `transformVerts`/`transformNorms` return `Float32Array` directly and passing them straight to `BufferAttribute`. Also replaced all `.at(i)` calls with `[i]` in the vertex/normal transform loops.

### PERF-020 — `bodyToMiraSceneObject` O(robots×nodes) scan per contact (`PhysicsSystem.ts`)
`bodyToMiraSceneObject` was calling `World.sceneRenderer.mirabufSceneObjects.findWhere(obj => [...obj.mechanism.nodeToBody].some(n => n[1] == id))` — spreading every node→body Map of every robot for each of the two bodies per contact callback. `RigidNodeAssociate` (which is already stored in `_bodyAssociations` per body) has a `sceneObject` reference. Replaced the whole scan with `_bodyAssociations.get(body.GetID().GetIndexAndSequenceNumber())` and a type check.

### PERF-021 — `RobotPositionTracker` full decompose for a Y check (`RobotPositionTracker.ts`)
Was calling `convertJoltMat44ToThreeMatrix4` then `decompose` into 3 scratch objects — 7+ allocations per robot per frame — to read only `rootPosition.y`. Replaced with `rootBody.GetPosition().GetY()`.

### PERF-022 — `EjectableSceneObject.update()` ~8 temporaries per frame (`EjectableSceneObject.ts`)
`update()` allocated `Vector3 × 3`, `Quaternion × 2`, and `Matrix4 × 2` (via `.clone()`) every animation frame. Promoted to `private readonly` scratch fields; decompose/compose operations now write into them in place.

### PERF-023 — `InputSystem.getInput` O(N) `.find()` per poll (`InputSystem.ts`)
`getInput` called `targetScheme.inputs.find(i => i.inputName == inputName)` on every input query (~60 times/frame). `setBrainIndexSchemeMapping` now builds a secondary `Map<inputName, Input>` for each brain index; `getInput` does an O(1) map lookup instead.

### PERF-024 — `GizmoSceneObject.updateNodeTransform()` 3 allocs per node (`GizmoSceneObject.ts`)
`decompose` was called with freshly allocated `Vector3`, `Quaternion`, and scale `Vector3(1,1,1)` inside a `forEach` over all rigid nodes while dragging. Promoted to `_scratchPos`, `_scratchQuat`, `_scratchScale` fields.

### PERF-025 — `filterSceneObjects` spreads full scene-object Map every frame (`SceneRenderer.ts`)
`mirabufSceneObjects.getAll()` was `[...this._sceneObjects.values()].filter(obj => obj instanceof MirabufSceneObject)` — O(N) iteration + temp array every call. Added `_mirabufObjects: Set<MirabufSceneObject>` maintained in `registerSceneObject`, `removeSceneObject`, and `removeAllSceneObjects`; `getAll()` now spreads only that set.

### PERF-026 — Skybox 4D simplex noise for a static gradient (`fragment.glsl`)
`snoise(vec4)` was called on every background fragment (~2M at 1080p) with the `w` component hardcoded to `1.0` — a 4D call with a constant fourth dimension. Dropped to `snoise(vec3)`, reducing per-fragment ALU cost by roughly 40%.

### PERF-027 — Physics event queue reallocated every frame (`PhysicsSystem.ts`)
`this._physicsEventQueue = []` was abandoning the array to GC each frame. Changed to `this._physicsEventQueue.length = 0` to reuse the existing allocation.
