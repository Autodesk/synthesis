import React from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "../../../ModalContext"
import { useTheme } from "../../../ThemeContext"

const DeleteAllThemesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { deleteAllThemes } = useTheme()

    return (
        <Modal
            name="Delete All Themes?"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                deleteAllThemes()
                openModal("theme-editor")
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        ></Modal>
    )
}

export default DeleteAllThemesModal
