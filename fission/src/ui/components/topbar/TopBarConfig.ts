/**
 * Height of the desktop top bar, in pixels. Single source of truth shared by the
 * TopBar layout and the vertical space it reserves from the 3D scene (see
 * SceneRenderer.sceneTopOffset). The top bar is desktop-only; when it is not
 * rendered (mobile) no space is reserved and the scene fills the viewport.
 */
export const TOP_BAR_HEIGHT = 48

/**
 * Shared `sx` for top bar icon buttons. Strips MUI's default circular hover
 * background so the bare icon shows on hover.
 */
export const TOP_BAR_ICON_BUTTON_SX = {
    color: "topBarText.main",
    "&:hover": { backgroundColor: "transparent" },
    "&:focus, &:focus-visible": { outline: "none" },
} as const

/**
 * Shared base `sx` for the borderless MUI `Select` dropdowns in the top bar and
 * mobile drawer (mode picker, assembly picker). Per-instance sizing (borderRadius,
 * height, minWidth, fontSize) is layered on at each call site.
 */
export const DROPDOWN_SELECT_SX = {
    bgcolor: "surface.main",
    color: "topBarText.main",
    cursor: "pointer",
    alignItems: "stretch", // ensures the inner select div stretches to full height
    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
    "& .MuiSelect-icon": { color: "topBarText.main", right: 14, pointerEvents: "none" },
} as const
