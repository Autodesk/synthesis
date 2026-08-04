import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import MatchMode from "@/systems/match_mode/MatchMode"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import MultiplayerStartModal from "@/ui/modals/MultiplayerStartModal"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

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

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton
                label={isMatchRunning ? "Abort Match" : "Start Match"}
                icon={<TopBarIcon name={isMatchRunning ? "gp-2-abort" : "gp-2"} size={30} />}
                onClick={isMatchRunning ? abortMatchMode : openMatchMode}
            />
            <TopBarButton
                label="Open Multiplayer"
                icon={<TopBarIcon name="gp-1" size={30} />}
                onClick={openMultiplayer}
            />
        </Stack>
    )
}

export default GameplayControls
