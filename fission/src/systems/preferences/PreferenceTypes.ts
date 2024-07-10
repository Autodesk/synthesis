import { Vector3Tuple, Vector4Tuple } from "three"
import { InputScheme } from "../input/DefaultInputs"

export type GlobalPreference =
    | "ScreenMode"
    | "QualitySettings"
    | "ZoomSensitivity"
    | "PitchSensitivity"
    | "YawSensitivity"
    | "ReportAnalytics"
    | "UseMetric"
    | "RenderScoringZones"

export const RobotPreferencesKey: string = "Robots"
export const FieldPreferencesKey: string = "Fields"

export const DefaultGlobalPreferences: { [key: string]: unknown } = {
    ScreenMode: "Windowed",
    QualitySettings: "High",
    ZoomSensitivity: 15,
    PitchSensitivity: 10,
    YawSensitivity: 3,
    ReportAnalytics: false,
    UseMetric: false,
    RenderScoringZones: true,
}

export type IntakePreferences = {
    location: Vector3Tuple,
    diameter: number
}

export type EjectorPreferences = {
    location: Vector3Tuple, 
    ejectorVelocity: number
}

export type RobotPreferences = {
    inputsSchemes: InputScheme[],
    intake: IntakePreferences,
    ejector: EjectorPreferences
}

export type Alliance = "Blue" | "Red"

export type ScoringZonePreferences = {
    name: string,
    alliance: Alliance,
    parent: string,
    points: number,
    destroyGamepiece: boolean,
    persistentPoints: boolean,
    localPosition: Vector3Tuple,
    localRotation: Vector4Tuple,
    localScale: Vector3Tuple
}

export type FieldPreferences = {
    defaultSpawnLocation: Vector3Tuple
    scoringZones: ScoringZonePreferences[]
}


export function DefaultRobotPreferences(): RobotPreferences {
    return {
        inputsSchemes: [],
        intake: { location: [0, 0, 0], diameter: 1 }, 
        ejector: { location: [0, 0, 0], ejectorVelocity: 1 }
    };
}

export function DefaultFieldPreferences(): FieldPreferences {
    return { defaultSpawnLocation: [0, 1, 0], scoringZones: [] }
}