import { Box, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useIsTouchDevice } from "@/ui/helpers/useIsMobile"
import { deobf } from "@/util/Utility"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import APSManagementModal from "@/modals/APSManagementModal"
import SettingsModal from "@/modals/configuring/SettingsModal"
import type { ConfigurationType } from "@/panels/configuring/assembly-config/ConfigTypes"
import CameraSelectionPanel from "@/panels/configuring/CameraSelectionPanel"
import DeveloperToolPanel from "@/panels/DeveloperToolPanel"
import DebugPanel from "@/panels/DebugPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { setAddToast, setOpenModal, setOpenPanel } from "./GlobalUIControls"
import { IconButton, SynthesisIcons } from "./StyledComponents"
import ConfigureControls from "./topbar/ConfigureControls"
import GameplayControls from "./topbar/GameplayControls"
import ModeDropdown from "./topbar/ModeDropdown"
import { TOP_BAR_HEIGHT, TOP_BAR_ICON_BUTTON_SX } from "./topbar/TopBarConfig"
import { TopBarIcon } from "./topbar/TopBarIcons"
import UserIcon from "./UserIcon"

const TopBar: React.FC = () => {
    const { openModal, openPanel, addToast } = useUIContext()
    const { appMode } = useStateContext()
    const isTouchDevice = useIsTouchDevice()

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)

    const [userInfo, setUserInfo] = useState(APS.userInfo)
    const [modeHovered, setModeHovered] = useState(false)
    const [modeMenuOpen, setModeMenuOpen] = useState(false)

    useEffect(() => {
        // biome-ignore-start lint/suspicious/noExplicitAny: allow any for window and document access
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
        } catch (_e) {}
        // biome-ignore-end lint/suspicious/noExplicitAny: disallow any

        return EventSystem.listen("APSUserInfoUpdate", () => {
            setUserInfo(APS.userInfo)
        })
    }, [])

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
            sx={{ top: 0, left: 0, right: 0, height: TOP_BAR_HEIGHT, px: 1.5, zIndex: 1200 }}
            bgcolor="topBar.main"
            color="topBarText.main"
        >
            <Stack direction="row" alignItems="center" height="100%" gap={1.5}>
                <Tooltip
                    title="Change Mode"
                    open={modeHovered && !modeMenuOpen}
                    disableHoverListener
                    disableFocusListener
                    disableTouchListener
                    disableInteractive
                >
                    <Box
                        component="span"
                        sx={{ display: "inline-flex" }}
                        onMouseEnter={() => setModeHovered(true)}
                        onMouseLeave={() => setModeHovered(false)}
                    >
                        <ModeDropdown
                            onOpenChange={open => {
                                setModeMenuOpen(open)
                                // Reset hover whenever the menu toggles so the tooltip starts
                                // hidden after the menu closes (a fresh hover re-opens it).
                                setModeHovered(false)
                            }}
                        />
                    </Box>
                </Tooltip>
                <Tooltip title="Add Assembly">
                    <IconButton
                        size="medium"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() =>
                            openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })
                        }
                    >
                        <TopBarIcon name="add" size={30} />
                    </IconButton>
                </Tooltip>

                {/* Divider line */}
                <Box sx={{ width: "2px", height: 28, bgcolor: "topBarText.main", opacity: 0.4 }} />

                {appMode === "Configure" && <ConfigureControls />}
                {appMode === "Gameplay" && <GameplayControls />}

                <Box flexGrow={1} />

                {import.meta.env.DEV && (
                    <>
                        <Tooltip title="Developer Tool">
                            <IconButton
                                size="medium"
                                disableRipple
                                sx={TOP_BAR_ICON_BUTTON_SX}
                                onClick={() => openPanel(DeveloperToolPanel, undefined)}
                            >
                                {/* Box sets the em-square so the icon scales to 26 px;
                                    color inherits from TOP_BAR_ICON_BUTTON_SX → topBarText.main */}
                                <Box sx={{ fontSize: 26, display: "flex" }}>
                                    <SynthesisIcons.CODE_SQUARE />
                                </Box>
                            </IconButton>
                        </Tooltip>
                        <Tooltip title="Debug Tools">
                            <IconButton
                                size="medium"
                                disableRipple
                                sx={TOP_BAR_ICON_BUTTON_SX}
                                onClick={() => openPanel(DebugPanel, undefined)}
                            >
                                <Box sx={{ fontSize: 26, display: "flex" }}>
                                    <SynthesisIcons.BUG />
                                </Box>
                            </IconButton>
                        </Tooltip>
                    </>
                )}

                {isTouchDevice && (
                    <Tooltip title="Toggle Joysticks">
                        <IconButton
                            size="medium"
                            disableRipple
                            sx={TOP_BAR_ICON_BUTTON_SX}
                            onClick={() => EventSystem.dispatch("ToggleTouchControlsVisibilityEvent")}
                        >
                            <Box sx={{ fontSize: 26, display: "flex" }}>
                                <SynthesisIcons.GAMEPAD />
                            </Box>
                        </IconButton>
                    </Tooltip>
                )}

                <Tooltip title="Configure Camera">
                    <IconButton
                        size="medium"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() => openPanel(CameraSelectionPanel, undefined)}
                    >
                        <Box sx={{ fontSize: 26, display: "flex" }}>
                            <SynthesisIcons.CAMERA />
                        </Box>
                    </IconButton>
                </Tooltip>

                <Tooltip title="Settings">
                    <IconButton
                        size="medium"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false })}
                    >
                        <TopBarIcon name="settings" size={30} />
                    </IconButton>
                </Tooltip>
                <Tooltip title={userInfo ? "Account" : "Login"}>
                    <IconButton
                        size="medium"
                        disableRipple
                        sx={TOP_BAR_ICON_BUTTON_SX}
                        onClick={() => (userInfo ? openModal(APSManagementModal, undefined) : APS.requestAuthCode())}
                    >
                        {userInfo ? <UserIcon className="h-6 rounded-full" /> : <TopBarIcon name="login" size={30} />}
                    </IconButton>
                </Tooltip>
            </Stack>
        </Box>
    )
}

export default TopBar
