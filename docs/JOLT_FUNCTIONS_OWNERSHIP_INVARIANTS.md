# Jolt Physics — Function Ownership Invariants

This document lists every Jolt Physics function used in `fission/src/` (production code, excluding
`fission/src/test/`) together with its memory-ownership invariants. Its purpose is to make correct
Jolt memory management possible without reading the Jolt source for every call.

Fission uses the WebAssembly port [`@synthesis.adsk/jolt-physics`](https://www.npmjs.com/package/@synthesis.adsk/jolt-physics)
(a fork of JoltPhysics.js). Ownership semantics are governed by the Emscripten WebIDL binding,
defined in `jolt/JoltJS.idl` and `jolt/JoltJS.h`, layered on top of Jolt's C++ memory model. The IDL
is compiled by `webidl_binder.py` (at build time) into `glue.cpp` (one file per build config, under
`jolt/Build/<Config>/<ST|MT>/`), which implements each binding's argument- and return-passing as
concrete C++: heap allocation (`new T(...)`), a function-local `static T` scratch, an in-place
mutation of the receiver, or a plain reference into existing state. The categories and per-function
invariants below describe what each generated binding actually does.

---

## How to read this document

Every Jolt object created in JS lives on the WASM heap and must be explicitly freed with
`JOLT.destroy(obj)` unless something else owns it. The categories below answer, for each call,
"what do I have to free, and what must I leave alone?"

### Argument categories

- **`CONSUMED`** — Jolt takes ownership of the argument and will free it later (e.g. when the
  owning Jolt object is destroyed). The caller **must not** `destroy()` it; doing so double-frees.
- **`CLONED`** — Jolt only reads the argument or copies its value/contents into its own storage; it
  does **not** take ownership. The argument is a heap Jolt object, so the caller **still owns it and
  must `destroy()` it**. **Exception — reference-counted (`RefTarget`) types**: see below.
- **`COPIED`** — the argument is a primitive or enum (`number`, `boolean`, `Jolt.EActivation`,
  `Jolt.EMotionType`, …). Nothing to free.

### Reference-counted (`RefTarget`) exception

`RefTarget` subclasses used here: `Shape`, `ShapeSettings`, `Constraint`, `ConstraintSettings`,
`PathConstraintPath`, `PhysicsMaterial`, `GroupFilter`, `SoftBodySharedSettings`,
`VehicleCollisionTester`, `VehicleControllerSettings`, `WheelSettings`, `CharacterBaseSettings`,
`CharacterBase`, `Skeleton`, `SkeletonAnimation`, `SkeletonMapper`, `PhysicsScene`,
`RagdollSettings`, `Ragdoll`. These start at **refcount 0**.

A reference is added either manually (`object.AddRef()`, released with `object.Release()`, which
frees the object once the count hits 0) or automatically when the object is handed to a parent
(`push_back` into a list, `AddShape`, `SetShape`, `AddConstraint`, …) — the parent calls `AddRef()`
for you, so no manual `AddRef()`/`Release()` is needed on your end.

`JOLT.destroy(x)` is a raw `delete x`, **not** `Release()` — it ignores the refcount entirely.
Destroying a handle that's still referenced elsewhere frees memory another owner still points to.

Practical rule for these types:

- **Handed to a parent, never explicitly removed** (`AddShape`, `SetShape`, `mWheels.push_back`,
  `mController = ...`, `BodyCreationSettings.shape`, …): **do not `destroy()` it.** The parent
  owns a reference now; its own teardown releases it.
- **Handed to a parent, later explicitly removed** (`AddConstraint` → `RemoveConstraint`,
  `AddStepListener` → `RemoveStepListener`): safe to `destroy()` **only after** the removal call
  has actually dropped the parent's reference.
- **Never handed to anything** (created, used locally, discarded): safe to `destroy()`
  immediately.

Plain (non-`RefTarget`) value types (`Vec3`, `RVec3`, `Quat`, `Mat44`, `Float3`,
`IndexedTriangle`, `AABox`, …) are unaffected — `CLONED` there always means a deep copy; always
`destroy()` your own handle.

### Return categories

- **`INTERNAL_REF`** — the return value is a reference/handle into state owned by another Jolt
  object (the parent body, the physics system, a result struct, …). It is valid only while that
  owner lives, and the caller **must not** `destroy()` it. In `JoltJS.idl` these are bare interface
  pointers or `[Ref]` / `[Const, Ref]` returns, implemented in `glue.cpp` as `return self->Getter();`
  or `return &self->member;` — the address of something that already exists independent of the call.
- **`COPY`** — the return value is a freshly allocated heap object (`glue.cpp` does `return new
  T(...)`) that the caller **owns and must `destroy()`** exactly once. This is **only** true for
  constructors (`new JOLT.X(...)`) and for `<TwoBody>ConstraintSettings.Create(body1, body2)` (which
  allocates a fresh refcounted `Constraint`).
- **`STATIC_ALIAS`** — the return value is the address of a **function-local `static` C++ variable**
  (`glue.cpp`: `static T temp; return (temp = self->Method(), &temp);`), not a heap allocation. The
  caller **must never `destroy()` it** — it was never `malloc`/`new`'d, so `JOLT.destroy()` on it is a
  bad-free. It is also **invalidated by the next call to that exact same bound function**, anywhere in
  the program — that specific `static` is overwritten in place, not reallocated, so holding a
  reference to it across another call to the same accessor silently returns stale/wrong data instead
  of crashing. This applies to essentially every non-constructor `[Value]`-returning getter, math
  operator, and static factory function in the binding: every vector/quaternion/matrix getter
  (`GetPosition`, `GetLinearVelocity`, `GetWorldTransform`, `GetCenterOfMass`, `GetTranslation`,
  `GetQuaternion`, …), every value-producing math operator (`Normalized`,
  `AddVec3`/`SubVec3`/`MulVec3`/`DivVec3`, `MulFloat`/`DivFloat`, …), static factories (`Vec3.sZero`,
  `Quat.sIdentity`, `Quat.sRotation`, `AABox.sBiggest`, …), and `ShapeSettings.Create()`. Snapshot the
  data you need (read components via `GetX()`/`GetY()`/`GetZ()`, or copy into a `THREE.js` object)
  before making any other call that returns the same C++ type from the same function.
- **`ALIASES_THIS`** — the return value is the **same object as the receiver** (`glue.cpp`:
  `return &(*self += *inV);` — an in-place compound-assignment operator that mutates `self` and
  returns a reference to it). The caller **must never `destroy()` it** — doing so double-frees the
  receiver, since the "returned" pointer and the receiver's pointer are identical. Applies to
  `Vec3`/`RVec3`'s in-place `Add`/`Sub`/`Mul`/`Div` (**not** the `*Vec3`/`*Float`-suffixed siblings,
  which are `STATIC_ALIAS` — see the rule of thumb below).
