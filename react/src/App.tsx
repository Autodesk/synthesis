import { ModalControlProvider, useModalManager } from "./ModalContext";
import { PanelControlProvider, usePanelManager } from "./PanelContext";
import MainHUD from "./components/MainHUD";
import ConfigurationModal from "./modals/ConfigurationModal";
import ControlsModal from "./modals/ControlsModal";
import CreateDeviceModal from "./modals/CreateDeviceModal";
import DownloadAssetsModal from "./modals/DownloadAssetsModal";
import RoboRIOModal from "./modals/RoboRIOModal";
import ViewModal from "./modals/ViewModal";
import MultiBotPanel from "./panels/MultiBotPanel";

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

const initialPanels = [
    {
        id: "multibot", component: (<MultiBotPanel />)
    }
]

function App() {
    const { activeModalId, openModal, closeModal, getActiveModalElement } = useModalManager(initialModals);
    const { openPanel, closePanel, closeAllPanels, getActivePanelElements } = usePanelManager(initialPanels);

    return (
        <ModalControlProvider openModal={(modalId: string) => { closeAllPanels(); openModal(modalId); }} closeModal={closeModal}>
            <PanelControlProvider openPanel={openPanel} closePanel={closePanel}>
                <MainHUD />
                {getActivePanelElements()}
                {getActiveModalElement()}
            </PanelControlProvider>
        </ModalControlProvider>
    );
}

export default App;
