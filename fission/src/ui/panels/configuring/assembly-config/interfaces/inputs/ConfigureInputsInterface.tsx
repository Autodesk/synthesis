import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import InputSystem from "@/systems/input/InputSystem"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import ConfigureSchemeInterface from "./ConfigureSchemeInterface"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useModalControlContext } from "@/ui/ModalContext"

let selectedScheme: InputScheme | undefined = undefined
// eslint-disable-next-line react-refresh/only-export-components
export function setSelectedScheme(scheme: InputScheme | undefined) {
    selectedScheme = scheme
}

function getSelectedScheme() {
    return selectedScheme
}

/** If a scheme is assigned to a robot, find the name of that robot */
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
            `${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`,
            robotName ? `Bound to: ${robotName}` : undefined
        )
        this.scheme = scheme
    }
}

const ConfigureInputsInterface = () => {
    const { openModal } = useModalControlContext()

    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(getSelectedScheme())
    const [schemes, setSchemes] = useState<InputScheme[]>(InputSchemeManager.allInputSchemes)

    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes()
    }, [])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            setSelectedScheme(undefined)
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    return (
        <>
            {/** Select menu with input schemes */}
            <SelectMenu
                options={schemes.map(s => new SchemeSelectionOption(s))}
                onOptionSelected={val => {
                    setSelectedScheme((val as SchemeSelectionOption)?.scheme)
                    if (val == undefined) {
                        new ConfigurationSavedEvent()
                    }
                }}
                defaultHeaderText={"Select an Input Scheme"}
                onDelete={val => {
                    if (!(val instanceof SchemeSelectionOption)) return

                    // Fetch current custom schemes
                    InputSchemeManager.saveSchemes()
                    InputSchemeManager.resetDefaultSchemes()

                    // Find the scheme to remove in preferences
                    const schemes = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")
                    const index = schemes.indexOf(val.scheme)

                    // If currently bound to a robot, remove the binding
                    for (const [key, value] of InputSystem.brainIndexSchemeMap.entries()) {
                        if (value == schemes[index]) {
                            InputSystem.brainIndexSchemeMap.delete(key)
                        }
                    }

                    // Find and remove this input scheme from preferences
                    schemes.splice(index, 1)

                    // Save to preferences
                    PreferencesSystem.setGlobalPreference("InputSchemes", schemes)
                    PreferencesSystem.savePreferences()

                    // Update UI with new schemes
                    setSchemes(InputSchemeManager.allInputSchemes)
                }}
                deleteCondition={val => {
                    if (!(val instanceof SchemeSelectionOption)) return false

                    return val.scheme.customized
                }}
                onAddClicked={() => {
                    openModal("new-scheme")
                }}
                defaultSelectedOption={options => {
                    if (options.length < 0 || !(options[0] instanceof SchemeSelectionOption)) return undefined

                    return options.find(o => (o as SchemeSelectionOption).scheme == getSelectedScheme())
                }}
            />
            {selectedScheme && <ConfigureSchemeInterface selectedScheme={selectedScheme} />}
        </>
    )
}

export default ConfigureInputsInterface