- **`NONE`** — the function returns `void` or a primitive (`number` / `boolean` / enum). Nothing to
  free.

### Rules of thumb (from the binding)

1. Value-returning getters, math operators, and static factory functions are implemented in
   `glue.cpp` as a function-local `static T temp; return (temp = ..., &temp);` — i.e.
   **`STATIC_ALIAS`**, not a heap copy. Never `destroy()` these, and never hold one across another
   call to that same bound function. This includes every vector/quaternion/matrix getter
   (`GetPosition`, `GetLinearVelocity`, `GetCenterOfMass`, `GetWorldTransform`, …) and every
   *value-producing* math operator (`Normalized`, `AddVec3`, `MulFloat`, …). The **only** genuine
   `COPY` returns are constructors (`new JOLT.X(...)`, which `glue.cpp` implements as a real
   `return new T(...)`) and `<TwoBody>ConstraintSettings.Create()`.
2. A bare interface-pointer return (`Body`, `Shape`, `BodyInterface`, `MotorSettings`, …) is an
   **INTERNAL_REF**; never `destroy()` it.
3. A Jolt heap object passed as an argument that Jolt merely reads (`[Const, Ref]` / `[Ref]`) is
   **CLONED** — you keep ownership. A primitive/enum argument is **COPIED**.
4. Arithmetic methods `Add`, `Sub`, `Mul`, `Div` on `Jolt.Vec3` and `Jolt.RVec3` do not consume the
   vector nor produce a new one — they modify the `this` vector in place
   (`glue.cpp`: `return &(*self += *inV);`) and return a reference to `this`. This is `ALIASES_THIS`:
   never `destroy()` the return, since it's the same object as the receiver.
5. The corresponding `*Vec3`/`*Float`-suffixed arithmetic methods (`AddVec3`, `DivFloat`, `MulFloat`,
   etc.) do **not** consume the operand and do **not** produce a newly allocated vector — `glue.cpp`
   implements these the same way as every other math-op getter: a function-local `static T temp`.
   They are `STATIC_ALIAS`, not `COPY`. Never `destroy()` their return value.

---

## AABox

- `AABox.sBiggest()` (static)
  - Arguments: None
  - Returns: `STATIC_ALIAS` — `glue.cpp`: `static AABox temp; return (temp = AABox::sBiggest(), &temp);`.
    Do **not** `destroy()`; invalidated by the next call to `sBiggest()` anywhere in the program.
- `AABox.mMin` / `AABox.mMax` (field read → `Vec3`)
  - Reading these fields yields references into the box; treat values pulled out via further
    `[Value]` getters (`GetY()`, etc.) per their own rules. The fields themselves: No Ownership Concerns.

## Body

`Body` is `[NoDelete]` — bodies are owned by Jolt's body manager and are created/destroyed only
through `BodyInterface`. Never `destroy()` a `Body`.

