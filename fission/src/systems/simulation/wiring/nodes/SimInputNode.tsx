import type { NodeProps } from "@xyflow/react"
import { memo } from "react"
import { handleInfoDisplayCompare, handlesOf, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"
import { HandleRow, WiringNodeShell } from "../NodeKinds"

export const NODE_ID_SIM_IN = "sim-input-node"

export const SimInputNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData
    const targets = handlesOf(simConfig, id)
        .filter(x => x.enabled && !x.isSource)
        .sort(handleInfoDisplayCompare)

    return (
        <WiringNodeShell title="Simulation Input">
            <div className="flex flex-col gap-4 pr-8">
                {targets.map(x => (
                    <HandleRow key={x.id} handle={x} />
                ))}
            </div>
        </WiringNodeShell>
    )
})
