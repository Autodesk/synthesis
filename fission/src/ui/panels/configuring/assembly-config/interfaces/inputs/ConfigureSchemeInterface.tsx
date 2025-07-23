import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import { Checkbox, Divider, FormControlLabel, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import EditInputInterface from "./EditInputInterface"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    const [useGamepad, setUseGamepad] = useState(selectedScheme.usesGamepad)
    const [useTouchControls, setUseTouchControls] = useState(selectedScheme.usesTouchControls)
    const scrollRef = useRef<HTMLDivElement>(null)

    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes()
    }, [])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
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
            {/** Toggle the input scheme between controller and keyboard mode */}
            <StatefulCheckbox
                label="Use Controller"
                checked={useGamepad}
                onClick={val => {
                    setUseGamepad(val)
                    if (val) {
                        setUseTouchControls(false)
                        selectedScheme.usesTouchControls = false
                    }
                    selectedScheme.usesGamepad = val
                }}
                tooltipText="Supported controllers: Xbox one, Xbox 360."
            />
            <StatefulCheckbox
                label="Use Touch Controls"
                checked={useTouchControls}
                onClick={val => {
                    setUseTouchControls(val)
                    if (val) {
                        setUseGamepad(false)
                        selectedScheme.usesGamepad = false
                    }
                    selectedScheme.usesTouchControls = val
                }}
                tooltipText="Enable on-screen touch controls (only for mobile devices)."
            />
            <Divider />

            {/* Scroll view for inputs */}
            <Stack ref={scrollRef} gap={2}>
                {selectedScheme.inputs.map(i => {
                    return (
                        <EditInputInterface
                            key={i.inputName}
                            input={i}
                            useGamepad={useGamepad}
                            useTouchControls={useTouchControls}
                            onInputChanged={() => {
                                selectedScheme.customized = true
                            }}
                        />
                    )
                })}
            </Stack>
        </>
    )
}

export default ConfigureSchemeInterface
