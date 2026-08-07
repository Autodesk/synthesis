import { Button, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import World from "@/systems/World.ts"
import type { SubpanelProps } from "@/components/UserModelConfig/ModelConfigPanel.tsx"
import Checkbox from "@/components/Checkbox.tsx"
import { setWheelReversal } from "@/mirabuf/WheelJointBuilder.ts"

const DrivetrainConfig: React.FC<SubpanelProps> = ({ sceneObject, pauseRef }) => {
    const [driveReversed, setDriveReversed] = useState<boolean>(false)
    const [isRunning, setIsRunning] = useState(false)
    const runningTimeoutHandle = useRef<number | undefined>(undefined)

    const toggleReverseDrive = useCallback(() => {
        const newDriveReversed = !driveReversed
        setDriveReversed(newDriveReversed)
        if (!sceneObject?.brain?.isSynthesis()) {
            console.warn("Can't reverse drive on non-synthesis brain", sceneObject)
            return
        }

        for (const driver of sceneObject.brain.getWheelDrivers()) {
            console.log(`drive reversed ${driver}`, newDriveReversed)
            driver.reversed = newDriveReversed
        }

        setWheelReversal(sceneObject.mirabufInstance.parser.assembly, newDriveReversed)
    }, [driveReversed, sceneObject])

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
            <Checkbox onClick={toggleReverseDrive} checked={driveReversed} label={"Reverse Drive"} />

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
