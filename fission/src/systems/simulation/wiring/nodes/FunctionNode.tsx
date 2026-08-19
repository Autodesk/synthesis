import type { NodeProps } from "@xyflow/react"
import { memo } from "react"
import { handleInfoDisplayCompare, type SimConfigData } from "@/systems/simulation/wiring/SimGraph"
import { DeleteButton } from "@/ui/components/StyledComponents"
import { HandleRow, WiringNodeShell } from "../NodeKinds"

export const FunctionNode = memo(({ id, data }: NodeProps) => {
    const simConfig = data.simConfig as SimConfigData
    const onDelete = data.onDelete as (() => void) | undefined

    const handles = Object.values(simConfig.handles)
        .filter(x => x.nodeId === id && x.enabled)
        .sort(handleInfoDisplayCompare)
    const targets = handles.filter(x => !x.isSource)
    const sources = handles.filter(x => x.isSource)

    return (
        <WiringNodeShell title={data.title as string}>
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
            {onDelete && (
                <div className="flex justify-center px-4">
                    <DeleteButton onClick={onDelete} />
                </div>
            )}
        </WiringNodeShell>
    )
})
