import { Connection, Edge, Handle, NodeProps, Position } from "@xyflow/react"
import { handleInfoDisplayCompare, SimConfig, SimConfigData, HandleInfo, NORA_TYPES_COLORS } from "./SimConfigShared"
import { useCallback, useMemo } from "react"
import { CustomTooltip, DeleteButton, EditButton, RefreshButton } from "@/ui/components/StyledComponents"

function WiringNode({ data, isConnectable }: NodeProps) {
    const robotInput = data["input"] as HandleInfo[] | undefined
    const robotOutput = data["output"] as HandleInfo[] | undefined
    const onEdit = data["onEdit"] as (() => void) | undefined
    const onRefresh = data["onRefresh"] as (() => void) | undefined
    const onDelete = data["onDelete"] as (() => void) | undefined
    const simConfig = data["simConfig"] as SimConfigData
    const title = data["title"] as string
    const tooltip = data["tooltip"] as string | undefined

    const validateConnection = useCallback(
        (edge: Edge | Connection) => {
            return SimConfig.ValidateConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)
        },
        [simConfig]
    )

    const inputHandles = useMemo(
        () =>
            robotInput ? (
                <div className="flex flex-col gap-4 justify-between">
                    {robotInput.sort(handleInfoDisplayCompare).map((x, i) => {
                        return (
                            <div key={i} className="relative">
                                <div className="px-3 text-lg">{x.displayName}</div>
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
                        )
                    })}
                </div>
            ) : (
                <></>
            ),
        [isConnectable, robotInput]
    )

    const outputHandles = useMemo(
        () =>
            robotOutput ? (
                <div className="flex flex-col gap-4 justify-between">
                    {robotOutput.sort(handleInfoDisplayCompare).map((x, i) => {
                        return (
                            <div key={i} className="relative">
                                <div className="px-3 text-lg text-right">{x.displayName}</div>
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
                        )
                    })}
                </div>
            ) : (
                <></>
            ),
        [isConnectable, robotOutput, validateConnection]
    )

    return (
        <div className="robot-io-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4">
            <div
                style={{
                    transform: "translateY(-100%) translateX(-50%)",
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                {tooltip ? CustomTooltip(tooltip) : <></>}
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
            </div>
            {onEdit || onDelete ? (
                <div className="flex justify-center px-4">
                    {onEdit ? EditButton(onEdit) : <></>}
                    {onRefresh ? RefreshButton(onRefresh) : <></>}
                    {onDelete ? DeleteButton(onDelete) : <></>}
                </div>
            ) : (
                <></>
            )}
        </div>
    )
}

export default WiringNode
