import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import type { InputScheme } from "@/systems/input/InputTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import NewInputSchemeModal from "@/ui/modals/configuring/inputs/NewInputSchemeModal"
import ConfigureSchemeInterface from "./ConfigureSchemeInterface"

/** If a scheme is assigned to a robot, find the name of that robot */
const findSchemeRobotName = (scheme: InputScheme): string | undefined => {
    for (const [key, value] of InputSystem.brainIndexSchemeMap.entries()) {
        if (value === scheme) return SynthesisBrain.brainIndexMap.get(key)?.assemblyName
    }

    return undefined
}

class SchemeSelectionOption extends SelectMenuOption {
    scheme: InputScheme

    constructor(scheme: InputScheme) {
        const robotName = findSchemeRobotName(scheme)
        const schemeName = `${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName} | ${scheme.supportedDrivetrains}`
        super(schemeName, schemeName, robotName ? `Bound to: ${robotName}` : undefined)
        this.scheme = scheme
    }
}

const ConfigureInputsInterface: React.FC = () => {
    const { openModal } = useUIContext()
    const { selectedScheme: currentSelectedScheme } = useStateContext()

    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(currentSelectedScheme)
    const [schemes, setSchemes] = useState<InputScheme[]>(InputSchemeManager.allInputSchemes)

    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes()
    }, [])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            setSelectedScheme(undefined)
            ConfigurationSavedEvent.removeListener(saveEvent)
        }
    }, [saveEvent])

    const schemeOptionMap = useMemo(() => {
        const map = new Map<InputScheme, SchemeSelectionOption>()
        schemes.forEach(x => map.set(x, new SchemeSelectionOption(x)))
        return map
    }, [schemes])

    return (
        <>
            {/** Select menu with input schemes */}
            {!selectedScheme ? (
                <SelectMenu
                    options={[...schemeOptionMap.values()]}
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
                        const schemes = PreferencesSystem.getGlobalPreference("InputSchemes")
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
                        openModal(NewInputSchemeModal, undefined)
                    }}
                    defaultSelectedOption={selectedScheme ? schemeOptionMap.get(selectedScheme) : undefined}
                />
            ) : (
                <ConfigureSchemeInterface selectedScheme={selectedScheme} />
            )}
        </>
    )
}

export default ConfigureInputsInterface
