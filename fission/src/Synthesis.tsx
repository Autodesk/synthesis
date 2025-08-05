import { AnimatePresence } from "framer-motion"
// <<<<<<< HEAD
// import type React from "react"
// import { type ReactElement, useCallback, useEffect, useRef, useState } from "react"
// =======
import { SnackbarProvider } from "notistack"
import { useCallback, useEffect, useRef, useState } from "react"
import MainHUD from "@/components/MainHUD"
import Scene from "@/components/Scene.tsx"
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
import { applyInitialGraphicsSettings } from "./ui/helpers/GraphicsSettings.ts"
import MainMenuModal from "./ui/modals/MainMenuModal.tsx"
import { StateProvider } from "./ui/StateProvider.tsx"
import { ThemeProvider } from "./ui/ThemeProvider.tsx"
import { UIProvider } from "./ui/UIProvider.tsx"

function Synthesis() {
    const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true)

    const mainLoopHandle = useRef(0)

    useEffect(() => {
        const urlParams = new URLSearchParams(document.location.search)
        if (urlParams.has("code")) {
            window.opener.convertAuthToken(urlParams.get("code"))
            window.close()
            return
        }
        const startSingleplayerCallback = () => {
            World.initWorld()

            applyInitialGraphicsSettings()

            if (!PreferencesSystem.getGlobalPreference("ReportAnalytics") && !import.meta.env.DEV) {
                setConsentPopupDisable(false)
            }

            const mainLoop = () => {
                mainLoopHandle.current = requestAnimationFrame(mainLoop)
                World.updateWorld()
            }

            mainLoop()
        }
        globalOpenModal(MainMenuModal, { startSingleplayerCallback })
        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle.current)
            World.destroyWorld()
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
                <SnackbarProvider maxSnack={5} anchorOrigin={{ horizontal: "right", vertical: "bottom" }}>
                    <StateProvider>
                        <UIProvider>
                            <GlobalUIComponent />
                            <Scene useStats={import.meta.env.DEV} key="scene-in-toast-provider" />
                            <SceneOverlay />
                            <ContextMenu />
                            <MainHUD key={"main-hud"} />
                            <UIRenderer />
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
