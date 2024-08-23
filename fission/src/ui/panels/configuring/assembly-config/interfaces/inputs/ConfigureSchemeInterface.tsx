import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import Checkbox from "@/ui/components/Checkbox"
import EditInputInterface from "./EditInputInterface"
import { useCallback, useEffect, useRef, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import { SectionDivider } from "@/ui/components/StyledComponents"
import { MainHUD_AddToast } from "@/ui/components/MainHUD"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

/** Interface to configure a specific input scheme */
const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    const [useGamepad, setUseGamepad] = useState<boolean>(selectedScheme.usesGamepad)
    const [useTouchControls, setUseTouchControls] = useState<boolean>(selectedScheme.usesTouchControls)
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
                tooltipText="Supported controllers: Xbox one, Xbox 360."
            />
            <Checkbox
                label="Use Touch Controls"
                defaultState={selectedScheme.usesTouchControls}
                onClick={val => {
                    setUseTouchControls(val)
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
            </div>
        </>
    )
}
export default ConfigureSchemeInterface
