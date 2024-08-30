import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { usePanelControlContext } from "@/ui/PanelContext"
import { setSelectedScheme } from "@/ui/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"
import { setSelectedConfigurationType, ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigurationType"

const AssignNewSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openPanel } = usePanelControlContext()

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

                InputSchemeManager.addCustomScheme(scheme)
                InputSchemeManager.saveSchemes()

                setSelectedConfigurationType(ConfigurationType.INPUTS)
                setSelectedScheme(scheme)
                openPanel("configure")
            }}
            cancelEnabled={false}
        >
            <Input label="Name" placeholder="" defaultValue={name} onInput={setName} />
        </Modal>
    )
}

export default AssignNewSchemeModal
