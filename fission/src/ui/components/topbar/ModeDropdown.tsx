import { MenuItem, Stack } from "@mui/material"
import { IoMdArrowDropdown } from "react-icons/io"
import type React from "react"
import { APP_MODES, type AppMode } from "@/systems/AppMode"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { Select } from "../StyledComponents"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

export const MODE_ICONS: Record<AppMode, TopBarIconName> = {
    Configure: "mode-configure",
    Codesim: "mode-codesim",
    Gameplay: "mode-gameplay",
}

const ModeLabel: React.FC<{ mode: AppMode }> = ({ mode }) => (
    <Stack direction="row" alignItems="center" gap={1.5} sx={{ pointerEvents: "none" }}>
        <TopBarIcon name={MODE_ICONS[mode]} size={24} />
        {mode}
    </Stack>
)

const ModeDropdown: React.FC<{ onOpenChange?: (open: boolean) => void }> = ({ onOpenChange }) => {
    const { appMode, setAppMode } = useStateContext()

    return (
        <Select
            value={appMode}
            onChange={e => setAppMode(e.target.value as AppMode)}
            onOpen={() => onOpenChange?.(true)}
            onClose={() => onOpenChange?.(false)}
            renderValue={value => <ModeLabel mode={value as AppMode} />}
            IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
            sx={{
                bgcolor: "surface.main",
                color: "topBarText.main",
                borderRadius: 3,
                height: 46,
                minWidth: 180,
                fontSize: 17,
                cursor: "pointer",
                alignItems: "stretch", // Ensures the inner select div stretches to full height
                "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
                "& .MuiSelect-icon": { color: "topBarText.main", right: 14, pointerEvents: "none" },
            }}
        >
            {APP_MODES.map(mode => (
                <MenuItem key={mode} value={mode}>
                    <ModeLabel mode={mode} />
                </MenuItem>
            ))}
        </Select>
    )
}

export default ModeDropdown
