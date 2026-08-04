import { Button, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useId, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import type { PanelImplProps } from "@/components/Panel.tsx"
import type { WheelSelection } from "@/systems/scene/WheelAssignmentMode.ts"
import Label from "@/components/Label.tsx"
import { DeleteButton } from "@/components/StyledComponents.tsx"

const WheelAssignmentPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [wheels, setWheels] = useState<WheelSelection[]>([])
    const [driveReversed, setDriveReversed] = useState<boolean>(false)
    const { configureScreen } = useUIContext()

    useEffect(() => {
        const unsubCount = EventSystem.listen("WheelAssignmentSelectionChanged", ({ wheels }) => {
            setWheels(wheels)
        })
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

    useEffect(() => {
        setEnabled(true)
    }, [])

    return (
        <Stack gap={2} direction="column">
            <Button
                variant={enabled ? "contained" : "outlined"}
                onClick={() => {
                    setEnabled(e => !e)
                }}
            >
                {enabled ? "Stop Picking" : "Start Picking"}
            </Button>
            <Stack direction="column" gap={1}>
                {wheels.length === 0 && <Label size={"sm"}>No wheels selected</Label>}
                {wheels.map((item, i) => (
                    <Stack
                        key={item.assignment.wheelPartGuid}
                        direction={"row"}
                        alignItems={"center"}
                        px={1}
                        borderRadius={1}
                        bgcolor="background.paper"
                        onMouseOver={() => World.wheelAssignmentMode.setHover(item.highlight)}
                        onMouseOut={() => World.wheelAssignmentMode.clearHover()}
                    >
                        <Label size={"sm"} flexGrow={1}>
                            Wheel {i + 1}
                        </Label>
                        <DeleteButton
                            onClick={() => {
                                World.wheelAssignmentMode.clearHover()
                                World.wheelAssignmentMode.pendingWheels.removePart(item.assignment.wheelPartGuid)
                            }}
                        />
                    </Stack>
                ))}
            </Stack>
            <Button
                size="small"
                variant="contained"
                color="success"
                disabled={wheels.length === 0}
                onClick={() => void World.wheelAssignmentMode.apply()}
            >
                Apply ({wheels.length})
            </Button>
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
