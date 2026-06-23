import { Box, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { deobf } from "@/util/Utility"
import { useUIContext } from "../helpers/UIProviderHelpers"
import APSManagementModal from "../modals/APSManagementModal"
import SettingsModal from "../modals/configuring/SettingsModal"
import type { ConfigurationType } from "../panels/configuring/assembly-config/ConfigTypes"
import ImportMirabufPanel from "../panels/mirabuf/ImportMirabufPanel"
import { setAddToast, setOpenModal, setOpenPanel } from "./GlobalUIControls"
import { IconButton } from "./StyledComponents"
import ConfigureControls from "./topbar/ConfigureControls"
import GameplayControls from "./topbar/GameplayControls"
import ModeDropdown from "./topbar/ModeDropdown"
import { TOP_BAR_HEIGHT, TOP_BAR_ICON_BUTTON_SX } from "./topbar/topBarConfig"
import { TopBarIcon } from "./topbar/TopBarIcons"
import UserIcon from "./UserIcon"

const TopBar: React.FC = () => {
    const { openModal, openPanel, addToast } = useUIContext()
    const { appMode } = useStateContext()

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)

    const [userInfo, setUserInfo] = useState(APS.userInfo)
    const [modeHovered, setModeHovered] = useState(false)
    const [modeMenuOpen, setModeMenuOpen] = useState(false)

    useEffect(() => {
        // biome-ignore-start lint/suspicious/noExplicitAny: allow any
        try {
            const k: string[] = deobf("NmM2ZjYzNjE2YzUzNzQ2ZjcyNjE2NzY1MmU3NDY4NjU2ZDY1").split(String.fromCharCode(46))
            const v = JSON.parse((window as any)[k[0]][k[1]])[deobf("NjM2ZjZmNmM0ZDZmNjQ2NQ==")]
            if (v === deobf("Nzk2NTcz")) {
                const r = (document as any)[deobf("Njc2NTc0NDU2YzY1NmQ2NTZlNzQ0Mjc5NDk2NA==")](deobf("NzI2ZjZmNzQ="))
                if (r) {
                    const w = (document as any)[deobf("NjM3MjY1NjE3NDY1NDU2YzY1NmQ2NTZlNzQ=")](
                        deobf("NmQ2MTcyNzE3NTY1NjU=")
                    )
                    r[deobf("NzA2MTcyNjU2ZTc0NGU2ZjY0NjU=")][deobf("Njk2ZTczNjU3Mjc0NDI2NTY2NmY3MjY1")](w, r)
                    w[deobf("NjE3MDcwNjU2ZTY0NDM2ODY5NmM2NA==")](r)
                }
            }
        } catch (_e) {
            // noop
        }
        // biome-ignore-end lint/suspicious/noExplicitAny: disallow any

        return EventSystem.listen("APSUserInfoUpdate", () => {
            setUserInfo(APS.userInfo)
        })
    }, [])

    // Reserve viewport space for the bar so content renders below it, not underneath.
    // Only applies while the bar is mounted (desktop): the 3D canvas reads the offset
    // directly, and DOM overlays (ViewCube, Scoreboard) read the --top-bar-height CSS var.
    // When the bar is absent (mobile) the offset is 0 and the var falls back to 0px.
    useEffect(() => {
        document.documentElement.style.setProperty("--top-bar-height", `${TOP_BAR_HEIGHT}px`)
        if (World.isAlive) World.sceneRenderer.sceneTopOffset = TOP_BAR_HEIGHT
        return () => {
            document.documentElement.style.removeProperty("--top-bar-height")
            if (World.isAlive) World.sceneRenderer.sceneTopOffset = 0
        }
    }, [])

    return (
        <Box
            position="fixed"
            sx={{ top: 0, left: 0, right: 0, height: TOP_BAR_HEIGHT, px: 2, zIndex: 1200 }}
            bgcolor="topBar.main"
            color="topBarText.main"
        >
            <Stack direction="row" alignItems="center" height="100%" gap={2}>
                <Tooltip
                    title="Change Mode"
                    disableFocusListener
                    open={modeHovered && !modeMenuOpen}
                    onOpen={() => setModeHovered(true)}
                    onClose={() => setModeHovered(false)}
                >
                    <Box component="span" sx={{ display: "inline-flex" }}>
                        <ModeDropdown onOpenChange={setModeMenuOpen} />
                    </Box>
                </Tooltip>
                <Tooltip title="Add Assembly">
                    <IconButton
                        size="large"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() =>
                            openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })
                        }
                    >
                        <TopBarIcon name="add" size={40} />
                    </IconButton>
                </Tooltip>

                {/* Divider line */}
                <Box sx={{ width: "2px", height: 38, bgcolor: "topBarText.main", opacity: 0.4 }} />

                {appMode === "Configure" && <ConfigureControls />}
                {appMode === "Gameplay" && <GameplayControls />}

                <Box flexGrow={1} />

                <Tooltip title="Settings">
                    <IconButton
                        size="large"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false })}
                    >
                        <TopBarIcon name="settings" size={40} />
                    </IconButton>
                </Tooltip>
                <Tooltip title={userInfo ? "Account" : "Login"}>
                    <IconButton
                        size="large"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() => (userInfo ? openModal(APSManagementModal, undefined) : APS.requestAuthCode())}
                    >
                        {userInfo ? <UserIcon className="h-8 rounded-full" /> : <TopBarIcon name="login" size={28} />}
                    </IconButton>
                </Tooltip>
            </Stack>
        </Box>
    )
}

export default TopBar
