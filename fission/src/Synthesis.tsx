import { AnimatePresence } from "framer-motion"
import { SnackbarProvider } from "notistack"
import Slide from "@mui/material/Slide"
import { useCallback, useEffect, useRef, useState } from "react"
import MainHUD from "@/components/MainHUD"
import MultiplayerHUD from "@/components/MultiplayerHUD.tsx"
import Scene from "@/components/Scene.tsx"
import MultiplayerStartModal, { MultiplayerInitProps } from "@/modals/MultiplayerStartModal.tsx"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import World from "@/systems/World.ts"
import { UIRenderer } from "@/ui/UIRenderer.tsx"
import PreferencesSystem from "./systems/preferences/PreferencesSystem.ts"
import AnalyticsConsent from "./ui/components/AnalyticsConsent.tsx"
import ContextMenu from "./ui/components/ContextMenu.tsx"
import DragModeIndicator from "./ui/components/DragModeIndicator.tsx"
import { globalOpenModal } from "./ui/components/GlobalUIControls.ts"
import ProgressNotifications from "./ui/components/ProgressNotification.tsx"
import SceneOverlay from "./ui/components/SceneOverlay.tsx"
import TouchControls from "./ui/components/TouchControls.tsx"
import WPILibConnectionStatus from "./ui/components/WPILibConnectionStatus.tsx"
import MainMenuModal from "./ui/modals/MainMenuModal.tsx"
import { StateProvider } from "./ui/StateProvider.tsx"
import { ThemeProvider } from "./ui/ThemeProvider.tsx"
import { UIProvider } from "./ui/UIProvider.tsx"
import CommandPalette from "@/ui/components/CommandPalette.tsx"
import SessionStorage, { applyAutoToast } from "@/util/SessionStorage.ts"

function Synthesis() {
    const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true)

    const mainLoopHandle = useRef(0)
    const startMainLoop = useCallback(async () => {
        await World.initWorld()
        if (!PreferencesSystem.getUserPreference("ReportAnalytics") && !import.meta.env.DEV) {
            setConsentPopupDisable(false)
        }

        const mainLoop = () => {
            mainLoopHandle.current = requestAnimationFrame(mainLoop)
            World.updateWorld()
        }

        mainLoop()
    }, [])

    const startWorldCallback = useCallback(
        async (info: MultiplayerInitProps) => {
            PreferencesSystem.setUserPreference("MultiplayerUsername", info.displayName)
            PreferencesSystem.savePreferences()
            const success = await MultiplayerSystem.setup(info.url, info.roomId ?? "create", info.displayName)
            if (success) {
                // if (isHost) {
                //     globalAddToast("info", "Room Code", room)
                // }
                await startMainLoop()
                return true
            }
            return false
        },
        [startMainLoop]
    )

    useEffect(() => {
        const urlParams = new URLSearchParams(document.location.search)
        if (urlParams.has("code")) {
            window.opener.convertAuthToken(urlParams.get("code"))
            window.close()
            return
        }

        applyAutoToast()
        const autoOpenTo = SessionStorage.load("autoOpenTo")
        if (autoOpenTo == "singleplayer") {
            setTimeout(startMainLoop)
        } else if (autoOpenTo == "multiplayer") {
            globalOpenModal(MultiplayerStartModal, {
                startWorldCallback: startWorldCallback,
            })
        } else {
            globalOpenModal(MainMenuModal, {
                startSingleplayerCallback: async () => await startMainLoop(),
                startMultiplayerCallback: () => {
                    globalOpenModal(MultiplayerStartModal, {
                        startWorldCallback: startWorldCallback,
                    })
                },
            })
        }
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
        PreferencesSystem.setUserPreference("ReportAnalytics", true)
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
                            <Scene useStats={import.meta.env.DEV} key="scene-in-toast-provider" />
                            <TouchControls />
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
