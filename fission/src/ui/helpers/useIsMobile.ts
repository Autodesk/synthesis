import { useMediaQuery } from "@mui/material"

// true when finger is main pointer so phones and tablets
export function useIsTouchDevice(): boolean {
    return useMediaQuery("(hover: none) and (pointer: coarse)", { noSsr: true })
}

// min width desktop topbar needs before stuff overflow keep roughly synced with topbar contents
export const DESKTOP_MIN_WIDTH = 1000

// true when compact mobile hud should replace desktop topbar
export function useIsMobile(): boolean {
    const isTouch = useIsTouchDevice()
    const isPhoneSized = useMediaQuery("(max-width: 600px), (max-height: 600px)", { noSsr: true })
    // minus 0.02 makes max width mean strictly below 1000 so it still catch fractional widths like 999.5 from zoom or display scaling
    const isTooNarrowForTopBar = useMediaQuery(`(max-width: ${DESKTOP_MIN_WIDTH - 0.02}px)`, { noSsr: true })
    return (isTouch && isPhoneSized) || isTooNarrowForTopBar
}
