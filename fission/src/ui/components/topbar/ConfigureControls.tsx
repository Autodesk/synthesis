import { Stack } from "@mui/material"
import type React from "react"
import { IconButton } from "../StyledComponents"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

const CONFIGURE_BUTTONS: TopBarIconName[] = ["cfg-1", "cfg-2", "cfg-3", "cfg-4", "cfg-5"]

const ConfigureControls: React.FC = () => {
    return (
        <Stack direction="row" alignItems="center" gap={1}>
            {/* TODO: populate + wire selection from spawned assemblies (see AssemblySelection.tsx) */}
            <Stack
                direction="row"
                alignItems="center"
                justifyContent="space-between"
                sx={{
                    bgcolor: "surface.main",
                    color: "topBarText.main",
                    borderRadius: 2,
                    height: 38,
                    minWidth: 178,
                    px: 1.5,
                }}
            >
                [Ernie] Dozer
                <TopBarIcon name="carat-down" size={20} />
            </Stack>
            {CONFIGURE_BUTTONS.map(name => (
                <IconButton key={name} sx={{ color: "topBarText.main" }}>
                    <TopBarIcon name={name} size={22} />
                </IconButton>
            ))}
        </Stack>
    )
}

export default ConfigureControls
