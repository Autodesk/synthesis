import type React from "react"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { ConfigMode } from "../ConfigTypes"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

export class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode

    constructor(name: string, configMode: ConfigMode, tooltip?: string) {
        super(name, name, tooltip)
        this.configMode = configMode
    }
}

interface ConfigModeSelectionProps {
    onModeSelected: (mode?: ConfigMode) => void
    modes: ConfigModeSelectionOption[]
}

const ConfigModeSelection: React.FC<ConfigModeSelectionProps> = ({ onModeSelected, modes }) => {
    const { configurePanelSettings } = useStateContext()
    const defaultSelected = modes.find(mode => mode.configMode === configurePanelSettings?.configMode)

    return (
        <SelectMenu
            options={modes}
            onOptionSelected={val => {
                onModeSelected((val as ConfigModeSelectionOption)?.configMode)
            }}
            defaultHeaderText="Select a Configuration Mode"
            // TODO:
            // indentation={1}
            defaultSelectedOption={defaultSelected}
        />
    )
}

export default ConfigModeSelection
