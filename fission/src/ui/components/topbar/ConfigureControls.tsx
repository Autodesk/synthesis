import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { IconButton } from "../StyledComponents"
import { AssemblySelect } from "./AssemblySelect"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const ConfigureControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectedValue, selectAssemblyById } =
        useConfigureAssembly()

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <AssemblySelect
                assemblies={assemblies}
                selectedConfigAssembly={selectedConfigAssembly}
                selectedValue={selectedValue}
                onSelect={selectAssemblyById}
                sx={{ borderRadius: 1, height: 34, minWidth: 195, fontSize: 12 }}
            />
            {configureButtons.map(({ name, label, mode }) => (
                <Tooltip key={label} title={!selectedConfigAssembly ? "Spawn an assembly first" : label}>
                    <span>
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
