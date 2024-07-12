import Scene from "@/components/Scene.tsx"
import MirabufSceneObject from "./mirabuf/MirabufSceneObject.ts"
import MirabufCachingService, { MiraType } from "./mirabuf/MirabufLoader.ts"
import { mirabuf } from "./proto/mirabuf"
import MirabufParser, { ParseErrorSeverity } from "./mirabuf/MirabufParser.ts"
import MirabufInstance from "./mirabuf/MirabufInstance.ts"
import { AnimatePresence } from "framer-motion"
import { ReactElement, useEffect } from "react"
import { ModalControlProvider, useModalManager } from "@/ui/ModalContext"
import { PanelControlProvider, usePanelManager } from "@/ui/PanelContext"
import { useTheme } from "@/ui/ThemeContext"
import { ToastContainer, ToastProvider } from "@/ui/ToastContext"
import {
    TOOLTIP_DURATION,
    TooltipControl,
    TooltipControlProvider,
    TooltipType,
    useTooltipManager,
} from "@/ui/TooltipContext"
import MainHUD from "@/components/MainHUD"
import DownloadAssetsModal from "@/modals/DownloadAssetsModal"
import ExitSynthesisModal from "@/modals/ExitSynthesisModal"
import MatchResultsModal from "@/modals/MatchResultsModal"
import UpdateAvailableModal from "@/modals/UpdateAvailableModal"
import ViewModal from "@/modals/ViewModal"
import ConnectToMultiplayerModal from "@/modals/aether/ConnectToMultiplayerModal"
import ServerHostingModal from "@/modals/aether/ServerHostingModal"
import ChangeInputsModal from "@/ui/modals/configuring/ChangeInputsModal.tsx"
import ChooseMultiplayerModeModal from "@/modals/configuring/ChooseMultiplayerModeModal"
import ChooseSingleplayerModeModal from "@/modals/configuring/ChooseSingleplayerModeModal"
import ConfigMotorModal from "@/modals/configuring/ConfigMotorModal"
import DrivetrainModal from "@/modals/configuring/DrivetrainModal"
import PracticeSettingsModal from "@/modals/configuring/PracticeSettingsModal"
import RoboRIOModal from "@/modals/configuring/RoboRIOModal"
import SettingsModal from "@/modals/configuring/SettingsModal"
import RCConfigEncoderModal from "@/modals/configuring/rio-config/RCConfigEncoderModal"
import RCConfigPwmGroupModal from "@/modals/configuring/rio-config/RCConfigPwmGroupModal"
import RCCreateDeviceModal from "@/modals/configuring/rio-config/RCCreateDeviceModal"
import DeleteAllThemesModal from "@/modals/configuring/theme-editor/DeleteAllThemesModal"
import DeleteThemeModal from "@/modals/configuring/theme-editor/DeleteThemeModal"
import NewThemeModal from "@/modals/configuring/theme-editor/NewThemeModal"
import ThemeEditorModal from "@/modals/configuring/theme-editor/ThemeEditorModal"
import MatchModeModal from "@/modals/spawning/MatchModeModal"
import RobotSwitchPanel from "@/panels/RobotSwitchPanel"
import SpawnLocationsPanel from "@/panels/SpawnLocationPanel"
import ConfigureGamepiecePickupPanel from "@/panels/configuring/ConfigureGamepiecePickupPanel"
import ConfigureShotTrajectoryPanel from "@/panels/configuring/ConfigureShotTrajectoryPanel"
import ScoringZonesPanel from "@/panels/configuring/scoring/ScoringZonesPanel"
import ZoneConfigPanel from "@/panels/configuring/scoring/ZoneConfigPanel"
import ScoreboardPanel from "@/panels/information/ScoreboardPanel"
import DriverStationPanel from "@/panels/simulation/DriverStationPanel"
import ManageAssembliesModal from "@/modals/spawning/ManageAssembliesModal.tsx"
import World from "@/systems/World.ts"
import { AddRobotsModal, AddFieldsModal, SpawningModal } from "@/modals/spawning/SpawningModals.tsx"
import ImportLocalMirabufModal from "@/modals/mirabuf/ImportLocalMirabufModal.tsx"
import APS from "./aps/APS.ts"
import ResetAllInputsModal from "./ui/modals/configuring/ResetAllInputsModal.tsx"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel.tsx"
import Skybox from "./ui/components/Skybox.tsx"
import PokerPanel from "@/panels/PokerPanel.tsx"

const DEFAULT_MIRA_PATH = "/api/mira/Robots/Team 2471 (2018)_v7.mira"

