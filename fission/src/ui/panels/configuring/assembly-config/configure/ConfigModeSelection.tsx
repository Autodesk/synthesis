import type React from "react"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import type { ConfigMode } from "../ConfigurePanel"

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
    return (
        <SelectMenu
            options={modes}
            onOptionSelected={val => {
                onModeSelected((val as ConfigModeSelectionOption)?.configMode)
            }}
            defaultHeaderText="Select a Configuration Mode"
            indentation={1}
            defaultSelectedOption={undefined}
        />
    )
}

export default ConfigModeSelection
