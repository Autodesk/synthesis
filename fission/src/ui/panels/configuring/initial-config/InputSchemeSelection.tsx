import { Divider, Stack, Typography } from "@mui/material"
import { useContext, useReducer } from "react"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { type InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { AddButtonInteractiveColor, DeleteButton, EditButton, SelectButton } from "@/ui/components/StyledComponents"
import { StateContext } from "@/ui/StateProvider"

interface InputSchemeSelectionProps {
    brainIndex: number
    onSelect?: () => void
    onEdit?: () => void
    onCreateNew?: () => void
}

export default function InputSchemeSelection({ brainIndex, onSelect, onEdit, onCreateNew }: InputSchemeSelectionProps) {
    const [_, update] = useReducer(x => !x, false)

    const { setSelectedScheme, setConfigurationType } = useContext(StateContext)

    return (
        <>
            {/** A scroll view with buttons to select default and custom input schemes */}
            <Typography variant="h3">{`${InputSchemeManager.availableInputSchemes.length} Input Schemes`}</Typography>
            <Divider />
            {InputSchemeManager.availableInputSchemes.map(scheme => (
                <Stack justifyContent="space-between" alignItems="center" gap="1rem" key={scheme.schemeName}>
                    <Typography variant="h5">
                        {`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                    </Typography>
                    <Stack direction="row-reverse" gap="0.25rem" justifyContent="center" alignItems="center">
                        {SelectButton(() => {
                            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
                            onSelect?.()
                            update()
                        })}
                        {EditButton(() => {
                            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)

                            setConfigurationType("INPUTS")
                            setSelectedScheme(scheme)
                            onEdit?.()
                        })}

                        {scheme.customized &&
                            DeleteButton(() => {
                                InputSchemeManager.saveSchemes()
                                InputSchemeManager.resetDefaultSchemes()
                                const schemes = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")

                                const index = schemes.indexOf(scheme)
                                schemes.splice(index, 1)

                                PreferencesSystem.setGlobalPreference("InputSchemes", schemes)
                                PreferencesSystem.savePreferences()

                                update()
                            })}
                    </Stack>
                </Stack>
            ))}
            {AddButtonInteractiveColor(() => {
                InputSystem.brainIndexSchemeMap.set(brainIndex, DefaultInputs.newBlankScheme)
                onCreateNew?.()
            })}
        </>
    )
}
