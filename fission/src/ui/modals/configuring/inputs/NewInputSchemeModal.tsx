import { FormControl, InputLabel, MenuItem, Select, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import type { ModalImplProps } from "@/ui/components/Modal"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import { Stack } from "@mui/system"

const NewInputSchemeModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openPanel, configureScreen } = useUIContext()
    const { setSelectedScheme, setConfigurationType } = useStateContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)
    const [type, setType] = useState<DriveType>(DriveType.ARCADE)

    useEffect(() => {
        const onBeforeAccept = () => {
            const scheme = DefaultInputs.newBlankScheme(type)
            scheme.schemeName = name

            InputSchemeManager.addCustomScheme(scheme)
            InputSchemeManager.saveSchemes()

            setConfigurationType("INPUTS")
            setSelectedScheme(scheme)
            openPanel(<ConfigurePanel />, modal)
        }
        configureScreen(modal!, { title: "New Input Scheme", hideCancel: true }, { onBeforeAccept })
    }, [name, setConfigurationType, setSelectedScheme, openPanel, modal])

    return (
        <>
            <Stack gap={2}>
                <TextField label="Name" placeholder="" defaultValue={name} onChange={e => setName(e.target.value)} />
                <FormControl fullWidth>
                    <InputLabel id="drive-type-label">Drive Type</InputLabel>
                    <Select
                        labelId="drive-type-label"
                        label="Drive Type"
                        value={type}
                        onChange={e => setType(e.target.value as DriveType)}
                    >
                        {[DriveType.TANK, DriveType.ARCADE].map(dt => (
                            <MenuItem key={dt} value={dt}>
                                {dt}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </Stack>
        </>
    )
}

export default NewInputSchemeModal
