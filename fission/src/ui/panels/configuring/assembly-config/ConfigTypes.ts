import { MiraType } from "@/mirabuf/MirabufLoader.ts"

export const CONFIG_OPTS = ["ROBOTS", "FIELDS", "INPUTS"] as const
export type ConfigurationType = (typeof CONFIG_OPTS)[number]

export function configTypeToMiraType(config: ConfigurationType): MiraType | undefined {
    switch (config) {
        case "FIELDS":
            return MiraType.FIELD
        case "ROBOTS":
            return MiraType.ROBOT
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
    MOVE,
    SIM,
    BRAIN,
    DRIVETRAIN,
    ALLIANCE,
}
