import React, { useState } from "react"
import Dropdown from "@/components/Dropdown.tsx"
import Input from "@/components/Input"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import {
    ConfigurationType,
    setSelectedConfigurationType,
} from "@/ui/panels/configuring/assembly-config/ConfigurationType"
import { setSelectedScheme } from "@/ui/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"

const NewInputSchemeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openPanel } = usePanelControlContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)
    const [type, setType] = useState<DriveType>(DriveType.ARCADE)
    return (
        <Modal
            name="New Input Scheme"
            icon={SynthesisIcons.ADD_LARGE}
            modalId={modalId}
            onAccept={() => {
                const scheme = DefaultInputs.newBlankScheme(type)
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
            <Dropdown
                label="Drive Type"
                options={[DriveType.TANK, DriveType.ARCADE]}
                defaultValue={type}
                onSelect={setType}
            />
        </Modal>
    )
}

export default NewInputSchemeModal
