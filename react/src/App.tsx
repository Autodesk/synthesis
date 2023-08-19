import { ModalControlProvider, useModalManager } from "./ModalContext"
import { PanelControlProvider, usePanelManager } from "./PanelContext"
import MainHUD from "./components/MainHUD"
import SpawningModal from "./modals/SpawningModal"
import CreateDeviceModal from "./modals/CreateDeviceModal"
import DownloadAssetsModal from "./modals/DownloadAssetsModal"
import RoboRIOModal from "./modals/configuring/RoboRIOModal"
import ViewModal from "./modals/ViewModal"
import MultiBotPanel from "./panels/MultiBotPanel"
import RobotsModal from "./modals/RobotsModal"
import FieldsModal from "./modals/FieldsModal"
import SettingsModal from "./modals/configuring/SettingsModal"
import DriverStationPanel from "./panels/DriverStationPanel"
import DrivetrainModal from "./modals/configuring/DrivetrainModal"
import { AnimatePresence } from "framer-motion"
import { motion } from "framer-motion"
import { ReactElement } from "react"
import { ToastContainer, ToastProvider } from "./ToastContext"
import ThemeEditorModal from "./modals/configuring/theme-editor/ThemeEditorModal"
import MatchResultsModal from "./modals/MatchResultsModal"
import UpdateAvailableModal from "./modals/UpdateAvailableModal"
import ExitSynthesisModal from "./modals/ExitSynthesisModal"
import ConnectToMultiplayerModal from "./modals/aether/ConnectToMultiplayerModal"
import ServerHostingModal from "./modals/aether/ServerHostingModal"
import ChangeInputsModal from "./modals/configuring/ChangeInputsModal"
import ChooseMultiplayerModeModal from "./modals/configuring/ChooseMultiplayerModeModal"
import ChooseSingleplayerModeModal from "./modals/configuring/ChooseSingleplayerModeModal"
import PracticeSettingsModal from "./modals/configuring/PracticeSettingsModal"
import DeleteThemeModal from "./modals/configuring/theme-editor/DeleteThemeModal"
import DeleteAllThemesModal from "./modals/configuring/theme-editor/DeleteAllThemesModal"
import NewThemeModal from "./modals/configuring/theme-editor/NewThemeModal"
import RCCreateDeviceModal from "./modals/configuring/rio-config/RCCreateDeviceModal"
import RCConfigPwmGroupModal from "./modals/configuring/rio-config/RCConfigPwmGroupModal"
import RCConfigEncoderModal from "./modals/configuring/rio-config/RCConfigEncoderModal"
import { Theme, ThemeProvider } from "./ThemeContext"

const initialModals = [
    <SettingsModal modalId="settings" />,
    <SpawningModal modalId="spawning" />,
    <RobotsModal modalId="robots" />,
    <FieldsModal modalId="fields" />,
    <ViewModal modalId="view" />,
    <DownloadAssetsModal modalId="download-assets" />,
    <RoboRIOModal modalId="roborio" />,
    <CreateDeviceModal modalId="create-device" />,
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

    const initialTheme = "Default"
    const defaultColors: Theme = {
        InteractiveElementSolid: { r: 250, g: 162, b: 27, a: 255 },
        InteractiveElementLeft: { r: 224, g: 130, b: 65, a: 255 },
        InteractiveElementRight: { r: 218, g: 102, b: 89, a: 255 },
        InteractiveSecondary: { r: 204, g: 124, b: 0, a: 255 },
        Background: { r: 33, g: 37, b: 41, a: 255 },
        BackgroundSecondary: { r: 52, g: 58, b: 64, a: 255 },
        MainText: { r: 248, g: 249, b: 250, a: 255 },
        Scrollbar: { r: 213, g: 216, b: 223, a: 255 },
        AcceptButton: { r: 34, g: 139, b: 230, a: 255 },
        CancelButton: { r: 250, g: 82, b: 82, a: 255 },
        InteractiveElementText: { r: 0, g: 0, b: 0, a: 255 },
        Icon: { r: 255, g: 255, b: 255, a: 255 },
        HighlightHover: { r: 89, g: 255, b: 133, a: 255 },
        HighlightSelect: { r: 255, g: 89, b: 133, a: 255 },
        SkyboxTop: { r: 255, g: 255, b: 255, a: 255 },
        SkyboxBottom: { r: 255, g: 255, b: 255, a: 255 },
        FloorGrid: { r: 93, g: 93, b: 93, a: 255 },
    }
    const themes = {
        Default: defaultColors,
    }

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
        <ThemeProvider
            initialTheme={initialTheme}
            themes={themes}
            defaultTheme={defaultColors}
        >
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
                            {motionPanelElements.length > 0 &&
                                motionPanelElements}
                            {motionModalElement && motionModalElement}
                            <ThemeEditorModal modalId="a" />
                        </AnimatePresence>
                        <ToastContainer />
                    </ToastProvider>
                </PanelControlProvider>
            </ModalControlProvider>
        </ThemeProvider>
    )
}

export default App
