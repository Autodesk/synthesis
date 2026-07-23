import { Box, MenuItem, Stack } from "@mui/material"
import type React from "react"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { ConfigMode, type ConfigurationType } from "@/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/panels/configuring/assembly-config/ConfigurePanel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

type MenuOnlyConfig = { label: string; mode: ConfigMode; icon: React.ReactNode }

const MENU_ONLY_CONFIGS: MenuOnlyConfig[] = [
    { label: "Metadata", mode: ConfigMode.METADATA, icon: <SynthesisIcons.METADATA /> },
    { label: "Move", mode: ConfigMode.MOVE, icon: <SynthesisIcons.MOVE /> },
    { label: "Brain", mode: ConfigMode.BRAIN, icon: <SynthesisIcons.BRAIN /> },
]

const MenuIcon: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <Box sx={{ width: 18, height: 18, fontSize: 16, display: "flex", alignItems: "center", justifyContent: "center" }}>
        {children}
    </Box>
)

/** split dropdown gear icon in configure and codesim menu to see all configuration options */
const ConfigureSplitDropdown: React.FC = () => {
    const { openPanel } = useUIContext()
    const { selectedConfigAssembly, configureButtons, isField, openConfig } = useConfigureAssembly()

    // fields have no brain to configure
    const menuOnlyConfigs = isField ? MENU_ONLY_CONFIGS.filter(c => c.mode !== ConfigMode.BRAIN) : MENU_ONLY_CONFIGS

    const configMenuItem = (
        key: string,
        icon: React.ReactNode,
        label: string,
        mode: ConfigMode,
        closeMenu: () => void
    ) => (
        <MenuItem
            key={key}
            dense
            disabled={!selectedConfigAssembly}
            onClick={() => {
                openConfig(mode)
                closeMenu()
            }}
        >
            <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
                {icon}
                {label}
            </Stack>
        </MenuItem>
    )

    return (
        <SplitButtonDropdown
            icon={
                <Box sx={{ fontSize: 22, display: "flex" }}>
                    <SynthesisIcons.GEAR />
                </Box>
            }
            iconTooltip="Configure Assets"
            caretTooltip="Configure options"
            onIconClick={() =>
                openPanel(ConfigurePanel, {
                    selectedAssembly: selectedConfigAssembly,
                    configurationType: (selectedConfigAssembly?.miraType === MiraType.FIELD
                        ? "FIELDS"
                        : "ROBOTS") as ConfigurationType,
                })
            }
            renderMenu={closeMenu => [
                ...configureButtons.map(({ name, label, mode }) =>
                    configMenuItem(label, <TopBarIcon name={name} size={18} />, label, mode, closeMenu)
                ),
                ...menuOnlyConfigs.map(({ label, mode, icon }) =>
                    configMenuItem(label, <MenuIcon>{icon}</MenuIcon>, label, mode, closeMenu)
                ),
            ]}
        />
    )
}

export default ConfigureSplitDropdown
