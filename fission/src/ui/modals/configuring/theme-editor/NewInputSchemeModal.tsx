import React, { useState } from "react"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import DefaultInputs from "@/systems/input/DefaultInputs"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { usePanelControlContext } from "@/ui/PanelContext"
import { setSelectedScheme } from "@/ui/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"
import {
    ConfigurationType,
    setSelectedConfigurationType,
} from "@/ui/panels/configuring/assembly-config/ConfigurationType"

const NewInputSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openPanel } = usePanelControlContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)

    return (
        <Modal
            name="New Input Scheme"
            icon={SynthesisIcons.AddLarge}
            modalId={modalId}
            onAccept={() => {
                const scheme = DefaultInputs.newBlankScheme
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

export default NewInputSchemeModal
