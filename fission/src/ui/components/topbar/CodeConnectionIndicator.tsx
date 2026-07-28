import { Box, Tooltip } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { getIsConnected, hasSimBrain } from "@/systems/simulation/wpilib_brain/WPILibState"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

/** small status glyph on the codesim menu reflecting wpilib code connection */
const CodeConnectionIndicator: React.FC = () => {
    const [connected, setConnected] = useState<boolean>(false)
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        const handle = setInterval(() => {
            setEnabled(hasSimBrain())
            setConnected(getIsConnected())
        }, 500)
        return () => clearInterval(handle)
    }, [])

    if (!enabled) return null

    const tooltip = connected ? "Code connection: connected" : "Code connection: not connected"

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
