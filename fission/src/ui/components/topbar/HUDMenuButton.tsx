import { Box, Stack, Typography } from "@mui/material"
import type { FC, ReactNode } from "react"
import { IconButton } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./topBarConfig"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

type HUDMenuButtonProps = {
    label: string
    onClick: () => void
    iconName?: TopBarIconName
    icon?: ReactNode
    iconSize?: number | string
}

/**
 * Grid tile for the mobile HUD drawer: a large centered icon with a label
 * underneath. Reused by both the root menu and the Configure submenu.
 */
export const HUDMenuButton: FC<HUDMenuButtonProps> = ({
    label,
    onClick,
    iconName,
    icon,
    iconSize = "clamp(28px, 8vw, 48px)",
}) => (
    <Stack alignItems="center" gap={0.5} sx={{ width: "100%" }}>
        <IconButton
            size="large"
            disableRipple
            sx={{ ...TOP_BAR_ICON_BUTTON_SX, flexDirection: "column" }}
            onClick={onClick}
        >
            {iconName ? (
                <TopBarIcon name={iconName} size={iconSize} />
            ) : (
                <Box sx={{ fontSize: iconSize, display: "flex" }}>{icon}</Box>
            )}
        </IconButton>
        <Typography
            variant="caption"
            sx={{
                color: "topBarText.main",
                textAlign: "center",
                lineHeight: 1.2,
                fontSize: "clamp(0.75rem, 3.5vw, 0.9rem)",
                fontWeight: 600,
            }}
        >
            {label}
        </Typography>
    </Stack>
)

export default HUDMenuButton
