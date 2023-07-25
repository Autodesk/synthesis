import { ModalControlProvider, useModalManager } from "./ModalContext"
import { PanelControlProvider, usePanelManager } from "./PanelContext"
import MainHUD from "./components/MainHUD"
import SpawningModal from "./modals/SpawningModal"
import ConfigurationModal from "./modals/ConfigurationModal"
import ControlsModal from "./modals/ControlsModal"
import CreateDeviceModal from "./modals/CreateDeviceModal"
import DownloadAssetsModal from "./modals/DownloadAssetsModal"
import RoboRIOModal from "./modals/RoboRIOModal"
import ViewModal from "./modals/ViewModal"
import MultiBotPanel from "./panels/MultiBotPanel"
import RobotsModal from "./modals/RobotsModal"
import FieldsModal from "./modals/FieldsModal"
import SettingsModal from "./modals/SettingsModal"
import DriverStationPanel from "./panels/DriverStationPanel"
import DrivetrainModal from "./modals/DrivetrainModal"

const initialModals = [
    {
        id: "settings",
        component: <SettingsModal />,
    },
    {
        id: "spawning",
        component: <SpawningModal />,
    },
    {
        id: "robots",
        component: <RobotsModal />,
    },
    {
        id: "fields",
        component: <FieldsModal />,
    },
    {
        id: "configuration",
        component: <ConfigurationModal />,
    },
    {
        id: "view",
        component: <ViewModal />,
    },
    {
        id: "controls",
        component: <ControlsModal />,
    },
    {
        id: "download-assets",
        component: <DownloadAssetsModal />,
    },
    {
        id: "roborio",
        component: <RoboRIOModal />,
    },
    {
        id: "create-device",
        component: <CreateDeviceModal />,
    },
    {
        id: "drivetrain",
        component: <DrivetrainModal />,
    },
]

const initialPanels = [
    {
        id: "multibot",
        component: <MultiBotPanel />,
    },
    {
        id: "driver-station",
        component: <DriverStationPanel />,
    },
]

function App() {
    const { openModal, closeModal, getActiveModalElement } =
        useModalManager(initialModals)
    const { openPanel, closePanel, closeAllPanels, getActivePanelElements } =
        usePanelManager(initialPanels)

    if (process.env.NODE_ENV && process.env.NODE_ENV !== "development") {
        document.body.style.background = "purple"
    }

    return (
        <ModalControlProvider
            openModal={(modalId: string) => {
                closeAllPanels()
                openModal(modalId)
            }}
            closeModal={closeModal}
        >
            <PanelControlProvider openPanel={openPanel} closePanel={closePanel}>
                <MainHUD />
                {getActivePanelElements()}
                {getActiveModalElement()}
            </PanelControlProvider>
        </ModalControlProvider>
    )
}

export default App
