# Jolt Physics — Refcounted Destroy Semantics

Correction/addendum to `JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md`. That doc's `CLONED` category
says refcounted objects can be handed off *and* still `destroy()`ed by the caller. **That's wrong
in the common case and caused a real crash (PR #1412, SYNTH-244, "spawning any field crashes
fission").**

## The rule (from the upstream Jolt.js README)

Classes inheriting `RefTarget` (full list per the Jolt.js README): `ShapeSettings`, `Shape`,
`ConstraintSettings`, `Constraint`, `PathConstraintPath`, `PhysicsMaterial`, `GroupFilter`,
`SoftBodySharedSettings`, `VehicleCollisionTester`, `VehicleControllerSettings`, `WheelSettings`,
`CharacterBaseSettings`, `CharacterBase`, `Skeleton`, `SkeletonAnimation`, `SkeletonMapper`,
`PhysicsScene`, `RagdollSettings`, `Ragdoll` — start at **refcount 0**.

Two ways a reference gets added:

- **Manual**: call `object.AddRef()` yourself to keep ownership (increments the count); call
  `object.Release()` to give it up (decrements the count, and deletes the object once it hits 0).
- **Automatic**: handing the object to a parent (`push_back` into a list, `AddShape`, `SetShape`,
  `AddConstraint`, …) makes the parent `AddRef()` it for you — in this case you don't need to
  `AddRef()` yourself first, and you can skip calling `Release()` too.

`JOLT.destroy(x)` is **neither** of those. It is a raw `delete x` regardless of refcount. Per the
README: "it is also possible to do `new Jolt.XXX` followed by `Jolt.destroy(...)` for a
reference counted object **if no one took a reference**." The inverse holds too: if something
else *did* take a reference and still holds it, `destroy()` frees the object out from under that
reference — a use-after-free the moment anyone touches it again. If you need to drop your own
reference on a shared object, call `.Release()`, not `JOLT.destroy()`.

## Practical rule

For any `RefTarget` type:

- **Handed to a parent and never explicitly removed** (`AddShape`, `SetShape`,
  `mWheels.push_back`, `mController = ...`, `BodyCreationSettings.shape`, …): **do not
  `destroy()` it.** The parent owns a reference now; let the parent's own teardown release it.
- **Handed to a parent, later explicitly removed** (`AddConstraint` → `RemoveConstraint`,
  `AddStepListener` → `RemoveStepListener`): safe to `destroy()` **only after** the removal call
  has actually dropped the parent's reference. Destroying before removal is the same bug.
- **Never handed to anything** (created, used locally, discarded): safe to `destroy()`
  immediately — this is the README's "if no one took a reference" case.

For plain (non-`RefTarget`) value types (`Vec3`, `RVec3`, `Quat`, `Mat44`, `Float3`,
`IndexedTriangle`, `AABox`, …): the existing `CLONED` guidance in the main doc is correct as-is —
`push_back`/assignment deep-copies, always `destroy()` your own handle.

## What to fix if auditing further

Every `CLONED`-refcounted entry in `JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md` needs re-checking
against actual fission call sites for the "destroy immediately after handoff, no removal step"
shape above. Confirmed bug: `PhysicsSystem.ts`, `PhysicsMaterialList.push_back(material)` +
`JOLT.destroy(material)` (fixed by PR #1412). Checked and confirmed *not* buggy in the same file:
`mWheels.push_back(wheelSettings)`, `mController = ...`, `AddConstraint`/`RemoveConstraint` — none
destroy before removal, or never destroy at all. Not yet re-checked: `BodyCreationSettings.shape`,
`BodyInterface.SetShape`, `VehicleConstraint.SetVehicleCollisionTester`.
