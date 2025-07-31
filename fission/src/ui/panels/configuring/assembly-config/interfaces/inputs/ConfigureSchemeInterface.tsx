import React, { useCallback, useEffect, useReducer, useRef, useState } from "react"
import Button from "@/components/Button.tsx"
import StatefulCheckbox from "@/components/StatefulCheckbox.tsx"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import { AxisInput } from "@/systems/input/InputSystem.ts"
import { SectionDivider } from "@/ui/components/StyledComponents"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"
import EditInputInterface from "./EditInputInterface"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

/** Interface to configure a specific input scheme */
const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    const [useGamepad, setUseGamepad] = useState(selectedScheme.usesGamepad)
    const [useTouchControls, setUseTouchControls] = useState(selectedScheme.usesTouchControls)
    const scrollRef = useRef<HTMLDivElement>(null)
    const [_, update] = useReducer(x => !x, false)
    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes()
    }, [])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.removeListener(saveEvent)
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
            <SectionDivider />

            {/* Scroll view for inputs */}
            <div ref={scrollRef} tabIndex={0} className="flex overflow-y-auto flex-col gap-2 bg-background-secondary">
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
                {
                    <Button
                        value={"Add Joint Control"}
                        onClick={() => {
                            const existingJointIndexes = selectedScheme.inputs
                                .map(input => parseInt(input.inputName.replace("joint ", "")))
                                .filter(val => !isNaN(val))
                            const newJointIndex = Math.max(0, ...existingJointIndexes) + 1
                            selectedScheme.inputs.push(AxisInput.unbound(`joint ${newJointIndex}`))
                            selectedScheme.customized = true
                            update()
                        }}
                    />
                }
            </div>
        </>
    )
}
export default ConfigureSchemeInterface
