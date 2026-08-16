// GENERATED FILE -- do not hand-edit. Regenerate with:
//   bun run jolt-ownership:generate
// (see fission/scripts/jolt-ownership/README.md for what each step does and why)
//
// Every entry below is proven, not attributed: each one's reason cites the exact generated
// glue.cpp line (ground truth for what the binder's C++ actually does, from webidl_binder.py
// over jolt/JoltJS.idl) and the fission/src production call site(s) that exercise it. A class only
// appears here if EVERY production call site that produces an instance of it is a return the
// binder can never legally destroy() (STATIC_ALIAS / INTERNAL_REF on a non-RefTarget class /
// ALIASES_THIS), a pure castObject/wrapPointer address-alias, or a class with no generated
// __destroy__ binding at all. A single disqualifying call site (a real allocation, a RefTarget's
// INTERNAL_REF -- which can hide a genuine refcounted allocation behind an identical-looking
// glue.cpp shape, see Constraint/Shape's manual entries in JoltClassClassification.ts -- or
// anything the tool couldn't classify) drops the class from this file entirely; it stays a manual,
// human-attested entry in JoltClassClassification.ts instead. See
// fission/scripts/jolt-ownership/generate-classification.mjs for the exact join logic.
import type { ClassClassification } from "@/test/JoltClassClassification"

