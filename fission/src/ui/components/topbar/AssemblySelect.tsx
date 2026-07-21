import { MenuItem, type SxProps, type Theme } from "@mui/material"
import type React from "react"
import { IoMdArrowDropdown } from "react-icons/io"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Select } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, DROPDOWN_SELECT_SX } from "./TopBarConfig"

type AssemblySelectProps = {
    assemblies: MirabufSceneObject[]
    selectedConfigAssembly?: MirabufSceneObject
    /** Current `<Select>` value: the selected assembly id as a string, or "" for none. */
    selectedValue: string
    onSelect: (id: string) => void
    /** Per-instance sizing layered on top of {DROPDOWN_SELECT_SX}. */
    sx?: SxProps<Theme>
}

/**
 * Borderless dropdown listing the spawned assemblies, shared by the desktop
 * ConfigureControls and the mobile Configure drawer. Shows a disabled
 * placeholder when nothing is spawned.
 */
export const AssemblySelect: React.FC<AssemblySelectProps> = ({
    assemblies,
    selectedConfigAssembly,
    selectedValue,
    onSelect,
    sx,
}) => (
    <Select
        displayEmpty
        value={selectedValue}
        onChange={e => onSelect(e.target.value as string)}
        renderValue={() => (selectedConfigAssembly ? selectedConfigAssembly.descriptiveName : "Select an assembly")}
        IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
        MenuProps={DROPDOWN_MENU_PROPS}
        sx={{ ...DROPDOWN_SELECT_SX, ...sx }}
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
)
