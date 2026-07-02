import { useMediaQuery, useTheme } from "@mui/material"

/**
 * True when viewport is smaller than 1200px
 * Needs to be called inside ThemeProvider (in Synthesis.tsx) while mounted
 */
export function useIsMobile(): boolean {
    const theme = useTheme()
    return useMediaQuery(theme.breakpoints.down("lg"))
}
