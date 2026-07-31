import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { getIsConnected, hasSimBrain } from "@/systems/simulation/ftc_brain/FTCState"
import Label from "@/ui/components/Label"
import { SynthesisIcons } from "./StyledComponents"

const FTCConnectionStatus: React.FC = () => {
    const [status, setStatus] = useState<boolean>(false)
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        const handle = setInterval(() => {
            setEnabled(hasSimBrain())
            setStatus(getIsConnected())
        }, 500)
        return () => clearInterval(handle)
    }, [])

    return enabled ? (
        <Stack
            direction="row"
            sx={{ bgcolor: "background.default" }}
            className="select-none absolute right-1 top-14 py-2 px-4 rounded-lg gap-2"
        >
            {status ? (
                <SynthesisIcons.CHECK className="text-green-500 self-center" />
            ) : (
                <SynthesisIcons.XMARK className="text-cancel-button self-center" />
            )}
            <Label size="sm">FTC Code Connection</Label>
        </Stack>
    ) : (
        <></>
    )
}

export default FTCConnectionStatus
