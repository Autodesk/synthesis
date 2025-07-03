export interface ExporterConfig {
    mode: "ROBOT" | "FIELD"
    destination: "UPLOAD" | "DOWNLOAD"
    autoCalculateRobotWeight: boolean
    userDefinedWeight: number
    calculatedWeight: number
    compressOutput: boolean
    exportAsPart: boolean
    overrideFriction: boolean
    userDefinedFriction: number
    openSynthesisWhenDone: boolean
    autoCalculateGamepieceWeight: boolean
}
export interface Gamepiece {
    entityToken: string
    name: string
    userDefinedMass: number
    calculatedMass: number
    entityIDs: string[]
    friction: number
}

export enum SignalType {
    PWM,
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
export interface Joint {
    id: string
    name: string
    type: JointType
    parentNode: string
    signalType: SignalType
    speed: number
    force: number
    isWheel: boolean
}

export function DefaultExporterConfig(): ExporterConfig {
    return {
        mode: "ROBOT",
        destination: "UPLOAD",
        autoCalculateRobotWeight: true,
        userDefinedWeight: 0,
        compressOutput: true,
        exportAsPart: false,
        overrideFriction: false,
        userDefinedFriction: 0,
        calculatedWeight: 0,
        autoCalculateGamepieceWeight: false,
        openSynthesisWhenDone: false,
    }
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
