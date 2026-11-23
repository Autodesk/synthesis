import CloseIcon from "@mui/icons-material/Close"
import { Box, IconButton, Stack, Typography, useMediaQuery } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { useIsMobile } from "@/ui/helpers/useIsMobile.ts"

/** "Rotate your device to landscape" screen; dismissible */
const PortraitOverlay: React.FC = () => {
    const isMobile = useIsMobile()
    const isPortrait = useMediaQuery("(orientation: portrait)")
    const [dismissed, setDismissed] = useState(false)

    if (!isMobile || !isPortrait || dismissed) return null

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
            <IconButton
                aria-label="Dismiss"
                onClick={() => setDismissed(true)}
                sx={{
                    position: "absolute",
                    top: theme => theme.spacing(1),
                    right: theme => theme.spacing(1),
                    color: "topBarText.main",
                }}
            >
                <CloseIcon />
            </IconButton>
            <Stack alignItems="center" gap={3} sx={{ px: 4, textAlign: "center" }}>
                <Box
                    component="img"
                    src={`${import.meta.env.BASE_URL}synthesis-logo.svg`}
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
