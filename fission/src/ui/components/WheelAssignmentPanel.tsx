import { Button, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useId, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import type { PanelImplProps } from "@/components/Panel.tsx"

const WheelAssignmentPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [pendingCount, setPendingCount] = useState<number>(0)
    const [driveReversed, setDriveReversed] = useState<boolean>(false)
    const { configureScreen } = useUIContext()

    useEffect(() => {
        const unsubCount = EventSystem.listen("WheelAssignmentPendingCountChanged", ({ count }) =>
            setPendingCount(count)
        )
        const unsubReversed = EventSystem.listen("WheelAssignmentDriveReversedChanged", ({ reversed }) =>
            setDriveReversed(reversed)
        )
        return () => {
            unsubCount()
            unsubReversed()
        }
    }, [])

    const pauseHandle = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(pauseHandle)
        return () => {
            World.physicsSystem.releasePause(pauseHandle)
        }
    }, [pauseHandle])

    useEffect(() => {
        World.wheelAssignmentMode.enabled = enabled
    }, [enabled])

    useEffect(() => {
        configureScreen(panel!, { title: "Assign Wheels", hideCancel: true, hideAccept: true }, {})
    }, [configureScreen, panel])

    return (
        <Stack gap={2} direction="column">
            <Stack direction="row" gap={1}>
                <Button
                    variant={enabled ? "contained" : "outlined"}
                    onClick={() => {
                        setEnabled(e => !e)
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

export default WheelAssignmentPanel
