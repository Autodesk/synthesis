import type { NodeProps } from "@xyflow/react"
import { memo } from "react"
import { handleInfoDisplayCompare, handlesOf, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"
import { RefreshButton } from "@/ui/components/StyledComponents"
import { HandleRow, WiringNodeShell } from "../NodeKinds"

export const NODE_ID_ROBOT_IO = "robot-io-node"

export const RobotIONode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData
    const onRefresh = data.onRefresh as (() => void) | undefined

    const handles = handlesOf(simConfig, id)
        .filter(x => x.enabled)
        .sort(handleInfoDisplayCompare)
    const targets = handles.filter(x => !x.isSource)
    const sources = handles.filter(x => x.isSource)

    return (
        <WiringNodeShell title="Robot IO">
            {handles.length === 0 ? (
                <div className="flex flex-col items-center gap-2 px-4 text-center">
                    <div className="text-lg text-nowrap">No devices found.</div>
                    <div className="text-sm">Connect your robot code, then refresh.</div>
                    {onRefresh && <RefreshButton onClick={onRefresh} />}
                </div>
            ) : (
                <div className="grid grid-cols-2 gap-x-2">
                    <div className="flex flex-col gap-4">
                        {targets.map(x => (
                            <HandleRow key={x.id} handle={x} />
                        ))}
                    </div>
                    <div className="flex flex-col gap-4">
                        {sources.map(x => (
                            <HandleRow key={x.id} handle={x} />
                        ))}
                    </div>
                </div>
            )}
        </WiringNodeShell>
    )
})
