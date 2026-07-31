import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import Label from "@/ui/components/Label"
import { SynthesisIcons } from "./StyledComponents"

interface CodeSimConnectionStatusProps {
    label: string
    className?: string
    hasSimBrain: () => boolean
    getIsConnected: () => boolean
}

const CodeSimConnectionStatus: React.FC<CodeSimConnectionStatusProps> = ({
    label,
    className,
    hasSimBrain,
    getIsConnected,
}) => {
    const [status, setStatus] = useState<boolean>(false)
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        const handle = setInterval(() => {
            setEnabled(hasSimBrain())
            setStatus(getIsConnected())
        }, 500)
        return () => clearInterval(handle)
    }, [hasSimBrain, getIsConnected])

    return enabled ? (
        <Stack
            direction="row"
            sx={{ bgcolor: "background.default" }}
            className={`select-none absolute right-1 py-2 px-4 rounded-lg gap-2 ${className ?? "top-1"}`}
        >
            {status ? (
                <SynthesisIcons.CHECK className="text-green-500 self-center" />
            ) : (
                <SynthesisIcons.XMARK className="text-cancel-button self-center" />
            )}
            <Label size="sm">{label}</Label>
        </Stack>
    ) : (
        <></>
    )
}

export default CodeSimConnectionStatus
