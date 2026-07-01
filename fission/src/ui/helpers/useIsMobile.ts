import { useMediaQuery, useTheme } from "@mui/material"

/**
 * True when the viewport is narrower than the MUI `md` breakpoint (~900px).
 * Drives the mobile/desktop HUD switch (desktop TopBar vs. mobile drawer).
 *
 * Must be called inside the app's `ThemeProvider` so `theme.breakpoints` is
 * available. Reactive: re-renders when the viewport crosses the breakpoint.
 */
export function useIsMobile(): boolean {
    const theme = useTheme()
    return useMediaQuery(theme.breakpoints.down("md"))
}
