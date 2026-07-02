import { Box, Drawer, MenuItem, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { FaBars } from "react-icons/fa6"
import { IoMdArrowDropdown } from "react-icons/io"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import APSManagementModal from "../modals/APSManagementModal"
import SettingsModal from "../modals/configuring/SettingsModal"
import MultiplayerStartModal from "../modals/MultiplayerStartModal"
import type { ConfigurationType } from "../panels/configuring/assembly-config/ConfigTypes"
import MatchModeConfigPanel from "../panels/configuring/MatchModeConfigPanel"
import DebugPanel from "../panels/DebugPanel"
import DeveloperToolPanel from "../panels/DeveloperToolPanel"
import ImportMirabufPanel from "../panels/mirabuf/ImportMirabufPanel"
import { globalAddToast } from "./GlobalUIControls"
import { IconButton, Select, SynthesisIcons } from "./StyledComponents"
import HUDMenuButton from "./topbar/HUDMenuButton"
import { TOP_BAR_ICON_BUTTON_SX } from "./topbar/topBarConfig"
import { TopBarIcon } from "./topbar/TopBarIcons"
import { assemblyLabel, useConfigureAssembly } from "./topbar/useConfigureAssembly"
import UserIcon from "./UserIcon"

const DRAWER_SX = {
    width: "min(80vw, 320px)",
    boxSizing: "border-box",
    bgcolor: "topBar.main",
    color: "topBarText.main",
    p: 2,
}

// Shared styling for the assembly dropdown inside the drawer.
const DRAWER_SELECT_SX = {
    bgcolor: "surface.main",
    color: "topBarText.main",
    borderRadius: 3,
    height: 44,
    fontSize: 15,
    cursor: "pointer",
    alignItems: "stretch",
    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
    "& .MuiSelect-icon": { color: "topBarText.main", right: 14, pointerEvents: "none" },
} as const

type DrawerView = "root" | "configure"

const MobileHUD: React.FC = () => {
    const { openModal, openPanel } = useUIContext()

    const [open, setOpen] = useState(false)
    const [view, setView] = useState<DrawerView>("root")
    const [userInfo, setUserInfo] = useState(APS.userInfo)

    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectedValue, selectAssemblyById } =
        useConfigureAssembly()

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

    const openMultiplayer = () =>
        openModal(MultiplayerStartModal, {
            startWorldCallback: async (name: string, room?: string) => {
                const isHost = room == null
                const roomId = room ?? Math.random().toString(10).substring(2, 8)
                PreferencesSystem.setGlobalPreference("MultiplayerUsername", name)
                PreferencesSystem.savePreferences()
                const success = await MultiplayerSystem.setup(roomId, name, isHost)
                if (success && isHost) globalAddToast("info", "Room Code", roomId)
                return success
            },
        })

    const rootGrid = (
        <Stack gap={2} sx={{ height: "100%" }}>
            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "repeat(2, 1fr)",
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

                <HUDMenuButton label="Multiplayer" iconName="gp-1" onClick={() => runAction(openMultiplayer)} />

                <HUDMenuButton
                    label="Start Match"
                    iconName="gp-2"
                    onClick={() => runAction(() => openPanel(MatchModeConfigPanel, undefined))}
                />



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
        <Stack gap={2} sx={{ height: "100%" }}>
            <Stack direction="row" alignItems="center" gap={1}>
                <IconButton disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={() => setView("root")}>
                    {SynthesisIcons.LEFT_ARROW_LARGE}
                </IconButton>
                <TopBarIcon name="mode-configure" size={24} />
                <Select
                    displayEmpty
                    value={selectedValue}
                    onChange={e => selectAssemblyById(e.target.value as string)}
                    renderValue={() =>
                        selectedConfigAssembly ? assemblyLabel(selectedConfigAssembly) : "Select an assembly"
                    }
                    IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
                    sx={{ ...DRAWER_SELECT_SX, flexGrow: 1, minWidth: 0 }}
                >
                    {assemblies.length === 0 && (
                        <MenuItem value="" disabled>
                            No assemblies spawned
                        </MenuItem>
                    )}
                    {assemblies.map(assembly => (
                        <MenuItem key={assembly.id} value={assembly.id.toString()}>
                            {assemblyLabel(assembly)}
                        </MenuItem>
                    ))}
                </Select>
            </Stack>

            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "repeat(2, 1fr)",
                    rowGap: { xs: 2, sm: 4 },
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
                        onClick={() => {
                            openConfig(mode)
                            if (selectedConfigAssembly) closeDrawer()
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
                sx={{
                    position: "fixed",
                    top: 12,
                    left: 12,
                    zIndex: theme => theme.zIndex.drawer - 1,
                    bgcolor: "surface.main",
                    color: "topBarText.main",
                    borderRadius: 2,
                    p: 1,
                    "&:hover": { bgcolor: "surface.main" },
                }}
            >
                <FaBars size={24} />
            </IconButton>

            <Drawer anchor="left" open={open} onClose={closeDrawer} PaperProps={{ sx: DRAWER_SX }}>
                {view === "root" ? rootGrid : configureGrid}
            </Drawer>
        </>
    )
}

export default MobileHUD
