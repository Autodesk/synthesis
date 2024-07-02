import { Vector3Tuple } from "three"
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

export const DefaultPreferences: { [key: string]: Object } = {
    "ScreenMode": "Windowed",
    "QualitySettings": "High",
    "ZoomSensitivity": 15,
    "PitchSensitivity": 10,
    "YawSensitivity": 3,
    "ReportAnalytics": false,
    "UseMetric": false,
    "RenderScoringZones": true,
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
    // TODO: after merge with advanced inputs
    //controls: InputScheme,
    test: boolean
    inputsScheme: InputScheme
    intake: IntakePreferences
    ejector: EjectorPreferences
}

export type FieldPreferences = {}


export function DefaultRobotPreferences(): RobotPreferences {
    return {
        test: false, 
        inputsScheme: {schemeName: "", usesGamepad: false, inputs: []},
        intake: { location: [0, 0, 0], diameter: 1 }, 
        ejector: { location: [0, 0, 0], ejectorVelocity: 1 }
    };
}
