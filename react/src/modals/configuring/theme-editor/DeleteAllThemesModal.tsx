import React from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "../../../ModalContext"

const DeleteAllThemesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal
            name="Delete All Themes?"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                // delete all themes somehow
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        ></Modal>
    )
}

export default DeleteAllThemesModal
