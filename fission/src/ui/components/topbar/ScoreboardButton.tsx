import { Box } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

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

export default ScoreboardButton