- `Body.GetID()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Const, Ref] BodyID` into the body. Do not `destroy()`.
- `Body.GetShape()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Const] Shape` owned by the body. Do not `destroy()`.
- `Body.GetMotionProperties()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — pointer to the body's motion properties. Do not `destroy()`.
- `Body.GetPosition()` / `GetRotation()` / `GetCenterOfMassPosition()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` — each is its own function-local `static RVec3`/`Quat temp` in `glue.cpp`.
    Do **not** `destroy()`. Calling `GetPosition()` again (on any body) overwrites the data the
    previous `GetPosition()` result pointed to; `GetRotation()` has its own separate static and does
    not alias `GetPosition()`'s.
- `Body.GetWorldTransform()` / `GetCenterOfMassTransform()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` — `static RMat44 temp` per function. Do **not** `destroy()`.
- `Body.GetWorldSpaceBounds()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` — `static AABox temp`. Do **not** `destroy()`.
- `Body.GetLinearVelocity()` / `GetAngularVelocity()` / `GetAccumulatedForce()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` — `static Vec3 temp` per function. Do **not** `destroy()`.
- `Body.SetLinearVelocity(velocity: Vec3)` / `SetAngularVelocity(velocity: Vec3)`
  - Arguments
    - `velocity`: CLONED (`[Const, Ref] Vec3`; value copied in, caller frees)
  - Returns: `NONE`
- `Body.AddForce(force: Vec3)` / `AddTorque(torque: Vec3)`
  - Arguments
    - `force` / `torque`: CLONED (`[Const, Ref] Vec3`; caller frees)
  - Returns: `NONE`
- `Body.GetID`-adjacent boolean/scalar getters — `IsActive()`, `IsSensor()`, `IsStatic()`,
  `GetObjectLayer()` — and scalar setters — `SetAllowSleeping(bool)`, `SetFriction(number)`,
  `SetIsSensor(bool)`, `SetRestitution(number)`
  - No Ownership Concerns (primitive in / primitive or void out).

## BodyCreationSettings

- `new BodyCreationSettings(shape: Shape, position: RVec3, rotation: Quat, motionType: EMotionType, objectLayer: number)`
  - Arguments
    - `shape`: CLONED — stored as a refcounted `RefConst<Shape>`; Jolt retains a reference, caller keeps and frees its own handle.
    - `position`: CLONED (`[Ref] RVec3`; value copied into `mPosition`, caller frees)
    - `rotation`: CLONED (`[Ref] Quat`; value copied into `mRotation`, caller frees)
    - `motionType`: COPIED (enum)
    - `objectLayer`: COPIED (number)
  - Returns: `COPY` — caller owns the settings object and must `destroy()` it after the body is
    created (the body keeps its own copy/refs; destroying the settings does not affect the body).
- `BodyCreationSettings.mOverrideMassProperties = EOverrideMassProperties` — COPIED (enum). No Ownership Concerns.
- `BodyCreationSettings.mMassPropertiesOverride.mMass = number` — COPIED (number). No Ownership Concerns.

## BodyID

- `new BodyID(indexAndSequenceNumber: number)`
  - Arguments
    - `indexAndSequenceNumber`: COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `BodyID.GetIndexAndSequenceNumber()`
  - Arguments: None
  - Returns: `NONE` (number). No Ownership Concerns.

## BodyInterface

`BodyInterface` is obtained from `PhysicsSystem.GetBodyInterface()` (an `INTERNAL_REF`); do not
`destroy()` it (fission destroys the handle once at teardown, which is the binding's documented
pattern).

- `BodyInterface.CreateBody(settings: BodyCreationSettings)`
  - Arguments
    - `settings`: CLONED (`[Const, Ref]`; read only — the body manager allocates the body from it, caller frees the settings)
  - Returns: `INTERNAL_REF` — the returned `Body` is owned by the body manager. Free it via
    `DestroyBody(id)`, never `destroy()`.
- `BodyInterface.AddBody(bodyID: BodyID, activationMode: EActivation)`
  - Arguments
    - `bodyID`: CLONED (`[Const, Ref] BodyID`; caller frees)
    - `activationMode`: COPIED (enum)
  - Returns: `NONE`
- `BodyInterface.RemoveBody(bodyID: BodyID)` / `DestroyBody(bodyID: BodyID)` / `ActivateBody(bodyID: BodyID)` / `DeactivateBody(bodyID: BodyID)` / `IsAdded(bodyID: BodyID)`
  - Arguments
    - `bodyID`: CLONED (`[Const, Ref] BodyID`; caller frees). `DestroyBody` frees the _body_, not
      the `BodyID` argument.
  - Returns: `NONE` (`IsAdded` returns a boolean → `NONE`).
- `BodyInterface.SetPosition(bodyID: BodyID, position: RVec3, activation: EActivation)`
  - Arguments
    - `bodyID`: CLONED
    - `position`: CLONED (`[Const, Ref]`)
    - `activation`: COPIED
  - Returns: `NONE`
- `BodyInterface.SetRotation(bodyID: BodyID, rotation: Quat, activation: EActivation)`
  - Arguments
    - `bodyID`: CLONED
    - `rotation`: CLONED
    - `activation`: COPIED
  - Returns: `NONE`
- `BodyInterface.SetPositionAndRotation(bodyID: BodyID, position: RVec3, rotation: Quat, activation: EActivation)`
  - Arguments
    - `bodyID`: CLONED
    - `position`: CLONED
    - `rotation`: CLONED
    - `activation`: COPIED
  - Returns: `NONE`
- `BodyInterface.SetLinearVelocity(bodyID: BodyID, velocity: Vec3)` / `SetAngularVelocity(bodyID: BodyID, velocity: Vec3)`
  - Arguments
    - `bodyID`: CLONED
    - `velocity`: CLONED
  - Returns: `NONE`
- `BodyInterface.SetShape(bodyID: BodyID, shape: Shape, updateMassProperties: boolean, activation: EActivation)`
  - Arguments
    - `bodyID`: CLONED
    - `shape`: CLONED (reference counted; Jolt retains a reference)
    - `updateMassProperties`: COPIED
    - `activation`: COPIED
  - Returns: `NONE`

## BodyLockInterface

- `BodyLockInterface.TryGetBody(bodyID: BodyID)`
  - Arguments
    - `bodyID`: CLONED (`[Const, Ref] BodyID`; caller frees)
  - Returns: `INTERNAL_REF` — `Body` owned by the body manager. Do not `destroy()`.

## BoxShape

- `new BoxShape(halfExtent: Vec3, convexRadius?: number)`
  - Arguments
    - `halfExtent`: CLONED (`[Ref] Vec3`; copied in, caller frees)
    - `convexRadius`: COPIED (number)
  - Returns: `COPY` — reference counted `Shape`; caller owns the handle.
- `BoxShape.GetHalfExtent()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` (`static Vec3 temp`). Do **not** `destroy()`.

## BoxShapeSettings

- `new BoxShapeSettings(halfExtent: Vec3)`
  - Arguments
    - `halfExtent`: CLONED (`[Ref] Vec3`; copied into `mHalfExtent`, caller frees)
  - Returns: `COPY` — caller owns the settings object.
- `BoxShapeSettings.Create()` — see `ShapeSettings.Create()`.

## BroadPhaseLayer

- `new BroadPhaseLayer(layerIndex: number)`
  - Arguments
    - `layerIndex`: COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.

## BroadPhaseLayerFilter

- `new BroadPhaseLayerFilter()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; used transiently for a raycast and must be destroyed.

## BroadPhaseLayerInterfaceTable

- `new BroadPhaseLayerInterfaceTable(numObjectLayers: number, numBroadPhaseLayers: number)`
  - Arguments
    - both: COPIED (number)
  - Returns: `COPY` — **but** once assigned to `JoltSettings.mBroadPhaseLayerInterface` and passed
    to `new JoltInterface(...)`, ownership transfers to the `JoltInterface`, which deletes it in its
    destructor. See `JoltSettings`. Do not `destroy()` it yourself after that hand-off.
- `BroadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(objectLayer: number, broadPhaseLayer: BroadPhaseLayer)`
  - Arguments
    - `objectLayer`: COPIED (number)
    - `broadPhaseLayer`: CLONED (`[Const, Ref] BroadPhaseLayer`; value copied into the table, caller frees)
  - Returns: `NONE`

## CastRayClosestHitCollisionCollector

- `new CastRayClosestHitCollisionCollector()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `CastRayClosestHitCollisionCollector.HadHit()`
  - Arguments: None
  - Returns: `NONE` (boolean).
- `CastRayClosestHitCollisionCollector.mHit` (field → `RayCastResult`)
  - `INTERNAL_REF` — reference to the result embedded in the collector. Do not `destroy()`; it dies
    with the collector. `.mHit.mFraction` / `.mHit.mBodyID` are values read out of it.

## ContactListenerJS

- `new ContactListenerJS()`
  - Arguments: None
  - Returns: `COPY` — caller owns it. Note: once passed to `PhysicsSystem.SetContactListener`, the
    physics system references it; fission retrieves it via `GetContactListener()` and `destroy()`s it
    explicitly at teardown.
- `.OnContactAdded` / `.OnContactPersisted` / `.OnContactRemoved` / `.OnContactValidate` (callback fields)
  - Assigning a JS function. The pointers handed to the callbacks (`bodyPtr1`, `manifoldPtr`, …) are
    `INTERNAL_REF`s owned by Jolt for the duration of the callback — wrap them with
    `JOLT.wrapPointer` to read, never `destroy()` them.

## ConvexHullShapeSettings

- `new ConvexHullShapeSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns the settings; `destroy()` after `Create()`.
- `ConvexHullShapeSettings.mPoints` (`ArrayVec3`, via `push_back` / `clear` / `reserve`)
  - `push_back(point: Vec3)`: the point's value is copied into the array → `point` is CLONED (caller
    frees the `Vec3` it pushed). `clear()` / `reserve(n)`: No Ownership Concerns.
