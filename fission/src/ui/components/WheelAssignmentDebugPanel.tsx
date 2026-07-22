import { Button, Stack } from "@mui/material"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import Label from "./Label"

/**
 * Throwaway dev-only panel for testing the manual wheel-joint-placement mechanism: toggles the
 * pick-a-wheel interaction loop and applies the accumulated pending assignments.
 */
const WheelAssignmentDebugPanel: React.FC = () => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [pendingCount, setPendingCount] = useState<number>(0)

    useEffect(() => {
        const unsubToggle = EventSystem.listen("WheelAssignmentModeToggled", ({ enabled }) => setEnabled(enabled))
        const unsubCount = EventSystem.listen("WheelAssignmentPendingCountChanged", ({ count }) =>
            setPendingCount(count)
        )
        return () => {
            unsubToggle()
            unsubCount()
        }
    }, [])

    if (!import.meta.env.DEV) return <></>

    return (
        <Stack
            className="select-none absolute right-1 bottom-1 py-2 px-4 rounded-lg gap-2"
            direction="column"
            sx={{ bgcolor: "background.paper", boxShadow: 6 }}
        >
            <Label size="sm" color="text.primary">
                Wheel Assignment (debug)
            </Label>
            <Stack direction="row" gap={1}>
                <Button
                    size="small"
                    variant={enabled ? "contained" : "outlined"}
                    onClick={() => {
                        World.wheelAssignmentMode.enabled = !World.wheelAssignmentMode.enabled
                    }}
                >
                    {enabled ? "Stop Picking" : "Start Picking"}
                </Button>
                <Button
                    size="small"
                    variant="contained"
                    color="success"
                    disabled={pendingCount === 0}
                    onClick={() => void World.wheelAssignmentMode.apply()}
                >
                    Apply ({pendingCount})
                </Button>
            </Stack>
        </Stack>
    )
}

export default WheelAssignmentDebugPanel
