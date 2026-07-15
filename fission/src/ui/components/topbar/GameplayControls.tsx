import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "../../modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "../../modals/startMultiplayerWorld"
import { IconButton } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"
import { TopBarIcon } from "./TopBarIcons"

const GameplayControls: React.FC = () => {
    const { openModal, openPanel } = useUIContext()

    const openMatchMode = () => openPanel(MatchModeConfigPanel, undefined)

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <Tooltip title="Start Match">
                <IconButton size="medium" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={openMatchMode}>
                    <TopBarIcon name="gp-2" size={30} />
                </IconButton>
            </Tooltip>
            <Tooltip title="Open Multiplayer">
                <IconButton size="medium" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={openMultiplayer}>
                    <TopBarIcon name="gp-1" size={30} />
                </IconButton>
            </Tooltip>
        </Stack>
    )
}

export default GameplayControls
