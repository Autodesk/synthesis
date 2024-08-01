import React, { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import InputSystem, { Input } from "@/systems/input/InputSystem"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import EditInputInterface from "./EditInputInterface"
import Checkbox from "@/ui/components/Checkbox"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"

// Converts camelCase to Title Case for the inputs modal
const toTitleCase = (camelCase: string) => {
    const result = camelCase.replace(/([A-Z])/g, " $1")
    const finalResult = result.charAt(0).toUpperCase() + result.slice(1)
    return finalResult
}

const findSchemeRobotName = (scheme: InputScheme): string | undefined => {
    for (const [key, value] of InputSystem.brainIndexSchemeMap.entries()) {
        if (value == scheme) return SynthesisBrain.brainIndexMap.get(key)?.assemblyName
    }

    return undefined
}

class SchemeSelectionOption extends SelectMenuOption {
    scheme: InputScheme

    constructor(scheme: InputScheme) {
        const robotName = findSchemeRobotName(scheme)
        super(
            `${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}` +
                (robotName ? ` [${robotName}]` : "")
        )
        this.scheme = scheme
    }
}

class InputSelectionOption extends SelectMenuOption {
    input: Input

    constructor(input: Input) {
        super(toTitleCase(input.inputName))

        this.input = input
    }
}

interface ChangeInputsProps {}

const ConfigureInputsInterface: React.FC<ChangeInputsProps> = () => {
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [selectedInput, setSelectedInput] = useState<Input | undefined>(undefined)
    const [useGamepad, setUseGamepad] = useState<boolean>(false)

    const saveEvent = useCallback(() => {
        console.log("save")
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
            <SelectMenu
                options={InputSchemeManager.allInputSchemes.map(s => new SchemeSelectionOption(s))}
                onOptionSelected={val => {
                    setSelectedScheme((val as SchemeSelectionOption)?.scheme)
                    if (val == undefined) {
                        new ConfigurationSavedEvent()
                        setSelectedInput(undefined)
                    }
                }}
                defaultHeaderText={"Select an Input Scheme"}
            />
            {selectedScheme != undefined && (
                <>
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
                </>
            )}
            {selectedScheme != undefined && selectedInput != undefined && (
                <EditInputInterface
                    input={selectedInput}
                    useGamepad={useGamepad}
                    onInputChanged={() => (selectedScheme.customized = true)}
                />
            )}
        </>
    )
}

export default ConfigureInputsInterface
