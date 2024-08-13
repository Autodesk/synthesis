import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import Checkbox from "@/ui/components/Checkbox"
import EditInputInterface from "./EditInputInterface"
import { useCallback, useEffect, useRef, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import { SectionDivider } from "@/ui/components/StyledComponents"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

/** Interface to configure a specific input scheme */
const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    const [useGamepad, setUseGamepad] = useState<boolean>(selectedScheme.usesGamepad)
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
            <Checkbox
                label="Use Controller"
                defaultState={selectedScheme.usesGamepad}
                onClick={val => {
                    setUseGamepad(val)
                    selectedScheme.usesGamepad = val
                }}
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
                            onInputChanged={() => {
                                selectedScheme.customized = true
                            }}
                        />
                    )
                })}
            </div>
        </>
    )
}
export default ConfigureSchemeInterface
