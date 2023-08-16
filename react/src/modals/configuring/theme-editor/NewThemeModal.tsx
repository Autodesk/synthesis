import React, { FormEvent, useState } from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { useModalControlContext } from "../../../ModalContext"
import { FaPlus } from "react-icons/fa6"
import Input from "../../../components/Input"

const NewThemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext();
    const [themeName, setThemeName] = useState<string>("");

    return (
        <Modal
            name="New Theme"
            icon={<FaPlus />}
            modalId={modalId}
            acceptBlocked={!themeName}
            onAccept={() => {
                // create theme somehow
            }}
            onCancel={() => {
                openModal("theme-editor")
            }}
        >
            <Input placeholder={"Theme Name"} onInput={(e: FormEvent<HTMLInputElement>) => setThemeName(e.target.value)} />
        </Modal>
    )
}

export default NewThemeModal 
