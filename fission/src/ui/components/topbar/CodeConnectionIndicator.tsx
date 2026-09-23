import { Box, Tooltip } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

interface CodeConnectionIndicatorProps {
    label: string
    getIsConnected: () => boolean
}

/** small status glyph on the codesim menu reflecting code connection */
const CodeConnectionIndicator: React.FC<CodeConnectionIndicatorProps> = ({ label, getIsConnected }) => {
    const [connected, setConnected] = useState<boolean>(false)

    useEffect(() => {
        const handle = setInterval(() => setConnected(getIsConnected()), 500)
        return () => clearInterval(handle)
    }, [getIsConnected])

    const tooltip = connected ? `${label}: connected` : `${label}: not connected`

    return (
        // not a TopBarIcon since it is a stateful component
        <Tooltip title={tooltip}>
            <Box sx={TOP_BAR_GLYPH_SX}>
                {connected ? (
                    <SynthesisIcons.CODE_CONNECTION className="text-green-600 text-3xl" />
                ) : (
                    <SynthesisIcons.NO_CODE_CONNECTION className="text-red-400 text-3xl" />
                )}
            </Box>
        </Tooltip>
    )
}

export default CodeConnectionIndicator
