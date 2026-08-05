import { Divider, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useRef } from "react"
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
    setSelectedScheme: (scheme: InputScheme) => void
    panelId?: string
    registerCleanupFunction: CleanupRegisterFunction
    onBack?: () => void
}

const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({
    selectedScheme,
    setSelectedScheme,
    panelId,
    onBack,
    registerCleanupFunction,
}) => {
    const scrollRef = useRef<HTMLDivElement>(null)
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

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    /** Disable scrolling with arrow keys to stop accidentally scrolling when binding keys */
    useEffect(() => {
        const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
            if (event.key === "ArrowUp" || event.key === "ArrowDown") {
                event.preventDefault()
            }
        }

        const scrollElement = scrollRef.current
        if (scrollElement) {
            scrollElement.addEventListener("keydown", handleKeyDown as unknown as EventListener)
        }

        return () => {
            if (scrollElement) {
                scrollElement.removeEventListener("keydown", handleKeyDown as unknown as EventListener)
            }
        }
    }, [])

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
                checked={selectedScheme.usesGamepad}
                onClick={val => {
                    setSelectedScheme({
                        ...selectedScheme,
                        usesGamepad: val,
                        usesTouchControls: val ? false : selectedScheme.usesTouchControls,
                        customized: true,
                    })
                }}
                tooltip="Supported controllers: Xbox one, Xbox 360."
            />
            <Checkbox
                label="Use Touch Controls"
                checked={selectedScheme.usesTouchControls}
                onClick={val => {
                    setSelectedScheme({
                        ...selectedScheme,
                        usesTouchControls: val,
                        usesGamepad: val ? false : selectedScheme.usesGamepad,
                    })
                }}
                tooltip="Enable on-screen touch controls (only for mobile devices)."
            />
            <Divider />

            {/* Inputs list (let parent panel handle scrolling to avoid double scrollbars) */}
            <Stack ref={scrollRef} gap={2}>
                {selectedScheme.inputs.map((i: Input) => {
                    return (
                        <EditInputInterface
                            key={i.inputName}
                            input={i}
                            useGamepad={selectedScheme.usesGamepad}
                            useTouchControls={selectedScheme.usesTouchControls}
                            onInputChanged={() => {
                                setSelectedScheme({ ...selectedScheme })
                            }}
                        />
                    )
                })}
                <Button
                    onClick={() => {
                        const existingJointIndexes = selectedScheme.inputs
                            .map(input => parseInt(input.inputName.replace("joint ", "")))
                            .filter(val => !isNaN(val))
                        const newJointIndex = Math.max(0, ...existingJointIndexes) + 1
                        setSelectedScheme({
                            ...selectedScheme,
                            inputs: [...selectedScheme.inputs, AxisInput.unbound(`joint ${newJointIndex}`)],
                        })
                    }}
                >
                    Add Joint Control
                </Button>
            </Stack>
        </>
    )
}

export default ConfigureSchemeInterface
