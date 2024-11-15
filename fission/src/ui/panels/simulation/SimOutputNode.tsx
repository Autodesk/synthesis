import { Handle, NodeProps, Position } from "@xyflow/react"

function SimOutputNode({ data, isConnectable }: NodeProps) {
    const simOutput = data["output"] as string[]

    return (
        <div
            className="sim-output-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex py-4"
        >
            <div
                style={{
                    transform: "translateY(-100%) translateX(-50%)"
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                Simulation Output
            </div>
            <div className="flex flex-col gap-4 justify-left pl-16">
                {simOutput.map((x, i) => {
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
    )
}

export default SimOutputNode