- `ConvexHullShapeSettings.mDensity = number` — COPIED. No Ownership Concerns.
- `ConvexHullShapeSettings.Create()` — see `ShapeSettings.Create()`.

## FixedConstraintSettings

- `new FixedConstraintSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `FixedConstraintSettings.mPoint1 = RVec3` / `mPoint2 = RVec3`
  - The RHS value is copied into the settings (`[Value] attribute RVec3`) → RHS is CLONED (caller frees).
- `FixedConstraintSettings.Create(body1, body2)` — see `*ConstraintSettings.Create` under "Constraint creation".

## Float3

- `new Float3(x: number, y: number, z: number)`
  - Arguments: all COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.

## HingeConstraint

Obtained by `JOLT.castObject(constraint, JOLT.HingeConstraint)`; the cast does not change ownership.

- `HingeConstraint.GetMotorSettings()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Ref] MotorSettings` living inside the constraint. Mutate in place;
    do not `destroy()`.
- `HingeConstraint.GetCurrentAngle()` / `GetLimitsMin()` / `GetLimitsMax()` / `GetTargetAngularVelocity()`
  - Returns: `NONE` (number).
- `HingeConstraint.SetMotorState(state: EMotorState)`
  - Arguments: `state`: COPIED (enum). Returns: `NONE`.
- `HingeConstraint.SetTargetAngle(number)` / `SetTargetAngularVelocity(number)`
  - Arguments: COPIED (number). Returns: `NONE`.

## HingeConstraintSettings

- `new HingeConstraintSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- Field writes — `mPoint1` / `mPoint2` (`RVec3`), `mHingeAxis1` / `mHingeAxis2`,
  `mNormalAxis1` / `mNormalAxis2` (`Vec3`)
  - RHS value is copied in (`[Value] attribute`) → RHS is CLONED (caller frees the vector).
- Field writes — `mLimitsMin` / `mLimitsMax`, `mMaxFrictionTorque`,
  `mMotorSettings.mMaxTorqueLimit` / `mMotorSettings.mMinTorqueLimit`
  - COPIED (number). No Ownership Concerns.
- `HingeConstraintSettings.Create(body1, body2)` — see "Constraint creation".

## IgnoreMultipleBodiesFilter

- `new IgnoreMultipleBodiesFilter()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `IgnoreMultipleBodiesFilter.IgnoreBody(bodyID: BodyID)`
  - Arguments
    - `bodyID`: CLONED (`[Const, Ref] BodyID`; value copied into the filter list, caller frees)
  - Returns: `NONE`

## IndexedTriangle

- `new IndexedTriangle(i1: number, i2: number, i3: number, materialIndex: number)`
  - Arguments: all COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()` (or is consumed by `push_back`, see below).

## IndexedTriangleList

- `new IndexedTriangleList()`
  - Arguments: None
  - Returns: `COPY` — caller owns it.
- `IndexedTriangleList.push_back(triangle: IndexedTriangle)`
  - Arguments
    - `triangle`: CLONED — the triangle's value is copied into the list; caller still owns the
      pushed object (fission constructs it inline and does not free it, which is a minor leak, but
      the ownership invariant is CLONED).
  - Returns: `NONE`
- `IndexedTriangleList.size()` — Returns `NONE` (number).

## JoltInterface

- `new JoltInterface(settings: JoltSettings)`
  - Arguments
    - `settings`: CLONED — read to construct the interface; caller must `destroy()` the
      `JoltSettings` afterward. **However**, the three filter/interface objects _referenced by_ the
      settings (`mBroadPhaseLayerInterface`, `mObjectVsBroadPhaseLayerFilter`,
      `mObjectLayerPairFilter`) become **CONSUMED** — see `JoltSettings`.
  - Returns: `COPY` — caller owns the interface and must `destroy()` it at teardown (which also
    deletes the consumed filter objects and the internal `PhysicsSystem`).
- `JoltInterface.GetPhysicsSystem()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — the `PhysicsSystem` owned by the interface. Do not `destroy()`.
- `JoltInterface.Step(deltaTime: number, collisionSteps: number)`
  - Arguments: COPIED (number). Returns: `NONE`.

## JoltSettings

- `new JoltSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; `destroy()` right after constructing the `JoltInterface`.
- `JoltSettings.mObjectLayerPairFilter = ObjectLayerPairFilterTable`
- `JoltSettings.mBroadPhaseLayerInterface = BroadPhaseLayerInterfaceTable`
- `JoltSettings.mObjectVsBroadPhaseLayerFilter = ObjectVsBroadPhaseLayerFilterTable`
  - Assigned object: **CONSUMED**. These store raw pointers in the settings; when the settings are
    handed to `new JoltInterface(...)`, the `JoltInterface` takes ownership and deletes all three
    in its destructor (see `JoltJS.h`, `JoltInterface::~JoltInterface`). The caller must **not**
    `destroy()` these objects — fission correctly destroys only the `JoltSettings` wrapper, not the
    filters.

## Mat44

- `new Mat44()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `Mat44.SetColumn4(columnIndex: number, column: Vec4)`
  - Arguments
    - `columnIndex`: COPIED (number)
    - `column`: CLONED (`[Const, Ref] Vec4`; copied in, caller frees — fission destroys it)
  - Returns: `NONE`
- `Mat44.GetTranslation()` / `GetQuaternion()` / `Multiply3x3(v: Vec3)`
  - Arguments (for `Multiply3x3`): `v`: CLONED (`[Const, Ref] Vec3`; caller frees).
  - Returns: `STATIC_ALIAS` — each has its own `static Vec3`/`Quat temp` in `glue.cpp`
    (`Multiply3x3`: `static Vec3 temp; return (temp = self->Multiply3x3(*inV), &temp);`). Do **not**
    `destroy()` the return; `JOLT.destroy()` on it is a bad-free, since the address was never
    `malloc`/`new`'d.

## MeshShapeSettings

- `new MeshShapeSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; `destroy()` after `Create()`.
- `MeshShapeSettings.mTriangleVertices = VertexList`
- `MeshShapeSettings.mIndexedTriangles = IndexedTriangleList`
- `MeshShapeSettings.mMaterials = PhysicsMaterialList`
  - Assigned list (`[Value] attribute`): its contents are copied into the settings → the assigned
    list is **CLONED** (caller still owns the list object). After assignment, reading the field back
    (e.g. `settings.mTriangleVertices.push_back(...)`) mutates the settings' own copy.
