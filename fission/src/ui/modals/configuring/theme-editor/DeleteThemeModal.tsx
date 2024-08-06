import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTheme } from "@/ui/ThemeContext"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const DeleteThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { currentTheme, deleteTheme } = useTheme()

    const { openModal } = useModalControlContext()

    return (
        <Modal
            name={`Delete ${currentTheme}?`}
            icon={SynthesisIcons.Xmark}
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
