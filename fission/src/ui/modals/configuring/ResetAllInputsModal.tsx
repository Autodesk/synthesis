import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSystem from "@/systems/input/InputSystem"
import InputSchemeManager from "@/systems/input/InputSchemeManager"

const ResetAllInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal
            name="Reset all Inputs??"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                let i = 0
                InputSystem.allInputs.forEach(currentScheme => {
                    const scheme = InputSchemeManager.availableInputSchemes[i]
                    if (!currentScheme || !scheme) return

                    scheme.inputs.forEach(newInput => {
                        const currentInput = currentScheme.inputs.find(i => i.inputName == newInput.inputName)

                        if (currentInput) {
                            const inputIndex = currentScheme.inputs.indexOf(currentInput)

                            currentScheme.inputs[inputIndex] = newInput.getCopy()
                        }
                    })
                    currentScheme.usesGamepad = scheme.usesGamepad

                    i++
                })
                openModal("change-inputs")
            }}
            onCancel={() => {
                openModal("change-inputs")
            }}
        ></Modal>
    )
}

export default ResetAllInputsModal
