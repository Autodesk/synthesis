export const TOP_BAR_HEIGHT = 48

const MUI_SPACING_UNIT_PX = 8

export const TOP_BAR_GAP = 1.5
export const TOP_BAR_GAP_PX = TOP_BAR_GAP * MUI_SPACING_UNIT_PX

export const TOP_BAR_ICON_BUTTON_SX = {
    color: "topBarText.main",
    borderRadius: 1,
    "&:hover": { backgroundColor: "rgba(255, 255, 255, 0.08)" },
    "&:focus, &:focus-visible": { outline: "none" },
} as const

export const TOP_BAR_ICON_BUTTON_ACTIVE_SX = {
    borderRadius: 1,
    color: "primary.main",
    backgroundColor: "color-mix(in srgb, currentColor 16%, transparent)",
    "&:hover": { backgroundColor: "color-mix(in srgb, currentColor 24%, transparent)" },
} as const

// making react-icon glyph consistent with buttons
export const TOP_BAR_GLYPH_SX = { fontSize: 26, display: "flex" } as const

export const TOP_BAR_DIVIDER_SX = { width: "2px", height: 28, bgcolor: "topBarText.main", opacity: 0.4 } as const

import type { MenuProps } from "@mui/material"

export const DROPDOWN_SELECT_SX = {
    bgcolor: "surface.main",
    color: "topBarText.main",
    cursor: "pointer",
    alignItems: "stretch", // ensures the inner select div stretches to full height
    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
    "& .MuiSelect-icon": { color: "topBarText.main", right: 8, pointerEvents: "none" },
} as const

export const DROPDOWN_MENU_ICON_SIZE = 18

export const DROPDOWN_MENU_PROPS = {
    marginThreshold: 8,
    slotProps: {
        paper: {
            sx: {
                bgcolor: "surface.main",
                color: "topBarText.main",
                "& .MuiMenuItem-root": {
                    color: "topBarText.main",
                    fontSize: 13,
                    px: 1.75,
                    "&:hover": { bgcolor: "topBar.main" },
                    "&.Mui-selected": {
                        bgcolor: "topBar.main",
                        "&:hover": { bgcolor: "topBar.main" },
                    },
                },
            },
        },
    },
} satisfies Partial<MenuProps>
