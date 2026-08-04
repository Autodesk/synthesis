import { Box, MenuItem, Stack } from "@mui/material"
import type React from "react"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/panels/configuring/assembly-config/ConfigurePanel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { ConfigureIcon } from "@/ui/components/topbar/ConfigureIcon"
import { type ConfigureButton, useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

const MENU_ICON_SIZE = 18

const MENU_ONLY_CONFIGS: ConfigureButton[] = [
    { label: "Metadata", mode: ConfigMode.METADATA, icon: { glyph: SynthesisIcons.METADATA } },
    { label: "Brain", mode: ConfigMode.BRAIN, icon: { glyph: SynthesisIcons.BRAIN } },
]

const ConfigureSplitDropdown: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { togglePanel } = useUIContext()
    const { configureButtons, isField, configurationType, openConfig } = useConfigureAssembly(selectedAssembly)

    const menuOnlyConfigs = isField ? MENU_ONLY_CONFIGS.filter(c => c.mode !== ConfigMode.BRAIN) : MENU_ONLY_CONFIGS

    const entries: ConfigureButton[] = [...configureButtons, ...menuOnlyConfigs]

    return (
        <SplitButtonDropdown
            icon={
                <Box sx={{ fontSize: 22, display: "flex" }}>
                    <SynthesisIcons.GEAR />
                </Box>
            }
            iconTooltip="Configure Assets"
            caretTooltip="Configure options"
            onIconClick={() => togglePanel(ConfigurePanel, { selectedAssembly, configurationType })}
        >
            {entries.map(({ icon, label, mode }) => (
                <MenuItem key={label} dense disabled={!selectedAssembly} onClick={() => openConfig(mode)}>
                    <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
                        <ConfigureIcon icon={icon} size={MENU_ICON_SIZE} />
                        {label}
                    </Stack>
                </MenuItem>
            ))}
        </SplitButtonDropdown>
    )
}

export default ConfigureSplitDropdown
