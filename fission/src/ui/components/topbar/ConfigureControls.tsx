import { Box, Stack } from "@mui/material"
import type React from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import ConfigureSplitDropdown from "./ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX } from "./TopBarConfig"
import { TopBarButton } from "./TopBarButton"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const ConfigureControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { configureButtons, openConfig } = useConfigureAssembly(selectedAssembly)

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            {configureButtons.map(({ name, label, mode }) => (
                <TopBarButton
                    key={label}
                    label={label}
                    icon={<TopBarIcon name={name} size={30} />}
                    disabledTooltip={selectedAssembly ? undefined : "Spawn an assembly first"}
                    onClick={() => openConfig(mode)}
                />
            ))}

            <Box sx={TOP_BAR_DIVIDER_SX} />

            <ConfigureSplitDropdown selectedAssembly={selectedAssembly} />
        </Stack>
    )
}

export default ConfigureControls
