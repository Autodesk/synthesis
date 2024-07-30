import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTheme } from "@/ui/ThemeContext"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const DeleteAllThemesModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { deleteAllThemes } = useTheme()

    return (
        <Modal
            name="Delete All Themes?"
            icon={SynthesisIcons.Xmark}
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
