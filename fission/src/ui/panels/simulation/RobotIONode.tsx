import Button from "@/ui/components/Button"
import { Handle, NodeProps, Position } from "@xyflow/react"

function RobotIONode({ data, isConnectable }: NodeProps) {
    const robotInput = data["input"] as string[]
    const robotOutput = data["output"] as string[]
    const onEdit = data["onEdit"] as (() => void)

    return (
        <div
            className="robot-io-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4"
        >
            <div
                style={{
                    transform: "translateY(-100%) translateX(-50%)"
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >Robot IO</div>
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "0.5rem",
                }}
            >
                <div className="flex flex-col gap-4 justify-center">
                    {robotInput.map((x, i) => {
                        return (<div
                            key={i}
                            className="relative"
                        >
                            <div className="px-3 text-lg">{x}</div>
                            <Handle className="absolute left-0" key={i} type="target" position={Position.Left} id={x} isConnectable={isConnectable} />
                        </div>)
                    })}
                </div>
                <div className="flex flex-col gap-4 justify-center">
                    {robotOutput.map((x, i) => {
                        return (<div
                            key={i}
                            className="relative"
                        >
                            <div className="px-3 text-lg text-right">{x}</div>
                            <Handle className="absolute right-0" key={i} type="source" position={Position.Right} id={x} isConnectable={isConnectable} />
                        </div>)
                    })}
                </div>
            </div>
            <div className="flex justify-center px-4">
                <Button value={"Edit"} onClick={onEdit} />
            </div>
        </div>
    )
}

export default RobotIONode