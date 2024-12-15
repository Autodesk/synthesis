import { useEffect, useState } from "react"
import Label, { LabelSize } from "./Label"
import { hasSimBrain, isConnected } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { FaCheck, FaXmark } from "react-icons/fa6"

export default function WPILibConnectionStatus() {
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
            <Label size={LabelSize.Small}>Code Connection</Label>
        </div>
    ) : (
        <></>
    )
}
