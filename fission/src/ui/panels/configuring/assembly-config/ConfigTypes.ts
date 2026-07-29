import { MiraType } from "@/mirabuf/MirabufLoader.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import type React from "react"
import type { ConfigurePanelCustomProps } from "@/panels/configuring/assembly-config/ConfigurePanel.tsx"
import type { UIScreen } from "@/ui/helpers/UIProviderHelpers.ts"
import { ConfigModeSelectionOption } from "@/panels/configuring/assembly-config/configure/ConfigModeSelection.tsx"

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

export type CleanupRegisterFunction = (
    applyFunc?: () => void | Promise<void>,
    revertFunc?: () => void | Promise<void>
) => void

export interface ConfigurationSubpanelProps {
    selectedAssembly: MirabufSceneObject
    /**
     * Registers a callback to be run when the config panel is closed.
     *
     * Note that `selectedAssembly.savePreferences()` should not be called by cleanup functions, it will be handled automatically
     *
     * @param applyFunc called when the save button is pressed, to perform any latent cleanup or saving. These will be called in FIFO order (older changes applied first)
     * @param revertFunc called when the cancel button is pressed, to restore preferences to their original state. These will be called in LIFO order (more recent changes reverted first)
     */
    registerCleanupFunction: CleanupRegisterFunction
    /**
     * This determines not if any change has been made, but whether it is possible for any change to have been made (any cleanup callbacks have been registered). Therefore, it will have false positives, but will not report false negatives.
     */
    hasMadeChanges: boolean
    /**
     * Passed from ConfigurePanel, allows panels to forbid proceeding (e.g., if the config is invalid)
     */
    setDisableAccept: React.Dispatch<React.SetStateAction<boolean>>,
    panel: UIScreen<void, ConfigurePanelCustomProps>
}

export type ConfigurationSubpanelComponent = (props: ConfigurationSubpanelProps) => React.ReactElement | null

export enum ConfigMode {
    JOINT_SUBSYSTEMS,
    EJECTOR,
    INTAKE,
    CONTROLS,
    JOINT_SEQUENCE,
    SCORING_ZONES,
    PROTECTED_ZONES,
    CAMERA_POINTS,
    MOVE,
    SIM,
    BRAIN,
    DRIVETRAIN,
    ALLIANCE,
    METADATA,
    CAMERA,
}

const baseRobotConfigModes = [
    new ConfigModeSelectionOption("Brain", ConfigMode.BRAIN, "Select and modify what is controlling of the robot."),

    new ConfigModeSelectionOption("Move", ConfigMode.MOVE, "Adjust position of robot relative to field."),
    new ConfigModeSelectionOption("Drivetrain", ConfigMode.DRIVETRAIN, "Sets the drivetrain type."),

    new ConfigModeSelectionOption(
        "Intake",
        ConfigMode.INTAKE,
        "Configure the robot’s intake position and parent node for picking up game pieces."
    ),

    new ConfigModeSelectionOption(
        "Ejector",
        ConfigMode.EJECTOR,
        "Configure the robot’s ejector mechanism, which controls the release or expulsion of game pieces."
    ),

    new ConfigModeSelectionOption(
        "USB Cameras",
        ConfigMode.CAMERA,
        "Add USB cameras and configure their position, resolution, and field of view for code simulation."
    ),

    new ConfigModeSelectionOption(
        "Configure Joints",
        ConfigMode.JOINT_SUBSYSTEMS,
        "Set the velocities, torques, and accelerations of your robot's motors."
    ),

    new ConfigModeSelectionOption(
        "Sequence Joints",
        ConfigMode.JOINT_SEQUENCE,
        "Set which joints follow each other. For example, the second stage of an elevator could follow the first, moving in unison with it."
    ),

    new ConfigModeSelectionOption(
        "Alliance / Station",
        ConfigMode.ALLIANCE,
        "Set the robot's alliance color for matches. (red or blue)"
    ),
    new ConfigModeSelectionOption("Metadata", ConfigMode.METADATA, "Update the robot's metadata"),
]

export const synthesisRobotConfigModes = [
    ...baseRobotConfigModes,
    new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS, "Set your controller scheme."),
]
export const wpilibRobotConfigModes = [
    ...baseRobotConfigModes,
    new ConfigModeSelectionOption(
        "Simulation",
        ConfigMode.SIM,
        "Configure the WPILib simulation settings for this robot."
    ),
]

export const fieldConfigModes = [
    new ConfigModeSelectionOption("Move", ConfigMode.MOVE, "Adjust position of field relative to robot."),
    new ConfigModeSelectionOption(
        "Scoring Zones",
        ConfigMode.SCORING_ZONES,
        "Define and manage zones on the field where robots can earn points during simulation."
    ),
    new ConfigModeSelectionOption(
        "Protected Zones",
        ConfigMode.PROTECTED_ZONES,
        "Define and manage protected zones on the field where robots can not enter."
    ),
    new ConfigModeSelectionOption(
        "Camera Positions",
        ConfigMode.CAMERA_POINTS,
        "Place and configure driver-station camera views for this field."
    ),
    new ConfigModeSelectionOption("Metadata", ConfigMode.METADATA, "Update the field's metadata"),
]
