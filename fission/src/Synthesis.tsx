import { AnimatePresence } from "framer-motion"
import { SnackbarProvider } from "notistack"
import { useCallback, useContext, useEffect, useRef, useState } from "react"
import MainHUD from "@/components/MainHUD"
import Scene from "@/components/Scene.tsx"
import World from "@/systems/World.ts"
import { UIRenderer } from "@/ui/UIRenderer.tsx"
import PreferencesSystem from "./systems/preferences/PreferencesSystem.ts"
import AnalyticsConsent from "./ui/components/AnalyticsConsent.tsx"
import ContextMenu from "./ui/components/ContextMenu.tsx"
import GlobalUIComponent from "./ui/components/GlobalUIComponent.tsx"
import ProgressNotifications from "./ui/components/ProgressNotification.tsx"
import SceneOverlay from "./ui/components/SceneOverlay.tsx"
import Skybox from "./ui/components/Skybox.tsx"
import WPILibConnectionStatus from "./ui/components/WPILibConnectionStatus.tsx"
import MainMenuModal from "./ui/modals/MainMenuModal.tsx"
import { StateProvider } from "./ui/StateProvider.tsx"
import { ThemeProvider } from "./ui/ThemeProvider.tsx"
import { UIContext } from "./ui/UIProvider.tsx"

function Synthesis() {
    const { openModal } = useContext(UIContext)
    const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true)

    const mainLoopHandle = useRef(0)

    useEffect(() => {
        const urlParams = new URLSearchParams(document.location.search)
        if (urlParams.has("code")) {
            window.opener.convertAuthToken(urlParams.get("code"))
            window.close()
            return
        }
        openModal(
            <MainMenuModal
                startSingleplayerCallback={() => {
                    World.initWorld()

                    if (!PreferencesSystem.getGlobalPreference("ReportAnalytics") && !import.meta.env.DEV) {
                        setConsentPopupDisable(false)
                    }

                    const mainLoop = () => {
                        mainLoopHandle.current = requestAnimationFrame(mainLoop)
                        World.updateWorld()
                    }

                    mainLoop()
                }}
            />,
            undefined,
            {
                hideCancel: true,
                hideAccept: true,
            }
        )
        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle.current)
            World.destroyWorld()
            // World.SceneRenderer.RemoveAllSceneObjects();
        }
    }, [])

    // useEffect(() => {
    // TODO:
    // const scoreboardExists = false
    // panelElements.forEach(x => {
    //     if (x.key == "scoreboard") scoreboardExists = true
    // })
    // if (PreferencesSystem.getGlobalPreference("RenderScoreboard") && !scoreboardExists) {
    //     openPanel("scoreboard")
    // }
    // });

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
                <SnackbarProvider maxSnack={5}>
                    <Skybox key={"skybox"} />
                    <StateProvider>
                        <GlobalUIComponent />
                        <Scene useStats={import.meta.env.DEV} key="scene-in-toast-provider" />
                        <SceneOverlay />
                        <ContextMenu />
                        <MainHUD key={"main-hud"} />
                        <UIRenderer />
                        <ProgressNotifications key={"progress-notifications"} />
                        <WPILibConnectionStatus />

                        {!consentPopupDisable && <AnalyticsConsent onClose={onDisableConsent} onConsent={onConsent} />}
                    </StateProvider>
                </SnackbarProvider>
            </ThemeProvider>
        </AnimatePresence>
    )
}

export default Synthesis
