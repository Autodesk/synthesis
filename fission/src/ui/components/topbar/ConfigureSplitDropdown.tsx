import { Box } from "@mui/material"
import type React from "react"
import { useMemo } from "react"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/panels/configuring/assembly-config/ConfigurePanel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { ConfigureIcon } from "@/ui/components/topbar/ConfigureIcon"
import { DROPDOWN_MENU_ICON_SIZE } from "@/ui/components/topbar/TopBarConfig"
import { type ConfigureButton, useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

const MENU_ONLY_CONFIGS: ConfigureButton[] = [
    { label: "Metadata", mode: ConfigMode.METADATA, icon: { glyph: SynthesisIcons.METADATA } },
    { label: "Brain", mode: ConfigMode.BRAIN, icon: { glyph: SynthesisIcons.BRAIN } },
]

const ConfigureSplitDropdown: React.FC<{ selectedAssembly?: MirabufSceneObject; disabledMessage?: string }> = ({
    selectedAssembly,
    disabledMessage,
}) => {
    const { togglePanel, blockState } = useUIContext()
    const { configureButtons, isField, configurationType, openConfig } = useConfigureAssembly(selectedAssembly)

    const items = useMemo(() => {
        const menuOnlyConfigs = isField ? MENU_ONLY_CONFIGS.filter(c => c.mode !== ConfigMode.BRAIN) : MENU_ONLY_CONFIGS

        return [...configureButtons, ...menuOnlyConfigs].map(({ icon, label, mode }) => ({
            key: label,
            label,
            icon: <ConfigureIcon icon={icon} size={DROPDOWN_MENU_ICON_SIZE} />,
            disabled: disabledMessage != null,
            onSelect: () => openConfig(mode),
        }))
    }, [configureButtons, isField, disabledMessage, openConfig])

    return (
        <SplitButtonDropdown
            icon={
                <Box sx={{ fontSize: 22, display: "flex" }}>
                    <SynthesisIcons.GEAR />
                </Box>
            }
            iconTooltip={disabledMessage ?? "Configure Asset"}
            caretTooltip={disabledMessage ?? "Configure options"}
            caretDisabled={disabledMessage != null || blockState.blocked}
            iconDisabled={disabledMessage != null || blockState.blocked}
            onIconClick={() => togglePanel(ConfigurePanel, { selectedAssembly, configurationType })}
            items={items}
        />
    )
}

export default ConfigureSplitDropdown
