# Jolt Physics — Function Ownership Invariants

This document lists every Jolt Physics function used in `fission/src/` (production code, excluding
`fission/src/test/`) together with its memory-ownership invariants. Its purpose is to make correct
Jolt memory management possible without reading the Jolt source for every call.

Fission uses the WebAssembly port [`@azaleacolburn/jolt-physics`](https://www.npmjs.com/package/jolt-physics)
(a fork of JoltPhysics.js). Ownership semantics are therefore governed by the Emscripten WebIDL
binding, defined in `jolt/JoltJS.idl` and `jolt/JoltJS.h`, layered on top of Jolt's C++ memory model.
All invariants below were derived from those two files.

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
  must `destroy()` it**. (For reference-counted types — `Shape`, `ShapeSettings`, `Constraint`,
  `*ControllerSettings`, `WheelSettings` — Jolt additionally retains its own reference, so the
  object survives as long as either side needs it; the caller is still responsible for releasing the
  handle it holds.)
- **`COPIED`** — the argument is a primitive or enum (`number`, `boolean`, `Jolt.EActivation`,
  `Jolt.EMotionType`, …). Nothing to free.

### Return categories

- **`INTERNAL_REF`** — the return value is a reference/handle into state owned by another Jolt
  object (the parent body, the physics system, a result struct, …). It is valid only while that
  owner lives, and the caller **must not** `destroy()` it. In `JoltJS.idl` these are bare interface
  pointers or `[Ref]` / `[Const, Ref]` returns.
- **`COPY`** — the return value is a freshly allocated heap object the caller **owns and must
  `destroy()`** when done. In `JoltJS.idl` these are `[Value]` returns. Constructors (`new JOLT.X`)
  and `*Settings.Create()` also produce caller-owned objects and are treated as `COPY` here.
- **`NONE`** — the function returns `void` or a primitive (`number` / `boolean` / enum). Nothing to
  free.

### Rules of thumb (from the binding)

1. `[Value] T SomeGetter()` → returns a **COPY**; you must `destroy()` it. This includes every
   vector/quaternion/matrix getter (`GetPosition`, `GetLinearVelocity`, `GetCenterOfMass`,
   `GetWorldTransform`, …) and every math operator (`Add`, `Sub`, `Mul`, `Div`, `Normalized`, …).
2. A bare interface-pointer return (`Body`, `Shape`, `BodyInterface`, `MotorSettings`, …) is an
   **INTERNAL_REF**; never `destroy()` it.
3. A Jolt heap object passed as an argument that Jolt merely reads (`[Const, Ref]` / `[Ref]`) is
   **CLONED** — you keep ownership. A primitive/enum argument is **COPIED**.
4. Contrary to what one might think, arithmetic methods (e.g. `Div`, `Add`, etc.) on `Jolt.Vec3` and `Jolt.RVec3` do not consume the vector nor do they produce a new one. They modify the `this` vector in place and return a reference to it.
5. Annoyingly, the corresponding float arithmetic functions (e.g. `DivFloat`, `AddFloat`, etc.) on the same classes do not consume the vector, but do produce a newly allocated vector.

---

## AABox

- `AABox.sBiggest()` (static)
  - Arguments: None
  - Returns: `COPY` — `[Value] AABox`; caller must `destroy()`.
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
  - Returns: `COPY` — `[Value] RVec3` / `Quat`. Caller must `destroy()`.
- `Body.GetWorldTransform()` / `GetCenterOfMassTransform()`
  - Arguments: None
  - Returns: `COPY` — `[Value] RMat44`. Caller must `destroy()`.
- `Body.GetWorldSpaceBounds()`
  - Arguments: None
  - Returns: `COPY` — `[Value] AABox`. Caller must `destroy()`.
- `Body.GetLinearVelocity()` / `GetAngularVelocity()` / `GetAccumulatedForce()`
  - Arguments: None
  - Returns: `COPY` — `[Value] Vec3`. Caller must `destroy()`.
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
  - Returns: `COPY` — `[Value] Vec3`. Caller must `destroy()`.

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
- `Mat44.GetTranslation()` → `COPY` (`[Value] Vec3`); `GetQuaternion()` → `COPY` (`[Value] Quat`);
  `Multiply3x3(v: Vec3)` → `COPY` (`[Value] Vec3`, arg `v` CLONED). Caller must `destroy()` returns.

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
- `MotionProperties.GetInverseInertiaDiagonal()` — Returns `COPY` (`[Value] Vec3`); caller `destroy()`s.

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
  - Returns: `COPY` — refcounted; caller owns the handle (consumed by `PhysicsMaterialList.push_back`).

## PhysicsMaterialList

- `new PhysicsMaterialList()`
  - Arguments: None
  - Returns: `COPY` — caller owns it (then assigned to `MeshShapeSettings.mMaterials`, CLONED).
- `PhysicsMaterialList.push_back(material: PhysicsMaterial)`
  - Arguments
    - `material`: CLONED — refcounted; the list retains a reference. Caller keeps its own handle.
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
- `Quat.sIdentity()` (static) — Returns `COPY` (`[Value] Quat`); caller `destroy()`s.
- `Quat.sRotation(axis: Vec3, angle: number)` (static)
  - Arguments
    - `axis`: CLONED (`[Const, Ref] Vec3`; caller frees)
    - `angle`: COPIED (number)
  - Returns: `COPY` (`[Value] Quat`); caller `destroy()`s.
- `Quat.GetEulerAngles()` → `COPY` (`[Value] Vec3`); caller `destroy()`s.
- `Quat.GetRotationAngle(axis: Vec3)`
  - Arguments: `axis`: CLONED. Returns: `NONE` (number).
- `Quat.GetX()` / `GetY()` / `GetZ()` / `GetW()` — Returns `NONE` (number). No Ownership Concerns.

## RMat44

Returned (by `[Value]`) from `Body.GetWorldTransform()` etc. — those returns are `COPY`s the caller
owns.

- `RMat44.GetTranslation()` → `COPY` (`[Value] RVec3`); caller `destroy()`s.
- `RMat44.GetQuaternion()` → `COPY` (`[Value] Quat`); caller `destroy()`s.

## RRayCast

- `new RRayCast(origin: RVec3, direction: Vec3)`
  - Arguments
    - `origin`: CLONED (`[Const, Ref] RVec3`; value copied into `mOrigin`, caller frees)
    - `direction`: CLONED (`[Const, Ref] Vec3`; value copied into `mDirection`, caller frees)
  - Returns: `COPY` — caller owns it, must `destroy()`.
- `RRayCast.GetPointOnRay(fraction: number)`
  - Arguments: `fraction`: COPIED (number)
  - Returns: `COPY` (`[Const, Value] RVec3`); caller `destroy()`s.

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
- Math methods — `AddRVec3(other: RVec3)`, `SubRVec3(other: RVec3)`, `Sub(other: Vec3|RVec3)`,
  `Mul(scalar: number)`, `Div(scalar: number)`, `Normalized()`
  - Arguments: an `RVec3`/`Vec3` operand is CLONED (`[Const, Ref]`, caller frees); a scalar is COPIED.
  - Returns: `COPY` (`[Value] RVec3`); caller must `destroy()` the result.
- `RVec3.Dot(other: RVec3)` — arg CLONED; Returns `NONE` (number).
- `RVec3.GetX()` / `GetY()` / `GetZ()` — Returns `NONE` (number). No Ownership Concerns.

## Shape

Obtained from `Body.GetShape()` / `ShapeResult.Get()` (`INTERNAL_REF`s). Refcounted — do not
`destroy()` an internal reference.

- `Shape.GetCenterOfMass()` → `COPY` (`[Value] Vec3`); caller `destroy()`s.
- `Shape.GetLocalBounds()` → `COPY` (`[Value] AABox`); caller `destroy()`s.
- `Shape.GetMassProperties()` → `COPY` (`[Value] MassProperties`); caller `destroy()`s.
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
      those are themselves `COPY`s that should be `destroy()`ed.)
  - Returns: `COPY` — caller owns the helper and must `destroy()` it (fission does).
