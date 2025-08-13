import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import type { InputScheme } from "@/systems/input/InputTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { PanelImplProps } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import NewInputSchemeModal from "@/ui/modals/configuring/inputs/NewInputSchemeModal"
import type { ConfigurePanelCustomProps } from "../../ConfigurePanel"
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
        const schemeName = `${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName} | ${scheme.supportedDrivetrains.join(", ")}`
        super(schemeName, schemeName, robotName ? `Bound to: ${robotName}` : undefined)
        this.scheme = scheme
    }
}

const ConfigureInputsInterface: React.FC<PanelImplProps<void, ConfigurePanelCustomProps>> = ({ panel }) => {
    const { openModal, closePanel } = useUIContext()
    const { selectedScheme: currentSelectedScheme, setSelectedScheme: setGlobalSelectedScheme } = useStateContext()

    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(currentSelectedScheme)
    const [schemes, setSchemes] = useState<InputScheme[]>(InputSchemeManager.allInputSchemes)

    const saveEvent = useCallback(() => {
        InputSchemeManager.saveSchemes(panel?.id)
    }, [])

    const handleSchemeChange = useCallback(() => {
        const newSchemes = InputSchemeManager.allInputSchemes
        setSchemes(newSchemes)

        // If the currently selected scheme was deleted, close the panel
        if (selectedScheme && !newSchemes.includes(selectedScheme)) {
            if (panel) {
                setTimeout(() => {
                    closePanel(panel.id, CloseType.Overwrite)
                }, 0)
            }
        }
    }, [panel])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)
        window.addEventListener("inputSchemeChanged", handleSchemeChange)

        return () => {
            setSelectedScheme(undefined)
            setGlobalSelectedScheme(undefined)
            ConfigurationSavedEvent.removeListener(saveEvent)
            window.removeEventListener("inputSchemeChanged", handleSchemeChange)
        }
    }, [saveEvent, setGlobalSelectedScheme, handleSchemeChange])

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
                        InputSchemeManager.saveSchemes(panel?.id)
                        InputSchemeManager.resetDefaultSchemes(panel?.id)

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

                        // Fire event to notify of input scheme changes
                        window.dispatchEvent(
                            new CustomEvent("inputSchemeChanged", {
                                detail: { panelId: panel?.id },
                            })
                        )

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
                <ConfigureSchemeInterface
                    selectedScheme={selectedScheme}
                    panelId={panel?.id}
                    onBack={() => setSelectedScheme(undefined)}
                />
            )}
        </>
    )
}

export default ConfigureInputsInterface
