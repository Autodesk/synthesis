import { useMediaQuery } from "@mui/material"

/**
 * True on touch-primary devices (finger is the primary pointer): phones AND tablets.
 * Use this to decide whether touch controls (on-screen joysticks) are relevant.
 * Client-only SPA, so matchMedia is always available (noSsr skips a double render).
 */
export function useIsTouchDevice(): boolean {
    return useMediaQuery("(hover: none) and (pointer: coarse)", { noSsr: true })
}

/**
 * Minimum viewport width (px) the desktop TopBar needs to lay out its controls
 * without running off-screen. Configure mode is the widest case (mode dropdown +
 * assembly select + up to 8 config buttons + the right-side action icons), which
 * needs ~966px in production; 1000 adds a small margin. Below this we fall back to
 * the compact MobileHUD. Keep roughly in sync with the TopBar's contents if
 * controls are added or removed.
 */
export const DESKTOP_MIN_WIDTH = 1000

/**
 * True when the compact MobileHUD should replace the desktop TopBar. That happens when
 * either:
 *  - the device is a touch-primary phone (touch AND short side <= 600px), or
 *  - the viewport is simply too narrow to fit the desktop TopBar without overflow.
 *
 * So tablets/iPads in landscape and normal desktops keep the TopBar, while phones,
 * portrait tablets, and small/narrow desktop windows get the MobileHUD. The two
 * conditions are complementary: the width floor covers ordinary narrow viewports,
 * and the phone-sized check still catches unusually wide-but-short touch devices.
 */
export function useIsMobile(): boolean {
    const isTouch = useIsTouchDevice()
    const isPhoneSized = useMediaQuery("(max-width: 600px), (max-height: 600px)", { noSsr: true })
    const isTooNarrowForTopBar = useMediaQuery(`(max-width: ${DESKTOP_MIN_WIDTH - 0.02}px)`, { noSsr: true })
    return (isTouch && isPhoneSized) || isTooNarrowForTopBar
}
