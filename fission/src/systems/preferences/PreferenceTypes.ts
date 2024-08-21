import { InputScheme } from "../input/InputSchemeManager"
import { Vector3Tuple } from "three"

export type GlobalPreference =
    | "ZoomSensitivity"
    | "PitchSensitivity"
    | "YawSensitivity"
    | "ReportAnalytics"
    | "UseMetric"
    | "RenderScoringZones"
    | "InputSchemes"
    | "RenderSceneTags"
    | "RenderScoreboard"
    | "SubsystemGravity"

export const RobotPreferencesKey: string = "Robots"
export const FieldPreferencesKey: string = "Fields"
export const GraphicsPreferenceKey: string = "Quality"

export const DefaultGlobalPreferences: { [key: string]: unknown } = {
    ZoomSensitivity: 15,
    PitchSensitivity: 10,
    YawSensitivity: 3,
    ReportAnalytics: false,
    UseMetric: false,
    RenderScoringZones: true,
    InputSchemes: [],
    RenderSceneTags: true,
    RenderScoreboard: true,
    SubsystemGravity: false,
}

export type GraphicsPreferences = {
    lightIntensity: number
    fancyShadows: boolean
    maxFar: number
    cascades: number
    shadowMapSize: number
    antiAliasing: boolean
}

export function DefaultGraphicsPreferences(): GraphicsPreferences {
    return {
        lightIntensity: 5,
        fancyShadows: false,
        maxFar: 30,
        cascades: 4,
        shadowMapSize: 4096,
        antiAliasing: false,
    }
}

export type IntakePreferences = {
    deltaTransformation: number[]
    zoneDiameter: number
    parentNode: string | undefined
}

export type EjectorPreferences = {
    deltaTransformation: number[]
    ejectorVelocity: number
    parentNode: string | undefined
}

export type BehaviorType = "Elevator" | "Arm"

export type SequentialBehaviorPreferences = {
    jointIndex: number
    parentJointIndex: number | undefined
    type: BehaviorType
    inverted: boolean
}

export function DefaultSequentialConfig(index: number, type: BehaviorType): SequentialBehaviorPreferences {
    return {
        jointIndex: index,
        parentJointIndex: undefined,
        type: type,
        inverted: false,
    }
}

export type RobotPreferences = {
    inputsSchemes: InputScheme[]
    motors: MotorPreferences[]
    intake: IntakePreferences
    ejector: EjectorPreferences
    driveVelocity: number
    driveAcceleration: number
    sequentialConfig?: SequentialBehaviorPreferences[]
}

export type MotorPreferences = {
    name: string
    maxVelocity: number
    maxForce: number
}

export type Alliance = "red" | "blue"

export type ScoringZonePreferences = {
    name: string
    alliance: Alliance
    parentNode: string | undefined
    points: number
    destroyGamepiece: boolean
    persistentPoints: boolean

    deltaTransformation: number[]
}

export type FieldPreferences = {
    defaultSpawnLocation: Vector3Tuple
    scoringZones: ScoringZonePreferences[]
}

export function DefaultRobotPreferences(): RobotPreferences {
    return {
        inputsSchemes: [],
        motors: [],
        intake: {
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            zoneDiameter: 0.5,
            parentNode: undefined,
        },
        ejector: {
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            ejectorVelocity: 1,
            parentNode: undefined,
        },
        driveVelocity: 0,
        driveAcceleration: 0,
    }
}

export function DefaultFieldPreferences(): FieldPreferences {
    return { defaultSpawnLocation: [0, 1, 0], scoringZones: [] }
}
