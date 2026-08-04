import { Box, MenuItem, Stack } from "@mui/material"
import type React from "react"
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

const MENU_ICON_SIZE = 18

const ScoreboardSplitDropdown: React.FC = () => {
    const { mode, visible, setMode, toggle } = useScoreboard()

    const ScoreboardGlyph = visible ? SynthesisIcons.SCOREBOARD : SynthesisIcons.SCOREBOARD_HIDDEN

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
        >
            {SCOREBOARD_MODES.map(option => (
                <MenuItem key={option} dense selected={option === mode} onClick={() => setMode(option)}>
                    <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
                        <Box sx={{ display: "flex", width: MENU_ICON_SIZE, fontSize: MENU_ICON_SIZE }}>
                            {option === mode && <SynthesisIcons.CHECK />}
                        </Box>
                        {SCOREBOARD_MODE_LABELS[option]}
                    </Stack>
                </MenuItem>
            ))}
        </SplitButtonDropdown>
    )
}

const GameplayControls: React.FC = () => {
    const { openModal, togglePanel } = useUIContext()

    const openMatchMode = () => togglePanel(MatchModeConfigPanel, undefined)

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton label="Start Match" icon={<TopBarIcon name="gp-2" size={30} />} onClick={openMatchMode} />
            <TopBarButton
                label="Open Multiplayer"
                icon={<TopBarIcon name="gp-1" size={30} />}
                onClick={openMultiplayer}
            />
            <ScoreboardSplitDropdown />
        </Stack>
    )
}

export default GameplayControls
