import { Box, Divider, FormControl, InputLabel, MenuItem, Stack, Tooltip } from "@mui/material"
import { type ReactElement, useCallback, useEffect, useReducer, useState } from "react"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { type InputScheme, type InputSchemeAvailability, InputSchemeUseType } from "@/systems/input/InputTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import {
    Button,
    DeleteButton,
    EditButton,
    PositiveButton,
    SynthesisIcons,
    Select,
} from "@/ui/components/StyledComponents"
import { TouchControlsEvent, TouchControlsEventKeys } from "@/ui/components/TouchControls"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

interface InputSchemeSelectionProps {
    brainIndex: number
    onSelect?: () => void
    onEdit?: () => void
    onCreateNew?: () => void
    panelId?: string
}

export default function InputSchemeSelection({
    brainIndex,
    onSelect,
    onEdit,
    onCreateNew,
    panelId,
}: InputSchemeSelectionProps) {
    const { setSelectedScheme } = useStateContext()
    const [_, update] = useReducer(x => !x, false)
    const [robotDriveType, setRobotDriveType] = useState<DriveType>(
        SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType ?? DriveType.ARCADE
    )
    const [availableSchemes, setAvailableSchemes] = useState<InputSchemeAvailability[]>()

    const refreshAvailableSchemes = useCallback(() => {
        setAvailableSchemes(InputSchemeManager.availableInputSchemesByType(robotDriveType))
    }, [robotDriveType])

    useEffect(() => {
        // Initial load and when robotDriveType changes
        refreshAvailableSchemes()

        // Set up event listener for external scheme changes
        const handleSchemeChange = () => {
            refreshAvailableSchemes()
        }

        window.addEventListener("inputSchemeChanged", handleSchemeChange)
        return () => window.removeEventListener("inputSchemeChanged", handleSchemeChange)
    }, [refreshAvailableSchemes])

    const SchemeSelector = (
        scheme: InputScheme,
        style: React.CSSProperties,
        message: string,
        disabled: boolean = false,
        status?: InputSchemeUseType
    ): ReactElement | null => {
        if (scheme.usesTouchControls && !matchMedia("(hover: none)").matches) return null
        return (
            <Tooltip title={message} key={scheme.schemeName} placement={"left"}>
                <Stack
                    direction="row"
                    justifyContent={"space-between"}
                    alignItems={"center"}
                    gap={"1rem"}
                    key={scheme.schemeName}
                >
                    <Label size="sm">
                        {`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                    </Label>
                    <Stack direction="row-reverse" gap="0.25rem" justifyContent={"center"} alignItems={"center"}>
                        {/** Select button */}
                        <Box sx={style}>
                            <PositiveButton
                                disabled={disabled}
                                onClick={() => {
                                    InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
                                    // TODO: if touch controls, then ensure that they are enabled.
                                    if (scheme.usesTouchControls) {
                                        new TouchControlsEvent(TouchControlsEventKeys.JOYSTICK)
                                    }
                                    window.dispatchEvent(
                                        new CustomEvent("inputSchemeChanged", {
                                            detail: { panelId },
                                        })
                                    )
                                    onSelect?.()
                                    update()
                                }}
                            >
                                {SynthesisIcons.SELECT_LARGE}
                            </PositiveButton>
                        </Box>
                        {/** Edit button - same as select but opens the inputs modal */}
                        {EditButton(() => {
                            InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)

                            setSelectedScheme(scheme)
                            onEdit?.()
                        })}

                        {/** Delete button (only if the scheme is customized and not in use) */}
                        {scheme.customized && status !== InputSchemeUseType.IN_USE ? (
                            DeleteButton(() => {
                                // Fetch current custom schemes
                                InputSchemeManager.saveSchemes(panelId)
                                InputSchemeManager.resetDefaultSchemes(panelId)
                                const schemes = PreferencesSystem.getGlobalPreference("InputSchemes")

                                // Find and remove this input scheme
                                const index = schemes.indexOf(scheme)
                                schemes.splice(index, 1)

                                // Save to preferences
                                PreferencesSystem.setGlobalPreference("InputSchemes", schemes)
                                PreferencesSystem.savePreferences()

                                // Update the available schemes list to reflect the deletion
                                window.dispatchEvent(
                                    new CustomEvent("inputSchemeChanged", {
                                        detail: { panelId },
                                    })
                                )
                                update()
                            })
                        ) : (
                            <></>
                        )}
                    </Stack>
                </Stack>
            </Tooltip>
        )
    }
    return (
        <>
            {/** A scroll view with buttons to select default and custom input schemes */}
            <>
                {/** The label and divider at the top of the scroll view */}
                <Divider />
                <FormControl fullWidth>
                    <InputLabel id="input-scheme-drivetrain-type-label">Drivetrain Type</InputLabel>
                    <Select
                        label="Drivetrain Type"
                        value={robotDriveType}
                        onChange={e => {
                            const brain = SynthesisBrain.brainIndexMap.get(brainIndex)
                            if (brain) {
                                brain.configureDriveBehavior(e.target.value as DriveType)
                            }
                            setRobotDriveType(e.target.value as DriveType)
                        }}
                    >
                        {[DriveType.TANK, DriveType.ARCADE].map(dt => (
                            <MenuItem key={dt} value={dt}>
                                {dt}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                <Divider />
                <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${availableSchemes?.length} Input Schemes`}
                </Label>
                <Divider />

                {/** Creates list items with buttons */}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.AVAILABLE)
                    .map(scheme => {
                        return SchemeSelector(scheme.scheme, {}, "Available", false, scheme.status)
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.CONFLICT)
                    .map((scheme, i) => {
                        return (
                            <div key={`conflict-${scheme.scheme.schemeName}`}>
                                {i == 0 && <Divider />}
                                {SchemeSelector(
                                    scheme.scheme,
                                    { filter: "brightness(60%)" },
                                    "Conflicts with " + scheme.conflictingSchemeNames,
                                    false
                                )}
                            </div>
                        )
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.IN_USE)
                    .map((scheme, i) => {
                        return (
                            <div key={`in-use-${scheme.scheme.schemeName}`}>
                                {i == 0 && <Divider />}
                                {SchemeSelector(scheme.scheme, {}, "In Use", true, scheme.status)}
                            </div>
                        )
                    })}
            </>
            {/** New scheme with a randomly assigned name button */}
            <Button
                color="success"
                variant="outlined"
                onClick={() => {
                    onCreateNew?.()
                }}
            >
                {SynthesisIcons.ADD_LARGE}
            </Button>
        </>
    )
}
