import { Box, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import APS from "@/aps/APS"
import EventSystem from "@/systems/EventSystem.ts"
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
import ModeDropdown from "./topbar/ModeDropdown"
import { TopBarIcon } from "./topbar/TopBarIcons"
import UserIcon from "./UserIcon"

const TopBar: React.FC = () => {
    const { openModal, openPanel, addToast } = useUIContext()
    const { appMode } = useStateContext()

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)

    const [userInfo, setUserInfo] = useState(APS.userInfo)

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

    return (
        <Box
            position="fixed"
            sx={{ top: 0, left: 0, right: 0, height: 54, px: 1, zIndex: 1200 }}
            bgcolor="topBar.main"
            color="topBarText.main"
        >
            <Stack direction="row" alignItems="center" height="100%" gap={1}>
                <ModeDropdown />
                <IconButton
                    sx={{ color: "topBarText.main" }}
                    onClick={() => openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })}
                >
                    <TopBarIcon name="add" size={22} />
                </IconButton>
                <Box sx={{ width: "1px", height: 34, bgcolor: "topBarText.main", opacity: 0.4 }} />

                {appMode === "Configure" && <ConfigureControls />}

                <Box flexGrow={1} />

                <IconButton
                    sx={{ color: "topBarText.main" }}
                    onClick={() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false })}
                >
                    <TopBarIcon name="settings" size={22} />
                </IconButton>
                <IconButton
                    sx={{ color: "topBarText.main" }}
                    onClick={() => (userInfo ? openModal(APSManagementModal, undefined) : APS.requestAuthCode())}
                >
                    {userInfo ? <UserIcon className="h-6 rounded-full" /> : <TopBarIcon name="login" size={22} />}
                </IconButton>
            </Stack>
        </Box>
    )
}

export default TopBar