- `MeshShapeSettings.mMaxTrianglesPerLeaf = number` — COPIED. No Ownership Concerns.
- `MeshShapeSettings.Sanitize()` — Arguments: None; Returns: `NONE`.
- `MeshShapeSettings.Create()` — see `ShapeSettings.Create()`.

## MotionProperties

Obtained from `Body.GetMotionProperties()` (an `INTERNAL_REF`).

- `MotionProperties.GetInverseMass()` — Returns `NONE` (number).
- `MotionProperties.GetInverseInertiaDiagonal()` — Returns `STATIC_ALIAS` (`static Vec3 temp`); do
  **not** `destroy()`.

## MotorSettings

Obtained from `HingeConstraint`/`SliderConstraint`.`GetMotorSettings()` (an `INTERNAL_REF`). Mutate
in place; never `destroy()`.

- `.mMaxForceLimit` / `.mMinForceLimit` / `.mMaxTorqueLimit` / `.mMinTorqueLimit` (read/write `number`),
  and the generated `get_*` / `set_*` accessors for the same — COPIED (number). No Ownership Concerns.
- `MotorSettings.mSpringSettings` (field → `SpringSettings`)
  - `INTERNAL_REF` — embedded spring settings. `.mFrequency` / `.mDamping` are `number` writes
    (No Ownership Concerns). Do not `destroy()` the `SpringSettings`.

## NarrowPhaseQuery

Obtained from `PhysicsSystem.GetNarrowPhaseQuery()` (an `INTERNAL_REF`). Do not `destroy()`.

- `NarrowPhaseQuery.CastRay(ray: RRayCast, settings: RayCastSettings, collector: CastRayCollector, bpFilter: BroadPhaseLayerFilter, objectFilter: ObjectLayerFilter, bodyFilter: BodyFilter, shapeFilter: ShapeFilter)`
  - Arguments
    - all parameters are `[Const, Ref]` / `[Ref]` heap Jolt objects that are read (and the collector
      written to) but **not** owned by Jolt → every one is **CLONED**; the caller must `destroy()`
      each object it created (`ray`, `settings`, `collector`, and the four filters).
  - Returns: `NONE`

## ObjectLayerFilter

- `new ObjectLayerFilter()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; transient raycast filter, must `destroy()`.

## ObjectLayerPairFilterTable

- `new ObjectLayerPairFilterTable(numObjectLayers: number)`
  - Arguments: COPIED (number)
  - Returns: `COPY` — **CONSUMED** once assigned to `JoltSettings.mObjectLayerPairFilter` (see
    `JoltSettings`). Do not `destroy()` after hand-off.
- `ObjectLayerPairFilterTable.EnableCollision(layer1: number, layer2: number)`
  - Arguments: COPIED (number). Returns: `NONE`.

## ObjectVsBroadPhaseLayerFilterTable

- `new ObjectVsBroadPhaseLayerFilterTable(broadPhaseLayerInterface: BroadPhaseLayerInterface, numBroadPhaseLayers: number, objectLayerPairFilter: ObjectLayerPairFilter, numObjectLayers: number)`
  - Arguments
    - `broadPhaseLayerInterface`: CLONED (`[Const, Ref]`; read only — caller keeps ownership; in
      fission this is the already-built table, which is itself CONSUMED by the `JoltInterface`)
    - `objectLayerPairFilter`: CLONED (`[Const, Ref]`; read only)
    - the two counts: COPIED (number)
  - Returns: `COPY` — **CONSUMED** once assigned to `JoltSettings.mObjectVsBroadPhaseLayerFilter`.

## PhysicsMaterial

- `new PhysicsMaterial()`
  - Arguments: None
  - Returns: `COPY` — refcounted (`RefTarget`); caller owns the handle **only until it's handed to
    something else** (e.g. `PhysicsMaterialList.push_back`). Once handed off, do not `destroy()`
    it — see `PhysicsMaterialList.push_back` below and the reference-counted exception above.

## PhysicsMaterialList

- `new PhysicsMaterialList()`
  - Arguments: None
  - Returns: `COPY` — caller owns it (then assigned to `MeshShapeSettings.mMaterials`, CLONED).
- `PhysicsMaterialList.push_back(material: PhysicsMaterial)`
  - Arguments
    - `material`: **CONSUMED** (not CLONED) — the list `AddRef()`s it, taking a reference. The
      caller must **not** `destroy()` it afterward: `JOLT.destroy()` is a raw `delete`, ignores
      the refcount, and frees memory the list still points to.
  - Returns: `NONE`

## PhysicsSettings

Obtained from `PhysicsSystem.GetPhysicsSettings()` — `[Const, Ref]`, an `INTERNAL_REF`. Mutate fields
in place; never `destroy()`.

- `.mDeterministicSimulation` (bool) / `.mSpeculativeContactDistance` (number) / `.mPenetrationSlop` (number)
  - COPIED. No Ownership Concerns.

## PhysicsSystem

Obtained from `JoltInterface.GetPhysicsSystem()` (an `INTERNAL_REF`). Never `destroy()`.

- `PhysicsSystem.GetBodyInterface()` → `INTERNAL_REF` (`[Ref] BodyInterface`).
- `PhysicsSystem.GetBodyLockInterface()` → `INTERNAL_REF` (`[Const, Ref]`).
- `PhysicsSystem.GetNarrowPhaseQuery()` → `INTERNAL_REF` (`[Const, Ref]`).
- `PhysicsSystem.GetPhysicsSettings()` → `INTERNAL_REF` (`[Const, Ref]`).
- `PhysicsSystem.GetContactListener()` → `INTERNAL_REF` — the listener owned (by reference) by the
  system. (fission `destroy()`s the `ContactListenerJS` it originally created at teardown; the getter
  itself does not transfer ownership.)
  - Arguments: None for all of the above.
- `PhysicsSystem.AddConstraint(constraint: Constraint)`
  - Arguments
    - `constraint`: CLONED — the system retains a reference (refcounted). The caller keeps its own
      handle and is responsible for `RemoveConstraint` + `destroy()` at teardown.
  - Returns: `NONE`
- `PhysicsSystem.RemoveConstraint(constraint: Constraint)`
  - Arguments
    - `constraint`: CLONED — releases the system's reference; does not free the caller's handle.
  - Returns: `NONE`
- `PhysicsSystem.AddStepListener(listener: PhysicsStepListener)` / `RemoveStepListener(listener: PhysicsStepListener)`
  - Arguments
    - `listener`: CLONED — the system references it while added; caller retains ownership and frees
      it after removal.
  - Returns: `NONE`
- `PhysicsSystem.SetContactListener(listener: ContactListener)`
  - Arguments
    - `listener`: CLONED — referenced by the system; caller owns it (see `ContactListenerJS`).
  - Returns: `NONE`
- `PhysicsSystem.SetGravity(gravity: Vec3)`
  - Arguments
    - `gravity`: CLONED (`[Const, Ref] Vec3`; value copied in, caller frees)
  - Returns: `NONE`

## Quat

- `new Quat(x: number, y: number, z: number, w: number)`
  - Arguments: all COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `Quat.sIdentity()` (static) — Returns `STATIC_ALIAS` (`static Quat temp`); do **not** `destroy()`.
- `Quat.sRotation(axis: Vec3, angle: number)` (static)
  - Arguments
    - `axis`: CLONED (`[Const, Ref] Vec3`; caller frees)
    - `angle`: COPIED (number)
  - Returns: `STATIC_ALIAS` (`static Quat temp`); do **not** `destroy()`.
- `Quat.GetEulerAngles()` → `STATIC_ALIAS` (`static Vec3 temp`); do **not** `destroy()`.
- `Quat.GetRotationAngle(axis: Vec3)`
  - Arguments: `axis`: CLONED. Returns: `NONE` (number).
- `Quat.GetX()` / `GetY()` / `GetZ()` / `GetW()` — Returns `NONE` (number). No Ownership Concerns.

## RMat44

Returned from `Body.GetWorldTransform()` etc. — those returns are themselves `STATIC_ALIAS`, not
caller-owned (see `Body` above).

- `RMat44.GetTranslation()` → `STATIC_ALIAS` (`static RVec3 temp`); do **not** `destroy()`.
- `RMat44.GetQuaternion()` → `STATIC_ALIAS` (`static Quat temp`); do **not** `destroy()`.

## RRayCast

- `new RRayCast(origin: RVec3, direction: Vec3)`
  - Arguments
    - `origin`: CLONED (`[Const, Ref] RVec3`; value copied into `mOrigin`, caller frees)
    - `direction`: CLONED (`[Const, Ref] Vec3`; value copied into `mDirection`, caller frees)
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `RRayCast.GetPointOnRay(fraction: number)`
  - Arguments: `fraction`: COPIED (number)
  - Returns: `STATIC_ALIAS` (`static RVec3 temp`); do **not** `destroy()`.

## RayCastResult

Accessed as `collector.mHit` (an `INTERNAL_REF` inside the collector). Do not `destroy()`.

- `.mBodyID` (field → `BodyID`) — reference into the result; do not `destroy()`.
- `.mFraction` (field → `number`) — No Ownership Concerns.

## RayCastSettings

- `new RayCastSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `RayCastSettings.mTreatConvexAsSolid = boolean` — COPIED. No Ownership Concerns.

