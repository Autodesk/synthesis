import { Box, Drawer, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { FaBars } from "react-icons/fa6"
import APS from "@/aps/APS.ts"
import EventSystem from "@/systems/EventSystem.ts"
import { useIsTouchDevice } from "@/ui/helpers/useIsMobile.ts"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import APSManagementModal from "@/modals/APSManagementModal.tsx"
import SettingsModal from "@/modals/configuring/SettingsModal.tsx"
import type { ConfigurationType } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import ImportMirabufPanel from "@/panels/mirabuf/ImportMirabufPanel.tsx"
import { globalOpenModal, setAddToast, setOpenModal, setOpenPanel } from "../GlobalUIControls.ts"
import { IconButton, SynthesisIcons } from "../StyledComponents.tsx"
import { AssemblySelect } from "../topbar/AssemblySelect.tsx"
import { ConfigureIcon } from "../topbar/ConfigureIcon.tsx"
import { HUD_MENU_ICON_SIZE, HUDMenuButton } from "../topbar/HUDMenuButton.tsx"
import { TOP_BAR_ICON_BUTTON_SX } from "../topbar/TopBarConfig.ts"
import { TopBarIcon } from "../topbar/TopBarIcons.tsx"
import { useAssemblySelection, useConfigureAssembly } from "../topbar/UseConfigureAssembly.ts"
import UserIcon from "../UserIcon.tsx"
import MultiplayerStartModal from "@/modals/multiplayer/MultiplayerStartModal.tsx"

const DRAWER_SX = {
    width: "min(92vw, 520px)",
    boxSizing: "border-box",
    bgcolor: "topBar.main",
    color: "topBarText.main",
    p: 2,
}

type DrawerView = "root" | "configure"

const MobileHUD: React.FC = () => {
    const { openModal, openPanel, addToast, blockState } = useUIContext()

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)

    const [open, setOpen] = useState(false)
    const [view, setView] = useState<DrawerView>("root")
    const [userInfo, setUserInfo] = useState(APS.userInfo)
    const isTouchDevice = useIsTouchDevice()

    const { assemblies, selectedAssembly, selectAssemblyById } = useAssemblySelection()
    const { configureButtons, openConfig, disabledMessage } = useConfigureAssembly(selectedAssembly)

    useEffect(() => EventSystem.listen("APSUserInfoUpdate", () => setUserInfo(APS.userInfo)), [])

    const closeDrawer = () => {
        setOpen(false)
        setView("root")
    }

    // Run an action that opens a panel/modal, closing the drawer first so the
    // opened surface isn't hidden behind it.
    const runAction = (fn: () => void) => {
        closeDrawer()
        fn()
    }

    const openMultiplayer = () => globalOpenModal(MultiplayerStartModal, undefined)

    const rootGrid = (
        <Stack gap={2} sx={{ minHeight: "100%" }}>
            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "repeat(3, 1fr)",
                    rowGap: { xs: 2, sm: 4 },
                    columnGap: { xs: 0.5, sm: 1 },
                    flexGrow: 1,
                    alignContent: "space-evenly",
                }}
            >
                <HUDMenuButton
                    label="Add Assembly"
                    iconName="add"
                    onClick={() =>
                        runAction(() =>
                            openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })
                        )
                    }
                />

                <HUDMenuButton label="Configure" iconName="mode-configure" onClick={() => setView("configure")} />

                <HUDMenuButton
                    label="Multiplayer"
                    iconName="gp-multiplayer"
                    onClick={() => runAction(openMultiplayer)}
                />

                {isTouchDevice && (
                    <HUDMenuButton
                        label="Toggle Joysticks"
                        icon={<SynthesisIcons.GAMEPAD />}
                        onClick={() => runAction(() => EventSystem.dispatch("ToggleTouchControlsVisibilityEvent"))}
                    />
                )}

                <HUDMenuButton
                    label="Settings"
                    iconName="settings"
                    onClick={() =>
                        runAction(() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false }))
                    }
                />
                <HUDMenuButton
                    label={userInfo ? "Account" : "Login"}
                    iconName={userInfo ? undefined : "login"}
                    icon={userInfo ? <UserIcon className="h-8 rounded-full" /> : undefined}
                    onClick={() =>
                        userInfo ? runAction(() => openModal(APSManagementModal, undefined)) : APS.requestAuthCode()
                    }
                />
            </Box>
        </Stack>
    )

    const configureGrid = (
        <Stack gap={2} sx={{ minHeight: "100%" }}>
            <Stack direction="row" alignItems="center" gap={1}>
                <IconButton disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={() => setView("root")}>
                    <SynthesisIcons.LEFT_ARROW_LARGE />
                </IconButton>
                <TopBarIcon name="mode-configure" size={24} />
                <AssemblySelect
                    assemblies={assemblies}
                    selectedAssembly={selectedAssembly}
                    onSelect={selectAssemblyById}
                    sx={{ borderRadius: 3, height: 44, fontSize: 15, flexGrow: 1, minWidth: 0 }}
                />
            </Stack>

            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "repeat(3, 1fr)",
                    rowGap: "clamp(4px, 2vh, 32px)",
                    columnGap: { xs: 0.5, sm: 1 },
                    flexGrow: 1,
                    alignContent: "space-evenly",
                }}
            >
                {configureButtons.map(({ icon, label, mode }) => (
                    <HUDMenuButton
                        key={label}
                        label={label}
                        icon={<ConfigureIcon icon={icon} size={HUD_MENU_ICON_SIZE} />}
                        disabled={disabledMessage != null}
                        disabledTooltip={disabledMessage}
                        onClick={() => {
                            openConfig(mode)
                            if (selectedAssembly) closeDrawer()
                        }}
                    />
                ))}
            </Box>
        </Stack>
    )

    return (
        <>
            <IconButton
                aria-label="Open menu"
                disableRipple
                onClick={() => setOpen(true)}
                disabled={blockState.blocked}
                sx={{
                    position: "fixed",
                    top: 12,
                    left: 12,
                    zIndex: theme => theme.zIndex.drawer - 1,
                    bgcolor: "surface.main",
                    color: "topBarText.main",
                    borderRadius: 2,
                    p: 1.25,
                    "&:hover": { bgcolor: "surface.main" },
                    "&:focus, &:focus-visible": { outline: "none" },
                }}
            >
                <FaBars size={32} />
            </IconButton>

            <Drawer anchor="left" open={open} onClose={closeDrawer} PaperProps={{ sx: DRAWER_SX }}>
                {view === "root" ? rootGrid : configureGrid}
            </Drawer>
        </>
    )
}

export default MobileHUD
