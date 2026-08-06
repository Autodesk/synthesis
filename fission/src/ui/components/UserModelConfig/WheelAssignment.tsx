import { Button, Stack } from "@mui/material"
import type React from "react"
import { useCallback } from "react"
import { useMemo } from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { WheelSelection } from "@/systems/scene/WheelAssignmentMode.ts"
import Label from "@/components/Label.tsx"
import { DeleteButton } from "@/components/StyledComponents.tsx"
import type { SubpanelProps } from "./ModelConfigPanel"

const WheelAssignment: React.FC<SubpanelProps> = ({ setDisableNextMessage, sceneObject }) => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [selected, setSelected] = useState<WheelSelection[]>([...World.wheelAssignmentMode.pendingWheels.values()])
    const wheelSlots = useMemo(() => {
        const slots = new Array<WheelSelection | null>(Math.max(selected.length, 4)).fill(null)
        selected.forEach((item, i) => {
            slots[i] = item
        })
        return slots
    }, [selected])
    useEffect(() => {
        return EventSystem.listen("WheelAssignmentSelectionChanged", ({ wheels }) => {
            setSelected(wheels)
        })
    }, [])

    useEffect(() => {
        if (enabled) {
            World.wheelAssignmentMode.enable(sceneObject)
        } else {
            World.wheelAssignmentMode.disable()
        }
    }, [enabled, sceneObject])

    useEffect(() => {
        setEnabled(World.wheelAssignmentMode.pendingWheels.size == 0)
        return () => {
            World.wheelAssignmentMode.disable()
        }
    }, [])

    useEffect(() => {
        setDisableNextMessage(selected.length < 4 ? "Must select at least 4 wheels" : null)
    }, [selected, setDisableNextMessage])

    return (
        <Stack gap={2} direction="column">
            <Button
                variant={enabled ? "contained" : "outlined"}
                onClick={useCallback(() => {
                    setEnabled(e => !e)
                }, [])}
            >
                {enabled ? "Stop Picking" : "Start Picking"}
            </Button>
            <Stack direction="column" gap={1}>
                {wheelSlots.map((item, i) => (
                    <Stack
                        key={i}
                        direction={"row"}
                        alignItems={"center"}
                        px={1}
                        borderRadius={1}
                        bgcolor={item != null ? "background.paper" : undefined}
                        border={item == null ? "dashed 1px gray" : undefined}
                        color={item == null ? "gray" : undefined}
                        onMouseOver={() => item != null && World.wheelAssignmentMode.setHover(item.highlight)}
                        onMouseOut={() => item != null && World.wheelAssignmentMode.clearHover()}
                    >
                        <Label size={"sm"} p={0.5} fontStyle={item == null ? "italic" : undefined} flexGrow={1}>
                            Wheel {i + 1} {item == null && "(Unassigned)"}
                        </Label>
                        {item != null && (
                            <DeleteButton
                                onClick={() => {
                                    World.wheelAssignmentMode.clearHover()
                                    World.wheelAssignmentMode.pendingWheels.removePart(item.assignment.wheelPartGuid)
                                }}
                            />
                        )}
                    </Stack>
                ))}
                <Stack
                    direction={"row"}
                    alignItems={"center"}
                    justifyContent={"center"}
                    px={1}
                    borderRadius={1}
                    border="dashed 1px gray"
                >
                    <Label size={"sm"} p={0.5} fontStyle={"italic"} textAlign={"center"}>
                        ...
                    </Label>
                </Stack>
            </Stack>
        </Stack>
    )
}

export default WheelAssignment
