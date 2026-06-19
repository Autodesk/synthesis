import { MenuItem, Stack } from "@mui/material"
import type React from "react"
import { APP_MODES, type AppMode } from "@/systems/AppMode"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { Select } from "../StyledComponents"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

const MODE_ICONS: Record<AppMode, TopBarIconName> = {
    Configure: "mode-configure",
    Codesim: "mode-codesim",
    Gameplay: "mode-gameplay",
}

const ModeLabel: React.FC<{ mode: AppMode }> = ({ mode }) => (
    <Stack direction="row" alignItems="center" gap={1.5}>
        <TopBarIcon name={MODE_ICONS[mode]} size={24} />
        {mode}
    </Stack>
)

const ModeDropdown: React.FC = () => {
    const { appMode, setAppMode } = useStateContext()

    return (
        <Select
            value={appMode}
            onChange={e => setAppMode(e.target.value as AppMode)}
            renderValue={value => <ModeLabel mode={value as AppMode} />}
            IconComponent={props => <TopBarIcon name="carat-down" size={22} className={props.className} />}
            sx={{
                bgcolor: "surface.main",
                color: "topBarText.main",
                borderRadius: 2,
                height: 46,
                minWidth: 200,
                fontSize: 17,
                "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0 },
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
