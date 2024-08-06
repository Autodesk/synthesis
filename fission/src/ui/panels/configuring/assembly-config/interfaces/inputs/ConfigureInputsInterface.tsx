import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import InputSystem from "@/systems/input/InputSystem"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import ConfigureSchemeInterface from "./ConfigureSchemeInterface"

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

const ConfigureInputsInterface = () => {
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)

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
            <SelectMenu
                options={InputSchemeManager.allInputSchemes.map(s => new SchemeSelectionOption(s))}
                onOptionSelected={val => {
                    setSelectedScheme((val as SchemeSelectionOption)?.scheme)
                    if (val == undefined) {
                        new ConfigurationSavedEvent()
                    }
                }}
                defaultHeaderText={"Select an Input Scheme"}
            />
            {selectedScheme && <ConfigureSchemeInterface selectedScheme={selectedScheme} />}
        </>
    )
}

export default ConfigureInputsInterface
