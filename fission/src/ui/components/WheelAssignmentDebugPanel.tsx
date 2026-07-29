import { Button, Stack } from "@mui/material"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import Label from "./Label"

/** Throwaway dev-only panel for testing manual wheel/pod-joint placement. */
const WheelAssignmentDebugPanel: React.FC = () => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [pickTarget, setPickTarget] = useState<"wheel" | "pod">("wheel")
    const [wheelCount, setWheelCount] = useState<number>(0)
    const [podCount, setPodCount] = useState<number>(0)
    const [driveReversed, setDriveReversed] = useState<boolean>(false)

    useEffect(() => {
        const unsubToggle = EventSystem.listen("WheelAssignmentModeToggled", ({ enabled }) => setEnabled(enabled))
        const unsubCount = EventSystem.listen("WheelAssignmentPendingCountChanged", ({ wheelCount, podCount }) => {
            setWheelCount(wheelCount)
            setPodCount(podCount)
        })
        const unsubReversed = EventSystem.listen("WheelAssignmentDriveReversedChanged", ({ reversed }) =>
            setDriveReversed(reversed)
        )
        return () => {
            unsubToggle()
            unsubCount()
            unsubReversed()
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
                    disabled={wheelCount === 0 && podCount === 0}
                    onClick={() => void World.wheelAssignmentMode.apply()}
                >
                    Apply (W:{wheelCount} P:{podCount})
                </Button>
            </Stack>
            <Stack direction="row" gap={1}>
                <Button
                    size="small"
                    variant={pickTarget === "wheel" ? "contained" : "outlined"}
                    onClick={() => {
                        World.wheelAssignmentMode.pickTarget = "wheel"
                        setPickTarget("wheel")
                    }}
                >
                    Wheel
                </Button>
                <Button
                    size="small"
                    variant={pickTarget === "pod" ? "contained" : "outlined"}
                    onClick={() => {
                        World.wheelAssignmentMode.pickTarget = "pod"
                        setPickTarget("pod")
                    }}
                >
                    Pod
                </Button>
            </Stack>
            <Stack direction="row" gap={1}>
                <Button
                    size="small"
                    variant={driveReversed ? "contained" : "outlined"}
                    color="warning"
                    onClick={() => World.wheelAssignmentMode.toggleReverseDrive()}
                >
                    Reverse Drive
                </Button>
            </Stack>
        </Stack>
    )
}

export default WheelAssignmentDebugPanel
