import type { Material } from "../ui/MaterialTaggingTab.tsx"

export type GeneralConfig = ExporterConfig & { calculatedRobotWeight: number }

export function DefaultExporterConfig(): GeneralConfig {
    return {
        fileLocation: "",

        gamepieces: [],
        joints: [],
        wheels: [],
        tags: {},

        materials: 0,
        exportMode: ExportMode.ROBOT,
        exportLocation: ExportLocation.DOWNLOAD,
        autoCalcRobotWeight: true,
        robotWeight: 0,
        compressOutput: true,
        exportAsPart: false,
        frictionOverride: false,
        frictionOverrideCoeff: 0.5,
        autoCalcGamepieceWeight: true,
        openSynthesisUponExport: false,

        calculatedRobotWeight: 0,

        hierarchy: ModelHierarchy.FusionAssembly,
        physicalCalculationLevel: CalculationAccuracy.LowCalculationAccuracy,
        physicalDepth: PhysicalDepth.AllOccurrence,
        visualQuality: TriangleMeshQualityOptions.LowQualityTriangleMesh,
    }
}
export interface Gamepiece {
    occurrenceToken: string
    name: string
    userDefinedMass: number
    calculatedMass: number
    entityIDs: string[]
    friction: number
}

export interface Joint {
    id: string
    name: string
    type: JointType
    parentNode: JointParentType
    signalType: SignalType
    speed: number
    force: number
    isWheel: boolean
    wheelType: WheelType
}

export enum ExportLocation {
    UPLOAD = 1,
    DOWNLOAD,
}

export enum ExportMode {
    ROBOT = 1,
    FIELD,
}
export enum WheelType {
    STANDARD = 1,
    OMNI,
    MECANUM,
}

export enum CalculationAccuracy {
    LowCalculationAccuracy = 0,
    MediumCalculationAccuracy = 1,
    HighCalculationAccuracy = 2,
    VeryHighCalculationAccuracy = 3,
}

export enum TriangleMeshQualityOptions {
    LowQualityTriangleMesh = 8,

    NormalQualityTriangleMesh = 11,
    HighQualityTriangleMesh = 13,
    VeryHighQualityTriangleMesh = 15,
}

enum PhysicalDepth {
    // No Physical Properties are generated
    NoPhysical = 0,

    // Only Body Physical Objects are generated
    Body = 1,

    // Only Occurrence that contain Bodies and Bodies have Physical Properties
    SurfaceOccurrence = 2,

    // Every Single Occurrence has Physical Properties even if empty
    AllOccurrence = 3,
}

enum ModelHierarchy {
    // Model exactly as it is shown in Fusion in the model view tree
    FusionAssembly = 0,

    // Flattened Assembly with all bodies as children of the root object
    FlatAssembly = 1,

    // A Model represented with parented objects that are part of a jointed tree
    PhysicalAssembly = 2,

    // Generates the root assembly as a single mesh and stores the associated data
    SingleMesh = 3,
}

export enum SignalType {
    PWM = 1,
    CAN,
    PASSIVE,
}
export enum JointType {
    RigidJointType,
    RevoluteJointType,
    SliderJointType,
    CylindricalJointType,
    PinSlotJointType,
    PlanarJointType,
    BallJointType,
}

export enum JointParentType {
    ROOT = 1,
    END,
}

export type SelectionFilter =
    | "Bodies"
    | "SolidBodies"
    | "SurfaceBodies"
    | "MeshBodies"
    | "Faces"
    | "SolidFaces"
    | "SurfaceFaces"
    | "PlanarFaces"
    | "CylindricalFaces"
    | "ConicalFaces"
    | "SphericalFaces"
    | "ToroidalFaces"
    | "SplineFaces"
    | "Edges"
    | "LinearEdges"
    | "CircularEdges"
    | "EllipticalEdges"
    | "TangentEdges"
    | "NonTangentEdges"
    | "Vertices"
    | "RootComponents"
    | "Occurrences"
    | "Sketches"
    | "SketchCurves"
    | "SketchLines"
    | "SketchCircles"
    | "SketchPoints"
    | "ConstructionPoints"
    | "ConstructionLines"
    | "ConstructionPlanes"
    | "Features"
    | "Canvases"
    | "Decals"
    | "JointOrigins"
    | "Joints"
    | "SketchConstraints"
    | "Profiles"
    | "Texts"
    | "CustomGraphics"

export interface ExporterConfig {
    fileLocation: string
    name?: string
    version?: string
    materials: number
    exportMode: ExportMode
    wheels: ExporterWheel[]
    joints: ExporterJoint[]
    tags:Record<string, Material>
    gamepieces: ExporterGamepiece[]
    robotWeight: number
    autoCalcRobotWeight: boolean
    autoCalcGamepieceWeight: boolean
    frictionOverride: boolean
    frictionOverrideCoeff: number
    compressOutput: boolean
    exportAsPart: boolean
    exportLocation: ExportLocation
    openSynthesisUponExport: boolean
    hierarchy: ModelHierarchy
    visualQuality: TriangleMeshQualityOptions
    physicalDepth: PhysicalDepth
    physicalCalculationLevel: CalculationAccuracy
}

interface ExporterJoint {
    jointToken: string
    parent: JointParentType
    signalType: SignalType
    speed: number
    force: number
    isWheel: boolean
}


interface ExporterGamepiece {
    occurrenceToken: string
    weight: number
    friction: number
}

interface ExporterWheel {
    jointToken: string
    wheelType: WheelType
    signalType: SignalType
}
