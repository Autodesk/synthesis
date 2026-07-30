import { Box, Menu, type MenuProps, Stack, type SxProps, type Theme, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useState } from "react"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { IconButton, type IconButtonSound, SynthesisIcons } from "@/ui/components/StyledComponents"
import { DROPDOWN_MENU_PROPS, TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"

const HALF_SX = {
    ...TOP_BAR_ICON_BUTTON_SX,
    height: "100%",
    borderRadius: 0,
    "&:hover": { backgroundColor: "rgba(255, 255, 255, 0.08)" },
} as const

interface HalfProps {
    onClick: React.MouseEventHandler<HTMLButtonElement>
    children: React.ReactNode
    tooltip?: string
    disabled?: boolean
    ariaLabel?: string
    sound?: IconButtonSound
    sx?: SxProps<Theme>
}

const Half: React.FC<HalfProps> = ({ onClick, children, tooltip, disabled, ariaLabel, sound, sx }) => {
    const button = (
        <IconButton
            size="medium"
            disableRipple
            disabled={disabled}
            aria-label={ariaLabel}
            onClick={onClick}
            sound={sound}
            sx={{ ...HALF_SX, ...sx }}
        >
            {children}
        </IconButton>
    )
    const wrapped = <span style={{ display: "flex", height: "100%" }}>{button}</span>
    return tooltip ? <Tooltip title={tooltip}>{wrapped}</Tooltip> : wrapped
}

interface SplitButtonDropdownProps {
    icon: React.ReactNode
    onIconClick: () => void
    children: React.ReactNode
    iconTooltip?: string
    caretTooltip?: string
    iconDisabled?: boolean
    caretDisabled?: boolean
    sx?: SxProps<Theme>
    menuProps?: Partial<MenuProps>
}

const SplitButtonDropdown: React.FC<SplitButtonDropdownProps> = ({
    icon,
    onIconClick,
    children,
    iconTooltip,
    caretTooltip,
    iconDisabled,
    caretDisabled,
    sx,
    menuProps,
}) => {
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
    const closeMenu = useCallback(() => setAnchorEl(null), [])

    const onItemSelect = useCallback(
        (e: React.MouseEvent) => {
            if (!(e.target as HTMLElement).closest("[role='menuitem']")) return
            SoundPlayer.getInstance().playDropdownSound()
            closeMenu()
        },
        [closeMenu]
    )

    return (
        <>
            <Stack direction="row" alignItems="stretch" sx={{ height: 34, borderRadius: 1, overflow: "hidden", ...sx }}>
                <Half tooltip={iconTooltip} disabled={iconDisabled} onClick={onIconClick}>
                    {icon}
                </Half>
                <Half
                    tooltip={caretTooltip}
                    disabled={caretDisabled}
                    ariaLabel="Open dropdown"
                    sound="dropdown"
                    onClick={e => setAnchorEl(e.currentTarget)}
                    sx={{ px: 0.25 }}
                >
                    <Box sx={{ fontSize: 18, display: "flex" }}>
                        <SynthesisIcons.DROPDOWN_CARET />
                    </Box>
                </Half>
            </Stack>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={closeMenu}
                onClick={onItemSelect}
                anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
                transformOrigin={{ vertical: "top", horizontal: "left" }}
                {...DROPDOWN_MENU_PROPS}
                {...menuProps}
            >
                {children}
            </Menu>
        </>
    )
}

export default SplitButtonDropdown
