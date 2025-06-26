import { useEffect, useState } from "react"
import Label, { LabelSize } from "./Label"
import { FaHandPaper } from "react-icons/fa"
import { Global_AddToast } from "./GlobalUIControls"

export default function DragModeIndicator() {
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        const handleDragModeToggle = (event: CustomEvent) => {
            setEnabled(event.detail.enabled)
        }

        window.addEventListener("dragModeToggled", handleDragModeToggle as EventListener)

        return () => {
            window.removeEventListener("dragModeToggled", handleDragModeToggle as EventListener)
        }
    }, [])

    const handleClick = () => {
        window.dispatchEvent(new CustomEvent("disableDragMode"))
        Global_AddToast?.("info", "Drag Mode", "Drag mode has been disabled")
    }

    return enabled ? (
        <div
            className="select-none absolute left-1 bottom-1 py-2 px-4 rounded-lg bg-gradient-to-r from-interactive-element-left to-interactive-element-right flex flex-row gap-2 cursor-pointer hover:opacity-80 transition-opacity"
            onClick={handleClick}
        >
            <FaHandPaper className="text-main-text self-center" />
            <Label size={LabelSize.Small}>Drag Mode</Label>
        </div>
    ) : (
        <></>
    )
}
