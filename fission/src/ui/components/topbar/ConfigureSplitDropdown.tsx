import { Box, MenuItem, Stack } from "@mui/material"
import type React from "react"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/panels/configuring/assembly-config/ConfigurePanel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"
import { useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

type ConfigEntry = { key: string; icon: React.ReactNode; label: string; mode: ConfigMode }

const MENU_ONLY_CONFIGS: Omit<ConfigEntry, "key">[] = [
    { label: "Metadata", mode: ConfigMode.METADATA, icon: <SynthesisIcons.METADATA /> },
    { label: "Move", mode: ConfigMode.MOVE, icon: <SynthesisIcons.MOVE /> },
    { label: "Brain", mode: ConfigMode.BRAIN, icon: <SynthesisIcons.BRAIN /> },
]

const MenuIcon: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <Box sx={{ width: 18, height: 18, fontSize: 16, display: "flex", alignItems: "center", justifyContent: "center" }}>
        {children}
    </Box>
)

const ConfigureSplitDropdown: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { togglePanel } = useUIContext()
    const { configureButtons, isField, configurationType, openConfig } = useConfigureAssembly(selectedAssembly)

    const menuOnlyConfigs = isField ? MENU_ONLY_CONFIGS.filter(c => c.mode !== ConfigMode.BRAIN) : MENU_ONLY_CONFIGS

    const entries: ConfigEntry[] = [
        ...configureButtons.map(({ name, label, mode }) => ({
            key: label,
            icon: <TopBarIcon name={name} size={18} />,
            label,
            mode,
        })),
        ...menuOnlyConfigs.map(({ label, mode, icon }) => ({
            key: label,
            icon: <MenuIcon>{icon}</MenuIcon>,
            label,
            mode,
        })),
    ]

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
            {entries.map(({ key, icon, label, mode }) => (
                <MenuItem key={key} dense disabled={!selectedAssembly} onClick={() => openConfig(mode)}>
                    <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
                        {icon}
                        {label}
                    </Stack>
                </MenuItem>
            ))}
        </SplitButtonDropdown>
    )
}

export default ConfigureSplitDropdown
