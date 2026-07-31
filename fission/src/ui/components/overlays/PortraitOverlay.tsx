import { Box, Stack, Typography, useMediaQuery } from "@mui/material"
import type React from "react"
import { useIsMobile } from "@/ui/helpers/useIsMobile.ts"

/**
 * Full-screen overlay shown when a mobile device is held in portrait orientation.
 * Prompts the user to rotate their device to landscape for the best experience.
 */
const PortraitOverlay: React.FC = () => {
    const isMobile = useIsMobile()
    const isPortrait = useMediaQuery("(orientation: portrait)")

    if (!isMobile || !isPortrait) return null

    return (
        <Box
            sx={{
                position: "fixed",
                inset: 0,
                zIndex: 9999,
                bgcolor: "topBar.main",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
            }}
        >
            <Stack alignItems="center" gap={3} sx={{ px: 4, textAlign: "center" }}>
                <Box
                    component="img"
                    src="/synthesis-logo.svg"
                    alt="Synthesis"
                    sx={{ width: 80, height: 80, objectFit: "contain" }}
                />
                <Typography
                    variant="h6"
                    sx={{
                        color: "topBarText.main",
                        fontWeight: 700,
                    }}
                >
                    Rotate your device to landscape
                </Typography>
            </Stack>
        </Box>
    )
}

export default PortraitOverlay
