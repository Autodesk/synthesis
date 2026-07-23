import { Box, Stack } from "@mui/material"
import type React from "react"
import { AssemblySelect } from "./AssemblySelect"
import ConfigureSplitDropdown from "./ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX } from "./TopBarConfig"
import { TopBarButton } from "./TopBarButton"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const ConfigureControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectAssemblyById } =
        useConfigureAssembly()

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <AssemblySelect
                assemblies={assemblies}
                selectedConfigAssembly={selectedConfigAssembly}
                onSelect={selectAssemblyById}
                sx={{ borderRadius: 1, height: 34, minWidth: 195, fontSize: 12 }}
            />

            {configureButtons.map(({ name, label, mode }) => (
                <TopBarButton
                    key={label}
                    label={label}
                    icon={<TopBarIcon name={name} size={30} />}
                    disabledTooltip={selectedConfigAssembly ? undefined : "Spawn an assembly first"}
                    onClick={() => openConfig(mode)}
                />
            ))}

            {/* Divider line */}
            <Box sx={TOP_BAR_DIVIDER_SX} />

            <ConfigureSplitDropdown />
        </Stack>
    )
}

export default ConfigureControls