## RVec3

- `new RVec3(x: number, y: number, z: number)`
  - Arguments: all COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.
- In-place math methods — `Add(other: Vec3)`, `Sub(other: Vec3)`, `Mul(scalar: number)`,
  `Div(scalar: number)`
  - Arguments: the `Vec3` operand is CLONED (`[Const, Ref]`, caller frees); a scalar is COPIED.
  - Returns: `ALIASES_THIS` — `glue.cpp`: `return &(*self += *inV);` etc. Mutates `self` in place and
    returns a reference to `self`, not a new object. Do **not** `destroy()` the return — it's the
    same object as the receiver.
- Value-producing math methods — `AddRVec3(other: RVec3)`, `SubRVec3(other: RVec3)`,
  `MulRVec3(other: RVec3)`, `DivRVec3(other: RVec3)`, `MulFloat(scalar: number)`,
  `DivFloat(scalar: number)`, `Normalized()`
  - Arguments: an `RVec3` operand is CLONED (`[Const, Ref]`, caller frees); a scalar is COPIED.
  - Returns: `STATIC_ALIAS` — each has its own `static RVec3 temp` in `glue.cpp`. Do **not**
    `destroy()`; invalidated by the next call to that same method (on any `RVec3`).
- `RVec3.Dot(other: RVec3)` — arg CLONED; Returns `NONE` (number).
- `RVec3.GetX()` / `GetY()` / `GetZ()` — Returns `NONE` (number). No Ownership Concerns.

## Shape

Obtained from `Body.GetShape()` / `ShapeResult.Get()` (`INTERNAL_REF`s). Refcounted — do not
`destroy()` an internal reference.

- `Shape.GetCenterOfMass()` → `STATIC_ALIAS` (`static Vec3 temp`); do **not** `destroy()`.
- `Shape.GetLocalBounds()` → `STATIC_ALIAS` (`static AABox temp`); do **not** `destroy()`.
- `Shape.GetMassProperties()` → `STATIC_ALIAS` (`static MassProperties temp`); do **not** `destroy()`.
- `Shape.GetSubType()` → `NONE` (enum). No Ownership Concerns.

## ShapeFilter

- `new ShapeFilter()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; transient raycast filter, must `destroy()`.

## ShapeGetTriangles

- `new ShapeGetTriangles(shape: Shape, box: AABox, centerOfMass: Vec3, rotation: Quat, scale: Vec3)`
  - Arguments
    - `shape`: CLONED (read only; caller keeps the shape reference)
    - `box`: CLONED (`[Const, Ref] AABox`); `centerOfMass`: CLONED (`[Const, Ref] Vec3`);
      `rotation`: CLONED (`[Const, Ref] Quat`); `scale`: CLONED (`[Const, Ref] Vec3`)
    - (fission passes throwaway `sBiggest()` / `GetCenterOfMass()` / `sIdentity()` results here —
      those are themselves `STATIC_ALIAS`, not `COPY`; they must **not** be `destroy()`ed, only read
      from before the next call to that same static factory/getter.)
  - Returns: `COPY` — caller owns the helper and must `destroy()` it (fission does).
- `ShapeGetTriangles.GetVerticesData()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — a pointer (`[Const] any`) into the helper's internal vertex buffer,
    valid only while the helper lives. Used to build a `Float32Array` view; do not `destroy()`.
