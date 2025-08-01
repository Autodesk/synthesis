import { FormControl, InputLabel, MenuItem, Select, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState, useMemo } from "react"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import type { ModalImplProps } from "@/ui/components/Modal"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext, CloseType } from "@/ui/helpers/UIProviderHelpers"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import { Stack } from "@mui/system"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"

const NewInputSchemeModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openPanel, configureScreen, closeModal, addToast } = useUIContext()
    const { setSelectedScheme, setConfigurationType } = useStateContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)
    const [type, setType] = useState<DriveType>(DriveType.ARCADE)

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType === MiraType.ROBOT ? assembly : undefined
    }, [])

    const brainIndex = useMemo(() => {
        return targetAssembly ? SynthesisBrain.getBrainIndex(targetAssembly) : undefined
    }, [targetAssembly])

    useEffect(() => {
        const onBeforeAccept = () => {
            const trimmedName = name.trim()
            if (trimmedName === "") {
                closeModal(CloseType.Cancel)
                addToast("error", "Name cannot be empty")
                return
            }

            if (InputSchemeManager.allInputSchemes.map(s => s.schemeName).includes(trimmedName)) {
                closeModal(CloseType.Cancel)
                addToast("error", "Name already exists")
                return
            }

            const scheme = DefaultInputs.newBlankScheme(type)

            scheme.schemeName = trimmedName

            InputSchemeManager.addCustomScheme(scheme)
            InputSchemeManager.saveSchemes()
            console.log(InputSchemeManager.allInputSchemes)

            if (brainIndex !== undefined) {
                InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
            }

            setConfigurationType("INPUTS")
            setSelectedScheme(scheme)
            openPanel(<ConfigurePanel />, modal)
        }

        const onCancel = () => {
            for (const [brainIndex, scheme] of InputSystem.brainIndexSchemeMap.entries()) {
                if (!scheme.schemeName || scheme.schemeName.trim() === "") {
                    InputSystem.brainIndexSchemeMap.delete(brainIndex)
                }
            }
        }

        configureScreen(modal!, { title: "New Input Scheme" }, { onBeforeAccept, onCancel })
    }, [name, type, brainIndex, openPanel, modal, configureScreen, closeModal])

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
