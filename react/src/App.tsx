import { AnimatePresence, motion } from "framer-motion"
import { ReactElement, useEffect } from "react"
import { ModalControlProvider, useModalManager } from "./ModalContext"
import { PanelControlProvider, usePanelManager } from "./PanelContext"
import { useTheme } from "./ThemeContext"
import { ToastContainer, ToastProvider } from "./ToastContext"
import { TooltipControl, TooltipControlProvider, TooltipType, useTooltipManager } from "./TooltipContext"
import MainHUD from "./components/MainHUD"
import DownloadAssetsModal from "./modals/DownloadAssetsModal"
import ExitSynthesisModal from "./modals/ExitSynthesisModal"
import MatchResultsModal from "./modals/MatchResultsModal"
import SpawningModal from "./modals/SpawningModal"
import UpdateAvailableModal from "./modals/UpdateAvailableModal"
import ViewModal from "./modals/ViewModal"
import ConnectToMultiplayerModal from "./modals/aether/ConnectToMultiplayerModal"
import ServerHostingModal from "./modals/aether/ServerHostingModal"
import ChangeInputsModal from "./modals/configuring/ChangeInputsModal"
import ChooseMultiplayerModeModal from "./modals/configuring/ChooseMultiplayerModeModal"
import ChooseSingleplayerModeModal from "./modals/configuring/ChooseSingleplayerModeModal"
import ConfigMotorModal from "./modals/configuring/ConfigMotorModal"
import DrivetrainModal from "./modals/configuring/DrivetrainModal"
import PracticeSettingsModal from "./modals/configuring/PracticeSettingsModal"
import RoboRIOModal from "./modals/configuring/RoboRIOModal"
import SettingsModal from "./modals/configuring/SettingsModal"
import RCConfigEncoderModal from "./modals/configuring/rio-config/RCConfigEncoderModal"
import RCConfigPwmGroupModal from "./modals/configuring/rio-config/RCConfigPwmGroupModal"
import RCCreateDeviceModal from "./modals/configuring/rio-config/RCCreateDeviceModal"
import DeleteAllThemesModal from "./modals/configuring/theme-editor/DeleteAllThemesModal"
import DeleteThemeModal from "./modals/configuring/theme-editor/DeleteThemeModal"
import NewThemeModal from "./modals/configuring/theme-editor/NewThemeModal"
import ThemeEditorModal from "./modals/configuring/theme-editor/ThemeEditorModal"
import AddFieldModal from "./modals/spawning/AddFieldModal"
import AddRobotModal from "./modals/spawning/AddRobotModal"
import MatchModeModal from "./modals/spawning/MatchModeModal"
import RobotSwitchPanel from "./panels/RobotSwitchPanel"
import SpawnLocationsPanel from "./panels/SpawnLocationPanel"
import ConfigureGamepiecePickupPanel from "./panels/configuring/ConfigureGamepiecePickupPanel"
import ConfigureShotTrajectoryPanel from "./panels/configuring/ConfigureShotTrajectoryPanel"
import ScoringZonesPanel from "./panels/configuring/scoring/ScoringZonesPanel"
import ZoneConfigPanel from "./panels/configuring/scoring/ZoneConfigPanel"
import ScoreboardPanel from "./panels/information/ScoreboardPanel"
import DriverStationPanel from "./panels/simulation/DriverStationPanel"

const initialModals = [
    <SettingsModal modalId="settings" />,
    <SpawningModal modalId="spawning" />,
    <AddRobotModal modalId="add-robot" />,
    <AddFieldModal modalId="add-field" />,
    <ViewModal modalId="view" />,
    <DownloadAssetsModal modalId="download-assets" />,
    <RoboRIOModal modalId="roborio" />,
    <DrivetrainModal modalId="drivetrain" />,
    <ThemeEditorModal modalId="theme-editor" />,
    <ExitSynthesisModal modalId="exit-synthesis" />,
    <MatchResultsModal modalId="match-results" />,
    <UpdateAvailableModal modalId="update-availale" />,
    <ConnectToMultiplayerModal modalId="connect-to-multiplayer" />,
    <ServerHostingModal modalId="server-hosting" />,
    <ChangeInputsModal modalId="change-inputs" />,
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
    <ConfigMotorModal modalId="config-motor" />
]

