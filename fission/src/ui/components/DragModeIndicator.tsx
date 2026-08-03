import { Stack } from "@mui/material"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "./GlobalUIControls"
import Label from "./Label"
import { SynthesisIcons } from "./StyledComponents"

const DragModeIndicator: React.FC = () => {
    const [enabled, setEnabled] = useState<boolean>(false)

    useEffect(() => {
        return EventSystem.listen("DragModeToggled", ({ enabled }) => setEnabled(enabled))
    }, [])

    const handleClick = () => {
        EventSystem.dispatch("SetDragModeEvent", { enabled: false })
        globalAddToast("info", "Drag Mode", "Drag mode has been disabled")
    }

    return enabled ? (
        <Stack
            className="select-none absolute left-1 bottom-1 py-2 px-4 rounded-lg gap-2 cursor-pointer hover:opacity-80 transition-opacity"
            direction="row"
            onClick={handleClick}
            sx={{
                bgcolor: "background.paper",
                boxShadow: 6,
            }}
        >
            <SynthesisIcons.HAND className="self-center" />
            <Label size="sm" color="text.primary">
                Drag Mode
            </Label>
        </Stack>
    ) : (
        <></>
    )
}

export default DragModeIndicator
