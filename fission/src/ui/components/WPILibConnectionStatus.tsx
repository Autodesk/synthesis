import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { FaCheck, FaXmark } from "react-icons/fa6"
import { getIsConnected, hasSimBrain } from "@/systems/simulation/wpilib_brain/WPILibState"
import Label from "@/ui/components/Label"

const WPILibConnectionStatus: React.FC = () => {
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
            className="select-none absolute right-1 top-1 py-2 px-4 rounded-lg gap-2"
        >
            {status ? (
                <FaCheck className="text-green-500 self-center" />
            ) : (
                <FaXmark className="text-cancel-button self-center" />
            )}
            <Label size="sm">Code Connection</Label>
        </Stack>
    ) : (
        <></>
    )
}

export default WPILibConnectionStatus
