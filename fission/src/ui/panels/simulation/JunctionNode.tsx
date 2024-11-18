import Button from "@/ui/components/Button"
import { Handle, NodeProps, Position } from "@xyflow/react"
import { ConfigItemInfo, SimConfigData } from "./SimConfigShared"
import { useCallback } from "react"

function JunctionNode({ id, data, isConnectable }: NodeProps) {
    const simConfig = data["simConfig"] as SimConfigData
    const onDelete = data["onDelete"] as () => void

    return (
        <div
            className="sim-input-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4"
        >
            <Handle className="absolute left-0 w-4 h-4" type="target" position={Position.Left} id={`in_${id}`} isConnectable={isConnectable} />
            <Handle className="absolute right-0 w-4 h-4" type="source" position={Position.Right} id={`out_${id}`} isConnectable={isConnectable} />
            <div className="flex justify-center px-4">
                <Button value={"X"} onClick={onDelete} />
            </div>
        </div>
    )
}

export default JunctionNode