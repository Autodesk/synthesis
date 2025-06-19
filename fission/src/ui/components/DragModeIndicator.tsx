import { useEffect, useState } from "react"
import Label, { LabelSize } from "./Label"
import { FaHandPaper } from "react-icons/fa"

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

    return enabled ? (
        <div className="select-none absolute left-1 top-1 py-2 px-4 rounded-lg bg-gradient-to-r from-interactive-element-left to-interactive-element-right flex flex-row gap-2">
            <FaHandPaper className="text-main-text self-center" />
            <Label size={LabelSize.Small}>Drag Mode</Label>
        </div>
    ) : (
        <></>
    )
}
