import { AnimatePresence } from "framer-motion"
import { SnackbarProvider } from "notistack"
import Slide from "@mui/material/Slide"
import { useCallback, useEffect, useRef, useState } from "react"
import MainHUD from "@/components/overlays/MainHUD.tsx"
import MultiplayerHUD from "@/components/overlays/MultiplayerHUD.tsx"
import Scene from "@/components/Scene.tsx"
import MultiplayerStartModal from "@/modals/multiplayer/MultiplayerStartModal.tsx"
import World from "@/systems/World.ts"
import { UIRenderer } from "@/ui/UIRenderer.tsx"
import PreferencesSystem from "./systems/preferences/PreferencesSystem.ts"
import AnalyticsConsent from "./ui/components/AnalyticsConsent.tsx"
import ContextMenu from "./ui/components/ContextMenu.tsx"
import DragModeIndicator from "@/components/overlays/DragModeIndicator.tsx"
import ProgressNotifications from "@/components/ProgressNotification.tsx"
import SceneOverlay from "@/components/overlays/SceneOverlay.tsx"
import PortraitOverlay from "@/components/overlays/PortraitOverlay.tsx"
import TouchControls from "./ui/components/TouchControls.tsx"
import { StateProvider } from "./ui/StateProvider.tsx"
import { ThemeProvider } from "./ui/ThemeProvider.tsx"
import { UIProvider } from "./ui/UIProvider.tsx"
import CommandPalette from "@/ui/components/CommandPalette.tsx"
import SessionStorage, { applyAutoToast } from "@/util/SessionStorage.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import { globalOpenModal } from "@/components/GlobalUIControls.ts"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld.ts"
import { Stack } from "@mui/material"

const Synthesis = () => {
    const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true)

    const mainLoopHandle = useRef(0)
    const startMainLoop = useCallback(async () => {
        World.initWorld()
        if (!PreferencesSystem.getUserPreference("ReportAnalytics") && !import.meta.env.DEV) {
            setConsentPopupDisable(false)
        }

        const mainLoop = () => {
            mainLoopHandle.current = requestAnimationFrame(mainLoop)
            World.updateWorld()
        }

        mainLoop()
    }, [])

    useEffect(() => {
        const urlParams = new URLSearchParams(document.location.search)
        if (urlParams.has("code")) {
            window.opener.convertAuthToken(urlParams.get("code"))
            window.close()
            return
        }
        if (urlParams.has("autojoin")) {
            const room = urlParams.get("autojoin")!
            const name = PreferencesSystem.getUserPreference("MultiplayerUsername") ?? "TestUser"
            const ws = new MultiplayerWebsocket(
                `ws${PreferencesSystem.getUserPreference("MultiplayerSecure") ? "s" : ""}://${PreferencesSystem.getUserPreference("MultiplayerHost") || "127.0.0.1"}:${PreferencesSystem.getUserPreference("MultiplayerPort")}`
            )
            MultiplayerWebsocket.init(room || null, name, ws)
            setTimeout(() => startMultiplayerWorld({ displayName: name, ws, keepAssets: false, isHost: false }))
        }

        applyAutoToast()
        const autoOpenMultiplayer = SessionStorage.load("autoOpenMultiplayer")

        if (autoOpenMultiplayer) {
            globalOpenModal(MultiplayerStartModal, undefined)
        }

        startMainLoop()

        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle.current)
            World.destroyWorld()
            World.multiplayerSystem?.destroy()
            // World.SceneRenderer.RemoveAllSceneObjects();
        }
    }, [startMainLoop])

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
                            <MainHUD key={"main-hud"} />
                            <UIRenderer />
                            <CommandPalette />
                            <ProgressNotifications key={"progress-notifications"} />
                            <Stack direction={"column"} gap={1} position={"absolute"} bottom={0} left={0}>
                                <DragModeIndicator />
                                <MultiplayerHUD />
                            </Stack>
                            <PortraitOverlay />

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
