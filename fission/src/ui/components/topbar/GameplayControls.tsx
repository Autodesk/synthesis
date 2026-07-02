import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "../../modals/MultiplayerStartModal"
import { globalAddToast } from "../GlobalUIControls"
import { IconButton } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./topBarConfig"
import { TopBarIcon } from "./TopBarIcons"

const GameplayControls: React.FC = () => {
    const { openModal, openPanel } = useUIContext()

    const openMatchMode = () => openPanel(MatchModeConfigPanel, undefined)

    const openMultiplayer = () => {
        openModal(MultiplayerStartModal, {
            startWorldCallback: async (name: string, room?: string) => {
                const isHost = room == null
                const roomId = room ?? Math.random().toString(10).substring(2, 8)
                PreferencesSystem.setGlobalPreference("MultiplayerUsername", name)
                PreferencesSystem.savePreferences()
                const success = await MultiplayerSystem.setup(roomId, name, isHost)
                if (success && isHost) {
                    globalAddToast("info", "Room Code", roomId)
                }
                return success
            },
        })
    }

    return (
        <Stack direction="row" alignItems="center" gap={2}>
            <Tooltip title="Start Match">
                <IconButton size="large" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={openMatchMode}>
                    <TopBarIcon name="gp-2" size={40} />
                </IconButton>
            </Tooltip>
            <Tooltip title="Open Multiplayer">
                <IconButton size="large" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={openMultiplayer}>
                    <TopBarIcon name="gp-1" size={40} />
                </IconButton>
            </Tooltip>
        </Stack>
    )
}

export default GameplayControls
