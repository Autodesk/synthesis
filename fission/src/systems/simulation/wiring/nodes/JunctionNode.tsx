import { Handle, type NodeProps, Position } from "@xyflow/react"
import { memo } from "react"
import { noraTypeToColorStr } from "@/systems/simulation/Nora"
import { handleInfoDisplayCompare, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"
import { DeleteButton } from "@/ui/components/StyledComponents"

export const JunctionNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData
    const onDelete = data.onDelete as (() => void) | undefined

    const handles = Object.values(simConfig.handles)
        .filter(x => x.nodeId === id && x.enabled)
        .sort(handleInfoDisplayCompare)
    const targets = handles.filter(x => !x.isSource)
    const sources = handles.filter(x => x.isSource)

    return (
        <div className="bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4">
            <div className="grid grid-cols-2 gap-x-2">
                <div className="flex flex-col gap-4">
                    {targets.map(x => (
                        <div key={x.id} className="relative">
                            <div className="px-3 text-lg">{x.displayName}</div>
                            <Handle
                                style={{
                                    position: "absolute",
                                    left: 0,
                                    width: "1rem",
                                    height: "1rem",
                                    backgroundColor: x.noraType ? noraTypeToColorStr(x.noraType) : undefined,
                                }}
                                type="target"
                                position={Position.Left}
                                id={x.id}
                            />
                        </div>
                    ))}
                </div>
                <div className="flex flex-col gap-4">
                    {sources.map(x => (
                        <div key={x.id} className="relative">
                            <div className="px-3 text-lg text-right">{x.displayName}</div>
                            <Handle
                                style={{
                                    position: "absolute",
                                    right: 0,
                                    width: "1rem",
                                    height: "1rem",
                                    backgroundColor: x.noraType ? noraTypeToColorStr(x.noraType) : undefined,
                                }}
                                type="source"
                                position={Position.Right}
                                id={x.id}
                            />
                        </div>
                    ))}
                </div>
            </div>
            {onDelete && (
                <div className="flex justify-center px-4">
                    <DeleteButton onClick={onDelete} />
                </div>
            )}
        </div>
    )
})
