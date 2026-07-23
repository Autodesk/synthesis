import { Box, Menu, type MenuProps, Stack, type SxProps, type Theme, Tooltip } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { IconButton, SynthesisIcons } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"

interface SplitButtonDropdownProps {
    icon: React.ReactNode
    onIconClick: () => void
    renderMenu: (closeMenu: () => void) => React.ReactNode
    iconTooltip?: string
    caretTooltip?: string
    iconDisabled?: boolean
    caretDisabled?: boolean
    sx?: SxProps<Theme>
    menuProps?: Partial<MenuProps>
}

// partial styling that is consistent for all SplitButtonDropdowns
const HALF_SX = {
    ...TOP_BAR_ICON_BUTTON_SX,
    height: "100%",
    borderRadius: 0,
    "&:hover": { backgroundColor: "rgba(255, 255, 255, 0.08)" },
} as const

const SplitButtonDropdown: React.FC<SplitButtonDropdownProps> = ({
    icon,
    onIconClick,
    renderMenu,
    iconTooltip,
    caretTooltip,
    iconDisabled,
    caretDisabled,
    sx,
    menuProps,
}) => {
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
    const closeMenu = () => setAnchorEl(null)

    const iconButton = (
        <IconButton size="medium" disableRipple disabled={iconDisabled} onClick={onIconClick} sx={HALF_SX}>
            {icon}
        </IconButton>
    )

    const caretButton = (
        <IconButton
            size="medium"
            disableRipple
            disabled={caretDisabled}
            aria-label="Open dropdown"
            onClick={e => setAnchorEl(e.currentTarget)}
            sx={{ ...HALF_SX, px: 0.25 }}
        >
            <Box sx={{ fontSize: 18, display: "flex" }}>
                <SynthesisIcons.DROPDOWN_CARET />
            </Box>
        </IconButton>
    )

    const half = (button: React.ReactNode, tooltip?: string) => {
        const wrapped = <span style={{ display: "flex", height: "100%" }}>{button}</span>
        return tooltip ? <Tooltip title={tooltip}>{wrapped}</Tooltip> : wrapped
    }

    return (
        <>
            <Stack direction="row" alignItems="stretch" sx={{ height: 34, borderRadius: 1, overflow: "hidden", ...sx }}>
                {half(iconButton, iconTooltip)}
                {half(caretButton, caretTooltip)}
            </Stack>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={closeMenu}
                anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
                transformOrigin={{ vertical: "top", horizontal: "left" }}
                {...DROPDOWN_MENU_PROPS}
                {...menuProps}
            >
                {renderMenu(closeMenu)}
            </Menu>
        </>
    )
}

export default SplitButtonDropdown
