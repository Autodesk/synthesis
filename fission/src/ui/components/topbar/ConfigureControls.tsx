import { Box } from "@mui/material"
import type React from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { ConfigureIcon } from "@/ui/components/topbar/ConfigureIcon"
import ConfigureSplitDropdown from "@/ui/components/topbar/ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

const ConfigureControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { configureButtons, openConfig } = useConfigureAssembly(selectedAssembly)

    const disabledTooltip = selectedAssembly ? undefined : "Spawn an assembly first"

    const items: CollapsibleItem[] = configureButtons.map(({ icon, label, mode }) => ({
        key: label,
        node: (
            <TopBarButton
                label={label}
                icon={<ConfigureIcon icon={icon} size={30} />}
                disabledTooltip={disabledTooltip}
                onClick={() => openConfig(mode)}
            />
        ),
    }))

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
