import { Button, Stack } from "@mui/material"
import type React from "react"
import { useCallback } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World.ts"
import type { PartDeletionSelection } from "@/systems/scene/PartDeletionMode.ts"
import Label from "@/components/Label.tsx"
import { SynthesisIcons } from "@/components/StyledComponents.tsx"
import type { SubpanelProps } from "@/components/UserModelConfig/ModelConfigPanel.tsx"
import { usePickingMode } from "./usePickingMode"
import { truncate } from "@/util/Utility.ts"

const DeleteParts: React.FC<SubpanelProps> = ({ sceneObject }) => {
    const subscribe = useCallback(
        (onChange: (items: PartDeletionSelection[]) => void) =>
            EventSystem.listen("PartDeletionSelectionChanged", ({ parts }) => onChange(parts)),
        []
    )
    const { enabled, setEnabled, items: pending } = usePickingMode(World.partDeletionMode, subscribe, sceneObject)

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
                        px={2}
                        py={0.5}
                        borderRadius={1}
                        bgcolor="background.paper"
                        onMouseOver={() => World.partDeletionMode.setHover(item.highlight)}
                        onMouseOut={() => World.partDeletionMode.clearHover()}
                        onClick={() => {
                            World.partDeletionMode.pendingDeletions.removePart(item.guid)
                        }}
                        sx={{ cursor: "pointer" }}
                    >
                        <Label size={"sm"} flexGrow={1}>
                            {truncate(item.name, 40, true)}
                        </Label>
                        <SynthesisIcons.UNDO />
                    </Stack>
                ))}
            </Stack>
        </Stack>
    )
}

export default DeleteParts
