import { Box, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

const ScoreboardButton: React.FC = () => {
    const [alwaysOn, setAlwaysOn] = useState(() => PreferencesSystem.getUserPreference("AlwaysShowScoreboard"))

    useEffect(
        () => PreferencesSystem.addPreferenceEventListener("AlwaysShowScoreboard", e => setAlwaysOn(e.prefValue)),
        []
    )

    const toggleAlwaysOn = () => {
        PreferencesSystem.setUserPreference("AlwaysShowScoreboard", !alwaysOn)
        PreferencesSystem.savePreferences()
    }

    return (
        <TopBarButton
            label={alwaysOn ? "Scoreboard: Always On" : "Scoreboard: Only During Matches"}
            active={alwaysOn}
            icon={
                <Box sx={TOP_BAR_GLYPH_SX}>
                    <SynthesisIcons.SCOREBOARD />
                </Box>
            }
            onClick={toggleAlwaysOn}
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
            <ScoreboardButton />
        </Stack>
    )
}

export default GameplayControls
