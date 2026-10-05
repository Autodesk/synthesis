import { Divider, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer } from "react"
import Checkbox from "@/components/Checkbox.tsx"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import type { InputScheme } from "@/systems/input/InputTypes"
import AxisInput from "@/systems/input/inputs/AxisInput.ts"
import type Input from "@/systems/input/inputs/Input"
import Label from "@/ui/components/Label"
import { Button, IconButton, SynthesisIcons } from "@/ui/components/StyledComponents"
import EditInputInterface from "./EditInputInterface"
import type { CleanupRegisterFunction } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
    panelId?: string
    registerCleanupFunction: CleanupRegisterFunction
    onBack?: () => void
}

const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({
    selectedScheme,
    panelId,
    onBack,
    registerCleanupFunction,
}) => {
    const [_, update] = useReducer(x => !x, false)
    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes(panelId)
    }, [panelId])

    useEffect(() => {
        const originalScheme: Partial<InputScheme> | undefined = structuredClone(selectedScheme)
        registerCleanupFunction(undefined, () => {
            if (originalScheme == null || selectedScheme == null || originalScheme.inputs == null) return

            // Can't assign inputs like other proeprties because they are classes and won't properly rehydrate
            selectedScheme.inputs.forEach((input, i) => Object.assign(input, originalScheme.inputs![i]))
            delete originalScheme.inputs

            Object.assign(selectedScheme, originalScheme)

            EventSystem.dispatch("InputSchemeChanged", {})
        })
    }, [registerCleanupFunction, selectedScheme])

    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", saveEvent), [saveEvent])

    return (
        <>
            {/** Back button to return to input scheme selection */}
            {onBack && (
                <>
                    <Stack direction="row" textAlign={"center"} minHeight={"30px"} key="selected-item">
                        {/** Back arrow button when an option is selected */}
                        <IconButton onClick={onBack} id="select-menu-back-button">
                            <SynthesisIcons.LEFT_ARROW_LARGE />
                        </IconButton>

                        <Stack alignSelf={"center"}>
                            <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                Back to Input Schemes
                            </Label>
                        </Stack>
                    </Stack>
                    <Divider />
                </>
            )}

            {/** Toggle the input scheme between controller and keyboard mode */}
            <Checkbox
                label="Use Controller"
                tooltip="Supported controllers: Xbox one, Xbox 360."
                checked={selectedScheme.usesGamepad}
                onClick={val => {
                    if (val) selectedScheme.usesTouchControls = false
                    selectedScheme.usesGamepad = val
                    selectedScheme.customized = true
                    update()
                }}
            />
            <Checkbox
                label="Use Touch Controls"
                tooltip="Enable on-screen touch controls (only for mobile devices)."
                checked={selectedScheme.usesTouchControls}
                onClick={val => {
                    if (val) selectedScheme.usesGamepad = false
                    selectedScheme.usesTouchControls = val
                    selectedScheme.customized = true
                    update()
                }}
            />
            <Divider />

            {/* Inputs list (let parent panel handle scrolling to avoid double scrollbars) */}
            <Stack
                gap={2}
                onKeyDown={event => {
                    // Disable scrolling with arrow keys to stop accidentally scrolling when binding keys
                    if (event.key === "ArrowUp" || event.key === "ArrowDown") event.preventDefault()
                }}
            >
                {selectedScheme.inputs.map((i: Input) => (
                    <EditInputInterface
                        key={i.inputName}
                        input={i}
                        useGamepad={selectedScheme.usesGamepad}
                        useTouchControls={selectedScheme.usesTouchControls}
                        onInputChanged={() => {
                            selectedScheme.customized = true
                        }}
                    />
                ))}
                <Button
                    onClick={() => {
                        const existingJointIndexes = selectedScheme.inputs
                            .map(input => parseInt(input.inputName.replace("joint ", "")))
                            .filter(val => !isNaN(val))
                        const newJointIndex = Math.max(0, ...existingJointIndexes) + 1
                        selectedScheme.inputs.push(AxisInput.unbound(`joint ${newJointIndex}`))
                        selectedScheme.customized = true
                        update()
                    }}
                >
                    Add Joint Control
                </Button>
            </Stack>
        </>
    )
}

export default ConfigureSchemeInterface
