import { Handle, type NodeProps, Position } from "@xyflow/react"
import { memo } from "react"
import { noraTypeToColorStr } from "@/systems/simulation/Nora"
import { handleInfoDisplayCompare, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"

export const NODE_ID_SIM_IN = "sim-input-node"

export const SimInputNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData

    const targets = Object.values(simConfig.handles)
        .filter(x => x.nodeId === id && x.enabled && !x.isSource)
        .sort(handleInfoDisplayCompare)

    return (
        <div className="bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4 pr-8">
            <div
                style={{ transform: "translateY(-100%) translateX(-50%)" }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                Simulation Input
            </div>
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
        </div>
    )
})
