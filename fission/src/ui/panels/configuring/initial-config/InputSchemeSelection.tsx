import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { InputScheme, InputSchemeAvailability } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "@/ui/components/Label"
import {
    AddButtonInteractiveColor,
    DeleteButton,
    EditButton,
    PositiveButton,
    SectionDivider,
    SectionLabel,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { Box } from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "@/panels/configuring/assembly-config/ConfigurationType"
import { setSelectedScheme } from "@/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelectionProps from "./InputSchemeSelectionProps"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
import Dropdown from "@/components/Dropdown.tsx"

function InputSchemeSelection({ brainIndex, onSelect, onEdit, onCreateNew }: InputSchemeSelectionProps) {
    const [_, update] = useReducer(x => !x, false)
    const [robotDriveType, setRobotDriveType] = useState<DriveType>(
        SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType ?? DriveType.ARCADE
    )
    const [availableSchemes, setAvailableSchemes] = useState<InputSchemeAvailability>()
    useEffect(() => {
        setAvailableSchemes(InputSchemeManager.availableInputSchemesByType(robotDriveType))
    }, [robotDriveType])
    function SchemeSelector(scheme: InputScheme, isAvailable: boolean) {
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
                    <div style={{ filter: isAvailable ? "" : "brightness(60%)" }}>
                        <PositiveButton
                            value={SynthesisIcons.SelectLarge}
                            onClick={() => {
                                InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
                                onSelect?.()
                                update()
                            }}
                        />
                    </div>
                    {/** Edit button - same as select but opens the inputs modal */}
                    {EditButton(() => {
                        InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)

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
                            const schemes = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")

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
    }
    return (
        <>
            {/** A scroll view with buttons to select default and custom input schemes */}
            <>
                {/** The label and divider at the top of the scroll view */}
                <SectionDivider />
                <Dropdown
                    label="Drivetrain Type"
                    options={[DriveType.TANK, DriveType.ARCADE, DriveType.SWERVE]}
                    defaultValue={robotDriveType}
                    onSelect={val => {
                        const brain = SynthesisBrain.brainIndexMap.get(brainIndex)
                        if (brain) {
                            brain.configure(val)
                        }
                        setRobotDriveType(val)
                    }}
                />
                <SectionDivider />
                <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${availableSchemes?.available.length}/${(availableSchemes?.available.length ?? 0) + (availableSchemes?.with_conflict.length ?? 0)} Input Schemes`}
                </SectionLabel>
                <SectionDivider />

                {/** Creates list items with buttons */}
                {availableSchemes?.available.map(scheme => {
                    return SchemeSelector(scheme, true)
                })}
                <SectionDivider />
                {availableSchemes?.with_conflict.map(scheme => {
                    return SchemeSelector(scheme, false)
                })}
            </>
            {/** New scheme with a randomly assigned name button */}
            {AddButtonInteractiveColor(() => {
                InputSystem.brainIndexSchemeMap.set(brainIndex, DefaultInputs.newBlankScheme(robotDriveType))
                onCreateNew?.()
            })}
        </>
    )
}

export default InputSchemeSelection
