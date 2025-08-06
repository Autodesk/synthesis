import type { DriveType } from "../simulation/behavior/Behavior"
import type Input from "./inputs/Input"

export type InputName =
    | "arcadeDrive"
    | "arcadeTurn"
    | "tankLeft"
    | "tankRight"
    | "intake"
    | "eject"
    | "unstick"
    | `joint ${number}`

export type ModifierState = Readonly<{
    alt: boolean
    ctrl: boolean
    shift: boolean
    meta: boolean
}>

export type InputScheme = {
    schemeName: string
    descriptiveName: string
    customized: boolean
    usesGamepad: boolean
    usesTouchControls: boolean
    supportedDrivetrains: DriveType[]
    inputs: Input[]
}

export enum InputSchemeUseType {
    IN_USE, // bound to a robot
    CONFLICT, // has keys overlapping with a bound scheme
    AVAILABLE, // no overlap and not bound
}

export type InputSchemeAvailability = {
    scheme: InputScheme
    status: InputSchemeUseType
    conflictingSchemeNames?: string
}

export const EMPTY_MODIFIER_STATE: ModifierState = {
    ctrl: false,
    alt: false,
    shift: false,
    meta: false,
}

// biome-ignore lint/style/useNamingConvention: prevent strings from being assigned without explicit casting
export type KeyDescriptor = (string & { __: "KeyDescriptor" }) | null
