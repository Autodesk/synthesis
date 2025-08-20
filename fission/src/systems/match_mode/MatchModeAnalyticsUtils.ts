import type { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import type { MatchEvent } from "../analytics/AnalyticsSystem"

/**
 * Converts MatchModeConfig to MatchEvent format for analytics.
 * Handles all the complex logic for determining penalty settings.
 */
export function createMatchEventFromConfig(config: MatchModeConfig, overrides?: Partial<MatchEvent>): MatchEvent {
    const hasHeightPenalty = config.maxHeight !== Number.MAX_SAFE_INTEGER
    const hasSideExtensionPenalty = config.sideMaxExtension !== Number.MAX_SAFE_INTEGER

    return {
        matchName: config.name,
        isDefault: config.isDefault,
        autonomousTime: config.autonomousTime,
        teleopTime: config.teleopTime,
        endgameTime: config.endgameTime,
        hasHeightPenalty: hasHeightPenalty,
        maxHeight: hasHeightPenalty ? config.maxHeight : undefined,
        heightLimitPenalty: hasHeightPenalty ? config.heightLimitPenalty : undefined,
        ignoreRotation: config.ignoreRotation,
        hasSideExtensionPenalty: hasSideExtensionPenalty,
        sideMaxExtension: hasSideExtensionPenalty ? config.sideMaxExtension : undefined,
        sideExtensionPenalty: hasSideExtensionPenalty ? config.sideExtensionPenalty : undefined,
        ...overrides,
    }
}
