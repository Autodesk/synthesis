import Button from "@/ui/components/Button"
import { Connection, Edge, Handle, NodeProps, Position } from "@xyflow/react"
import { ConfigItemInfo, configItemInfoCompare, genId, genIdToSavedId, savedIdToGenId, SimConfig, SimConfigData } from "./SimConfigShared"
import { useCallback, useMemo } from "react"
import { FaXmark } from "react-icons/fa6"
import { DeleteButton, EditButton } from "@/ui/components/StyledComponents"

function WiringNode({ data, isConnectable }: NodeProps) {
    const robotInput = data["input"] as ([string, ConfigItemInfo][] | undefined)
    const robotOutput = data["output"] as ([string, ConfigItemInfo][] | undefined)
    const onEdit = data["onEdit"] as ((() => void) | undefined)
    const onDelete = data["onDelete"] as ((() => void) | undefined)
    const simConfig = data["simConfig"] as SimConfigData
    const title = data["title"] as string

    const validateConnection = useCallback((edge: Edge | Connection) => {
        // return true
        return SimConfig.ValidateConnection(simConfig, genIdToSavedId(edge.sourceHandle as string)!, genIdToSavedId(edge.targetHandle as string)!)
    }, [simConfig])

    const inputHandles = useMemo(() => robotInput ? (
        <div className="flex flex-col gap-4 justify-center">
            {robotInput.sort(configItemInfoCompare).map((x, i) => {
                return (<div
                    key={i}
                    className="relative"
                >
                    <div className="px-3 text-lg">{x[1].displayName}</div>
                    <Handle
                        className="absolute left-0 w-4 h-4"
                        key={i}
                        type="target"
                        position={Position.Left}
                        id={savedIdToGenId(x[0])}
                        isConnectable={isConnectable}
                    />
                </div>)
            })}
        </div>
    ) : (<></>), [isConnectable, robotInput])

    const outputHandles = useMemo(() => robotOutput ? (
        <div className="flex flex-col gap-4 justify-center">
            {robotOutput.sort(configItemInfoCompare).map((x, i) => {
                return (<div
                    key={i}
                    className="relative"
                >
                    <div className="px-3 text-lg text-right">{x[1].displayName}</div>
                    <Handle
                        className="absolute right-0  w-4 h-4"
                        key={i}
                        type="source"
                        position={Position.Right}
                        id={savedIdToGenId(x[0])}
                        isConnectable={isConnectable}
                        isValidConnection={validateConnection}
                        
                    />
                </div>)
            })}
        </div>
    ) : (<></>), [isConnectable, robotOutput, validateConnection])

    return (
        <div
            className="robot-io-node bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4"
        >
            <div
                style={{
                    transform: "translateY(-100%) translateX(-50%)"
                }}
                className="absolute top-0 text-nowrap left-1/2 text-2xl"
            >
                {title}
            </div>
            <div
                style={(robotInput && robotOutput) ? {
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "0.5rem",
                } : robotInput ? {
                    paddingRight: "2rem"
                } : {
                    paddingLeft: "2rem"
                }}
            >
                {inputHandles}
                {outputHandles}
            </div>
            {onEdit || onDelete ? (
                <div className="flex justify-center px-4">
                    {onEdit ? EditButton(onEdit) : <></>}
                    {onDelete ? DeleteButton(onDelete) : <></>}
                </div>
            ) : (<></>)}
        </div>
    )
}

export default WiringNode