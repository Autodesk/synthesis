import { Box, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useEffect, useRef, useState } from "react"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World.ts"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useIsTouchDevice } from "@/ui/helpers/useIsMobile"
import { deobf } from "@/util/Utility"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import APSManagementModal from "@/modals/APSManagementModal"
import SettingsModal from "@/modals/configuring/SettingsModal"
import CameraSelectionPanel from "@/panels/configuring/CameraSelectionPanel"
import DeveloperToolPanel from "@/panels/DeveloperToolPanel"
import DebugPanel from "@/panels/DebugPanel"
import LibraryModal from "@/ui/modals/mirabuf/LibraryModal"
import { setAddToast, setCloseModal, setOpenModal, setOpenPanel } from "@/ui/components/GlobalUIControls"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { AssemblySelect } from "@/ui/components/topbar/AssemblySelect"
import CodesimControls from "@/ui/components/topbar/CodesimControls"
import CodeConnectionIndicator from "@/ui/components/topbar/CodeConnectionIndicator"
import ConfigureControls from "@/ui/components/topbar/ConfigureControls"
import GameplayControls from "@/ui/components/topbar/GameplayControls"
import ModeDropdown from "@/ui/components/topbar/ModeDropdown"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TOP_BAR_DIVIDER_SX, TOP_BAR_GAP, TOP_BAR_GLYPH_SX, TOP_BAR_HEIGHT } from "@/ui/components/topbar/TopBarConfig"
import { TopBarFitProvider } from "@/ui/components/topbar/TopBarFitProvider"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"
import { useAssemblySelection } from "@/ui/components/topbar/UseConfigureAssembly"
import UserIcon from "@/ui/components/UserIcon"
import { hasSimBrain } from "@/systems/simulation/wpilib_brain/WPILibState"
import { useTourAnchor } from "@/ui/tour/TourProviderHelpers"

const TUTORIALS_URL = "https://synthesis.autodesk.com/tutorials"

export const DragModeButton: React.FC = () => {
    const [enabled, setEnabled] = useState(World.isAlive && World.dragModeSystem.enabled)

    useEffect(() => EventSystem.listen("SetDragModeEvent", e => setEnabled(e.enabled)), [])

    const toggleDragMode = () => EventSystem.dispatch("SetDragModeEvent", { enabled: !enabled })

    return (
        <TopBarButton
            label={enabled ? "Disable Drag Mode" : "Drag Mode"}
            active={enabled}
            icon={
                <Box sx={TOP_BAR_GLYPH_SX}>
                    <SynthesisIcons.HAND />
                </Box>
            }
            onClick={toggleDragMode}
        />
    )
}

