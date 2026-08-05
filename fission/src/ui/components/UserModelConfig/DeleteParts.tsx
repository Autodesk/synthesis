import { Button, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { PartDeletionSelection } from "@/systems/scene/PartDeletionMode.ts"
import Label from "@/components/Label.tsx"
import { RefreshButton } from "@/components/StyledComponents.tsx"
import type { SubpanelProps } from "@/components/UserModelConfig/ModelConfigPanel.tsx"

const DeleteParts: React.FC<SubpanelProps> = () => {
    const [enabled, setEnabled] = useState<boolean>(false)
    const [pending, setPending] = useState<PartDeletionSelection[]>([
        ...World.partDeletionMode.pendingDeletions.values(),
    ])

    useEffect(() => {
        return EventSystem.listen("PartDeletionSelectionChanged", ({ parts }) => {
            setPending(parts)
        })
    }, [])

    useEffect(() => {
        World.partDeletionMode.enabled = enabled
        return () => {
            World.partDeletionMode.enabled = false
        }
    }, [enabled])

    useEffect(() => {
        setEnabled(World.partDeletionMode.pendingDeletions.size == 0)
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
                {pending.length === 0 && <Label size={"sm"}>No parts marked for deletion</Label>}
                {pending.map(item => (
                    <Stack
                        key={item.guid}
                        direction={"row"}
                        alignItems={"center"}
                        px={1}
                        borderRadius={1}
                        bgcolor="background.paper"
                        onMouseOver={() => World.partDeletionMode.setHover(item.highlight)}
                        onMouseOut={() => World.partDeletionMode.clearHover()}
                    >
                        <Label size={"sm"} flexGrow={1}>
                            {item.name}
                        </Label>
                        <RefreshButton
                            title="Undo"
                            onClick={() => {
                                World.partDeletionMode.clearHover()
                                World.partDeletionMode.pendingDeletions.removePart(item.guid)
                            }}
                        />
                    </Stack>
                ))}
            </Stack>
        </Stack>
    )
}

export default DeleteParts
