import { Box, Tooltip } from "@mui/material"
import React, { ReactElement, useEffect, useReducer, useState } from "react"
import Dropdown from "@/components/Dropdown.tsx"
import { ConfigurationType, setSelectedConfigurationType } from "@/panels/configuring/assembly-config/ConfigurationType"
import { setSelectedScheme } from "@/panels/configuring/assembly-config/interfaces/inputs/ConfigureInputsInterface"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, {
    InputScheme,
    InputSchemeAvailability,
    InputSchemeUseType,
} from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
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
import { TouchControlsEvent, TouchControlsEventKeys } from "@/ui/components/TouchControls"
import InputSchemeSelectionProps from "./InputSchemeSelectionProps"

const InputSchemeSelection: React.FC<InputSchemeSelectionProps> = ({ brainIndex, onSelect, onEdit, onCreateNew }) => {
    const [_, update] = useReducer(x => !x, false)
    const [robotDriveType, setRobotDriveType] = useState<DriveType>(
        SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType ?? DriveType.ARCADE
    )
    const [availableSchemes, setAvailableSchemes] = useState<InputSchemeAvailability[]>()
    useEffect(() => {
        setAvailableSchemes(InputSchemeManager.availableInputSchemesByType(robotDriveType))
    }, [robotDriveType])

    const SchemeSelector = (
        scheme: InputScheme,
        style: React.CSSProperties,
        message: string,
        disabled: boolean = false
    ): ReactElement | null => {
        if (scheme.usesTouchControls && !matchMedia("(hover: none)").matches) return null
        return (
            <Tooltip title={message} key={scheme.schemeName} placement={"left"}>
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
                        <div style={style}>
                            <PositiveButton
                                disabled={disabled}
                                value={SynthesisIcons.SELECT_LARGE}
                                onClick={() => {
                                    InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
                                    // TODO: if touch controls, then ensure that they are enabled.
                                    if (scheme.usesTouchControls) {
                                        new TouchControlsEvent(TouchControlsEventKeys.JOYSTICK)
                                    }
                                    setAvailableSchemes(InputSchemeManager.availableInputSchemesByType(robotDriveType))
                                    onSelect?.()
                                    update()
                                }}
                            />
                        </div>
                        {/** Edit button - same as select but opens the inputs modal */}
                        {EditButton(() => {
                            InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)

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
                                const schemes = PreferencesSystem.getGlobalPreference("InputSchemes")

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
            </Tooltip>
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
                    options={[DriveType.TANK, DriveType.ARCADE]}
                    defaultValue={robotDriveType}
                    onSelect={val => {
                        const brain = SynthesisBrain.brainIndexMap.get(brainIndex)
                        if (brain) {
                            brain.configureDriveBehavior(val)
                        }
                        setRobotDriveType(val)
                    }}
                />
                <SectionDivider />
                <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${availableSchemes?.length} Input Schemes`}
                </SectionLabel>
                <SectionDivider />

                {/** Creates list items with buttons */}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.AVAILABLE)
                    .map(scheme => {
                        return SchemeSelector(scheme.scheme, {}, "Available", false)
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.CONFLICT)
                    .map((scheme, i) => {
                        return (
                            <>
                                {i == 0 && <SectionDivider />}
                                {SchemeSelector(
                                    scheme.scheme,
                                    { filter: "brightness(60%)" },
                                    "Conflicts with " + scheme.conflictingSchemeNames,
                                    false
                                )}
                            </>
                        )
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.IN_USE)
                    .map((scheme, i) => {
                        return (
                            <>
                                {i == 0 && <SectionDivider />}
                                {SchemeSelector(scheme.scheme, {}, "In Use", true)}
                            </>
                        )
                    })}
            </>
            {/** New scheme with a randomly assigned name button */}
            {AddButtonInteractiveColor(() => {
                InputSystem.setBrainIndexSchemeMapping(brainIndex, DefaultInputs.newBlankScheme(robotDriveType))
                onCreateNew?.()
            })}
        </>
    )
}

export default InputSchemeSelection
