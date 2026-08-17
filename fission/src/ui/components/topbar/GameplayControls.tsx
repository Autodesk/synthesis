import { Box, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import MatchMode from "@/systems/match_mode/MatchMode"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/multiplayer/MultiplayerStartModal"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

export const ScoreboardButton: React.FC = () => {
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

    const [isMatchRunning, setIsMatchRunning] = useState(() => MatchMode.getInstance().isMatchEnabled())

    useEffect(() => {
        return EventSystem.listen("MatchStateChangedEvent", () => {
            setIsMatchRunning(MatchMode.getInstance().isMatchEnabled())
        })
    }, [])

    const openMatchMode = () => togglePanel(MatchModeConfigPanel, undefined)

    const abortMatchMode = () => {
        openModal(
            ConfirmModal,
            { message: "Are you sure you want to abort the current match? This cannot be undone." },
            undefined,
            {
                title: "Abort Match",
                acceptText: "Abort Match",
                cancelText: "Cancel",
                onAccept: () => {
                    MatchMode.getInstance().sandboxModeStart()
                    globalAddToast("info", "Match Mode Cancelled")
                },
            }
        )
    }

    const openMultiplayer = () => openModal(MultiplayerStartModal, undefined)

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton
                label={isMatchRunning ? "Abort Match" : "Start Match"}
                icon={<TopBarIcon name={isMatchRunning ? "gp-match-mode-abort" : "gp-match-mode"} size={30} />}
                onClick={isMatchRunning ? abortMatchMode : openMatchMode}
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
