import { Button, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { WheelSelection } from "@/systems/scene/WheelAssignmentMode.ts"
import Label from "@/components/Label.tsx"
import { DeleteButton } from "@/components/StyledComponents.tsx"

const WheelAssignment: React.FC = () => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [selected, setSelected] = useState<WheelSelection[]>([...World.wheelAssignmentMode.pendingWheels.values()])

    useEffect(() => {
        return EventSystem.listen("WheelAssignmentSelectionChanged", ({ wheels }) => {
            setSelected(wheels)
        })
    }, [])

    useEffect(() => {
        World.wheelAssignmentMode.enabled = enabled
    }, [enabled])

    useEffect(() => {
        setEnabled(World.wheelAssignmentMode.pendingWheels.size == 0)
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
                {selected.length === 0 && <Label size={"sm"}>No wheels selected</Label>}
                {selected.map((item, i) => (
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
        </Stack>
    )
}

export default WheelAssignment
