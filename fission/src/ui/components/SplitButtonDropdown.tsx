import { Box, Menu, MenuItem, type MenuProps, Stack, type SxProps, type Theme, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useState } from "react"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { IconButton, type IconButtonSound, SynthesisIcons } from "@/ui/components/StyledComponents"
import {
    DROPDOWN_MENU_ICON_SIZE,
    DROPDOWN_MENU_PROPS,
    TOP_BAR_ICON_BUTTON_SX,
} from "@/ui/components/topbar/TopBarConfig"

const ICON_SLOT_SX = { display: "flex", width: DROPDOWN_MENU_ICON_SIZE, fontSize: DROPDOWN_MENU_ICON_SIZE } as const

const HALF_SX = {
    ...TOP_BAR_ICON_BUTTON_SX,
    height: "100%",
    borderRadius: 0,
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
            // the icons are pngs wrapped in svgs so disabled `color` won't dim them so we have to fade the icon
            sx={{ ...HALF_SX, ...(disabled && { opacity: 0.4 }), ...sx }}
        >
            {children}
        </IconButton>
    )
    const wrapped = <span style={{ display: "flex", height: "100%" }}>{button}</span>
    return tooltip ? <Tooltip title={tooltip}>{wrapped}</Tooltip> : wrapped
}

export interface SplitButtonMenuItem {
    key: string
    label: string
    icon?: React.ReactNode
    selected?: boolean
    disabled?: boolean
    onSelect: () => void
}

interface SplitButtonDropdownProps {
    icon: React.ReactNode
    onIconClick: () => void
    items: SplitButtonMenuItem[]
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
    items,
    iconTooltip,
    caretTooltip,
    iconDisabled,
    caretDisabled,
    sx,
    menuProps,
}) => {
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
    const closeMenu = useCallback(() => setAnchorEl(null), [])

    const selectItem = useCallback(
        (item: SplitButtonMenuItem) => {
            SoundPlayer.getInstance().playDropdownSound()
            closeMenu()
            item.onSelect()
        },
        [closeMenu]
    )

    // !== undefined because row needs room for checkmark
    const reservesIconColumn = items.some(item => item.icon !== undefined || item.selected !== undefined)

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
                anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
                transformOrigin={{ vertical: "top", horizontal: "left" }}
                {...DROPDOWN_MENU_PROPS}
                {...menuProps}
            >
                {items.map(item => (
                    <MenuItem
                        key={item.key}
                        dense
                        disabled={item.disabled}
                        selected={item.selected}
                        onClick={() => selectItem(item)}
                        sx={{ gap: 1 }}
                    >
                        {reservesIconColumn && (
                            <Box sx={ICON_SLOT_SX}>
                                {item.icon ?? (item.selected ? <SynthesisIcons.CHECK /> : null)}
                            </Box>
                        )}
                        {item.label}
                    </MenuItem>
                ))}
            </Menu>
        </>
    )
}

export default SplitButtonDropdown
