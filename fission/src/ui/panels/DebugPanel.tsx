import { Box, Button, Stack, Typography } from "@mui/material"
import type React from "react"
import { useContext } from "react"
import APS from "@/aps/APS"
import MirabufCachingService, {
    backUpFields as hashedMiraFields,
    backUpRobots as hashedMiraRobots,
    MiraType,
} from "@/mirabuf/MirabufLoader"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import { random } from "@/util/Random"
import { globalAddToast } from "../components/GlobalUIControls"
import type { PanelImplProps } from "../components/Panel"
import { UIContext, useUIContext } from "../UIProvider"
import PokerPanel from "./PokerPanel"
import WsViewPanel from "./WsViewPanel"

function ToggleDragMode() {
    const dragSystem = World.dragModeSystem
    if (dragSystem) {
        dragSystem.enabled = !dragSystem.enabled
        const status = dragSystem.enabled ? "enabled" : "disabled"
        globalAddToast("info", "Drag Mode", `Drag mode has been ${status}`)
    }
}

const DebugPanel: React.FC<PanelImplProps<void>> = ({ panel, parent }) => {
    const { openPanel } = useUIContext()
    return (
        <Box
            component="div"
            alignItems="center"
            sx={{
                padding: "0.25rem",
                overflowY: "auto",
                borderRadius: "0.5rem",
            }}
            justifyContent="center"
            textAlign="center"
            minWidth="290px"
        >
            <Stack>
                <Typography variant="h5">Generic</Typography>
                <Button
                    onClick={() => {
                        const toastType = (["info", "warning", "error"] as const)[Math.floor(random() * 3)]
                        globalAddToast(toastType, "This is a test toast to test the toast system")
                    }}
                    className="w-full"
                >
                    Toasts
                </Button>
                <Button onClick={() => openPanel(<PokerPanel />, panel)}>The Poker</Button>
                <Button onClick={ToggleDragMode} className="w-full">
                    Toggle Drag Mode
                </Button>
                <Button onClick={() => PreferencesSystem.clearPreferences()} className="w-full">
                    Clear Preferences
                </Button>

                <Typography variant="h5">Autodesk Platform Services</Typography>
                <Button
                    onClick={async () =>
                        await APS.isSignedIn() && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
                    }
                    className="w-full"
                >
                    Refresh APS Token
                </Button>
                <Button
                    onClick={async () => {
                        if (await APS.isSignedIn()) {
                            APS.setExpiresAt(Date.now())
                            APS.getAuthOrLogin()
                        }
                    }}
                    className="w-full"
                >
                    Expire APS Token
                </Button>

                <Typography variant="h5">Caching Services</Typography>
                <Button
                    onClick={() => {
                        console.log(MirabufCachingService.getCacheMap(MiraType.ROBOT))
                        console.log(MirabufCachingService.getCacheMap(MiraType.FIELD))
                        console.log(hashedMiraRobots)
                        console.log(hashedMiraFields)
                    }}
                    className="w-full"
                >
                    Print Mira Maps
                </Button>
                <Button onClick={() => MirabufCachingService.removeAll()} className="w-full">
                    Clear Mira Cache
                </Button>

                <Typography variant="h5">Code Simulation</Typography>
                <Button onClick={() => openPanel(<WsViewPanel />, panel)} className="w-full">
                    WS Viewer
                </Button>
            </Stack>
        </Box>
    )
}

export default DebugPanel
