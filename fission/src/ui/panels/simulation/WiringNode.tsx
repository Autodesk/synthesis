import { Box } from "@mui/material"
import { type Connection, type Edge, Handle, type NodeProps, Position } from "@xyflow/react"
import { useCallback, useMemo } from "react"
import {
    type HandleInfo,
    handleInfoDisplayCompare,
    NORA_TYPES_COLORS,
    SimConfig,
    type SimConfigData,
} from "@/systems/simulation/SimConfigShared"
import { CustomTooltip, DeleteButton, EditButton, RefreshButton } from "@/ui/components/StyledComponents"

const WiringNode = ({ data, isConnectable }: NodeProps) => {
    const robotInput = useMemo(() => data.input as HandleInfo[] | undefined, [data])
    const robotOutput = useMemo(() => data.output as HandleInfo[] | undefined, [data])
    const onEdit = useMemo(() => data.onEdit as (() => void) | undefined, [data])
    const onRefresh = useMemo(() => data.onRefresh as (() => void) | undefined, [data])
    const onDelete = useMemo(() => data.onDelete as (() => void) | undefined, [data])
    const simConfig = useMemo(() => data.simConfig as SimConfigData, [data])
    const title = useMemo(() => data.title as string, [data])
    const tooltip = useMemo(() => data.tooltip as string | undefined, [data])

    const validateConnection = useCallback(
        (edge: Edge | Connection) => {
            return SimConfig.ValidateConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)
        },
        [simConfig]
    )

    const inputHandles = useMemo(
        () =>
            robotInput ? (
                <Box
                    sx={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "1rem",
                        justifyContent: "space-between",
                    }}
                >
                    {robotInput.sort(handleInfoDisplayCompare).map((x, i) => {
                        return (
                            <div key={i} className="relative">
                                <div className="px-3 text-lg">{x.displayName}</div>
                                <Handle
                                    style={{
                                        position: "absolute",
                                        left: 0,
                                        width: "1rem",
                                        height: "1rem",
                                        backgroundColor: NORA_TYPES_COLORS[x.noraType],
                                    }}
                                    key={i}
                                    type="target"
                                    position={Position.Left}
                                    id={x.id}
                                    isConnectable={isConnectable}
                                    isValidConnection={validateConnection}
                                />
                            </div>
                        )
                    })}
                </Box>
            ) : (
                <></>
            ),
        [isConnectable, robotInput]
    )

    const outputHandles = useMemo(
        () =>
            robotOutput ? (
                <Box
                    sx={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "1rem",
                        justifyContent: "space-between",
                    }}
                >
                    {robotOutput.sort(handleInfoDisplayCompare).map((x, i) => {
                        return (
                            <div key={i} className="relative">
                                <div className="px-3 text-lg text-right">{x.displayName}</div>
                                <Handle
                                    style={{
                                        position: "absolute",
                                        right: 0,
                                        width: "1rem",
                                        height: "1rem",
                                        backgroundColor: NORA_TYPES_COLORS[x.noraType],
                                    }}
                                    key={i}
                                    type="source"
                                    position={Position.Right}
                                    id={x.id}
                                    isConnectable={isConnectable}
                                    isValidConnection={validateConnection}
                                />
                            </div>
                        )
                    })}
                </Box>
            ) : (
                <></>
            ),
        [isConnectable, robotOutput, validateConnection]
    )

    return (
        <Box className="robot-io-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4">
            <Box
                style={{
                    transform: "translateY(-100%) translateX(-50%)",
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                {tooltip ? CustomTooltip(tooltip) : <></>}
                {title}
            </Box>
            <Box
                sx={
                    robotInput && robotOutput
                        ? {
                              display: "grid",
                              gridTemplateColumns: "1fr 1fr",
                              columnGap: "0.5rem",
                          }
                        : robotInput
                          ? {
                                paddingRight: "2rem",
                            }
                          : {
                                paddingLeft: "2rem",
                            }
                }
            >
                {inputHandles}
                {outputHandles}
            </Box>
            {onEdit || onDelete ? (
                <Box className="flex justify-center px-4">
                    {onEdit ? EditButton(onEdit) : <></>}
                    {onRefresh ? RefreshButton(onRefresh) : <></>}
                    {onDelete ? DeleteButton(onDelete) : <></>}
                </Box>
            ) : (
                <></>
            )}
        </Box>
    )
}

export default WiringNode
