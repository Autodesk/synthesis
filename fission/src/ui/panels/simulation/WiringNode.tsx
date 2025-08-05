import { Stack } from "@mui/material"
import { type Connection, type Edge, Handle, type NodeProps, Position } from "@xyflow/react"
import { useCallback, useMemo } from "react"
import {
    type HandleInfo,
    handleInfoDisplayCompare,
    NORA_TYPES_COLORS,
    SimConfig,
    type SimConfigData,
} from "@/systems/simulation/SimConfigShared"
import Label from "@/ui/components/Label"
import { CustomTooltip, DeleteButton, EditButton, RefreshButton } from "@/ui/components/StyledComponents"

const WiringNode = ({ data, isConnectable }: NodeProps) => {
    const robotInput = data.input as HandleInfo[] | undefined
    const robotOutput = data.output as HandleInfo[] | undefined
    const onEdit = data.onEdit as (() => void) | undefined
    const onRefresh = data.onRefresh as (() => void) | undefined
    const onDelete = data.onDelete as (() => void) | undefined
    const simConfig = data.simConfig as SimConfigData
    const title = data.title as string
    const tooltip = data.tooltip as string | undefined

    const validateConnection = useCallback(
        (edge: Edge | Connection) => {
            return SimConfig.ValidateConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)
        },
        [simConfig]
    )

    const inputHandles = useMemo(
        () =>
            robotInput && (
                <Stack gap={4}>
                    {robotInput.sort(handleInfoDisplayCompare).map((x, i) => (
                        <div key={i} className="relative">
                            <Label size="md">{x.displayName}</Label>
                            <Handle
                                style={{
                                    backgroundColor: NORA_TYPES_COLORS[x.noraType],
                                }}
                                className="absolute left-0 w-4 h-4"
                                key={i}
                                type="target"
                                position={Position.Left}
                                id={x.id}
                                isConnectable={isConnectable}
                            />
                        </div>
                    ))}
                </Stack>
            ),
        [isConnectable, robotInput]
    )

    const outputHandles = useMemo(
        () =>
            robotOutput && (
                <Stack gap={4}>
                    {robotOutput.sort(handleInfoDisplayCompare).map((x, i) => (
                        <div key={i} className="relative">
                            <Label size="md">{x.displayName}</Label>
                            <Handle
                                style={{
                                    backgroundColor: NORA_TYPES_COLORS[x.noraType],
                                }}
                                className="absolute right-0 w-4 h-4"
                                key={i}
                                type="source"
                                position={Position.Right}
                                id={x.id}
                                isConnectable={isConnectable}
                                isValidConnection={validateConnection}
                            />
                        </div>
                    ))}
                </Stack>
            ),
        [isConnectable, robotOutput, validateConnection]
    )

    return (
        <Stack gap={4}>
            <div
                style={{ transform: "translateY(-100%) translateX(-50%)" }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                {tooltip && CustomTooltip(tooltip)}
                {title}
            </div>
            <div
                style={
                    robotInput && robotOutput
                        ? {
                              display: "grid",
                              gridTemplateColumns: "1fr 1fr",
                              columnGap: "0.5rem",
                          }
                        : robotInput
                          ? { paddingRight: "2rem" }
                          : { paddingLeft: "2rem" }
                }
            >
                {inputHandles}
                {outputHandles}
            </div>
            {(onEdit || onDelete) && (
                <div className="flex justify-center px-4">
                    {onEdit && EditButton(onEdit)}
                    {onRefresh && RefreshButton(onRefresh)}
                    {onDelete && DeleteButton(onDelete)}
                </div>
            )}
        </Stack>
    )
}

export default WiringNode
