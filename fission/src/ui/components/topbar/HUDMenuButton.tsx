import { Box, Stack, Tooltip, Typography } from "@mui/material"
import type { FC, ReactNode } from "react"
import { IconButton } from "@/ui/components/StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarIcon, type TopBarIconName } from "@/ui/components/topbar/TopBarIcons"

type HUDMenuButtonProps = {
    label: string
    onClick: () => void
    iconName?: TopBarIconName
    icon?: ReactNode
    iconSize?: number | string
    disabled?: boolean
    disabledTooltip?: string
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
    iconSize = "clamp(32px, min(10vw, 12vh), 56px)",
    disabled = false,
    disabledTooltip,
}) => {
    const content = (
        <Stack
            alignItems="center"
            gap={"clamp(0px, 0.5vh, 4px)"}
            sx={{ width: "100%", ...(disabled && { opacity: 0.4, pointerEvents: "none" }) }}
        >
            <IconButton
                size="large"
                disableRipple
                sx={{ ...TOP_BAR_ICON_BUTTON_SX, flexDirection: "column", p: "clamp(4px, 1vh, 8px)" }}
                onClick={disabled ? undefined : onClick}
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
                    fontSize: "clamp(0.6rem, min(3.5vw, 2.5vh), 0.9rem)",
                    fontWeight: 600,
                }}
            >
                {label}
            </Typography>
        </Stack>
    )

    if (disabled && disabledTooltip) {
        return (
            <Tooltip title={disabledTooltip}>
                <span style={{ display: "contents" }}>{content}</span>
            </Tooltip>
        )
    }

    return content
}