- `ShapeGetTriangles.GetVerticesSize()` — Returns `NONE` (number).

## ShapeResult

Returned by `*Settings.Create()`. It is itself `STATIC_ALIAS`, **not** `COPY` — do not `destroy()` it
(see `ShapeSettings.Create()` below).

- `ShapeResult.HasError()` / `.IsValid` — boolean → `NONE`. No Ownership Concerns.
- `ShapeResult.Get()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — the `Shape` (refcounted) held by the result. The shape outlives the
    result only because it is refcounted and gets held by whatever consumes it (e.g. a body); do not
    `destroy()` the value returned by `Get()`.
- `ShapeResult.GetError()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Const, Ref] JPHString`; read via `.c_str()`, do not `destroy()`.

## ShapeSettings (base of all `*ShapeSettings`)

- `ShapeSettings.Create()`
  - Arguments: None
  - Returns: `STATIC_ALIAS`, **not** `COPY` — `glue.cpp`: `static Shape::ShapeResult temp; return
    (temp = self->Create(), &temp);`. Do **not** `destroy()` the returned `ShapeResult`. Because the
    IDL binder generates one function per *base* interface, this single `static` is shared by
    **every** `*ShapeSettings` subtype used here (`BoxShapeSettings`, `MeshShapeSettings`,
    `ConvexHullShapeSettings`, `StaticCompoundShapeSettings`, …) — calling `.Create()` on any one of
    them overwrites the same slot every other one's `.Create()` result pointed to. Read/consume the
    result (`HasError()`, `.Get()`) before calling `.Create()` again on any `*ShapeSettings` object.
    The `Shape` it wraps (via `ShapeResult.Get()`) is refcounted and unaffected by this. The settings
    object itself (`BoxShapeSettings`, etc.) is a separate, genuinely heap-allocated `COPY` and must
    still be `destroy()`ed.

## Constraint creation — `FixedConstraintSettings` / `HingeConstraintSettings` / `SliderConstraintSettings`.`Create`

- `<TwoBody>ConstraintSettings.Create(body1: Body, body2: Body)`
  - Arguments
    - `body1`: CLONED (`[Ref] Body`; the constraint stores a pointer to it but does not own it —
      bodies are `[NoDelete]`, owned by the body manager. Do not free bodies through this call.)
    - `body2`: CLONED (same as above)
  - Returns: `COPY` — a freshly allocated refcounted `Constraint` (or `TwoBodyConstraint`). The
    caller owns this handle; in fission it is stored, handed to `PhysicsSystem.AddConstraint`
    (which adds a reference), and `destroy()`ed at teardown after `RemoveConstraint`.

## SliderConstraint

Obtained via `JOLT.castObject(constraint, JOLT.SliderConstraint)`.

- `SliderConstraint.GetMotorSettings()` → `INTERNAL_REF` (`[Ref] MotorSettings`). Do not `destroy()`.
- `GetCurrentPosition()` / `GetLimitsMin()` / `GetLimitsMax()` → `NONE` (number).
- `SetMotorState(state: EMotorState)` — arg COPIED (enum); Returns `NONE`.
- `SetTargetPosition(number)` / `SetTargetVelocity(number)` — args COPIED; Returns `NONE`.

## SliderConstraintSettings

- `new SliderConstraintSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- Field writes — `mPoint1` / `mPoint2` (`RVec3`), `mSliderAxis1` / `mSliderAxis2`,
  `mNormalAxis1` / `mNormalAxis2` (`Vec3`)
  - RHS value copied in (`[Value] attribute`) → RHS is CLONED (caller frees).
- Field writes — `mLimitsMin` / `mLimitsMax`, `mMotorSettings.mMaxForceLimit` / `mMotorSettings.mMinForceLimit`
  - COPIED (number). No Ownership Concerns.
- `SliderConstraintSettings.Create(body1, body2)` — see "Constraint creation".

## SphereShapeSettings

- `new SphereShapeSettings(radius: number)`
  - Arguments: `radius`: COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()`.

## StaticCompoundShapeSettings

- `new StaticCompoundShapeSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; `destroy()` after `Create()`.
- `StaticCompoundShapeSettings.AddShape(position: Vec3, rotation: Quat, shape: ShapeSettings, userData: number)`
  - Arguments
    - `position`: CLONED (`[Const, Ref] Vec3`; value copied into the sub-shape, caller frees)
    - `rotation`: CLONED (`[Const, Ref] Quat`; value copied, caller frees)
    - `shape`: CLONED — `[Const] ShapeSettings`, refcounted; the compound retains a reference, the
      caller keeps and must free its own handle.
    - `userData`: COPIED (number)
  - Returns: `NONE`
- `StaticCompoundShapeSettings.Create()` — see `ShapeSettings.Create()`.

## SubShapeIDPair

Provided to contact callbacks as an `INTERNAL_REF` (wrapped pointer). Do not `destroy()`.

- `SubShapeIDPair.GetBody1ID()` / `GetBody2ID()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Const, Ref] BodyID` into the pair. Do not `destroy()`.

## TwoBodyConstraint

Obtained via `JOLT.castObject(...)`.

- `TwoBodyConstraint.GetConstraintToBody1Matrix()`
  - Arguments: None
  - Returns: `STATIC_ALIAS` (`static Mat44 temp`). Do **not** `destroy()`.

## Vec3

- `new Vec3(x?: number, y?: number, z?: number)` (also `new Vec3(float3: Float3)`)
  - Arguments: numbers are COPIED; a `Float3` argument is CLONED (`[Const, Ref]`, caller frees).
  - Returns: `COPY` — caller owns it, must `destroy()`.
- In-place math methods — `Add(other: Vec3)`, `Sub(other: Vec3)`, `Mul(scalar: number)`,
  `Div(scalar: number)`
  - Arguments: a `Vec3` operand is CLONED; a scalar is COPIED.
  - Returns: `ALIASES_THIS` — `glue.cpp`: `return &(*self += *inV);` etc. Mutates `self` in place and
    returns a reference to `self`. Do **not** `destroy()` the return — same object as the receiver.
- Value-producing math methods — `AddVec3(other: Vec3)`, `SubVec3(other: Vec3)`,
  `MulVec3(other: Vec3)`, `DivVec3(other: Vec3)`, `MulFloat(scalar: number)`,
  `DivFloat(scalar: number)`, `Normalized()`, `NormalizedOr(zero: Vec3)`,
  `GetNormalizedPerpendicular()`
  - Arguments: a `Vec3` operand is CLONED; a scalar is COPIED.
  - Returns: `STATIC_ALIAS` — each has its own `static Vec3 temp` in `glue.cpp`. Do **not**
    `destroy()`; invalidated by the next call to that same method (on any `Vec3`).
