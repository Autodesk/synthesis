import { Box, Divider, FormControl, InputLabel, MenuItem, Stack, Tooltip } from "@mui/material"
import { type ReactElement, useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { type InputScheme, type InputSchemeAvailability, InputSchemeUseType } from "@/systems/input/InputTypes"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import { PositiveButton, SynthesisIcons, Select } from "@/ui/components/StyledComponents"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

interface SchemeSelectorProps {
    scheme: InputScheme
    panelId?: string
    brainIndex: number

    style?: React.CSSProperties
    message: string
    disabled?: boolean

    onSelect?: () => void
}

const SchemeSelector: React.FC<SchemeSelectorProps> = ({
    scheme,
    panelId,
    brainIndex,
    style,
    message,
    disabled = false,
    onSelect,
}): ReactElement | null => {
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
                                // Ensure touch controls are shown when a touch scheme is selected
                                if (scheme.usesTouchControls) {
                                    EventSystem.dispatch("SetTouchControlsVisibilityEvent", true)
                                }
                                EventSystem.dispatch("InputSchemeChanged", { panelId })
                                onSelect?.()
                            }}
                        >
                            <SynthesisIcons.SELECT_LARGE />
                        </PositiveButton>
                    </Box>
                </Stack>
            </Stack>
        </Tooltip>
    )
}

interface InputSchemeSelectionProps {
    brainIndex: number
    onSelect?: () => void
    panelId?: string
}

export default function InputSchemeSelection({ brainIndex, onSelect, panelId }: InputSchemeSelectionProps) {
    const { setSelectedScheme } = useStateContext()
    const [robotDriveType, setRobotDriveType] = useState<DriveType>(
        SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType ?? DriveType.ARCADE
    )
    const [availableSchemes, setAvailableSchemes] = useState<InputSchemeAvailability[]>()

    const refreshAvailableSchemes = useCallback(() => {
        const schemes = [...InputSchemeManager.availableInputSchemesByType(robotDriveType)]
        if (matchMedia("(hover: none)").matches) {
            // showing input schemes that support touch controls first (on mobile devices)
            schemes.sort((a, b) => (b.scheme.usesTouchControls ? 1 : 0) - (a.scheme.usesTouchControls ? 1 : 0))
        }
        setAvailableSchemes(schemes)
    }, [robotDriveType])

    useEffect(() => {
        // Initial load and when robotDriveType changes
        refreshAvailableSchemes()

        return EventSystem.listen("InputSchemeChanged", () => refreshAvailableSchemes())
    }, [refreshAvailableSchemes])

    const onSchemeSelected = useCallback(() => {
        onSelect?.()
    }, [onSelect])

    return (
        // A scroll view with buttons to select default and custom input schemes
        <>
            {/** The label and divider at the top of the scroll view */}
            <Divider />
            <FormControl fullWidth>
                <InputLabel id="input-scheme-drivetrain-type-label">Drivetrain Type</InputLabel>
                <Select
                    label="Drivetrain Type"
                    value={robotDriveType}
                    onChange={e => {
                        const newDriveType = e.target.value as DriveType
                        const brain = SynthesisBrain.brainIndexMap.get(brainIndex)
                        if (brain) {
                            brain.configureDriveBehavior(newDriveType)
                        }
                        setRobotDriveType(newDriveType)

                        const scheme = InputSchemeManager.applyCompatibleScheme(brainIndex)
                        if (scheme) setSelectedScheme(scheme)
                        EventSystem.dispatch("InputSchemeChanged", { panelId })
                    }}
                >
                    {[DriveType.TANK, DriveType.ARCADE, DriveType.SWERVE].map(dt => (
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
                    return (
                        <SchemeSelector
                            key={`available-${scheme.scheme.schemeName}`}
                            scheme={scheme.scheme}
                            panelId={panelId}
                            brainIndex={brainIndex}
                            message="Available"
                            onSelect={onSchemeSelected}
                        />
                    )
                })}
            {availableSchemes
                ?.filter(scheme => scheme.status == InputSchemeUseType.CONFLICT)
                .map((scheme, i) => {
                    return (
                        <div key={`conflict-${scheme.scheme.schemeName}`}>
                            {i == 0 && <Divider />}
                            <SchemeSelector
                                scheme={scheme.scheme}
                                panelId={panelId}
                                brainIndex={brainIndex}
                                style={{ filter: "brightness(60%)" }}
                                message={"Conflicts with " + scheme.conflictingSchemeNames}
                                onSelect={onSchemeSelected}
                            />
                        </div>
                    )
                })}
            {availableSchemes
                ?.filter(scheme => scheme.status == InputSchemeUseType.IN_USE)
                .map((scheme, i) => {
                    return (
                        <div key={`in-use-${scheme.scheme.schemeName}`}>
                            {i == 0 && <Divider />}
                            <SchemeSelector
                                scheme={scheme.scheme}
                                panelId={panelId}
                                brainIndex={brainIndex}
                                message="In Use"
                                disabled={true}
                                onSelect={onSchemeSelected}
                            />
                        </div>
                    )
                })}
        </>
    )
}
