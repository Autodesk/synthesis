import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const AssignNewSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)

    return (
        <Modal
            name="New Input Scheme"
            icon={SynthesisIcons.Xmark}
            modalId={modalId}
            onAccept={() => {
                const scheme = InputSystem.brainIndexSchemeMap.get(SynthesisBrain.brainIndexMap.size - 1)

                if (scheme == undefined) return

                scheme.schemeName = name

                InputSystem.selectedScheme = scheme
                InputSchemeManager.addCustomScheme(scheme)

                InputSchemeManager.saveSchemes()

                openModal("change-inputs")
            }}
            cancelEnabled={false}
        >
            <Input label="Name" placeholder="" defaultValue={name} onInput={setName} />
        </Modal>
    )
}

export default AssignNewSchemeModal
