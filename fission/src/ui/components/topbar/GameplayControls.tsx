import { Box, Stack } from "@mui/material"
import type React from "react"
import { useMemo } from "react"
import { SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { SCOREBOARD_GLYPH_SX, SCOREBOARD_MODE_LABELS } from "@/ui/helpers/ScoreboardVisibility"
import { useScoreboard } from "@/ui/helpers/useScoreboard"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

const ScoreboardSplitDropdown: React.FC = () => {
    const { mode, visible, setMode, toggle } = useScoreboard()

    const ScoreboardGlyph = visible ? SynthesisIcons.SCOREBOARD : SynthesisIcons.SCOREBOARD_HIDDEN

    const items = useMemo(
        () =>
            SCOREBOARD_MODES.map(option => ({
                key: option,
                label: SCOREBOARD_MODE_LABELS[option],
                selected: option === mode,
                onSelect: () => setMode(option),
            })),
        [mode, setMode]
    )

    return (
        <SplitButtonDropdown
            icon={
                <Box sx={SCOREBOARD_GLYPH_SX[mode]}>
                    <ScoreboardGlyph />
                </Box>
            }
            iconTooltip={visible ? "Hide Scoreboard" : "Show Scoreboard"}
            caretTooltip={`Scoreboard visibility: ${SCOREBOARD_MODE_LABELS[mode]}`}
            onIconClick={toggle}
            items={items}
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
            <ScoreboardSplitDropdown />
        </Stack>
    )
}

export default GameplayControls
