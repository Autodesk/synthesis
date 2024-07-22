import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaXmark } from "react-icons/fa6"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTheme } from "@/ui/ThemeContext"

const DeleteThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { currentTheme, deleteTheme } = useTheme()

    const { openModal } = useModalControlContext()

    return (
        <Modal
            name={`Delete ${currentTheme}?`}
            icon={<FaXmark />}
            modalId={modalId}
            onAccept={() => {
                deleteTheme(currentTheme)
                openModal("theme-editor")
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        ></Modal>
    )
}

export default DeleteThemeModal
