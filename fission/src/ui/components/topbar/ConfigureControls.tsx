import { Box } from "@mui/material"
import type React from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import CollapsibleGroup, { type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import ConfigureSplitDropdown from "@/ui/components/topbar/ConfigureSplitDropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TOP_BAR_DIVIDER_SX, TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"
import { useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

const ConfigureControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { configureButtons, openConfig } = useConfigureAssembly(selectedAssembly)

    const disabledTooltip = selectedAssembly ? undefined : "Spawn an assembly first"

    const items: CollapsibleItem[] = [
        ...configureButtons.map(({ name, label, mode }) => ({
            key: label,
            node: (
                <TopBarButton
                    label={label}
                    icon={<TopBarIcon name={name} size={30} />}
                    disabledTooltip={disabledTooltip}
                    onClick={() => openConfig(mode)}
                />
            ),
        })),
        {
            key: "Move",
            node: (
                <TopBarButton
                    label="Move"
                    icon={
                        <Box sx={{ ...TOP_BAR_GLYPH_SX, fontSize: 22 }}>
                            <SynthesisIcons.MOVE />
                        </Box>
                    }
                    disabledTooltip={disabledTooltip}
                    onClick={() => openConfig(ConfigMode.MOVE)}
                />
            ),
        },
    ]

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <CollapsibleGroup
            items={items}
            always={
                <>
                    <Box sx={TOP_BAR_DIVIDER_SX} />
                    <ConfigureSplitDropdown selectedAssembly={selectedAssembly} />
                </>
            }
        />
    )
}

export default ConfigureControls
