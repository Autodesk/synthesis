import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "@/ModalContext"
import { useTheme } from "@/ThemeContext"

const DeleteThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { currentTheme, deleteTheme } = useTheme()

    const { openModal } = useModalControlContext()

    return (
        <Modal
            name={`Delete ${currentTheme}?`}
            icon={<GrFormClose />}
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
