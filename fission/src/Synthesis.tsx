import { AnimatePresence } from "framer-motion"
import { SnackbarProvider } from "notistack"
import Slide from "@mui/material/Slide"
import { useCallback, useEffect, useRef, useState } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import MainHUD from "@/components/MainHUD"
import MultiplayerHUD from "@/components/MultiplayerHUD.tsx"
import Scene from "@/components/Scene.tsx"
import MultiplayerStartModal from "@/modals/MultiplayerStartModal.tsx"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import World from "@/systems/World.ts"
import { UIRenderer } from "@/ui/UIRenderer.tsx"
import PreferencesSystem from "./systems/preferences/PreferencesSystem.ts"
import AnalyticsConsent from "./ui/components/AnalyticsConsent.tsx"
import ContextMenu from "./ui/components/ContextMenu.tsx"
import DragModeIndicator from "./ui/components/DragModeIndicator.tsx"
import GlobalUIComponent from "./ui/components/GlobalUIComponent.tsx"
import { globalOpenModal } from "./ui/components/GlobalUIControls.ts"
import ProgressNotifications from "./ui/components/ProgressNotification.tsx"
import SceneOverlay from "./ui/components/SceneOverlay.tsx"
import WPILibConnectionStatus from "./ui/components/WPILibConnectionStatus.tsx"
import MainMenuModal from "./ui/modals/MainMenuModal.tsx"
import { StateProvider } from "./ui/StateProvider.tsx"
import { ThemeProvider } from "./ui/ThemeProvider.tsx"
import { UIProvider } from "./ui/UIProvider.tsx"
import CommandPalette from "@/ui/components/CommandPalette.tsx"

function Synthesis() {
    const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true)

    const mainLoopHandle = useRef(0)
    const startMainLoop = async () => {
        await World.initWorld()
        if (!PreferencesSystem.getGlobalPreference("ReportAnalytics") && !import.meta.env.DEV) {
            setConsentPopupDisable(false)
        }

        const mainLoop = () => {
            mainLoopHandle.current = requestAnimationFrame(mainLoop)
            World.updateWorld()
        }

        mainLoop()
    }
    useEffect(() => {
        const urlParams = new URLSearchParams(document.location.search)
        if (urlParams.has("code")) {
            window.opener.convertAuthToken(urlParams.get("code"))
            window.close()
            return
        }

        globalOpenModal(MainMenuModal, {
            startSingleplayerCallback: async () => await startMainLoop(),
            startMultiplayerCallback: () => {
                globalOpenModal(MultiplayerStartModal, {
                    startWorldCallback: async (name, room) => {
                        const isHost = room == null
                        if (room == null) {
                            room = Math.random().toString(10).substring(2, 8)
                        }
                        PreferencesSystem.setGlobalPreference("MultiplayerUsername", name)
                        PreferencesSystem.savePreferences()
                        const success = await MultiplayerSystem.setup(room, name, isHost)
                        if (success) {
                            if (isHost) {
                                globalAddToast("info", "Room Code", room)
                            }
                            await startMainLoop()
                            return true
                        }
                        return false
                    },
                })
            },
        })
        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle.current)
            World.destroyWorld()
            World.multiplayerSystem?.destroy()
            // World.SceneRenderer.RemoveAllSceneObjects();
        }
    }, [])

    const onConsent = useCallback(() => {
        setConsentPopupDisable(true)
        PreferencesSystem.setGlobalPreference("ReportAnalytics", true)
        PreferencesSystem.savePreferences()
    }, [])

    const onDisableConsent = useCallback(() => {
        setConsentPopupDisable(true)
    }, [])

    return (
        <AnimatePresence key={"animate-presence"}>
            <ThemeProvider>
                <SnackbarProvider
                    maxSnack={5}
                    anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
                    TransitionComponent={Slide}
                >
                    <StateProvider>
                        <UIProvider>
                            <GlobalUIComponent />
                            <Scene useStats={import.meta.env.DEV} key="scene-in-toast-provider" />
                            <SceneOverlay />
                            <ContextMenu />
                            <MultiplayerHUD />
                            <MainHUD key={"main-hud"} />
                            <UIRenderer />
                            <CommandPalette />
                            <ProgressNotifications key={"progress-notifications"} />
                            <WPILibConnectionStatus />
                            <DragModeIndicator />

                            {!consentPopupDisable && (
                                <AnalyticsConsent onClose={onDisableConsent} onConsent={onConsent} />
                            )}
                        </UIProvider>
                    </StateProvider>
                </SnackbarProvider>
            </ThemeProvider>
        </AnimatePresence>
    )
}

export default Synthesis
