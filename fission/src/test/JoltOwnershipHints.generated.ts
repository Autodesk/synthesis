// GENERATED FILE -- do not hand-edit. Regenerate with: bun run jolt-ownership:generate
//
// Lightweight diagnostic hints for every class the jolt-ownership pipeline has seen (safe or
// disqualified), consulted by JoltLeakDetection.ts when a class shows a nonzero delta with no
// entry in JoltClassClassification.ts. Purpose: turn "go read glue.cpp and grep call sites
// yourself" into "read 3 lines and confirm" for the first triage pass on a genuinely new class.
// A class with no entry here was never touched by scan-callsites.mjs at all -- either added to
// fission/src after the last regen, or reached via a call shape the scanner doesn't cover yet
// ([Value] attribute field reads -- see fission/scripts/jolt-ownership/README.md).
export type OwnershipHint = {
    status: "SAFE" | "DISQUALIFIED"
    isRefTarget: boolean
    hasDestructor: boolean
    evidence: string[]
}

export const JOLT_OWNERSHIP_HINTS: Record<string, OwnershipHint> = {
    VehicleController: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "VehicleConstraint.GetController() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:115)",
        ],
    },
    VehicleEngine: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "WheeledVehicleController.GetEngine() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:116)",
        ],
    },
    Wheel: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "VehicleConstraint.GetWheel() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:121)",
            "VehicleConstraint.GetWheel() is INTERNAL_REF (src/systems/simulation/SimulationSystem.ts:95)",
        ],
    },
    RMat44: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "VehicleConstraint.GetWheelWorldTransform() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/MecanumLayout.ts:73)",
            "Body.GetCenterOfMassTransform() is STATIC_ALIAS (src/systems/simulation/driver/HingeDriver.ts:44)",
            "Body.GetCenterOfMassTransform() is STATIC_ALIAS (src/systems/simulation/driver/HingeDriver.ts:54)",
            "VehicleConstraint.GetWheelWorldTransform() is STATIC_ALIAS (src/systems/simulation/synthesis_brain/SynthesisBrain.ts:272)",
            "VehicleConstraint.GetWheelWorldTransform() is STATIC_ALIAS (src/systems/simulation/synthesis_brain/SynthesisBrain.ts:421)",
        ],
    },
    Body: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: false,
        evidence: [
            "HingeConstraint.GetBody1() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:44)",
            "HingeConstraint.GetBody1() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:54)",
            "BodyInterface.CreateBody() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:409)",
            "BodyInterface.CreateBody() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1176)",
            "BodyLockInterfaceLocking.TryGetBody() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1549)",
        ],
    },
    MotorSettings: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "HingeConstraint.GetMotorSettings() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:87)",
            "HingeConstraint.GetMotorSettings() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:91)",
            "HingeConstraint.GetMotorSettings() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:124)",
            "HingeConstraint.GetMotorSettings() is INTERNAL_REF (src/systems/simulation/driver/HingeDriver.ts:141)",
            "SliderConstraint.GetMotorSettings() is INTERNAL_REF (src/systems/simulation/driver/SliderDriver.ts:37)",
        ],
    },
    MotionProperties: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "Body.GetMotionProperties() is INTERNAL_REF (src/systems/simulation/stimulus/ChassisStimulus.ts:28)",
            "Body.GetMotionProperties() is INTERNAL_REF (src/systems/simulation/synthesis_brain/SynthesisBrain.ts:218)",
            "Body.GetMotionProperties() is INTERNAL_REF (src/mirabuf/MirabufSceneObject.ts:588)",
            "Body.GetMotionProperties() is INTERNAL_REF (src/systems/scene/DragModeSystem.ts:314)",
            "Body.GetMotionProperties() is INTERNAL_REF (src/systems/scene/DragModeSystem.ts:527)",
        ],
    },
    AABox: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "AABox.sBiggest() is STATIC_ALIAS (src/util/threejs/MeshCreation.ts:14)",
            "Body.GetWorldSpaceBounds() is STATIC_ALIAS (src/mirabuf/ScoringZoneSceneObject.ts:69)",
            "AABox.sBiggest() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:829)",
            "AABox.sBiggest() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:986)",
            "Body.GetWorldSpaceBounds() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:1041)",
        ],
    },
    PhysicsSystem: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["JoltInterface.GetPhysicsSystem() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:198)"],
    },
    BodyInterface: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "PhysicsSystem.GetBodyInterface() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:199)",
            "PhysicsSystem.GetBodyInterface() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1553)",
        ],
    },
    PhysicsSettings: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "PhysicsSystem.GetPhysicsSettings() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:206)",
            "PhysicsSystem.GetPhysicsSettings() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:207)",
            "PhysicsSystem.GetPhysicsSettings() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:208)",
            "PhysicsSystem.GetPhysicsSettings() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:209)",
        ],
    },
    ShapeResult: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "ConvexHullShapeSettings.Create() is STATIC_ALIAS (src/systems/physics/PhysicsSystem.ts:449)",
            "StaticCompoundShapeSettings.Create() is STATIC_ALIAS (src/systems/physics/PhysicsSystem.ts:1114)",
            "RotatedTranslatedShapeSettings.Create() is STATIC_ALIAS (src/systems/physics/PhysicsSystem.ts:1144)",
            "ShapeSettings.Create() is STATIC_ALIAS (src/systems/physics/PhysicsSystem.ts:1766)",
        ],
    },
    JPHString: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "ShapeResult.GetError() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1119)",
            "ShapeResult.GetError() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1768)",
        ],
    },
    NarrowPhaseQuery: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["PhysicsSystem.GetNarrowPhaseQuery() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1416)"],
    },
    BodyLockInterfaceLocking: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["PhysicsSystem.GetBodyLockInterface() is INTERNAL_REF (src/systems/physics/PhysicsSystem.ts:1549)"],
    },
    WheeledVehicleController: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "JOLT.castObject(_, JOLT.WheeledVehicleController) at src/systems/simulation/driver/WheelDriver.ts:115",
        ],
    },
    WheelWV: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["JOLT.castObject(_, JOLT.WheelWV) at src/systems/simulation/driver/WheelDriver.ts:121"],
    },
    HingeConstraint: {
        status: "SAFE",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["JOLT.castObject(_, JOLT.HingeConstraint) at src/systems/simulation/SimulationSystem.ts:86"],
    },
    SliderConstraint: {
        status: "SAFE",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["JOLT.castObject(_, JOLT.SliderConstraint) at src/systems/simulation/SimulationSystem.ts:98"],
    },
    TwoBodyConstraint: {
        status: "SAFE",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["JOLT.castObject(_, JOLT.TwoBodyConstraint) at src/systems/physics/PhysicsSystem.ts:680"],
    },
    ContactManifold: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "JOLT.wrapPointer(_, JOLT.ContactManifold) at src/systems/physics/PhysicsSystem.ts:2008",
            "JOLT.wrapPointer(_, JOLT.ContactManifold) at src/systems/physics/PhysicsSystem.ts:2034",
        ],
    },
    ContactSettings: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "JOLT.wrapPointer(_, JOLT.ContactSettings) at src/systems/physics/PhysicsSystem.ts:2009",
            "JOLT.wrapPointer(_, JOLT.ContactSettings) at src/systems/physics/PhysicsSystem.ts:2035",
        ],
    },
    SubShapeIDPair: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["JOLT.wrapPointer(_, JOLT.SubShapeIDPair) at src/systems/physics/PhysicsSystem.ts:2042"],
    },
    CollideShapeResult: {
        status: "SAFE",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["JOLT.wrapPointer(_, JOLT.CollideShapeResult) at src/systems/physics/PhysicsSystem.ts:2052"],
    },
    Vec3: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "AABox.GetCenter() is STATIC_ALIAS (src/util/TypeConversions.ts:139)",
            "AABox.GetExtent() is STATIC_ALIAS (src/util/TypeConversions.ts:140)",
            "WheelSettingsWV.get_mWheelForward() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:127)",
            "WheelSettingsWV.get_mSteeringAxis() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:128)",
            "WheelSettingsWV.get_mPosition() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:132)",
        ],
    },
    Quat: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "RMat44.GetQuaternion() is STATIC_ALIAS (src/util/TypeConversions.ts:82)",
            "Body.GetRotation() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/MecanumDriveBehavior.ts:196)",
            "Body.GetRotation() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/SwerveDriveBehavior.ts:52)",
            "Body.GetRotation() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/SwerveDriveBehavior.ts:73)",
            "Body.GetRotation() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/SwerveDriveBehavior.ts:81)",
        ],
    },
    RVec3: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "RMat44.GetTranslation() is STATIC_ALIAS (src/util/TypeConversions.ts:82)",
            "RMat44.GetTranslation() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/MecanumLayout.ts:73)",
            "RMat44.MulVec3() is STATIC_ALIAS (src/systems/simulation/driver/HingeDriver.ts:44)",
            "Body.GetCenterOfMassPosition() is STATIC_ALIAS (src/systems/simulation/behavior/synthesis/drive/SwerveDriveBehavior.ts:111)",
            "Body.GetCenterOfMassPosition() is STATIC_ALIAS (src/systems/simulation/synthesis_brain/SynthesisBrain.ts:263)",
        ],
    },
    Mat44: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "Mat44.sTranslation() is STATIC_ALIAS (src/util/TypeConversions.ts:141)",
            "Mat44.sRotationTranslation() is STATIC_ALIAS (src/mirabuf/ZoneSceneObject.ts:98)",
            "Mat44.Inversed() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:809)",
            "RMat44.GetRotation() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:809)",
            "Mat44.sRotationTranslation() is STATIC_ALIAS (src/mirabuf/MirabufSceneObject.ts:937)",
        ],
    },
    Vec4: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.Vec4(...) at src/util/TypeConversions.ts:54"],
    },
    Float3: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.Float3(...) at src/util/TypeConversions.ts:131"],
    },
    OrientedBox: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.OrientedBox(...) at src/util/TypeConversions.ts:143",
            "new JOLT.OrientedBox(...) at src/mirabuf/ZoneSceneObject.ts:100",
            "new JOLT.OrientedBox(...) at src/mirabuf/MirabufSceneObject.ts:939",
        ],
    },
    LinearCurve: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.LinearCurve(...) at src/systems/simulation/driver/WheelDriver.ts:157"],
    },
    SpringSettings: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "WheelSettingsWV.get_mSuspensionSpring() is INTERNAL_REF (src/systems/simulation/driver/WheelDriver.ts:130)",
            "new JOLT.SpringSettings(...) at src/systems/simulation/driver/WheelDriver.ts:247",
            "new JOLT.SpringSettings(...) at src/systems/simulation/driver/WheelDriver.ts:267",
        ],
    },
    ShapeGetTriangles: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.ShapeGetTriangles(...) at src/util/threejs/MeshCreation.ts:12",
            "new JOLT.ShapeGetTriangles(...) at src/mirabuf/MirabufSceneObject.ts:843",
            "new JOLT.ShapeGetTriangles(...) at src/mirabuf/MirabufSceneObject.ts:988",
        ],
    },
    SphereShapeSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "new JOLT.SphereShapeSettings(...) at src/mirabuf/IntakeSensorSceneObject.ts:40",
            "new JOLT.SphereShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:1135",
        ],
    },
    BodyID: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "Body.GetID() is INTERNAL_REF (src/ui/panels/configuring/assembly-config/interfaces/ConfigureGamepieceIntakeInterface.tsx:261)",
            "Body.GetID() is INTERNAL_REF (src/ui/panels/configuring/assembly-config/interfaces/ConfigureGamepieceEjectorInterface.tsx:215)",
            "Body.GetID() is INTERNAL_REF (src/util/threejs/MeshCreation.ts:96)",
            "Body.GetID() is INTERNAL_REF (src/ui/panels/configuring/assembly-config/interfaces/zones/ZoneConfigBase.tsx:247)",
            "Body.GetID() is INTERNAL_REF (src/ui/panels/configuring/assembly-config/interfaces/cameras/CameraConfigInterface.tsx:176)",
        ],
    },
    WheeledVehicleControllerSettings: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.WheeledVehicleControllerSettings(...) at src/systems/physics/ConstraintSettingsUtilities.ts:29",
        ],
    },
    JoltSettings: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.JoltSettings(...) at src/systems/physics/PhysicsSystem.ts:193"],
    },
    JoltInterface: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.JoltInterface(...) at src/systems/physics/PhysicsSystem.ts:196"],
    },
    BoxShape: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "JOLT.castObject(_, JOLT.BoxShape) at src/util/threejs/MeshCreation.ts:46",
            "new JOLT.BoxShape(...) at src/systems/physics/PhysicsSystem.ts:349",
            "new JOLT.BoxShape(...) at src/systems/physics/PhysicsSystem.ts:1740",
        ],
    },
    BodyCreationSettings: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.BodyCreationSettings(...) at src/systems/physics/PhysicsSystem.ts:397",
            "new JOLT.BodyCreationSettings(...) at src/systems/physics/PhysicsSystem.ts:1159",
            "new JOLT.BodyCreationSettings(...) at src/systems/physics/PhysicsSystem.ts:1743",
        ],
    },
    ConvexHullShapeSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "new JOLT.ConvexHullShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:438",
            "new JOLT.ConvexHullShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:1261",
        ],
    },
    HingeConstraintSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "new JOLT.HingeConstraintSettings(...) at src/systems/physics/PhysicsSystem.ts:625",
            "new JOLT.HingeConstraintSettings(...) at src/systems/physics/PhysicsSystem.ts:910",
        ],
    },
    SliderConstraintSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.SliderConstraintSettings(...) at src/systems/physics/PhysicsSystem.ts:659"],
    },
    FixedConstraintSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.FixedConstraintSettings(...) at src/systems/physics/PhysicsSystem.ts:677"],
    },
    VehicleConstraintSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.VehicleConstraintSettings(...) at src/systems/physics/PhysicsSystem.ts:695"],
    },
    VehicleConstraint: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "JOLT.castObject(_, JOLT.VehicleConstraint) at src/systems/simulation/SimulationSystem.ts:92",
            "new JOLT.VehicleConstraint(...) at src/systems/physics/PhysicsSystem.ts:708",
        ],
    },
    VehicleCollisionTesterCastCylinder: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.VehicleCollisionTesterCastCylinder(...) at src/systems/physics/PhysicsSystem.ts:787"],
    },
    VehicleConstraintStepListener: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.VehicleConstraintStepListener(...) at src/systems/physics/PhysicsSystem.ts:791"],
    },
    WheelSettingsWV: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "new JOLT.WheelSettingsWV(...) at src/systems/physics/PhysicsSystem.ts:839",
            "WheelWV.GetSettings() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/simulation/driver/WheelDriver.ts:126)",
            "WheelWV.GetSettings() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/simulation/driver/WheelDriver.ts:156)",
            "WheelWV.GetSettings() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/simulation/driver/WheelDriver.ts:174)",
            "WheelWV.GetSettings() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/simulation/driver/WheelDriver.ts:241)",
        ],
    },
    ArrayBodyID: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.ArrayBodyID(...) at src/systems/physics/PhysicsSystem.ts:967",
            "new JOLT.ArrayBodyID(...) at src/systems/physics/PhysicsSystem.ts:968",
            "new JOLT.ArrayBodyID(...) at src/systems/physics/PhysicsSystem.ts:1482",
        ],
    },
    StaticCompoundShapeSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.StaticCompoundShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:983"],
    },
    RotatedTranslatedShapeSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.RotatedTranslatedShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:1137"],
    },
    MeshShapeSettings: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.MeshShapeSettings(...) at src/systems/physics/PhysicsSystem.ts:1301"],
    },
    VertexList: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.VertexList(...) at src/systems/physics/PhysicsSystem.ts:1305"],
    },
    IndexedTriangleList: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.IndexedTriangleList(...) at src/systems/physics/PhysicsSystem.ts:1306"],
    },
    PhysicsMaterialList: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.PhysicsMaterialList(...) at src/systems/physics/PhysicsSystem.ts:1307"],
    },
    PhysicsMaterial: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: ["new JOLT.PhysicsMaterial(...) at src/systems/physics/PhysicsSystem.ts:1309"],
    },
    IndexedTriangle: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.IndexedTriangle(...) at src/systems/physics/PhysicsSystem.ts:1342"],
    },
    RRayCast: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.RRayCast(...) at src/systems/physics/PhysicsSystem.ts:1403"],
    },
    RayCastSettings: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.RayCastSettings(...) at src/systems/physics/PhysicsSystem.ts:1405"],
    },
    CastRayClosestHitCollisionCollector: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.CastRayClosestHitCollisionCollector(...) at src/systems/physics/PhysicsSystem.ts:1408"],
    },
    BroadPhaseLayerFilter: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.BroadPhaseLayerFilter(...) at src/systems/physics/PhysicsSystem.ts:1409"],
    },
    ObjectLayerFilter: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.ObjectLayerFilter(...) at src/systems/physics/PhysicsSystem.ts:1410"],
    },
    IgnoreMultipleBodiesFilter: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.IgnoreMultipleBodiesFilter(...) at src/systems/physics/PhysicsSystem.ts:1411"],
    },
    ShapeFilter: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.ShapeFilter(...) at src/systems/physics/PhysicsSystem.ts:1412"],
    },
    ContactListenerJS: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.ContactListenerJS(...) at src/systems/physics/PhysicsSystem.ts:1995"],
    },
    ObjectLayerPairFilterTable: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.ObjectLayerPairFilterTable(...) at src/systems/physics/PhysicsSystem.ts:2099"],
    },
    BroadPhaseLayer: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: [
            "new JOLT.BroadPhaseLayer(...) at src/systems/physics/PhysicsSystem.ts:2117",
            "new JOLT.BroadPhaseLayer(...) at src/systems/physics/PhysicsSystem.ts:2118",
            "new JOLT.BroadPhaseLayer(...) at src/systems/physics/PhysicsSystem.ts:2120",
        ],
    },
    BroadPhaseLayerInterfaceTable: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.BroadPhaseLayerInterfaceTable(...) at src/systems/physics/PhysicsSystem.ts:2124"],
    },
    ObjectVsBroadPhaseLayerFilterTable: {
        status: "DISQUALIFIED",
        isRefTarget: false,
        hasDestructor: true,
        evidence: ["new JOLT.ObjectVsBroadPhaseLayerFilterTable(...) at src/systems/physics/PhysicsSystem.ts:2138"],
    },
    Shape: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "Body.GetShape() is INTERNAL_REF but RefTarget-derived, unprovable (src/util/threejs/MeshCreation.ts:42)",
            "Body.GetShape() is INTERNAL_REF but RefTarget-derived, unprovable (src/mirabuf/MirabufSceneObject.ts:296)",
            "Body.GetShape() is INTERNAL_REF but RefTarget-derived, unprovable (src/mirabuf/MirabufSceneObject.ts:842)",
            "Body.GetShape() is INTERNAL_REF but RefTarget-derived, unprovable (src/mirabuf/MirabufSceneObject.ts:984)",
            "Body.GetShape() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/physics/PhysicsSystem.ts:757)",
        ],
    },
    Constraint: {
        status: "DISQUALIFIED",
        isRefTarget: true,
        hasDestructor: true,
        evidence: [
            "FixedConstraintSettings.Create() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/physics/PhysicsSystem.ts:680)",
            "HingeConstraintSettings.Create() is INTERNAL_REF but RefTarget-derived, unprovable (src/systems/physics/PhysicsSystem.ts:925)",
        ],
    },
}
