import { MenuItem, Stack } from "@mui/material"
import { IoMdArrowDropdown } from "react-icons/io"
import type React from "react"
import { APP_MODES, type AppMode } from "@/systems/AppMode"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { Select } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, DROPDOWN_SELECT_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon, type TopBarIconName } from "@/ui/components/topbar/TopBarIcons"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"

export const MODE_ICONS: Record<AppMode, TopBarIconName> = {
    Configure: "mode-configure",
    Codesim: "mode-codesim",
    Gameplay: "mode-gameplay",
}

const ModeLabel: React.FC<{ mode: AppMode }> = ({ mode }) => (
    <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
        <TopBarIcon name={MODE_ICONS[mode]} size={18} />
        {mode}
    </Stack>
)

const ModeDropdown: React.FC<{ onOpenChange?: (open: boolean) => void }> = ({ onOpenChange }) => {
    const { appMode, setAppMode } = useStateContext()
    const { blockState } = useUIContext()
    return (
        <Select
            value={appMode}
            disabled={blockState.blocked}
            onChange={e => setAppMode(e.target.value as AppMode)}
            onOpen={() => onOpenChange?.(true)}
            onClose={() => onOpenChange?.(false)}
            renderValue={value => <ModeLabel mode={value as AppMode} />}
            IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
            MenuProps={DROPDOWN_MENU_PROPS}
            sx={{ ...DROPDOWN_SELECT_SX, borderRadius: 1, height: 34, minWidth: 135, fontSize: 13 }}
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
