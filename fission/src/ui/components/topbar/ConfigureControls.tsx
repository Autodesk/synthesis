import { Box, MenuItem, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { IoMdArrowDropdown } from "react-icons/io"
import { ConfigMode } from "../../panels/configuring/assembly-config/ConfigTypes"
import { useTourAnchor } from "@/ui/tour/useTourAnchor"
import { IconButton, Select } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./topBarConfig"
import { TopBarIcon } from "./TopBarIcons"
import { assemblyLabel, useConfigureAssembly } from "./useConfigureAssembly"

const ConfigureControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectedValue, selectAssemblyById } =
        useConfigureAssembly()

    const assemblySelectRef = useTourAnchor("configure-assembly-select")
    const intakeButtonRef = useTourAnchor("configure-intake-button")

    {
        /* TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow */
    }
    return (
        <Stack direction="row" alignItems="center" gap={2}>
            <Box ref={assemblySelectRef} component="span" sx={{ display: "inline-flex" }}>
                <Select
                    displayEmpty
                    value={selectedValue}
                    onChange={e => selectAssemblyById(e.target.value as string)}
                    renderValue={() =>
                        selectedConfigAssembly ? assemblyLabel(selectedConfigAssembly) : "Select an assembly"
                    }
                    IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
                    sx={{
                        bgcolor: "surface.main",
                        color: "topBarText.main",
                        borderRadius: 3,
                        height: 46,
                        minWidth: 260,
                        fontSize: 16,
                        cursor: "pointer",
                        alignItems: "stretch",
                        "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                        "& .MuiSelect-select": {
                            display: "flex",
                            alignItems: "center",
                            py: 0,
                            boxSizing: "border-box",
                        },
                        "& .MuiSelect-icon": { color: "topBarText.main", right: 14, pointerEvents: "none" },
                    }}
                >
                    {assemblies.length === 0 && (
                        <MenuItem value="" disabled>
                            No assemblies spawned
                        </MenuItem>
                    )}
                    {assemblies.map(assembly => (
                        <MenuItem key={assembly.id} value={assembly.id.toString()}>
                            {assemblyLabel(assembly)}
                        </MenuItem>
                    ))}
                </Select>
            </Box>
            {configureButtons.map(({ name, label, mode }) => (
                <Tooltip key={label} title={!selectedConfigAssembly ? "Spawn an assembly first" : label}>
                    <span ref={mode === ConfigMode.INTAKE ? intakeButtonRef : undefined}>
                        <IconButton
                            size="large"
                            disableRipple
                            disabled={!selectedConfigAssembly}
                            sx={{
                                ...TOP_BAR_ICON_BUTTON_SX,
                                ...(!selectedConfigAssembly && { opacity: 0.4 }),
                            }}
                            onClick={() => openConfig(mode)}
                        >
                            <TopBarIcon name={name} size={40} />
                        </IconButton>
                    </span>
                </Tooltip>
            ))}
        </Stack>
    )
}

export default ConfigureControls
