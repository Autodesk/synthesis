import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const ResetAllInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal
            name="Reset all Inputs??"
            icon={SynthesisIcons.Xmark}
            modalId={modalId}
            onAccept={() => {
                // Wipe global input scheme prefs
                PreferencesSystem.setGlobalPreference("InputSchemes", [])
                InputSystem.brainIndexSchemeMap.clear()
                InputSchemeManager.resetDefaultSchemes()
                PreferencesSystem.savePreferences()

                // Remove all robot assemblies
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
            }}
            onCancel={() => {
                openModal("change-inputs")
            }}
        ></Modal>
    )
}

export default ResetAllInputsModal
