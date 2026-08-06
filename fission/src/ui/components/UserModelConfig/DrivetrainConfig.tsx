import { Button, Stack } from "@mui/material"
import type React from "react"
import { useRef } from "react"
import { useCallback } from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { SubpanelProps } from "@/components/UserModelConfig/ModelConfigPanel.tsx"
import { ToggleButton } from "@/components/StyledComponents.tsx"

const DrivetrainConfig: React.FC<SubpanelProps> = () => {
    const [driveReversed, setDriveReversed] = useState<boolean>(false)
    const [isRunning, setIsRunning] = useState(false)
    const runningTimeoutHandle = useRef<number | undefined>(undefined)

    useEffect(() => {
        return EventSystem.listen("WheelAssignmentDriveReversedChanged", ({ reversed }) => setDriveReversed(reversed))
    }, [])

    useEffect(() => {
        if (isRunning) {
            World.wheelAssignmentMode
        }
    }, [isRunning])

    return (
        <Stack gap={2} direction="column">
            <ToggleButton
                color="warning"
                onClick={useCallback(() => World.wheelAssignmentMode.toggleReverseDrive(), [])}
                value={driveReversed}
            >
                Reverse Drive
            </ToggleButton>
            <Button
                variant={isRunning ? "contained" : "outlined"}
                color="secondary"
                onClick={useCallback(() => {
                    clearTimeout(runningTimeoutHandle.current)
                    if (isRunning) {
                        setIsRunning(false)
                    } else {
                        setIsRunning(true)
                        runningTimeoutHandle.current = window.setTimeout(() => setIsRunning(false), 1000)
                    }
                }, [isRunning])}
            >
                Test
            </Button>
        </Stack>
    )
}

export default DrivetrainConfig
