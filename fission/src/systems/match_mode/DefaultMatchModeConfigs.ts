import { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { convertFeetToMeters } from "@/util/UnitConversions"

/** The purpose of this class is to store any defaults related to match mode configurations. */
class DefaultMatchModeConfigs {
    static frcReefscape2025 = (): MatchModeConfig => {
        return {
            id: "FRC-Reefscape-2025",
            name: "FRC Reefscape 2025",
            isDefault: true,
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 20,
            ignoreRotation: true,
            maxHeight: Infinity,
            heightPenalty: 0,
        }
    }

    static frcCrescendo2024 = (): MatchModeConfig => {
        return {
            id: "FRC-Crescendo-2024",
            name: "FRC Crescendo 2024",
            isDefault: true,
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 20,
            ignoreRotation: true,
            maxHeight: convertFeetToMeters(4),
            heightPenalty: 2,
        }
    }

    static frcPowerUp2023 = (): MatchModeConfig => {
        return {
            id: "FRC-Power-Up-2023",
            name: "FRC Power Up 2023",
            isDefault: true,
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 30,
            ignoreRotation: true,
            maxHeight: convertFeetToMeters(6.5),
            heightPenalty: 5,
        }
    }

    static matchTest = (): MatchModeConfig => {
        return {
            id: "Match-Test",
            name: "Match Test",
            isDefault: true,
            autonomousTime: 5,
            teleopTime: 15,
            endgameTime: 5,
            ignoreRotation: true,
            maxHeight: Infinity,
            heightPenalty: 0,
        }
    }

    static fallbackValues = (): MatchModeConfig => {
        return {
            id: "default",
            name: "Default",
            isDefault: true,
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 20,
            ignoreRotation: true,
            maxHeight: Infinity,
            heightPenalty: 2,
        }
    }

    /** @returns {MatchModeConfig[]} New copies of the default match mode configs without reference to any others. */
    public static get defaultMatchModeConfigCopies(): MatchModeConfig[] {
        return [
            DefaultMatchModeConfigs.frcReefscape2025(),
            DefaultMatchModeConfigs.frcCrescendo2024(),
            DefaultMatchModeConfigs.frcPowerUp2023(),
            DefaultMatchModeConfigs.matchTest(),
        ]
    }
}

export default DefaultMatchModeConfigs
