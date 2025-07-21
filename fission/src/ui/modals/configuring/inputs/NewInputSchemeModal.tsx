import { TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import type { ModalImplProps } from "@/ui/components/Modal"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import { useStateContext } from "@/ui/StateProvider"
import { useUIContext } from "@/ui/UIProvider"

const NewInputSchemeModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openPanel } = useUIContext()
    const { setSelectedScheme, setConfigurationType } = useStateContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)

    useEffect(() => {
        const onAccept = () => {
            const scheme = DefaultInputs.newBlankScheme
            scheme.schemeName = name

            InputSchemeManager.addCustomScheme(scheme)
            InputSchemeManager.saveSchemes()

            setConfigurationType("INPUTS")
            setSelectedScheme(scheme)
            openPanel(<ConfigurePanel />, modal)
        }
        modal!.onAccept.setDefaultFunc(onAccept)
        modal!.props.hideCancel = true
    }, [name, setConfigurationType, setSelectedScheme, openPanel, modal])

    return <TextField label="Name" placeholder="" defaultValue={name} onChange={e => setName(e.target.value)} />
}

export default NewInputSchemeModal
