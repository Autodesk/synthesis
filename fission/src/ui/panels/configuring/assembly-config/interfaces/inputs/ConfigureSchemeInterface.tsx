import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import Checkbox from "@/ui/components/Checkbox"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import EditInputInterface from "./EditInputInterface"
import { useCallback, useEffect, useState } from "react"
import { Input } from "@/systems/input/InputSystem"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"

// Converts camelCase to Title Case for the inputs modal
const toTitleCase = (camelCase: string) => {
    const result = camelCase.replace(/([A-Z])/g, " $1")
    const finalResult = result.charAt(0).toUpperCase() + result.slice(1)
    return finalResult
}

class InputSelectionOption extends SelectMenuOption {
    input: Input

    constructor(input: Input) {
        super(toTitleCase(input.inputName))

        this.input = input
    }
}
interface ConfigSchemeProps {
    selectedScheme: InputScheme
}

const ConfigureSchemeInterface: React.FC<ConfigSchemeProps> = ({ selectedScheme }) => {
    const [selectedInput, setSelectedInput] = useState<Input | undefined>(undefined)
    const [useGamepad, setUseGamepad] = useState<boolean>(false)

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
        <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary">
            <Checkbox
                label="Use Controller"
                defaultState={selectedScheme.usesGamepad}
                onClick={val => {
                    setUseGamepad(val)
                    selectedScheme.usesGamepad = val
                }}
            />
            <SelectMenu
                options={selectedScheme.inputs.map(i => new InputSelectionOption(i))}
                onOptionSelected={val => setSelectedInput((val as InputSelectionOption)?.input)}
                defaultHeaderText={"Select an Input"}
                indentation={1}
            />

            {selectedInput != undefined && (
                <EditInputInterface
                    input={selectedInput}
                    useGamepad={useGamepad}
                    onInputChanged={() => (selectedScheme.customized = true)}
                />
            )}
        </div>
    )
}
export default ConfigureSchemeInterface
