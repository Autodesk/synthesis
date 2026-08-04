import { Button, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"

const WheelAssignment: React.FC = () => {
    const [driveReversed, setDriveReversed] = useState<boolean>(false)

    useEffect(() => {
        return EventSystem.listen("WheelAssignmentDriveReversedChanged", ({ reversed }) => setDriveReversed(reversed))
    }, [])

    return (
        <Stack gap={2} direction="column">
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

export default WheelAssignment
