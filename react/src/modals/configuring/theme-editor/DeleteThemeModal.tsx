import React from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "../../../ModalContext"

const DeleteThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const theme = "Theme 1";

    const { openModal } = useModalControlContext();

    return (
        <Modal
            name={`Delete ${theme}?`}
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                // delete theme somehow
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        >
        </Modal>
    )
}

export default DeleteThemeModal 
