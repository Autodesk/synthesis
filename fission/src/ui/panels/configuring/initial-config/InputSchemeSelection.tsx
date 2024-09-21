import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "@/ui/components/Label"
import {
    EditButton,
    DeleteButton,
    SelectButton,
    AddButtonInteractiveColor,
    SectionLabel,
    SectionDivider,
} from "@/ui/components/StyledComponents"
import { Box } from "@mui/material"
import { useReducer } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "@/panels/configuring/assembly-config/ConfigurePanel"
import { setSelectedScheme } from "@/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelectionProps from "./InputSchemeSelectionProps"

function InputSchemeSelection({ getBrainIndex, onSelect, onEdit, onCreateNew }: InputSchemeSelectionProps) {
    const [_, update] = useReducer(x => !x, false)

    return (
        <>
            {/** A scroll view with buttons to select default and custom input schemes */}
            <>
            {/** The label and divider at the top of the scroll view */}
                <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${InputSchemeManager.availableInputSchemes.length} Input Schemes`}
                </SectionLabel>
                <SectionDivider />

                {/** Creates list items with buttons */}
                {InputSchemeManager.availableInputSchemes.map(scheme => {
                    return (
                        <Box
                            component={"div"}
                            display={"flex"}
                            justifyContent={"space-between"}
                            alignItems={"center"}
                            gap={"1rem"}
                            key={scheme.schemeName}
                        >
                            <SectionLabel>
                                {`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                            </SectionLabel>
                            <Box
                                component={"div"}
                                display={"flex"}
                                flexDirection={"row-reverse"}
                                gap={"0.25rem"}
                                justifyContent={"center"}
                                alignItems={"center"}
                            >
                                {/** Select button */}
                                {SelectButton(() => {
                                    InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)
                                    onSelect?.()
                                })}
                                {/** Edit button - same as select but opens the inputs modal */}
                                {EditButton(() => {
                                    InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)

                                    setSelectedConfigurationType(ConfigurationType.INPUTS)
                                    setSelectedScheme(scheme)
                                    onEdit?.()
                                })}

                                {/** Delete button (only if the scheme is customized) */}
                                {scheme.customized ? (
                                    DeleteButton(() => {
                                        // Fetch current custom schemes
                                        InputSchemeManager.saveSchemes()
                                        InputSchemeManager.resetDefaultSchemes()
                                        const schemes =
                                            PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")

                                        // Find and remove this input scheme
                                        const index = schemes.indexOf(scheme)
                                        schemes.splice(index, 1)

                                        // Save to preferences
                                        PreferencesSystem.setGlobalPreference("InputSchemes", schemes)
                                        PreferencesSystem.savePreferences()

                                        update()
                                    })
                                ) : (
                                    <></>
                                )}
                            </Box>
                        </Box>
                    )
                })}
            </>
            {/** New scheme with a randomly assigned name button */}
            {AddButtonInteractiveColor(() => {
                InputSystem.brainIndexSchemeMap.set(getBrainIndex(), DefaultInputs.newBlankScheme)
                onCreateNew?.()
            })}
        </>
    )
}

export default InputSchemeSelection