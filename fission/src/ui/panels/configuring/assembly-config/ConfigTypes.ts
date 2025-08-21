export const CONFIG_OPTS = ["ROBOTS", "FIELDS", "PIECES", "INPUTS"] as const
export type ConfigurationType = (typeof CONFIG_OPTS)[number]

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
