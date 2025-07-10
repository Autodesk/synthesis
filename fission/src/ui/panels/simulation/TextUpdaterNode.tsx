import { Handle, type NodeProps, Position } from "@xyflow/react"
import type React from "react"
import { type ChangeEvent, useCallback } from "react"

const handleStyle = { left: 10 }

const TextUpdaterNode: React.FC<NodeProps> = ({ data, isConnectable }) => {
    const onChange = useCallback((evt: ChangeEvent<HTMLInputElement>) => {
        console.log(evt.target.value)
    }, [])

    return (
        <div className="text-updater-node">
            <Handle type="target" position={Position.Top} isConnectable={isConnectable} />
            <div>
                <label htmlFor="text">Text:</label>
                <input
                    id="text"
                    name="text"
                    onChange={onChange}
                    className="nodrag"
                    defaultValue={data.value as number | string}
                />
            </div>
            <Handle type="source" position={Position.Bottom} id="a" style={handleStyle} isConnectable={isConnectable} />
            <Handle type="source" position={Position.Bottom} id="b" isConnectable={isConnectable} />
        </div>
    )
}

export default TextUpdaterNode
