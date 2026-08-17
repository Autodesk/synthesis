import { MenuItem, type SxProps, type Theme } from "@mui/material"
import type React from "react"
import { IoMdArrowDropdown } from "react-icons/io"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Select } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, DROPDOWN_SELECT_SX } from "@/ui/components/topbar/TopBarConfig"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"

type AssemblySelectProps = {
    assemblies: MirabufSceneObject[]
    selectedAssembly?: MirabufSceneObject
    onSelect: (id: string) => void
    /** Per-instance sizing layered on top of {DROPDOWN_SELECT_SX}. */
    sx?: SxProps<Theme>
}

/** Borderless dropdown listing the spawned assemblies */
export const AssemblySelect: React.FC<AssemblySelectProps> = ({ assemblies, selectedAssembly, onSelect, sx }) => {
    const { blockState } = useUIContext()

    return (
        <Select
            disabled={blockState.blocked}
            displayEmpty
            value={selectedAssembly?.id.toString() ?? ""}
            onChange={e => onSelect(e.target.value as string)}
            renderValue={() => (selectedAssembly ? selectedAssembly.descriptiveName : "Select an assembly")}
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
}
