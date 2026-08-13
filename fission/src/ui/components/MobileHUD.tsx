import { Box, Drawer, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { FaBars } from "react-icons/fa6"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import { useIsTouchDevice } from "@/ui/helpers/useIsMobile"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import APSManagementModal from "../modals/APSManagementModal"
import SettingsModal from "../modals/configuring/SettingsModal"
import MultiplayerStartModal from "../modals/MultiplayerStartModal"
import { startMultiplayerWorld } from "../helpers/StartMultiplayerWorld"
import LibraryModal from "../modals/mirabuf/LibraryModal"
import { setAddToast, setOpenModal, setOpenPanel } from "./GlobalUIControls"
import { IconButton, SynthesisIcons } from "./StyledComponents"
import { AssemblySelect } from "./topbar/AssemblySelect"
import { HUDMenuButton } from "./topbar/HUDMenuButton"
import { TOP_BAR_ICON_BUTTON_SX } from "./topbar/TopBarConfig"
import { TopBarIcon } from "./topbar/TopBarIcons"
import { useAssemblySelection, useConfigureAssembly } from "./topbar/UseConfigureAssembly"
import UserIcon from "./UserIcon"

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
    const { configureButtons, openConfig } = useConfigureAssembly(selectedAssembly)

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

    const openMultiplayer = () => openModal(MultiplayerStartModal, { startWorldCallback: startMultiplayerWorld })

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
                    onClick={() => runAction(() => openModal(LibraryModal, undefined))}
                />

                <HUDMenuButton label="Configure" iconName="mode-configure" onClick={() => setView("configure")} />

                <HUDMenuButton label="Multiplayer" iconName="gp-1" onClick={() => runAction(openMultiplayer)} />

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
                {configureButtons.map(({ name, label, mode }) => (
                    <HUDMenuButton
                        key={label}
                        label={label}
                        iconName={name}
                        disabled={!selectedAssembly}
                        disabledTooltip="Spawn an assembly first"
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
