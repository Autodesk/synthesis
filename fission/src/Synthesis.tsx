import Scene from "@/components/Scene.tsx"
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
import ScoreboardPanel from "@/panels/information/ScoreboardPanel"
import DriverStationPanel from "@/panels/simulation/DriverStationPanel"
import PokerPanel from "@/panels/PokerPanel.tsx"
import ManageAssembliesModal from "@/modals/spawning/ManageAssembliesModal.tsx"
import World from "@/systems/World.ts"
import { AddRobotsModal, AddFieldsModal, SpawningModal } from "@/modals/spawning/SpawningModals.tsx"
import ImportLocalMirabufModal from "@/modals/mirabuf/ImportLocalMirabufModal.tsx"
import APS from "./aps/APS.ts"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel.tsx"
import Skybox from "./ui/components/Skybox.tsx"
import ChooseInputSchemePanel from "./ui/panels/configuring/ChooseInputSchemePanel.tsx"
import ProgressNotifications from "./ui/components/ProgressNotification.tsx"
import ConfigureRobotModal from "./ui/modals/configuring/ConfigureRobotModal.tsx"
import ResetAllInputsModal from "./ui/modals/configuring/ResetAllInputsModal.tsx"
import ZoneConfigPanel from "./ui/panels/configuring/scoring/ZoneConfigPanel.tsx"
import SceneOverlay from "./ui/components/SceneOverlay.tsx"

import WPILibWSWorker from "@/systems/simulation/wpilib_brain/WPILibWSWorker.ts?worker"
import WSViewPanel from "./ui/panels/WSViewPanel.tsx"
import Lazy from "./util/Lazy.ts"
import NewInputSchemeModal from "./ui/modals/configuring/theme-editor/NewInputSchemeModal.tsx"
import AssignNewSchemeModal from "./ui/modals/configuring/theme-editor/AssignNewSchemeModal.tsx"

const worker = new Lazy<Worker>(() => new WPILibWSWorker())

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

    const { currentTheme, applyTheme, defaultTheme } = useTheme()

    useEffect(() => {
        applyTheme(currentTheme)
    }, [currentTheme, applyTheme])

    const panelElements = getActivePanelElements()
    const modalElement = getActiveModalElement()

    useEffect(() => {
        if (has_code) return

        World.InitWorld()

        worker.getValue()

        let mainLoopHandle = 0
        const mainLoop = () => {
            mainLoopHandle = requestAnimationFrame(mainLoop)

            World.UpdateWorld()
        }
        mainLoop()

        World.SceneRenderer.UpdateSkyboxColors(defaultTheme)

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
                        closeAllPanels={closeAllPanels}
                    >
                        <ToastProvider key="toast-provider">
                            <Scene useStats={true} key="scene-in-toast-provider" />
                            <SceneOverlay />
                            <MainHUD key={"main-hud"} />
                            <ScoreboardPanel key="scoreboard" panelId="scoreboard" openLocation="top" sidePadding={8} />
                            {panelElements.length > 0 && panelElements}
                            {modalElement && (
                                <div className="absolute w-full h-full left-0 top-0" key={"modal-element"}>
                                    {modalElement}
                                </div>
                            )}
                            <ProgressNotifications key={"progress-notifications"} />
                            <ToastContainer key={"toast-container"} />
                        </ToastProvider>
                    </PanelControlProvider>
                </ModalControlProvider>
            </TooltipControlProvider>
        </AnimatePresence>
    )
}

const initialModals = [
    <SettingsModal key="settings" modalId="settings" />,
    <SpawningModal key="spawning" modalId="spawning" />,
    <AddRobotsModal key="add-robot" modalId="add-robot" />,
    <AddFieldsModal key="add-field" modalId="add-field" />,
    <ViewModal key="view" modalId="view" />,
    <DownloadAssetsModal key="download-assets" modalId="download-assets" />,
    <RoboRIOModal key="roborio" modalId="roborio" />,
    <DrivetrainModal key="drivetrain" modalId="drivetrain" />,
    <ThemeEditorModal key="theme-editor" modalId="theme-editor" />,
    <ExitSynthesisModal key="exit-synthesis" modalId="exit-synthesis" />,
    <MatchResultsModal key="match-results" modalId="match-results" />,
    <UpdateAvailableModal key="update-available" modalId="update-available" />,
    <ConnectToMultiplayerModal key="connect-to-multiplayer" modalId="connect-to-multiplayer" />,
    <ServerHostingModal key="server-hosting" modalId="server-hosting" />,
    <ChangeInputsModal key="change-inputs" modalId="change-inputs" />,
    <ChooseMultiplayerModeModal key="multiplayer-mode" modalId="multiplayer-mode" />,
    <ChooseSingleplayerModeModal key="singleplayer-mode" modalId="singleplayer-mode" />,
    <PracticeSettingsModal key="practice-settings" modalId="practice-settings" />,
    <DeleteThemeModal key="delete-theme" modalId="delete-theme" />,
    <NewInputSchemeModal key="new-scheme" modalId="new-scheme" />,
    <AssignNewSchemeModal key="assign-new-scheme" modalId="assign-new-scheme" />,
    <DeleteAllThemesModal key="delete-all-themes" modalId="delete-all-themes" />,
    <NewThemeModal key="new-theme" modalId="new-theme" />,
    <RCCreateDeviceModal key="create-device" modalId="create-device" />,
    <RCConfigPwmGroupModal key="config-pwm" modalId="config-pwm" />,
    <RCConfigEncoderModal key="config-encoder" modalId="config-encoder" />,
    <MatchModeModal key="match-mode" modalId="match-mode" />,
    <ConfigMotorModal key="config-motor" modalId="config-motor" />,
    <ManageAssembliesModal key="manage-assemblies" modalId="manage-assemblies" />,
    <ImportLocalMirabufModal key="import-local-mirabuf" modalId="import-local-mirabuf" />,
    <ConfigureRobotModal key="config-robot" modalId="config-robot" />,
    <ScoringZonesPanel panelId="scoring-zones" openLocation="right" />,
    <ZoneConfigPanel panelId="zone-config" openLocation="right" />,
    <ResetAllInputsModal key="reset-inputs" modalId="reset-inputs" />,
]

const initialPanels: ReactElement[] = [
    <RobotSwitchPanel key="multibot" panelId="multibot" openLocation="right" sidePadding={8} />,
    <DriverStationPanel key="driver-station" panelId="driver-station" />,
    <SpawnLocationsPanel key="spawn-locations" panelId="spawn-locations" />,
    <ScoreboardPanel key="scoreboard" panelId="scoreboard" />,
    <ConfigureGamepiecePickupPanel
        key="config-gamepiece-pickup"
        panelId="config-gamepiece-pickup"
        openLocation="right"
        sidePadding={8}
    />,
    <ConfigureShotTrajectoryPanel
        key="config-shot-trajectory"
        panelId="config-shot-trajectory"
        openLocation="right"
        sidePadding={8}
    />,
    <ScoringZonesPanel key="scoring-zones" panelId="scoring-zones" openLocation="right" sidePadding={8} />,
    <ZoneConfigPanel key="zone-config" panelId="zone-config" openLocation="right" sidePadding={8} />,
    <ImportMirabufPanel key="import-mirabuf" panelId="import-mirabuf" />,
    <PokerPanel key="poker" panelId="poker" />,
    <ChooseInputSchemePanel key="choose-scheme" panelId="choose-scheme" />,
    <WSViewPanel key="ws-view" panelId="ws-view" />,
]

export default Synthesis
