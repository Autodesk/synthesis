import { Box, MenuItem, Stack } from "@mui/material"
import { IoMdArrowDropdown } from "react-icons/io"
import type React from "react"
import { APP_MODES, type AppMode } from "@/systems/AppMode"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { Select, SynthesisIcons } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, DROPDOWN_SELECT_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon, type TopBarIconName } from "@/ui/components/topbar/TopBarIcons"

export const MODE_LABELS: Record<AppMode, string> = {
    Configure: "Configure",
    Codesim: "Codesim",
    Gameplay: "Gameplay",
    MixAndMatch: "Mix and Match",
}

const MODE_ICONS: Record<AppMode, TopBarIconName | undefined> = {
    Configure: "mode-configure",
    Codesim: "mode-codesim",
    Gameplay: "mode-gameplay",
    MixAndMatch: undefined,
}

const ModeIcon: React.FC<{ mode: AppMode }> = ({ mode }) => {
    const iconName = MODE_ICONS[mode]
    if (!iconName) return <SynthesisIcons.SCREWDRIVER_WRENCH size={18} />
    return <TopBarIcon name={iconName} size={18} />
}

const ModeLabel: React.FC<{ mode: AppMode }> = ({ mode }) => (
    <Stack direction="row" alignItems="center" gap={1} sx={{ pointerEvents: "none" }}>
        <Box sx={{ display: "flex" }}>
            <ModeIcon mode={mode} />
        </Box>
        {MODE_LABELS[mode]}
    </Stack>
)

// TRANSITION: SYNTH-30
// Mix and Match mode is unfinished, only expose it outside of dev builds once it's ready to be shipped.
const VISIBLE_MODES = import.meta.env.DEV ? APP_MODES : APP_MODES.filter(mode => mode !== "MixAndMatch")

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
            MenuProps={DROPDOWN_MENU_PROPS}
            sx={{ ...DROPDOWN_SELECT_SX, borderRadius: 1, height: 34, minWidth: 135, fontSize: 13 }}
        >
            {VISIBLE_MODES.map(mode => (
                <MenuItem key={mode} value={mode}>
                    <ModeLabel mode={mode} />
                </MenuItem>
            ))}
        </Select>
    )
}

export default ModeDropdown
