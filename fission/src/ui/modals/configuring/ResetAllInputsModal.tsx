import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import InputSchemeManager from "@/systems/input/InputSchemeManager"

const ResetAllInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal
            name="Reset all Inputs??"
            icon={<GrFormClose />}
            modalId={modalId}
            onAccept={() => {
                // Wipe global input scheme prefs
                PreferencesSystem.setGlobalPreference("InputSchemes", [])
                InputSystem.brainIndexSchemeMap.clear()
                InputSchemeManager.resetDefaultSchemes()
                PreferencesSystem.savePreferences()

                const assemblies = [...World.SceneRenderer.sceneObjects.entries()]
                    .filter(x => {
                        const y =
                            x[1] instanceof MirabufSceneObject &&
                            (x[1] as MirabufSceneObject).miraType == MiraType.ROBOT
                        return y
                    })
                    .map(x => x[0])

                assemblies.forEach(a => {
                    World.SceneRenderer.RemoveSceneObject(a)
                })

                // // Reset default schemes in scheme map
                // InputSystem.brainIndexSchemeMap.forEach((scheme, brainIndex) => {
                //     // If the scheme is in default, reset it to the default

                //     const defaults = InputSchemeManager.defaultInputSchemes

                //     defaults.forEach(d => {
                //         if (d.schemeName == scheme.schemeName) {
                //             InputSystem.brainIndexSchemeMap.set(brainIndex, d)
                //         }
                //         return
                //     })

                //     return
                // })

                // Regenerate blank schemes in scheme map and add them to preferences

                // Save preferences

                // let i = 0
                // InputSystem.allInputs.forEach(currentScheme => {
                //     const scheme = InputSchemeManager.availableInputSchemes[i]
                //     if (!currentScheme || !scheme) return

                //     scheme.inputs.forEach(newInput => {
                //         const currentInput = currentScheme.inputs.find(i => i.inputName == newInput.inputName)

                //         if (currentInput) {
                //             const inputIndex = currentScheme.inputs.indexOf(currentInput)

                //             currentScheme.inputs[inputIndex] = newInput.getCopy()
                //         }
                //     })
                //     currentScheme.usesGamepad = scheme.usesGamepad

                //     i++
                // })
                // openModal("change-inputs")
            }}
            onCancel={() => {
                openModal("change-inputs")
            }}
        ></Modal>
    )
}

export default ResetAllInputsModal