- `ShapeGetTriangles.GetVerticesData()`
  - Arguments: None
  - Returns: `INTERNAL_REF` — a pointer (`[Const] any`) into the helper's internal vertex buffer,
    valid only while the helper lives. Used to build a `Float32Array` view; do not `destroy()`.
- `ShapeGetTriangles.GetVerticesSize()` — Returns `NONE` (number).

## ShapeResult

Returned by `*Settings.Create()`. It is itself a `COPY` (caller owns it, must `destroy()`).

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
  - Returns: `COPY` — `[Value] ShapeResult`. The caller owns the returned `ShapeResult` and must
    `destroy()` it; the `Shape` it wraps is refcounted (see `ShapeResult.Get()`). The settings object
    itself is unaffected and must be `destroy()`ed separately.

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
  - Returns: `COPY` — `[Value] Mat44`. Caller must `destroy()`.

## Vec3

- `new Vec3(x?: number, y?: number, z?: number)` (also `new Vec3(float3: Float3)`)
  - Arguments: numbers are COPIED; a `Float3` argument is CLONED (`[Const, Ref]`, caller frees).
  - Returns: `COPY` — caller owns it, must `destroy()`.
- Math methods — `Add(other: Vec3)`, `Sub(other: Vec3)`, `Mul(scalar: number)`, `Div(scalar: number)`,
  `Normalized()`
  - Arguments: a `Vec3` operand is CLONED; a scalar is COPIED.
  - Returns: `COPY` (`[Value] Vec3`); caller must `destroy()` the result.
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
