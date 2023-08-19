import { AnimatePresence, motion } from "framer-motion"
import { ReactElement, useEffect } from "react"
import { ModalControlProvider, useModalManager } from "./ModalContext"
import { PanelControlProvider, usePanelManager } from "./PanelContext"
import { useTheme } from "./ThemeContext"
import { ToastContainer, ToastProvider } from "./ToastContext"
import MainHUD from "./components/MainHUD"
import DownloadAssetsModal from "./modals/DownloadAssetsModal"
import ExitSynthesisModal from "./modals/ExitSynthesisModal"
import FieldsModal from "./modals/FieldsModal"
import MatchResultsModal from "./modals/MatchResultsModal"
import RobotsModal from "./modals/RobotsModal"
import SpawningModal from "./modals/SpawningModal"
import UpdateAvailableModal from "./modals/UpdateAvailableModal"
import ViewModal from "./modals/ViewModal"
import ConnectToMultiplayerModal from "./modals/aether/ConnectToMultiplayerModal"
import ServerHostingModal from "./modals/aether/ServerHostingModal"
import ChangeInputsModal from "./modals/configuring/ChangeInputsModal"
import ChooseMultiplayerModeModal from "./modals/configuring/ChooseMultiplayerModeModal"
import ChooseSingleplayerModeModal from "./modals/configuring/ChooseSingleplayerModeModal"
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
import DriverStationPanel from "./panels/DriverStationPanel"
import MultiBotPanel from "./panels/MultiBotPanel"

const initialModals = [
    <SettingsModal modalId="settings" />,
    <SpawningModal modalId="spawning" />,
    <RobotsModal modalId="robots" />,
    <FieldsModal modalId="fields" />,
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
]

const initialPanels: ReactElement[] = [
    <MultiBotPanel panelId="multibot" />,
    <DriverStationPanel panelId="driver-station" />,
]

function App() {
    const { openModal, closeModal, getActiveModalElement } =
        useModalManager(initialModals)
    const { openPanel, closePanel, closeAllPanels, getActivePanelElements } =
        usePanelManager(initialPanels)

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
                    <AnimatePresence>
                        {motionPanelElements.length > 0 && motionPanelElements}
                        {motionModalElement && motionModalElement}
                    </AnimatePresence>
                    <ToastContainer />
                </ToastProvider>
            </PanelControlProvider>
        </ModalControlProvider>
    )
}

export default App
