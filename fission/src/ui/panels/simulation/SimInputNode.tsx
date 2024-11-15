import Button from "@/ui/components/Button"
import { Handle, NodeProps, Position } from "@xyflow/react"

function SimInputNode({ data, isConnectable }: NodeProps) {
    const simInput = data["input"] as string[]
    const onEdit = data["onEdit"] as (() => void)

    return (
        <div
            className="sim-input-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4"
        >
            <div
                style={{
                    transform: "translateY(-100%) translateX(-50%)"
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                Simulation Input
            </div>
            
            <div className="flex flex-col gap-4 justify-left pr-16">
                {simInput.map((x, i) => {
                    return (<div
                        key={i}
                        className="relative"
                    >
                        <div className="px-3 text-lg">{x}</div>
                        <Handle className="absolute left-0" key={i} type="target" position={Position.Left} id={x} isConnectable={isConnectable} />
                    </div>)
                })}
            </div>
            <div className="flex justify-center px-4">
                <Button value={"Edit"} onClick={onEdit} />
            </div>
        </div>
    )
}

export default SimInputNode