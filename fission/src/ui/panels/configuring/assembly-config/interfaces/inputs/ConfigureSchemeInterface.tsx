import { Divider, MenuItem, Select, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useRef, useState } from "react"
import Checkbox from "@/components/Checkbox.tsx"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import type { InputScheme } from "@/systems/input/InputTypes"
import AxisInput from "@/systems/input/inputs/AxisInput.ts"
import type Input from "@/systems/input/inputs/Input"
import Label from "@/ui/components/Label"
import { Button, IconButton, SynthesisIcons } from "@/ui/components/StyledComponents"
import EditInputInterface from "./EditInputInterface"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
    panelId?: string
    onBack?: () => void
}

const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme, panelId, onBack }) => {
    const [useGamepad, setUseGamepad] = useState(selectedScheme.usesGamepad)
    const [useTouchControls, setUseTouchControls] = useState(selectedScheme.usesTouchControls)
    // account for zero indexing
    const [controllerNumber, setControllerNumber] = useState((selectedScheme.playerSlot ?? 0) + 1)
    const [connectedPlayerCount, setConnectedPlayerCount] = useState(InputSystem.getConnectedPlayerCount())
    const scrollRef = useRef<HTMLDivElement>(null)
    const [_, update] = useReducer(x => !x, false)
    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes(panelId)
    }, [panelId])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    useEffect(() => {
        const refreshGamepads = () => setConnectedPlayerCount(InputSystem.getConnectedPlayerCount())
        window.addEventListener("gamepadconnected", refreshGamepads)
        window.addEventListener("gamepaddisconnected", refreshGamepads)

        return () => {
            window.removeEventListener("gamepadconnected", refreshGamepads)
            window.removeEventListener("gamepaddisconnected", refreshGamepads)
        }
    }, [])

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
                checked={useGamepad}
                onClick={val => {
                    setUseGamepad(val)
                    if (val) {
                        setUseTouchControls(false)
                        selectedScheme.usesTouchControls = false
                    }
                    selectedScheme.usesGamepad = val
                    selectedScheme.customized = true
                }}
                tooltip="Supported controllers: Xbox one, Xbox 360."
            />
            {useGamepad &&
                (connectedPlayerCount > 0 ? (
                    <Select
                        value={controllerNumber}
                        onChange={e => {
                            const value = Number(e.target.value)
                            setControllerNumber(value)
                            selectedScheme.playerSlot = value - 1
                            selectedScheme.customized = true
                        }}
                    >
                        {Array.from({ length: connectedPlayerCount }, (_unused, slot) => (
                            <MenuItem key={`controller-${slot}`} value={slot + 1}>
                                {slot + 1}
                            </MenuItem>
                        ))}
                    </Select>
                ) : (
                    <Label size="sm">No controllers detected</Label>
                ))}
            <Checkbox
                label="Use Touch Controls"
                checked={useTouchControls}
                onClick={val => {
                    setUseTouchControls(val)
                    if (val) {
                        setUseGamepad(false)
                        selectedScheme.usesGamepad = false
                    }
                    selectedScheme.usesTouchControls = val
                    selectedScheme.customized = true
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
                            useGamepad={useGamepad}
                            useTouchControls={useTouchControls}
                            playerSlot={controllerNumber - 1}
                            onInputChanged={() => {
                                selectedScheme.customized = true
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