const initialPanels: ReactElement[] = [
    <RobotSwitchPanel panelId="multibot" />,
    <DriverStationPanel panelId="driver-station" />,
    <SpawnLocationsPanel panelId="spawn-locations" />,
    <ScoreboardPanel panelId="scoreboard" />,
    <ConfigureGamepiecePickupPanel panelId="config-gamepiece-pickup" />,
    <ConfigureShotTrajectoryPanel panelId="config-shot-trajectory" />,
    <ScoringZonesPanel panelId="scoring-zones" />,
    <ZoneConfigPanel panelId="zone-config" />
]

function App() {
    const { openModal, closeModal, getActiveModalElement } =
        useModalManager(initialModals)
    const { openPanel, closePanel, closeAllPanels, getActivePanelElements } =
        usePanelManager(initialPanels)
    const { showTooltip } = useTooltipManager();

    const { currentTheme, applyTheme } = useTheme()

    useEffect(() => {
        applyTheme(currentTheme)
    }, [currentTheme, applyTheme])

    if (process.env.NODE_ENV && process.env.NODE_ENV !== "development") {
        document.body.style.background = "purple"
    }

    const panelElements = getActivePanelElements()

    const motionPanelElements = panelElements.map((el, i) => (
        <motion.div
            initial={{
                scale: 0,
                opacity: 0,
                width: "min-content",
                height: "min-content",
            }}
            animate={{
                scale: 1,
                opacity: 1,
                width: "min-content",
                height: "min-content",
            }}
            exit={{
                scale: 0,
                opacity: 0,
                width: "min-content",
                height: "min-content",
            }}
            transition={{
                type: "spring",
                stiffness: 300,
                damping: 20,
            }}
            style={{ translateX: "-50%", translateY: "-50%" }}
            className="absolute left-1/2 top-1/2"
            key={"panel-" + i}
        >
            {el}
        </motion.div>
    ))

    const modalElement = getActiveModalElement()
    const motionModalElement =
        modalElement == null ? null : (
            <motion.div
                initial={{
                    scale: 0,
                    opacity: 0,
                    width: "min-content",
                    height: "min-content",
                }}
                animate={{
                    scale: 1,
                    opacity: 1,
                    width: "min-content",
                    height: "min-content",
                }}
                exit={{
                    scale: 0,
                    opacity: 0,
                    width: "min-content",
                    height: "min-content",
                }}
                transition={{
                    type: "spring",
                    stiffness: 300,
                    damping: 25,
                }}
                style={{ translateX: "-50%", translateY: "-50%" }}
                className="absolute left-1/2 top-1/2"
                key={"modal"}
            >
                {getActiveModalElement()}
            </motion.div>
        )

    return (
        <AnimatePresence>
            <TooltipControlProvider showTooltip={(type: TooltipType, duration: number, controls?: TooltipControl[]) => {
                showTooltip(type, duration, controls)
            }}>
                <ModalControlProvider
                    openModal={(modalId: string) => {
                        closeAllPanels()
                        openModal(modalId)
                    }}
                    closeModal={closeModal}
                >
                    <PanelControlProvider
                        openPanel={openPanel}
                        closePanel={(id: string) => {
                            closePanel(id)
                        }}
                    >
                        <ToastProvider>
                            <MainHUD />
                            {motionPanelElements.length > 0 && motionPanelElements}
                            {motionModalElement && motionModalElement}
                            <ToastContainer />
                        </ToastProvider>
                    </PanelControlProvider>
                </ModalControlProvider>
            </TooltipControlProvider>
        </AnimatePresence>
    )
}

export default App
