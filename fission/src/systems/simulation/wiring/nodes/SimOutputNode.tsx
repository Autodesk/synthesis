import { Handle, type NodeProps, Position } from "@xyflow/react"
import { memo } from "react"
import { noraTypeToColorStr } from "@/systems/simulation/Nora"
import { handleInfoDisplayCompare, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"

export const NODE_ID_SIM_OUT = "sim-output-node"

export const SimOutputNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData

    const sources = Object.values(simConfig.handles)
        .filter(x => x.nodeId === id && x.enabled && x.isSource)
        .sort(handleInfoDisplayCompare)

    return (
        <div className="bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4 pl-8">
            <div
                style={{ transform: "translateY(-100%) translateX(-50%)" }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                Simulation Output
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
    )
})
