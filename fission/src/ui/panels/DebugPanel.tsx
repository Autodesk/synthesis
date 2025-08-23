import { Box, Stack } from "@mui/material"
import type React from "react"
import { useEffect } from "react"
import APS from "@/aps/APS"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import { random } from "@/util/Random"
import { globalAddToast, globalOpenPanel } from "../components/GlobalUIControls"
import Label from "../components/Label"
import type { PanelImplProps } from "../components/Panel"
import { Button } from "../components/StyledComponents"
import { useUIContext } from "../helpers/UIProviderHelpers"
import PokerPanel from "./PokerPanel"
import WsViewPanel from "./WsViewPanel"
import CommandRegistry from "@/ui/components/CommandRegistry"

// Register command: Open Debug Panel (module-scope side effect)
CommandRegistry.get().registerCommand({
    id: "open-debug-panel",
    label: "Open Debug Panel",
    description: "Open the Debug tools panel.",
    keywords: ["panel", "debug"],
    perform: () => import("./DebugPanel").then(m => globalOpenPanel(m.default, undefined)),
})

function toggleDragMode() {
    const dragSystem = World.dragModeSystem
    if (dragSystem) {
        dragSystem.enabled = !dragSystem.enabled
        const status = dragSystem.enabled ? "enabled" : "disabled"
        globalAddToast("info", "Drag Mode", `Drag mode has been ${status}`)
    }
}

const DebugPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { openPanel, openModal, configureScreen } = useUIContext()

    useEffect(() => {
        configureScreen(panel!, { title: "Debug Tools", hideAccept: true, cancelText: "Close" }, {})
    }, [configureScreen, panel])

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
            <Stack gap={1}>
                <Label size="sm">Generic</Label>
                <Button
                    onClick={() => {
                        const toastType = (["info", "warning", "error"] as const)[Math.floor(random() * 3)]
                        globalAddToast(toastType, "This is a test toast to test the toast system")
                        globalAddToast(
                            toastType,
                            "This is a test toast to test the toast system",
                            "with multiple",
                            "arguments"
                        )
                    }}
                    className="w-full"
                >
                    Toasts
                </Button>
                <Button onClick={() => openPanel(PokerPanel, undefined, panel)}>The Poker</Button>
                <Button onClick={toggleDragMode} className="w-full">
                    Toggle Drag Mode
                </Button>
                <Button onClick={() => PreferencesSystem.clearPreferences()} className="w-full">
                    Clear Preferences
                </Button>
                <Button
                    onClick={() => {
                        openModal(
                            ConfirmModal,
                            {
                                message:
                                    "Are you sure you want to clear all preferences and cached data? This cannot be undone.",
                            },
                            panel,
                            {
                                title: "Clear All Data",
                                acceptText: "Clear & Reload",
                                cancelText: "Cancel",
                                onAccept: async () => {
                                    window.localStorage.clear()
                                    sessionStorage.clear()
                                    await navigator.storage
                                        .getDirectory()
                                        .then(async root => {
                                            for await (const key of root.keys()) {
                                                await root.removeEntry(key, { recursive: true })
                                            }
                                        })
                                        .catch(() => {
                                            console.warn("couldn't empty opfs")
                                        })
                                    window.location.reload()
                                    console.log("All data cleared")
                                },
                            }
                        )
                    }}
                    className="w-full"
                >
                    Clear All Data
                </Button>

                <Label size="sm">Autodesk Platform Services</Label>
                <Button
                    onClick={async () =>
                        (await APS.isSignedIn()) && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
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

                <Label size="sm">Caching Services</Label>
                <Button
                    onClick={() => {
                        console.log(MirabufCachingService.getAll(MiraType.ROBOT))
                    }}
                    className="w-full"
                >
                    Print Mira Maps
                </Button>
                <Button onClick={() => MirabufCachingService.removeAll()} className="w-full">
                    Clear Mira Cache
                </Button>

                <Label size="sm">Code Simulation</Label>
                <Button onClick={() => openPanel(WsViewPanel, undefined, panel)} className="w-full">
                    WS Viewer
                </Button>
            </Stack>
        </Box>
    )
}

export default DebugPanel