export const GENERATED_CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
    AABox: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`AABox.sBiggest()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:5682), confirmed as this class's only non-alias contributor (3 call sites, e.g. src/util/threejs/MeshCreation.ts:14); \`Body.GetWorldSpaceBounds()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:13574), confirmed as this class's only non-alias contributor (src/mirabuf/ScoringZoneSceneObject.ts:69, src/mirabuf/MirabufSceneObject.ts:1041); \`Shape.GetLocalBounds()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:870), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:757, src/systems/physics/PhysicsSystem.ts:820).`,
    },
    Body: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`HingeConstraint.GetBody1()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:10872), confirmed as this class's only non-alias contributor (src/systems/simulation/driver/HingeDriver.ts:44, src/systems/simulation/driver/HingeDriver.ts:54); \`BodyInterface.CreateBody()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:13625), confirmed as this class's only non-alias contributor (3 call sites, e.g. src/systems/physics/PhysicsSystem.ts:409); \`BodyLockInterfaceLocking.TryGetBody()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:14111), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1549); only ever populated via \`JOLT.wrapPointer(_, JOLT.Body)\` (same never-allocates \`k()\` cache-getter as castObject); checked that no \`JOLT.destroy()\` call site in the same file targets its bound local (\`body1\`) (6 call sites, e.g. src/systems/physics/PhysicsSystem.ts:1999); has no generated \`__destroy__\` binding at all (glue.cpp confirms no \`emscripten_bind_<Class>___destroy___0\`) -- \`JOLT.destroy()\` throws \`"Cannot destroy object. (Did you create it yourself?)"\` unconditionally, for every instance, independent of any call site.`,
    },
    BodyInterface: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`PhysicsSystem.GetBodyInterface()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:16383), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:199, src/systems/physics/PhysicsSystem.ts:1553).`,
    },
    BodyLockInterfaceLocking: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`PhysicsSystem.GetBodyLockInterface()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:16395), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1549).`,
    },
    CollideShapeResult: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.wrapPointer(_, JOLT.CollideShapeResult)\` (same never-allocates \`k()\` cache-getter as castObject); embedded directly in an object literal with no local binding to check -- safety here rests on the transient-callback-argument contract (ContactListenerJS's *Ptr args), not an absence-of-destroy scan (src/systems/physics/PhysicsSystem.ts:2052).`,
    },
    ContactManifold: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.wrapPointer(_, JOLT.ContactManifold)\` (same never-allocates \`k()\` cache-getter as castObject); embedded directly in an object literal with no local binding to check -- safety here rests on the transient-callback-argument contract (ContactListenerJS's *Ptr args), not an absence-of-destroy scan (src/systems/physics/PhysicsSystem.ts:2008, src/systems/physics/PhysicsSystem.ts:2034).`,
    },
    ContactSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.wrapPointer(_, JOLT.ContactSettings)\` (same never-allocates \`k()\` cache-getter as castObject); embedded directly in an object literal with no local binding to check -- safety here rests on the transient-callback-argument contract (ContactListenerJS's *Ptr args), not an absence-of-destroy scan (src/systems/physics/PhysicsSystem.ts:2009, src/systems/physics/PhysicsSystem.ts:2035).`,
    },
    HingeConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.castObject(_, JOLT.HingeConstraint)\`, which the binder implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` constructors/getters use, called with the existing pointer, never a new allocation (src/systems/simulation/SimulationSystem.ts:86).`,
    },
    JPHString: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`ShapeResult.GetError()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:6565), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1119, src/systems/physics/PhysicsSystem.ts:1768).`,
    },
    MotionProperties: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`Body.GetMotionProperties()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:13594), confirmed as this class's only non-alias contributor (7 call sites, e.g. src/systems/simulation/stimulus/ChassisStimulus.ts:28).`,
    },
    MotorSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`HingeConstraint.GetMotorSettings()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:10722), confirmed as this class's only non-alias contributor (4 call sites, e.g. src/systems/simulation/driver/HingeDriver.ts:87); \`SliderConstraint.GetMotorSettings()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:11281), confirmed as this class's only non-alias contributor (4 call sites, e.g. src/systems/simulation/driver/SliderDriver.ts:37).`,
    },
    NarrowPhaseQuery: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`PhysicsSystem.GetNarrowPhaseQuery()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:16403), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1416).`,
    },
    PhysicsSettings: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`PhysicsSystem.GetPhysicsSettings()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:16322), confirmed as this class's only non-alias contributor (4 call sites, e.g. src/systems/physics/PhysicsSystem.ts:206).`,
    },
    PhysicsSystem: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`JoltInterface.GetPhysicsSystem()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:22245), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:198).`,
    },
    RMat44: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`VehicleConstraint.GetWheelWorldTransform()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:19750), confirmed as this class's only non-alias contributor (3 call sites, e.g. src/systems/simulation/behavior/synthesis/drive/MecanumLayout.ts:73); \`Body.GetCenterOfMassTransform()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:13564), confirmed as this class's only non-alias contributor (6 call sites, e.g. src/systems/simulation/driver/HingeDriver.ts:44); \`Body.GetWorldTransform()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:13554), confirmed as this class's only non-alias contributor (26 call sites, e.g. src/mirabuf/EjectableSceneObject.ts:107); \`RMat44.Decompose()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:5662), confirmed as this class's only non-alias contributor (src/ui/panels/configuring/assembly-config/interfaces/MoveInterface.tsx:16); \`RMat44.MulMat44()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:5526), confirmed as this class's only non-alias contributor (src/mirabuf/MirabufSceneObject.ts:840).`,
    },
    ShapeResult: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`ConvexHullShapeSettings.Create()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:7922), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:449); \`StaticCompoundShapeSettings.Create()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:8174), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1114); \`RotatedTranslatedShapeSettings.Create()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:8855), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1144); \`ShapeSettings.Create()\` is STATIC_ALIAS in generated glue.cpp (glue.cpp:822), confirmed as this class's only non-alias contributor (src/systems/physics/PhysicsSystem.ts:1766).`,
    },
    SliderConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.castObject(_, JOLT.SliderConstraint)\`, which the binder implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` constructors/getters use, called with the existing pointer, never a new allocation (src/systems/simulation/SimulationSystem.ts:98).`,
    },
    SubShapeIDPair: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.wrapPointer(_, JOLT.SubShapeIDPair)\` (same never-allocates \`k()\` cache-getter as castObject); checked that no \`JOLT.destroy()\` call site in the same file targets its bound local (\`shapePair\`) (src/systems/physics/PhysicsSystem.ts:2042).`,
    },
    TwoBodyConstraint: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.castObject(_, JOLT.TwoBodyConstraint)\`, which the binder implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` constructors/getters use, called with the existing pointer, never a new allocation (src/systems/physics/PhysicsSystem.ts:680).`,
    },
    VehicleController: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`VehicleConstraint.GetController()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:19737), confirmed as this class's only non-alias contributor (src/systems/simulation/driver/WheelDriver.ts:115).`,
    },
    VehicleEngine: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`WheeledVehicleController.GetEngine()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:3721), confirmed as this class's only non-alias contributor (src/systems/simulation/driver/WheelDriver.ts:116).`,
    },
    Wheel: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `\`VehicleConstraint.GetWheel()\` is INTERNAL_REF in generated glue.cpp (glue.cpp:19741), confirmed as this class's only non-alias contributor (src/systems/simulation/driver/WheelDriver.ts:121, src/systems/simulation/SimulationSystem.ts:95).`,
    },
    WheelWV: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.castObject(_, JOLT.WheelWV)\`, which the binder implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` constructors/getters use, called with the existing pointer, never a new allocation (src/systems/simulation/driver/WheelDriver.ts:121).`,
    },
    WheeledVehicleController: {
        bucket: "INTERNAL_REF_UNPROVABLE",
        reason: `only ever populated via \`JOLT.castObject(_, JOLT.WheeledVehicleController)\`, which the binder implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` constructors/getters use, called with the existing pointer, never a new allocation (src/systems/simulation/driver/WheelDriver.ts:115).`,
    },
}
