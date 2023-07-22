import { ModalControlProvider, useModalManager } from "./ModalContext";
import Button from "./components/Button";
import Modal from "./components/Modal";

const initialModals = [
    {
        id: "configuration", component: (
            <Modal name={"Configuration"} icon="https://placeholder.co/512x512">
                <Button value={"Test"} />
            </Modal>
        )
    }
]

function App() {
    const { activeModalId, openModal, closeModal, getActiveModalElement } = useModalManager(initialModals);

    return (
        <ModalControlProvider openModal={openModal} closeModal={closeModal}>
            <Button value={"Open Configuration"} onClick={() => openModal("configuration")} />
            {getActiveModalElement()}
        </ModalControlProvider>
    );
}

export default App;
