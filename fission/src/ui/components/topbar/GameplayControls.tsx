import { Box, Stack } from "@mui/material"
import type React from "react"
import { useCallback } from "react"
import type { ScoreboardMode } from "@/systems/preferences/PreferenceTypes"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { nextScoreboardMode, SCOREBOARD_GLYPH_SX, useScoreboard } from "@/ui/helpers/ScoreboardVisibility"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

const SCOREBOARD_TOOLTIPS: Record<ScoreboardMode, string> = {
    auto: "Scoreboard: auto (shown in gameplay only)",
    on: "Scoreboard: always on",
    off: "Scoreboard: always off",
}

const ScoreboardModeButton: React.FC = () => {
    const { mode, visible, setMode } = useScoreboard()

    const ScoreboardGlyph = visible ? SynthesisIcons.SCOREBOARD : SynthesisIcons.SCOREBOARD_HIDDEN
    const nextMode = nextScoreboardMode(mode)
    const cycleMode = useCallback(() => setMode(nextMode), [setMode, nextMode])

    return (
        <TopBarButton
            label={SCOREBOARD_TOOLTIPS[mode]}
            icon={
                <Box sx={SCOREBOARD_GLYPH_SX[mode]}>
                    <ScoreboardGlyph />
                </Box>
            }
            onClick={cycleMode}
        />
    )
}

const GameplayControls: React.FC = () => {
    const { openModal, togglePanel } = useUIContext()

    const openMatchMode = () => togglePanel(MatchModeConfigPanel, undefined)

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton
                label="Start Match"
                icon={<TopBarIcon name="gp-match-mode" size={30} />}
                onClick={openMatchMode}
            />
            <TopBarButton
                label="Open Multiplayer"
                icon={<TopBarIcon name="gp-multiplayer" size={30} />}
                onClick={openMultiplayer}
            />
            <ScoreboardModeButton />
        </Stack>
    )
}

export default GameplayControls
