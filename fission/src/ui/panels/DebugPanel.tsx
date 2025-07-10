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
import { Random } from "@/util/Random"
import { Global_AddToast } from "../components/GlobalUIControls"
import type { PanelImplProps } from "../components/Panel"
import { UIContext } from "../UIProvider"
import PokerPanel from "./PokerPanel"
import WsViewPanel from "./WsViewPanel"

function ToggleDragMode() {
    const dragSystem = World.DragModeSystem
    if (dragSystem) {
        dragSystem.enabled = !dragSystem.enabled
        const status = dragSystem.enabled ? "enabled" : "disabled"
        Global_AddToast?.<"info">("Drag Mode", `Drag mode has been ${status}`)
    }
}

const DebugPanel: React.FC<PanelImplProps> = ({ panel, parent }) => {
    const { openPanel } = useContext(UIContext)
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
                        const type = (["info", "warning", "error"] as const)[Math.floor(Random() * 3)]
                        Global_AddToast?.<typeof type>(type, "This is a test toast to test the toast system")
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
                        APS.isSignedIn() && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
                    }
                    className="w-full"
                >
                    Refresh APS Token
                </Button>
                <Button
                    onClick={() => {
                        if (APS.isSignedIn()) {
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
                        console.log(MirabufCachingService.GetCacheMap(MiraType.ROBOT))
                        console.log(MirabufCachingService.GetCacheMap(MiraType.FIELD))
                        console.log(hashedMiraRobots)
                        console.log(hashedMiraFields)
                    }}
                    className="w-full"
                >
                    Print Mira Maps
                </Button>
                <Button onClick={() => MirabufCachingService.RemoveAll()} className="w-full">
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
