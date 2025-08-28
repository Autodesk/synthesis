import { FormControl, InputLabel, MenuItem, TextField } from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import { Stack } from "@mui/system"
import type React from "react"
import { useEffect, useMemo, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"

const NewInputSchemeModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openPanel, configureScreen } = useUIContext()
    const { setSelectedScheme } = useStateContext()

    const [name, setName] = useState<string>(InputSchemeManager.randomAvailableName)
    const [type, setType] = useState<DriveType>(DriveType.ARCADE)
    const [nameError, setNameError] = useState<boolean>(false)
    const [nameErrorText, setNameErrorText] = useState<string>("")

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType === MiraType.ROBOT ? assembly : undefined
    }, [])

    const brainIndex = useMemo(() => {
        return targetAssembly ? SynthesisBrain.getBrainIndex(targetAssembly) : undefined
    }, [targetAssembly])

    useEffect(() => {
        const onBeforeAccept = () => {
            const scheme = DefaultInputs.newBlankScheme(type)

            scheme.schemeName = name

            InputSchemeManager.addCustomScheme(scheme, modal?.id)
            InputSchemeManager.saveSchemes(modal?.id)

            if (brainIndex !== undefined) {
                InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
            }

            window.dispatchEvent(
                new CustomEvent("inputSchemeChanged", {
                    detail: { modalId: modal?.id },
                })
            )

            setSelectedScheme(scheme)
            openPanel(
                ConfigurePanel,
                {
                    configMode: undefined,
                    selectedAssembly: undefined,
                    configurationType: "INPUTS",
                },
                modal,
                { position: "left" }
            )
        }

        configureScreen(modal!, { title: "New Input Scheme", disableAccept: nameError }, { onBeforeAccept })
    }, [name, type, brainIndex, openPanel, modal, configureScreen, nameError])

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setName(e.target.value)

        const trimmedName = e.target.value.trim()
        if (trimmedName === "") {
            setNameError(true)
            setNameErrorText("Name cannot be empty")
            return
        }

        if (InputSchemeManager.allInputSchemes.map(s => s.schemeName).includes(trimmedName)) {
            setNameError(true)
            setNameErrorText("Name already exists")
            return
        }

        setNameError(false)
        setNameErrorText("")
    }

    return (
        <>
            <Stack gap={2}>
                <TextField
                    label="Name"
                    placeholder=""
                    value={name}
                    onChange={handleNameChange}
                    error={nameError}
                    helperText={nameErrorText}
                />
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
