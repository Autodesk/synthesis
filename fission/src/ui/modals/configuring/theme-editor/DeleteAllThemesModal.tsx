import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaXmark } from "react-icons/fa6"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTheme } from "@/ui/ThemeContext"

const DeleteAllThemesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { deleteAllThemes } = useTheme()

    return (
        <Modal
            name="Delete All Themes?"
            icon={<FaXmark />}
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
