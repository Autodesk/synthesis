import { MenuItem, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { IoMdArrowDropdown } from "react-icons/io"
import { IconButton, Select } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const ConfigureControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, configureButtons, openConfig, selectedValue, selectAssemblyById } =
        useConfigureAssembly()

    {
        /* TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow */
    }
    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <Select
                displayEmpty
                value={selectedValue}
                onChange={e => selectAssemblyById(e.target.value as string)}
                renderValue={() =>
                    selectedConfigAssembly ? selectedConfigAssembly.descriptiveName : "Select an assembly"
                }
                IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
                sx={{
                    bgcolor: "surface.main",
                    color: "topBarText.main",
                    borderRadius: 1,
                    height: 34,
                    minWidth: 195,
                    fontSize: 12,
                    cursor: "pointer",
                    alignItems: "stretch",
                    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
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
                        {assembly.descriptiveName}
                    </MenuItem>
                ))}
            </Select>
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
