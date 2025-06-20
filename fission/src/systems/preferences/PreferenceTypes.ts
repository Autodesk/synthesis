import { SimConfigData } from "@/ui/panels/simulation/SimConfigShared"
import { InputScheme } from "../input/InputSchemeManager"
import { Vector3Tuple } from "three"

/** Names of all global preferences. */

export type GlobalPreferences = {
    QualitySettings: QualitySetting
    ZoomSensitivity: number
    PitchSensitivity: number
    YawSensitivity: number
    ReportAnalytics: boolean
    UseMetric: boolean
    RenderScoringZones: boolean
    InputSchemes: InputScheme[]
    RenderSceneTags: boolean
    RenderScoreboard: boolean
    SubsystemGravity: boolean
    SimAutoReconnect: boolean
}

export type GlobalPreference = keyof GlobalPreferences

export type Preferences = GlobalPreferences & {
    [RobotPreferencesKey]: Record<string, RobotPreferences>
    [FieldPreferencesKey]: Record<string, FieldPreferences>
}

export const RobotPreferencesKey = "Robots" as const
export const FieldPreferencesKey = "Fields" as const

/**
 * Default values for GlobalPreferences as a fallback if they are not configured by the user.
 * Every global preference should have a default value.
 */
export const DefaultGlobalPreferences: GlobalPreferences = {
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
    SubsystemGravity: false,
    SimAutoReconnect: false,
}

export type QualitySetting = "Low" | "Medium" | "High"

export type IntakePreferences = {
    deltaTransformation: number[]
    zoneDiameter: number
    parentNode: string | undefined
    showZoneAlways: boolean
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
    motors: MotorPreferences[]
    intake: IntakePreferences
    ejector: EjectorPreferences
    driveVelocity: number
    driveAcceleration: number
    sequentialConfig?: SequentialBehaviorPreferences[]
    simConfig?: SimConfigData
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
    // TODO: implement this
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
            showZoneAlways: false,
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
