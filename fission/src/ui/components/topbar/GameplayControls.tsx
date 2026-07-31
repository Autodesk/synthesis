import { Box, Stack } from "@mui/material"
import type React from "react"
import EventSystem from "@/systems/EventSystem.ts"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

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
            <TopBarButton
                label="Toggle Scoreboard"
                icon={
                    <Box sx={TOP_BAR_GLYPH_SX}>
                        <SynthesisIcons.SCOREBOARD />
                    </Box>
                }
                onClick={() => EventSystem.dispatch("ToggleScoreboardEvent")}
            />
        </Stack>
    )
}

export default GameplayControls