function Synthesis() {
    const urlParams = new URLSearchParams(document.location.search)
    const has_code = urlParams.has("code")
    if (has_code) {
        const code = urlParams.get("code")
        if (code) {
            APS.convertAuthToken(code).then(() => {
                document.location.search = ""
            })
        }
    }
    const { openModal, closeModal, getActiveModalElement } = useModalManager(initialModals)
    const { openPanel, closePanel, closeAllPanels, getActivePanelElements } = usePanelManager(initialPanels)
    const { showTooltip } = useTooltipManager()

    const { currentTheme, applyTheme } = useTheme()

    useEffect(() => {
        applyTheme(currentTheme)
    }, [currentTheme, applyTheme])

    const panelElements = getActivePanelElements()
    const modalElement = getActiveModalElement()

    useEffect(() => {
        if (has_code) return

        World.InitWorld()

        let mira_path = DEFAULT_MIRA_PATH

        if (urlParams.has("mira")) {
            mira_path = `test_mira/${urlParams.get("mira")!}`
            console.debug(`Selected Mirabuf File: ${mira_path}`)
        }

        const setup = async () => {
            const info = await MirabufCachingService.CacheRemote(mira_path, MiraType.ROBOT)
                .catch(_ => MirabufCachingService.CacheRemote(DEFAULT_MIRA_PATH, MiraType.ROBOT))
                .catch(console.error)

            const miraAssembly = await MirabufCachingService.Get(info!.id, MiraType.ROBOT)

            await (async () => {
                if (!miraAssembly || !(miraAssembly instanceof mirabuf.Assembly)) {
                    return
                }

                const parser = new MirabufParser(miraAssembly)
                if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
                    console.error(`Assembly Parser produced significant errors for '${miraAssembly.info!.name!}'`)
                    return
                }

                const mirabufSceneObject = new MirabufSceneObject(new MirabufInstance(parser), miraAssembly.info!.name!)
                World.SceneRenderer.RegisterSceneObject(mirabufSceneObject)
            })()
        }

        setup()

        let mainLoopHandle = 0
        const mainLoop = () => {
            mainLoopHandle = requestAnimationFrame(mainLoop)

            World.UpdateWorld()
        }
        mainLoop()
        // Cleanup
        return () => {
            // TODO: Teardown literally everything
            cancelAnimationFrame(mainLoopHandle)
            World.DestroyWorld()
            // World.SceneRenderer.RemoveAllSceneObjects();
        }

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    return (
        <AnimatePresence key={"animate-presence"}>
            <Skybox key={"skybox"} />
            <TooltipControlProvider
                key={"tooltip-control-provider"}
                showTooltip={(type: TooltipType, controls?: TooltipControl[], duration: number = TOOLTIP_DURATION) => {
                    showTooltip(type, controls, duration)
                }}
            >
                <ModalControlProvider
                    key={"modal-control-provider"}
                    openModal={(modalId: string) => {
                        closeAllPanels()
                        openModal(modalId)
                    }}
                    closeModal={closeModal}
                >
                    <PanelControlProvider
                        key={"panel-control-provider"}
                        openPanel={openPanel}
                        closePanel={(id: string) => {
                            closePanel(id)
                        }}
                    >
                        <ToastProvider key="toast-provider">
                            <Scene useStats={true} key="scene-in-toast-provider" />
                            <MainHUD key={"main-hud"} />
                            {panelElements.length > 0 && panelElements}
                            {modalElement && (
                                <div className="absolute w-full h-full left-0 top-0" key={"modal-element"}>
                                    {modalElement}
                                </div>
                            )}
                            <ToastContainer key={"toast-container"} />
                        </ToastProvider>
                    </PanelControlProvider>
                </ModalControlProvider>
            </TooltipControlProvider>
        </AnimatePresence>
    )
}

const initialModals = [
    <SettingsModal modalId="settings" />,
    <SpawningModal modalId="spawning" />,
    <AddRobotsModal modalId="add-robot" />,
    <AddFieldsModal modalId="add-field" />,
    <ViewModal modalId="view" />,
    <DownloadAssetsModal modalId="download-assets" />,
    <RoboRIOModal modalId="roborio" />,
    <DrivetrainModal modalId="drivetrain" />,
    <ThemeEditorModal modalId="theme-editor" />,
    <ExitSynthesisModal modalId="exit-synthesis" />,
    <MatchResultsModal modalId="match-results" />,
    <UpdateAvailableModal modalId="update-available" />,
    <ConnectToMultiplayerModal modalId="connect-to-multiplayer" />,
    <ServerHostingModal modalId="server-hosting" />,
    <ChangeInputsModal modalId="change-inputs" />,
    <ResetAllInputsModal modalId="reset-inputs" />,
    <ChooseMultiplayerModeModal modalId="multiplayer-mode" />,
    <ChooseSingleplayerModeModal modalId="singleplayer-mode" />,
    <PracticeSettingsModal modalId="practice-settings" />,
    <DeleteThemeModal modalId="delete-theme" />,
    <DeleteAllThemesModal modalId="delete-all-themes" />,
    <NewThemeModal modalId="new-theme" />,
    <RCCreateDeviceModal modalId="create-device" />,
    <RCConfigPwmGroupModal modalId="config-pwm" />,
    <RCConfigEncoderModal modalId="config-encoder" />,
    <MatchModeModal modalId="match-mode" />,
    <SpawningModal modalId="spawning" />,
    <ConfigMotorModal modalId="config-motor" />,
    <ManageAssembliesModal modalId="manage-assembles" />,
    <ImportLocalMirabufModal modalId="import-local-mirabuf" />,
]

const initialPanels: ReactElement[] = [
    <RobotSwitchPanel panelId="multibot" openLocation="right" sidePadding={8} />,
    <DriverStationPanel panelId="driver-station" />,
    <SpawnLocationsPanel panelId="spawn-locations" />,
    <ScoreboardPanel panelId="scoreboard" />,
    <ConfigureGamepiecePickupPanel panelId="config-gamepiece-pickup" />,
    <ConfigureShotTrajectoryPanel panelId="config-shot-trajectory" />,
    <ScoringZonesPanel panelId="scoring-zones" />,
    <ZoneConfigPanel panelId="zone-config" />,
    <ImportMirabufPanel panelId="import-mirabuf" />,
    <PokerPanel key="poker" panelId="poker" />,
]

export default Synthesis
