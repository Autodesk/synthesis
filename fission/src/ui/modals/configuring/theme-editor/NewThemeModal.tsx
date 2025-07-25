import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { useTheme } from "@/ui/helpers/UseThemeHelpers"

const NewThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { createTheme, setTheme } = useTheme()
    const [themeName, setThemeName] = useState<string>("")

    return (
        <Modal
            name="New Theme"
            icon={SynthesisIcons.ADD}
            modalId={modalId}
            acceptBlocked={!themeName}
            onAccept={() => {
                createTheme(themeName)
                setTheme(themeName)
                openModal("theme-editor")
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        >
            <Input placeholder={"Theme Name"} onInput={setThemeName} />
        </Modal>
    )
}

export default NewThemeModal
