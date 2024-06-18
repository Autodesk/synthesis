import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ModalContext"
import { FaPlus } from "react-icons/fa6"
import Input from "@/components/Input"
import { useTheme } from "@/ThemeContext"

const NewThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { createTheme, setTheme } = useTheme()
    const [themeName, setThemeName] = useState<string>("")

    return (
        <Modal
            name="New Theme"
            icon={<FaPlus />}
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
