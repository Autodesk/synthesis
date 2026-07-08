import { Box, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { ConfigMode } from "../../panels/configuring/assembly-config/ConfigTypes"
import { useTourAnchor } from "@/ui/tour/useTourAnchor"
import { IconButton } from "../StyledComponents"
import { AssemblySelect } from "./AssemblySelect"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const ConfigureControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectedValue, selectAssemblyById } =
        useConfigureAssembly()

    const assemblySelectRef = useTourAnchor("configure-assembly-select")
    const intakeButtonRef = useTourAnchor("configure-intake-button")

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <Box ref={assemblySelectRef} component="span" sx={{ display: "inline-flex" }}>
                <AssemblySelect
                    assemblies={assemblies}
                    selectedConfigAssembly={selectedConfigAssembly}
                    selectedValue={selectedValue}
                    onSelect={selectAssemblyById}
                    sx={{ borderRadius: 1, height: 34, minWidth: 195, fontSize: 12 }}
                />
            </Box>
            {configureButtons.map(({ name, label, mode }) => (
                <Tooltip key={label} title={!selectedConfigAssembly ? "Spawn an assembly first" : label}>
                    <span ref={mode === ConfigMode.INTAKE ? intakeButtonRef : undefined}>
                        <IconButton
                            size="medium"
                            disableRipple
                            disabled={!selectedConfigAssembly}
                            sx={{
                                ...TOP_BAR_ICON_BUTTON_SX,
                                ...(!selectedConfigAssembly && { opacity: 0.4 }),
                            }}
                            onClick={() => openConfig(mode)}
                        >
                            <TopBarIcon name={name} size={30} />
                        </IconButton>
                    </span>
                </Tooltip>
            ))}
        </Stack>
    )
}

export default ConfigureControls
