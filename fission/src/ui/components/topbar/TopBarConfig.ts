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
