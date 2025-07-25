import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { useTheme } from "@/ui/helpers/UseThemeHelpers"

const DeleteThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { currentTheme, deleteTheme } = useTheme()

    const { openModal } = useModalControlContext()

    return (
        <Modal
            name={`Delete ${currentTheme}?`}
            icon={SynthesisIcons.XMARK}
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
