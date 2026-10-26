import type React from "react"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import type { ConfigMode } from "../ConfigTypes"

export class ConfigModeSelectionOption extends SelectMenuOption {
    constructor(
        name: string,
        public readonly configMode: ConfigMode,
        tooltip?: string
    ) {
        super(configMode.toString(), name, tooltip)
    }
}

interface ConfigModeSelectionProps {
    onModeSelected: (mode?: ConfigMode) => void
    modes: ConfigModeSelectionOption[]
    configMode?: ConfigMode
}

const ConfigModeSelection: React.FC<ConfigModeSelectionProps> = ({ onModeSelected, modes, configMode }) => {
    const defaultSelected = modes.find(mode => mode.configMode === configMode)

    return (
        <SelectMenu
            options={modes}
            onOptionSelected={val => {
                onModeSelected(val?.configMode)
            }}
            defaultHeaderText="Select a Configuration Mode"
            // TODO:
            // indentation={1}
            defaultSelectedOption={defaultSelected}
        />
    )
}

export default ConfigModeSelection
