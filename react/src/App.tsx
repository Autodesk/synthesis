import { ModalControlProvider, useModalManager } from "./ModalContext";
import MainHUD from "./components/MainHUD";
import ConfigurationModal from "./modals/ConfigurationModal";
import ControlsModal from "./modals/ControlsModal";
import CreateDeviceModal from "./modals/CreateDeviceModal";
import DownloadAssetsModal from "./modals/DownloadAssetsModal";
import RoboRIOModal from "./modals/RoboRIOModal";
import ViewModal from "./modals/ViewModal";

const initialModals = [
    {
        id: "configuration", component: (<ConfigurationModal />)
    },
    {
        id: "view", component: (<ViewModal />)
    },
    {
        id: "controls", component: (<ControlsModal />)
    },
    {
        id: "download-assets", component: (<DownloadAssetsModal />)
    },
    {
        id: "roborio", component: (<RoboRIOModal />)
    },
    {
        id: "create-device", component: (<CreateDeviceModal />)
    }
]

function App() {
    const { activeModalId, openModal, closeModal, getActiveModalElement } = useModalManager(initialModals);

    return (
        <ModalControlProvider openModal={openModal} closeModal={closeModal}>
            <MainHUD />
            {getActiveModalElement()}
        </ModalControlProvider>
    );
}

export default App;
