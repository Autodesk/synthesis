import { Box, Divider, FormControl, InputLabel, MenuItem, Stack, Tooltip } from "@mui/material"
import { type ReactElement, useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
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
    SynthesisIcons,
    Select,
    PositiveButton,
} from "@/ui/components/StyledComponents"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useIsTouchDevice } from "@/ui/helpers/useIsMobile"

interface SchemeSelectorProps {
    scheme: InputScheme
    status?: InputSchemeUseType
    panelId?: string
    brainIndex: number

    message: string
    disabled?: boolean
    conflict?: boolean

    onSelect?: () => void
    onEdit?: () => void
}

const SchemeSelector: React.FC<SchemeSelectorProps> = ({
    scheme,
    status,
    panelId,
    brainIndex,
    message,
    disabled = false,
    conflict = false,
    onSelect,
    onEdit,
}): ReactElement | null => {
    const { setSelectedScheme } = useStateContext()
    const isTouch = useIsTouchDevice()

    if (scheme.usesTouchControls && !isTouch) return null
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
                    <Box>
                        <PositiveButton
                            style={conflict ? { filter: "brightness(60%)" } : {}}
                            disabled={disabled}
                            onClick={() => {
                                InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
                                // TODO: if touch controls, then ensure that they are enabled.
                                if (scheme.usesTouchControls) {
                                    EventSystem.dispatch("ToggleTouchControlsVisibilityEvent")
                                }
                                EventSystem.dispatch("InputSchemeChanged", { panelId })
                                onSelect?.()
                            }}
                        >
                            <SynthesisIcons.SELECT_LARGE />
                        </PositiveButton>
                    </Box>
                    {/** Edit button - same as select but opens the inputs modal */}
                    <EditButton
                        onClick={() => {
                            InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)

                            setSelectedScheme(scheme)
                            onEdit?.()
                        }}
                    />

                    {/** Delete button (only if the scheme is customized and not in use) */}
                    {scheme.customized && status !== InputSchemeUseType.IN_USE && (
                        <DeleteButton
                            onClick={() => {
                                // Fetch current custom schemes
                                InputSchemeManager.saveSchemes(panelId)
                                InputSchemeManager.resetDefaultSchemes(panelId)
                                const schemes = PreferencesSystem.getUserPreference("InputSchemes")

                                // Find and remove this input scheme
                                const index = schemes.indexOf(scheme)
                                schemes.splice(index, 1)

                                // Save to preferences
                                PreferencesSystem.setUserPreference("InputSchemes", schemes)
                                PreferencesSystem.savePreferences()

                                // Update the available schemes list to reflect the deletion
                                EventSystem.dispatch("InputSchemeChanged", { panelId })
                            }}
                        />
                    )}
                </Stack>
            </Stack>
        </Tooltip>
    )
}

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

        return EventSystem.listen("InputSchemeChanged", () => refreshAvailableSchemes())
    }, [refreshAvailableSchemes])

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
                            <div key={`avaiable-${scheme.scheme.schemeName}`}>
                                <SchemeSelector
                                    scheme={scheme.scheme}
                                    status={scheme.status}
                                    panelId={panelId}
                                    brainIndex={brainIndex}
                                    message="Available"
                                    onEdit={onEdit}
                                    onSelect={onSelect}
                                />
                            </div>
                        )
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.CONFLICT)
                    .map((scheme, i) => {
                        return (
                            <>
                                {i == 0 && <Divider />}
                                <div key={`conflict-${scheme.scheme.schemeName}`}>
                                    <SchemeSelector
                                        scheme={scheme.scheme}
                                        panelId={panelId}
                                        brainIndex={brainIndex}
                                        message={"Conflicts with " + scheme.conflictingSchemeNames}
                                        conflict={true}
                                        onEdit={onEdit}
                                        onSelect={onSelect}
                                    />
                                </div>
                            </>
                        )
                    })}
                {availableSchemes
                    ?.filter(scheme => scheme.status == InputSchemeUseType.IN_USE)
                    .map((scheme, i) => {
                        return (
                            <>
                                {i == 0 && <Divider />}
                                <div key={`in-use-${scheme.scheme.schemeName}`}>
                                    <SchemeSelector
                                        scheme={scheme.scheme}
                                        status={scheme.status}
                                        panelId={panelId}
                                        brainIndex={brainIndex}
                                        message="In Use"
                                        disabled={true}
                                        onEdit={onEdit}
                                        onSelect={onSelect}
                                    />
                                </div>
                            </>
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
                <SynthesisIcons.ADD_LARGE />
            </Button>
        </>
    )
}
