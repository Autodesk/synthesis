import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import DefaultInputs from "@/systems/input/DefaultInputs"

const NewInputSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)

    return (
        <Modal
            name="New Input Scheme"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                const scheme = DefaultInputs.newBlankScheme
                scheme.schemeName = name

                InputSchemeManager.addCustomScheme(scheme)
                InputSchemeManager.saveSchemes()

                InputSystem.selectedScheme = scheme
                openModal("change-inputs")
            }}
            cancelEnabled={false}
        >
            <Input label="Name" placeholder="" defaultValue={name} onInput={setName} />
        </Modal>
    )
}

export default NewInputSchemeModal