- `Vec3.Dot(other: Vec3)` — arg CLONED; Returns `NONE` (number).
- `Vec3.Length()` — Returns `NONE` (number).
- `Vec3.GetX()` / `GetY()` / `GetZ()` — Returns `NONE` (number). No Ownership Concerns.
- `Vec3.SetX(number)` / `SetY(number)` / `SetZ(number)` — args COPIED; Returns `NONE`.

## Vec4

- `new Vec4(x: number, y: number, z: number, w: number)`
  - Arguments: all COPIED (number)
  - Returns: `COPY` — caller owns it, must `destroy()` (fission does).

## VehicleCollisionTesterCastCylinder

- `new VehicleCollisionTesterCastCylinder(objectLayer: number, convexRadiusFraction?: number)`
  - Arguments: COPIED (number)
  - Returns: `COPY` — refcounted; the caller owns the handle. Passed to
    `VehicleConstraint.SetVehicleCollisionTester` (which retains a reference).

## VehicleConstraint

- `new VehicleConstraint(body: Body, settings: VehicleConstraintSettings)`
  - Arguments
    - `body`: CLONED (`[Ref] Body`; referenced, not owned — body is `[NoDelete]`)
    - `settings`: CLONED (`[Const, Ref]`; read only, caller frees the settings)
  - Returns: `COPY` — a refcounted constraint; caller owns the handle (added via `AddConstraint`).
- `VehicleConstraint.GetController()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — the controller owned by the constraint (cast with `JOLT.castObject`).
    Do not `destroy()`.
- `VehicleConstraint.GetWheel(index: number)`
  - Arguments: `index`: COPIED (number)
  - Returns: `INTERNAL_REF` — `[Const] Wheel` owned by the constraint. Do not `destroy()`.
- `VehicleConstraint.SetVehicleCollisionTester(tester: VehicleCollisionTester)`
  - Arguments
    - `tester`: CLONED — refcounted; the constraint retains a reference, caller keeps its handle.
  - Returns: `NONE`

## VehicleConstraintSettings

- `new VehicleConstraintSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `VehicleConstraintSettings.mWheels` (`ArrayWheelSettings`, via `clear` / `push_back`)
  - `push_back(wheelSettings)`: CLONED — refcounted `Ref<WheelSettings>` retained; caller keeps its
    handle. `clear()`: No Ownership Concerns.
- `VehicleConstraintSettings.mController = VehicleControllerSettings`
  - Assigned object: CLONED — refcounted; the settings retain a reference, caller keeps its handle.
- `VehicleConstraintSettings.mAntiRollBars.clear()` — No Ownership Concerns.

## VehicleConstraintStepListener

- `new VehicleConstraintStepListener(constraint: VehicleConstraint)`
  - Arguments
    - `constraint`: CLONED — the listener stores a pointer to the constraint; caller keeps ownership.
  - Returns: `COPY` — caller owns the listener; it is added via `PhysicsSystem.AddStepListener`
    (CLONED there) and must be removed + `destroy()`ed at teardown.

## VertexList

- `new VertexList()`
  - Arguments: None
  - Returns: `COPY` — caller owns it (then assigned to `MeshShapeSettings.mTriangleVertices`, CLONED).
- `VertexList.push_back(vertex: Float3)`
  - Arguments
    - `vertex`: CLONED — value copied into the list; caller owns and frees the `Float3` (fission
      `destroy()`s it right after).
  - Returns: `NONE`
- `VertexList.size()` — Returns `NONE` (number).

## Wheel

Obtained from `VehicleConstraint.GetWheel(...)` (`INTERNAL_REF`).

- `Wheel.GetAngularVelocity()` / `GetRotationAngle()` — Returns `NONE` (number). No Ownership Concerns.

## WheelWV

Obtained via `JOLT.castObject(wheel, JOLT.WheelWV)`.

- `.set_mCombinedLateralFriction(number)` / `.set_mCombinedLongitudinalFriction(number)` /
  `SetAngularVelocity(number)` — args COPIED; Returns `NONE`. No Ownership Concerns.

## WheeledVehicleController

Obtained via `JOLT.castObject(constraint.GetController(), JOLT.WheeledVehicleController)`
(`INTERNAL_REF`). Do not `destroy()`.

- `WheeledVehicleController.GetEngine()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — `[Ref] VehicleEngine` inside the controller; `.mMaxTorque` is a
    `number` field. Do not `destroy()`.

## WheeledVehicleControllerSettings

- `new WheeledVehicleControllerSettings()`
  - Arguments: None
  - Returns: `COPY` — caller owns it; assigned to `VehicleConstraintSettings.mController` (CLONED).
- Field writes — `.mEngine.mMaxTorque`, `.mTransmission.mClutchStrength`, `.mTransmission.mMode`
  (enum), `.mTransmission.mGearRatios` (`clear` / `push_back` of numbers)
  - COPIED (number / enum). No Ownership Concerns.

---

## Global / namespace utilities

- `JOLT.destroy(object)` — frees the WASM-heap object. Call exactly once on every `COPY`/owned
  object and never on an `INTERNAL_REF` or a `CONSUMED` argument.
- `JOLT.castObject(pointer, targetClass)` — reinterprets a pointer as another type. **No ownership
  change**: the result aliases the same object. Do not `destroy()` the cast result separately.
- `JOLT.wrapPointer(pointer, targetClass)` — wraps a raw pointer (from a callback) in a typed
  handle. **No ownership change**: the underlying object is owned by Jolt for the callback's
  duration. Do not `destroy()`.
- `JOLT.HEAP32` (and friends) — a view onto the WASM heap; `JOLT.HEAP32.buffer` is used to build
  `Float32Array` views over Jolt-internal pointers. No Ownership Concerns (do not free the buffer).

## Enums / constants

`JOLT.EActivation_*`, `JOLT.EMotionType_*`, `JOLT.EMotorState_*`, `JOLT.EOverrideMassProperties_*`,
`JOLT.EShapeSubType_*`, `JOLT.EConstraintSubType_*`, `JOLT.ETransmissionMode_*`,
`JOLT.ValidateResult_*` — all are plain enum values. **No Ownership Concerns** (always `COPIED` as
arguments, never freed).
