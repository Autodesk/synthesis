import { Stack } from "@mui/material"
import type React from "react"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

const GameplayControls: React.FC = () => {
    const { openModal, openPanel } = useUIContext()

    const openMatchMode = () => openPanel(MatchModeConfigPanel, undefined)

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton label="Start Match" icon={<TopBarIcon name="gp-2" size={30} />} onClick={openMatchMode} />
            <TopBarButton
                label="Open Multiplayer"
                icon={<TopBarIcon name="gp-1" size={30} />}
                onClick={openMultiplayer}
            />
        </Stack>
    )
}

export default GameplayControls
