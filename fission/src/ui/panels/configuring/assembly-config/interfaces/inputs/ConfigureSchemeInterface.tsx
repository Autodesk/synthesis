import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import Checkbox from "@/ui/components/Checkbox"
import EditInputInterface from "./EditInputInterface"
import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import { SectionDivider } from "@/ui/components/StyledComponents"

interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

/** Interface to configure a specific input scheme */
const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    //const [selectedInput, setSelectedInput] = useState<Input | undefined>(undefined)
    const [useGamepad, setUseGamepad] = useState<boolean>(selectedScheme.usesGamepad)

    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes()
    }, [])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

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
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary">
                {selectedScheme.inputs.map(i => {
                    return (
                        <EditInputInterface
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
