import type { NodeProps } from "@xyflow/react"
import { memo } from "react"
import { handleInfoDisplayCompare, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"
import { HandleRow, WiringNodeShell } from "../NodeKinds"

export const NODE_ID_SIM_OUT = "sim-output-node"

export const SimOutputNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData

    const sources = Object.values(simConfig.handles)
        .filter(x => x.nodeId === id && x.enabled && x.isSource)
        .sort(handleInfoDisplayCompare)

    return (
        <WiringNodeShell title="Simulation Output">
            <div className="flex flex-col gap-4">
                {sources.map(x => (
                    <HandleRow key={x.id} handle={x} />
                ))}
            </div>
        </WiringNodeShell>
    )
})
