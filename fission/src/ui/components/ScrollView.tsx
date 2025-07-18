import { Box } from "@mui/material"
import type React from "react"

interface ScrollViewProps {
    maxHeight?: string
}

const ScrollView: React.FC<React.PropsWithChildren<ScrollViewProps>> = ({ children, maxHeight }) => {
    return (
        <Box
            sx={{
                width: "100%",
                overflowY: "scroll",
                maxHeight: maxHeight || "70vh",
            }}
        >
            {children}
        </Box>
    )
}

export default ScrollView
