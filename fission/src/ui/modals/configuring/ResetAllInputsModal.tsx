import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "@/ui/ModalContext"
import DefaultInputs from "@/systems/input/DefaultInputs"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const ResetAllInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal
            name="Reset all Inputs??"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                const roboPrefs = PreferencesSystem.getAllRobotPreferences()

                // TODO: This will be improved to make more sense to a user in the "named inputs" PR 
                Object.values(roboPrefs).forEach(roboPref => {
                    roboPref.inputsSchemes.forEach(currentScheme => {
                        const resetScheme = DefaultInputs.ALL_INPUT_SCHEMES[0]
                        if (!currentScheme || !resetScheme) return

                        resetScheme.inputs.forEach(newInput => {
                            const currentInput = currentScheme.inputs.find(i => i.inputName == newInput.inputName)

                            if (currentInput) {
                                const inputIndex = currentScheme.inputs.indexOf(currentInput)

                                currentScheme.inputs[inputIndex] = newInput.getCopy()
                            }
                        })
                        currentScheme.usesGamepad = resetScheme.usesGamepad
                    })
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
