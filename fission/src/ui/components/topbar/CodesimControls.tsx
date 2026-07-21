import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import { IconButton } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"
import { TopBarIcon } from "./TopBarIcons"
import { useConfigureAssembly } from "./UseConfigureAssembly"

const CodeSimControls: React.FC = () => {
    const { openConfig } = useConfigureAssembly()

    const openBrainConfiguration = () => openConfig(ConfigMode.BRAIN)

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <Tooltip title="Configure Brain">
                <IconButton size="medium" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={openBrainConfiguration}>
                    <TopBarIcon name="mode-codesim" size={30} />
                </IconButton>
            </Tooltip>
        </Stack>
    )
}

export default CodeSimControls
