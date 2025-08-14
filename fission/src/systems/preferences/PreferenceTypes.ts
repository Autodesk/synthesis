import type { Vector3Tuple } from "three"
import type { ContactType } from "@/mirabuf/ZoneTypes"
import type { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import type { InputScheme } from "../input/InputTypes"
import type { SimConfigData } from "../simulation/SimConfigShared"

/** Names of all global preferences. */

export type GlobalPreferences = {
    ZoomSensitivity: number
    PitchSensitivity: number
    YawSensitivity: number
    SceneRotationSensitivity: number
    ViewCubeRotationSensitivity: number
    ReportAnalytics: boolean
    UseMetric: boolean
    RenderScoringZones: boolean
    RenderProtectedZones: boolean
    InputSchemes: InputScheme[]
    RenderSceneTags: boolean
    RenderScoreboard: boolean
    SubsystemGravity: boolean
    TouchControls: boolean
    SimAutoReconnect: boolean
    ShowViewCube: boolean
    MuteAllSound: boolean
    SFXVolume: number
    ShowCenterOfMassIndicators: boolean
    MultiplayerUsername: string
    MultiplayerClientID: string
}

export type GlobalPreference = keyof GlobalPreferences

export const ROBOT_PREFERENCE_KEY = "Robots" as const
export const FIELD_PREFERENCE_KEY = "Fields" as const
export const MOTOR_PREFERENCES_KEY = "Motors" as const
export const GRAPHICS_PREFERENCE_KEY = "Quality" as const

export type Preferences = GlobalPreferences & {
    [ROBOT_PREFERENCE_KEY]: Record<string, RobotPreferences>
    [FIELD_PREFERENCE_KEY]: Record<string, FieldPreferences>
    [MOTOR_PREFERENCES_KEY]: Record<string, MotorPreferences>
    [GRAPHICS_PREFERENCE_KEY]: GraphicsPreferences
}

/**
 * Default values for GlobalPreferences as a fallback if they are not configured by the user.
 * Every global preference should have a default value.
 */
export const defaultGlobalPreferences: GlobalPreferences = {
    ZoomSensitivity: 15,
    PitchSensitivity: 10,
    YawSensitivity: 3,
    SceneRotationSensitivity: 0.5,
    ViewCubeRotationSensitivity: 0.025,
    ReportAnalytics: false,
    UseMetric: false,
    RenderScoringZones: true,
    RenderProtectedZones: true,
    InputSchemes: [],
    RenderSceneTags: true,
    RenderScoreboard: true,
    SubsystemGravity: false,
    TouchControls: false,
    SimAutoReconnect: false,
    ShowViewCube: true,
    MuteAllSound: false,
    SFXVolume: 25,
    ShowCenterOfMassIndicators: false,
    MultiplayerClientID: "",
    MultiplayerUsername: "",
}

export type GraphicsPreferences = {
    lightIntensity: number
    fancyShadows: boolean
    maxFar: number
    cascades: number
    shadowMapSize: number
    antiAliasing: boolean
}

export function defaultGraphicsPreferences(): GraphicsPreferences {
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
    showZoneAlways: boolean
    maxPieces: number
    animationDuration: number
}

export type EjectorPreferences = {
    deltaTransformation: number[]
    ejectorVelocity: number
    parentNode: string | undefined
    ejectOrder: "FIFO" | "LIFO"
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
export function defaultSequentialConfig(index: number, type: BehaviorType): SequentialBehaviorPreferences {
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
    unstickForce: number
    sequentialConfig?: SequentialBehaviorPreferences[]
    simConfig?: SimConfigData
}

export type MotorPreferences = {
    name: string
    maxVelocity: number
    maxForce: number
}

export type Alliance = "red" | "blue"

export type Station = 1 | 2 | 3

export type ScoringZonePreferences = {
    name: string
    alliance: Alliance
    parentNode: string | undefined
    points: number
    destroyGamepiece: boolean
    persistentPoints: boolean

    deltaTransformation: number[]
}

export type ProtectedZonePreferences = {
    name: string
    alliance: Alliance
    penaltyPoints: number
    parentNode: string | undefined
    contactType: ContactType
    activeDuring: MatchModeType[]

    deltaTransformation: number[]
}

export type SpawnLocation = Readonly<{
    pos: Readonly<Vector3Tuple>
    yaw: number
}>
export type FieldPreferences = {
    spawnLocations: {
        [A in Alliance]: {
            [S in Station]: SpawnLocation
        }
    } & { default: SpawnLocation; hasConfiguredLocations: boolean }
    scoringZones: ScoringZonePreferences[]
    protectedZones: ProtectedZonePreferences[]
}

export function defaultRobotPreferences(): RobotPreferences {
    return {
        inputsSchemes: [],
        motors: [],
        intake: {
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            zoneDiameter: 0.5,
            parentNode: undefined,
            showZoneAlways: false,
            maxPieces: 1,
            animationDuration: 0.5,
        },
        ejector: {
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            ejectorVelocity: 1,
            parentNode: undefined,
            ejectOrder: "FIFO",
        },
        driveVelocity: 0,
        driveAcceleration: 0,
        unstickForce: 8000,
    }
}

// The object will be moved such that the y-value specified is the bottom of the object, and the x and z values are the center
export function defaultFieldSpawnLocation(): SpawnLocation {
    return { pos: [0, 0.1, 0], yaw: 0 }
}
export function defaultRobotSpawnLocation(): SpawnLocation {
    return { pos: [0, 0.1, 0], yaw: 0 }
}
export function defaultFieldPreferences(): FieldPreferences {
    return {
        spawnLocations: {
            red: {
                1: { pos: [-1, 0.1, -1], yaw: Math.PI / 2 },
                2: { pos: [-1, 0.1, 0], yaw: Math.PI / 2 },
                3: { pos: [-1, 0.1, 1], yaw: Math.PI / 2 },
            },
            blue: {
                1: { pos: [1, 0.1, 1], yaw: -Math.PI / 2 },
                2: { pos: [1, 0.1, 0], yaw: -Math.PI / 2 },
                3: { pos: [1, 0.1, -1], yaw: -Math.PI / 2 },
            },
            default: defaultRobotSpawnLocation(),
            hasConfiguredLocations: false,
        },
        scoringZones: [],
        protectedZones: [],
    }
}

export function defaultMotorPreferences(name: string): MotorPreferences {
    return {
        name: name,
        maxVelocity: 1,
        maxForce: 1,
    }
}
