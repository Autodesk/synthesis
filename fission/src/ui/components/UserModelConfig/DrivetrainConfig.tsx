import { Button, Stack } from "@mui/material"
import type React from "react"
import { useRef } from "react"
import { useCallback } from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { SubpanelProps } from "@/components/UserModelConfig/ModelConfigPanel.tsx"
import Checkbox from "@/components/Checkbox.tsx"

const DrivetrainConfig: React.FC<SubpanelProps> = ({ sceneObject, pauseRef }) => {
    const [driveReversed, setDriveReversed] = useState<boolean>(false)
    const [isRunning, setIsRunning] = useState(false)
    const runningTimeoutHandle = useRef<number | undefined>(undefined)

    useEffect(() => {
        return EventSystem.listen("WheelAssignmentDriveReversedChanged", ({ reversed }) => setDriveReversed(reversed))
    }, [])

    useEffect(() => {
        if (!sceneObject.brain?.isSynthesis()) return
        const driveBehavior = sceneObject.brain.getDriveBehavior()
        if (!driveBehavior) return
        if (isRunning) {
            World.physicsSystem.releasePause(pauseRef)
            driveBehavior.runTestingForward(0.5)
        } else {
            driveBehavior.releaseTesting()
            World.physicsSystem.holdPause(pauseRef)
        }
    }, [isRunning, sceneObject, pauseRef])

    return (
        <Stack gap={2} direction="column">
            <Checkbox
                onClick={useCallback(() => World.wheelAssignmentMode.toggleReverseDrive(), [])}
                checked={driveReversed}
                label={"Reverse Drive"}
            />

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