const TopBar: React.FC = () => {
    const { openModal, openPanel, togglePanel, closeModal, addToast } = useUIContext()
    const { appMode } = useStateContext()
    const isTouchDevice = useIsTouchDevice()
    const { assemblies, selectedAssembly, selectAssemblyById } = useAssemblySelection()

    const addAssemblyRef = useTourAnchor("add-assembly")
    const modeDropdownRef = useTourAnchor("mode-dropdown")
    // AssemblySelect moved up from ConfigureControls in the MainHUD redesign, so its tour anchor lives here now.
    const assemblySelectRef = useTourAnchor("configure-assembly-select")

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)
    setCloseModal(closeModal)

    const [userInfo, setUserInfo] = useState(APS.userInfo)
    const [modeHovered, setModeHovered] = useState(false)
    const [modeMenuOpen, setModeMenuOpen] = useState(false)
    const [touchControlsVisible, setTouchControlsVisible] = useState(() =>
        PreferencesSystem.getUserPreference("TouchControls")
    )

    const rowRef = useRef<HTMLDivElement>(null)
    const spacerRef = useRef<HTMLDivElement>(null)

    useEffect(
        () =>
            EventSystem.listen("TouchControlsVisibilityChangedEvent", ({ visible }) =>
                setTouchControlsVisible(visible)
            ),
        []
    )

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
            <Stack ref={rowRef} direction="row" alignItems="center" height="100%" gap={TOP_BAR_GAP}>
                <TopBarFitProvider rowRef={rowRef} spacerRef={spacerRef}>
                    <Tooltip
                        title="Change Mode"
                        open={modeHovered && !modeMenuOpen}
                        disableHoverListener
                        disableFocusListener
                        disableTouchListener
                        disableInteractive
                    >
                        <Box
                            ref={modeDropdownRef}
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

                    <TopBarButton
                        label="Add Assembly"
                        icon={<TopBarIcon name="add" size={30} />}
                        onClick={() => openModal(LibraryModal, undefined)}
                        anchorRef={addAssemblyRef}
                    />

                    <Box sx={TOP_BAR_DIVIDER_SX} />

                    {(appMode === "Configure" || appMode === "Codesim") && (
                        <Box ref={assemblySelectRef} component="span" sx={{ display: "inline-flex" }}>
                            <AssemblySelect
                                assemblies={assemblies}
                                selectedAssembly={selectedAssembly}
                                onSelect={selectAssemblyById}
                                sx={{ borderRadius: 1, height: 34, minWidth: 195, fontSize: 12 }}
                            />
                        </Box>
                    )}

                    {appMode === "Configure" && <ConfigureControls selectedAssembly={selectedAssembly} />}
                    {appMode === "Codesim" && <CodesimControls selectedAssembly={selectedAssembly} />}
                    {appMode === "Gameplay" && <GameplayControls />}
                    <Box ref={spacerRef} flexGrow={1} />

                    {hasSimBrain() && (
                        <>
                            <CodeConnectionIndicator />
                            <Box sx={TOP_BAR_DIVIDER_SX} />
                        </>
                    )}

                    {import.meta.env.DEV && (
                        <>
                            <TopBarButton
                                label="Developer Tool"
                                icon={
                                    <Box sx={TOP_BAR_GLYPH_SX}>
                                        <SynthesisIcons.CODE_SQUARE />
                                    </Box>
                                }
                                onClick={() => togglePanel(DeveloperToolPanel, undefined)}
                            />
                            <TopBarButton
                                label="Debug Tools"
                                icon={
                                    <Box sx={TOP_BAR_GLYPH_SX}>
                                        <SynthesisIcons.BUG />
                                    </Box>
                                }
                                onClick={() => togglePanel(DebugPanel, undefined)}
                            />
                        </>
                    )}
                    {isTouchDevice && (
                        <TopBarButton
                            label={touchControlsVisible ? "Hide Joysticks" : "Show Joysticks"}
                            active={touchControlsVisible}
                            icon={
                                <Box sx={TOP_BAR_GLYPH_SX}>
                                    <SynthesisIcons.GAMEPAD />
                                </Box>
                            }
                            onClick={() => EventSystem.dispatch("ToggleTouchControlsVisibilityEvent")}
                        />
                    )}
                    <DragModeButton />
                    <TopBarButton
                        label="Configure Camera"
                        icon={
                            <Box sx={TOP_BAR_GLYPH_SX}>
                                <SynthesisIcons.CAMERA />
                            </Box>
                        }
                        onClick={() => togglePanel(CameraSelectionPanel, undefined)}
                    />
                    <TopBarButton
                        label="Settings"
                        icon={
                            <Box sx={TOP_BAR_GLYPH_SX}>
                                <SynthesisIcons.GEAR />
                            </Box>
                        }
                        onClick={() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false })}
                    />
                    <TopBarButton
                        label="Tutorials"
                        icon={
                            <Box sx={TOP_BAR_GLYPH_SX}>
                                <SynthesisIcons.QUESTION />
                            </Box>
                        }
                        onClick={() => window.open(TUTORIALS_URL, "_blank", "noopener,noreferrer")}
                    />
                    <TopBarButton
                        label={userInfo ? "Account" : "Login"}
                        icon={
                            userInfo ? <UserIcon className="h-6 rounded-full" /> : <TopBarIcon name="login" size={30} />
                        }
                        onClick={() => (userInfo ? openModal(APSManagementModal, undefined) : APS.requestAuthCode())}
                    />
                </TopBarFitProvider>
            </Stack>
        </Box>
    )
}

export default TopBar
