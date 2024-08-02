import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import DefaultInputs from "@/systems/input/DefaultInputs"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const NewInputSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)

    return (
        <Modal
            name="New Input Scheme"
            icon={SynthesisIcons.Add}
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
