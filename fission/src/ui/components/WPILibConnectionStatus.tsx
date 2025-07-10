import React, { useEffect, useState } from "react"
import { hasSimBrain, isConnected } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { FaCheck, FaXmark } from "react-icons/fa6"
import { Typography } from "@mui/material"

const WPILibConnectionStatus: React.FC = () => {
    const [status, setStatus] = useState<boolean>(false)
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        const handle = setInterval(() => {
            setEnabled(hasSimBrain())
            setStatus(isConnected)
        }, 500)
        return () => clearInterval(handle)
    }, [])

    return enabled ? (
        <div className="select-none absolute right-1 top-1 py-2 px-4 rounded-lg bg-background flex flex-row gap-2">
            {status ? (
                <FaCheck className="text-green-500 self-center" />
            ) : (
                <FaXmark className="text-cancel-button self-center" />
            )}
            <Typography variant="h6">Code Connection</Typography>
        </div>
    ) : (
        <></>
    )
}

export default WPILibConnectionStatus
