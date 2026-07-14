import { MiraType } from "@/mirabuf/MirabufLoader.ts"

export const CONFIG_OPTS = ["ROBOTS", "FIELDS", "PIECES", "INPUTS"] as const
export type ConfigurationType = (typeof CONFIG_OPTS)[number]

export function configTypeToMiraType(config: ConfigurationType): MiraType | undefined {
    switch (config) {
        case "FIELDS":
            return MiraType.FIELD
        case "ROBOTS":
            return MiraType.ROBOT
        case "PIECES":
            return MiraType.PIECE
        default:
            return undefined
    }
}

export function miraTypeToConfigType(config: MiraType): ConfigurationType {
    switch (config) {
        case MiraType.ROBOT:
            return "ROBOTS"
        case MiraType.FIELD:
            return "FIELDS"
        case MiraType.PIECE:
            return "PIECES"
    }
}

export enum ConfigMode {
    SUBSYSTEMS,
    EJECTOR,
    INTAKE,
    CONTROLS,
    SEQUENTIAL,
    SCORING_ZONES,
    PROTECTED_ZONES,
    CAMERA_POINTS,
    MOVE,
    SIM,
    BRAIN,
    DRIVETRAIN,
    ALLIANCE,
    METADATA,
}
