import { InputScheme } from "../input/InputSchemeManager"
import { Vector3Tuple } from "three"

/** Names of all global preferences. */
export type GlobalPreference =
    | "QualitySettings"
    | "ZoomSensitivity"
    | "PitchSensitivity"
    | "YawSensitivity"
    | "ReportAnalytics"
    | "UseMetric"
    | "RenderScoringZones"
    | "InputSchemes"
    | "RenderSceneTags"
    | "RenderScoreboard"

export const RobotPreferencesKey: string = "Robots"
export const FieldPreferencesKey: string = "Fields"

/**
 * Default values for GlobalPreferences as a fallback if they are not configured by the user.
 * Every global preference should have a default value.
 */
export const DefaultGlobalPreferences: { [key: string]: unknown } = {
    QualitySettings: "High" as QualitySetting,
    ZoomSensitivity: 15,
    PitchSensitivity: 10,
    YawSensitivity: 3,
    ReportAnalytics: false,
    UseMetric: false,
    RenderScoringZones: true,
    InputSchemes: [],
    RenderSceneTags: true,
    RenderScoreboard: true,
}

export type QualitySetting = "Low" | "Medium" | "High"

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

/** The behavior types that can be sequenced. */
export type BehaviorType = "Elevator" | "Arm"

/** Data for sequencing and inverting elevator and behaviors. */
export type SequentialBehaviorPreferences = {
    jointIndex: number
    parentJointIndex: number | undefined
    type: BehaviorType
    inverted: boolean
}

/** Default preferences for a joint with not parent specified and inverted set to false. */
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
    intake: IntakePreferences
    ejector: EjectorPreferences
    sequentialConfig?: SequentialBehaviorPreferences[]
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
    // TODO: implement this
    defaultSpawnLocation: Vector3Tuple
    scoringZones: ScoringZonePreferences[]
}

export function DefaultRobotPreferences(): RobotPreferences {
    return {
        inputsSchemes: [],
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
    }
}

export function DefaultFieldPreferences(): FieldPreferences {
    return { defaultSpawnLocation: [0, 1, 0], scoringZones: [] }
}
