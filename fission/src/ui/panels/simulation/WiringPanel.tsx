import '@xyflow/react/dist/style.css'
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SectionDivider, SectionLabel, SynthesisIcons } from "@/ui/components/StyledComponents"
import React, { ComponentType, useCallback, useEffect, useMemo, useReducer, useState } from "react"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, NodeProps, Connection, FinalConnectionState, useReactFlow, ReactFlowProvider } from "@xyflow/react"
import { ConfigItemInfo, ConfigState, genId, genIdToSavedId, NODE_ID_ROBOT_IO, NODE_ID_SIM_IN, NODE_ID_SIM_OUT, savedIdToGenId, SimConfig, SourceHandle, TargetHandle } from "./SimConfigShared"
import Label, { LabelSize } from "@/ui/components/Label";
import ScrollView from "@/ui/components/ScrollView";
import Checkbox from "@/ui/components/Checkbox";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import World from "@/systems/World";
import Button from "@/ui/components/Button";
import { usePanelControlContext } from "@/ui/PanelContext";
import { Global_AddToast } from "@/ui/components/GlobalUIControls";
import { SimConfigData } from "./SimConfigShared";
import FlowControls from "./FlowControls";
import WiringNode from "./WiringNode";
import { SimType } from '@/systems/simulation/wpilib_brain/WPILibBrain'
import { isNoraDeconstructable } from '@/systems/simulation/Nora'

type ConfigComponentProps = {
    setConfigState: (state: ConfigState) => void,
    selectedAssembly: MirabufSceneObject,
    simConfig: SimConfigData,
}

type NodeType = ComponentType<NodeProps & {
    data: Record<string, unknown>;
    type: string;
}>

// This took way too long
const nodeTypes: Record<string, NodeType> = [
    WiringNode
].reduce<{ [k: string]: NodeType }>((prev, next) => { prev[next.name] = next; return prev }, {})

function generateGraph(simConfig: SimConfigData, refreshGraph: () => void, setConfigState: (state: ConfigState) => void): [FlowNode[], FlowEdge[]] {

    const nodes: Map<string, FlowNode> = new Map()
    const edges: FlowEdge[] = []

    simConfig.nodes.forEach(v => {
        let onEdit: (() => void) | undefined = undefined
        let onDelete: (() => void) | undefined = undefined
        let title: string = ""

        switch (v.id) {
            case NODE_ID_ROBOT_IO:
                title = "Robot IO"
                onEdit = () => setConfigState("robotIO")
                break
            case NODE_ID_SIM_IN:
                title = "Simulation Input"
                onEdit = () => setConfigState("simIO")
                break
            case NODE_ID_SIM_OUT:
                title = "Simulation Output"
                onEdit = () => setConfigState("simIO")
                break
            default:
                onDelete = () => {
                    if (SimConfig.DeleteNode(simConfig, v.id))
                        refreshGraph()
                }
                break
        }

        nodes.set(v.id, {
            id: v.id,
            type: v.type,
            position: v.position,
            data: {
                title: title,
                onEdit: onEdit,
                onDelete: onDelete,
                simConfig: simConfig,
                input: [],
                output: []
            }
        })
    })

    simConfig.sourceHandles.forEach((v, k) => {
        if (!v.enabled)
            return
        const handle = JSON.parse(k) as SourceHandle
        const sourceInfo = simConfig.sourceHandles.get(k)
        const node = nodes.get(handle.nodeId)
        if (!node) {
            console.warn('Orphaned handle found')
            return
        }
        (node.data.output as unknown[]).push([ k, v ])

        if (sourceInfo && sourceInfo.enabled) {
            const connections = simConfig.connections.get(k)!
            connections.forEach(x => {
                const targetHandle = JSON.parse(x) as TargetHandle
                const targetInfo = simConfig.targetHandles.get(x)
                if (targetInfo && targetInfo.enabled) {
                    edges.push({
                        id: genId().toString(),
                        source: handle.nodeId,
                        target: targetHandle.nodeId,
                        sourceHandle: savedIdToGenId(k),
                        targetHandle: savedIdToGenId(x)
                    })
                }
            })
        }
    })

    simConfig.targetHandles.forEach((v, k) => {
        if (!v.enabled)
            return
        const handle = JSON.parse(k) as TargetHandle
        const node = nodes.get(handle.nodeId)
        if (!node) {
            console.warn('Orphaned handle found')
            return
        }
        (node.data.input as unknown[]).push([ k, v ])
    })

    return [[...nodes.values()], edges]
}

function SimIOComponent({ setConfigState, simConfig }: ConfigComponentProps) {

    const simOut: [string, ConfigItemInfo][] = []
    const simIn: [string, ConfigItemInfo][] = []
    simConfig.sourceHandles.forEach((v, k) => {
        const handle = JSON.parse(k) as SourceHandle
        if (handle.nodeId == NODE_ID_SIM_OUT) {
            simOut.push([ k, v ])
        }
    })
    simConfig.targetHandles.forEach((v, k) => {
        const handle = JSON.parse(k) as TargetHandle
        if (handle.nodeId == NODE_ID_SIM_IN) {
            simIn.push([ k, v ])
        }
    })

    return (
        <div className="flex flex-col w-full gap-4">
            <Label className="text-center" size={LabelSize.Medium}>Configure the Simulation's IO Modules</Label>
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "1rem"
                }}
            >
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Output</Label>
                    <ScrollView className="h-full px-2">
                        {simOut.map(([k, v]) => (
                            <Checkbox
                                key={k}
                                label={`${v.displayName}`}
                                defaultState={v.enabled}
                                onClick={checked => {
                                    v.enabled = checked
                                }}
                            />
                        ))}
                    </ScrollView>
                </div>
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Input</Label>
                    <ScrollView className="h-full px-2">
                        {simIn.map(([k, v]) => (
                            <Checkbox
                                key={k}
                                label={`${v.displayName}`}
                                defaultState={v.enabled}
                                onClick={checked => {
                                    v.enabled = checked
                                }}
                            />
                        ))}
                    </ScrollView>
                </div>
            </div>
            <Button className="self-center" value={"Back to wiring view"} onClick={() => setConfigState("wiring")} />
        </div>
    )
}

function RobotIOComponent({ setConfigState, simConfig }: ConfigComponentProps) {

    const [canEncoders, canMotors, pwmDevices, accelerometers] = useMemo(() => {
        const canEncoders: JSX.Element[] = []
        const canMotors: JSX.Element[] = []
        const pwmDevices: JSX.Element[] = []
        const accelerometers: JSX.Element[] = []

        simConfig.sourceHandles.forEach((v, k) => {
            const handle = JSON.parse(k) as SourceHandle
            if (handle.nodeId != NODE_ID_ROBOT_IO)
                return

            const checkbox = (
                <Checkbox
                    key={k}
                    label={`${v.displayName}`}
                    defaultState={v.enabled}
                    onClick={checked => {
                        v.enabled = checked
                    }}
                />
            )

            switch (v.handleType) {
                case SimType.CANMotor:
                    canMotors.push(checkbox)
                    break
                case SimType.PWM:
                    pwmDevices.push(checkbox)
                    break
            }
        })
        simConfig.targetHandles.forEach((v, k) => {
            const handle = JSON.parse(k) as TargetHandle
            if (handle.nodeId != NODE_ID_ROBOT_IO)
                return

            const checkbox = (
                <Checkbox
                    key={k}
                    label={`${v.displayName}`}
                    defaultState={v.enabled}
                    onClick={checked => {
                        v.enabled = checked
                    }}
                />
            )

            switch (v.handleType) {
                case SimType.CANEncoder:
                    canEncoders.push(checkbox)
                    break
                case SimType.Accel:
                    accelerometers.push(checkbox)
            }
        })

        return [canEncoders, canMotors, pwmDevices, accelerometers]
    }, [simConfig])

    return (
        <div className="flex flex-col w-full gap-4">
            <Label className="text-center" size={LabelSize.Medium}>Configure your Robot's IO Module</Label>
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "1rem"
                }}
            >
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Input</Label>
                    <ScrollView className="h-full px-2">
                        <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            CAN Encoders
                        </SectionLabel>
                        <SectionDivider />
                        {canEncoders}
                        <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            Accelerometers
                        </SectionLabel>
                        <SectionDivider />
                        {accelerometers}
                    </ScrollView>
                </div>
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Output</Label>
                    <ScrollView className="h-full px-2">
                        <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            CAN Motors
                        </SectionLabel>
                        <SectionDivider />
                        {canMotors}
                        <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            PWM Devices
                        </SectionLabel>
                        <SectionDivider />
                        {pwmDevices}
                    </ScrollView>
                </div>
            </div>
            <Button className="self-center" value={"Back to wiring view"} onClick={() => setConfigState("wiring")} />
        </div>
    )
}

function WiringComponent({ setConfigState, simConfig }: ConfigComponentProps) {
    const { screenToFlowPosition } = useReactFlow()
    const [nodes, setNodes, onNodesChange] = useNodesState([] as FlowNode[]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([] as FlowEdge[]);
    const [refreshHook, refreshGraph] = useReducer(x => !x, false) // Whenever I use reducers, it's always sketch. -Hunter

    // Essentially a callback, but it can use itself.
    useEffect(() => {
        const [nodes, edges] = generateGraph(simConfig, refreshGraph, setConfigState)
        setNodes(nodes)
        setEdges(edges)
    }, [setConfigState, setEdges, setNodes, simConfig, refreshHook])

    const onEdgeDoubleClick = useCallback((_: React.MouseEvent, edge: FlowEdge) => {
        if (SimConfig.DeleteConnection(simConfig, genIdToSavedId(edge.sourceHandle!)!, genIdToSavedId(edge.targetHandle!)!)) {
            refreshGraph()
        }
    }, [simConfig])

    const onNodeDragStop = useCallback((_event: React.MouseEvent, node: FlowNode, _nodes: FlowNode[]) => {
        const nodeInfo = simConfig.nodes.get(node.id)
        if (!nodeInfo) {
            console.warn(`Unregistered Node detected: ${node.id}`)
            return
        }
        nodeInfo.position = node.position
    }, [simConfig])

    const onConnect = useCallback((connection: Connection) => {
        const sourceId = genIdToSavedId(connection.sourceHandle as string)
        const targetId = genIdToSavedId(connection.targetHandle as string)
        if (SimConfig.MakeConnection(simConfig, sourceId!, targetId!)) {
            refreshGraph()
        }
    }, [simConfig])

    const onConnectEnd = useCallback((event: MouseEvent | TouchEvent, state: FinalConnectionState) => {
        if (state.isValid)
            return

        if (state.fromHandle == null)
            return

        const { clientX, clientY } = 'changedTouches' in event ? event.changedTouches[0] : event

        if (state.fromHandle.type == "source") {
            const sourceId = genIdToSavedId(state.fromHandle.id!)
            if (sourceId == undefined)
                return
            const sourceInfo = simConfig.sourceHandles.get(sourceId)
            if (sourceInfo == undefined)
                return
            if (isNoraDeconstructable(sourceInfo.noraType)) {
                const targetId = SimConfig.AddDeconstructorNode(simConfig, sourceInfo.noraType, screenToFlowPosition({ x: clientX, y: clientY }))
                if (targetId == undefined)
                    return
                if (SimConfig.MakeConnection(simConfig, sourceId, targetId))
                    refreshGraph()
            }
        } else {
            const targetId = genIdToSavedId(state.fromHandle.id!)
            if (targetId == undefined)
                return
            const targetInfo = simConfig.targetHandles.get(targetId)
            if (targetInfo == undefined)
                return
            if (isNoraDeconstructable(targetInfo.noraType) && (targetInfo.many || (simConfig.connectionCount.get(targetId) ?? 0) < 1)) {
                const sourceId = SimConfig.AddConstructorNode(simConfig, targetInfo.noraType, screenToFlowPosition({ x: clientX, y: clientY }))
                if (sourceId == undefined)
                    return
                if (SimConfig.MakeConnection(simConfig, sourceId, targetId))
                    refreshGraph()
            }
        }
    }, [screenToFlowPosition, simConfig])

    const onCreateJunction = useCallback(() => {
        SimConfig.AddJunctionNode(simConfig)
        refreshGraph()
    }, [refreshGraph, simConfig])

    return (
        <ReactFlow
            colorMode="dark"
            nodes={nodes}
            edges={edges}
            onNodeDragStop={onNodeDragStop}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onEdgeDoubleClick={onEdgeDoubleClick}
            onConnect={onConnect}
            onConnectEnd={onConnectEnd}
            nodeTypes={nodeTypes}
            fitView
        >
            {/* <Controls /> */}
            <FlowControls onCreateJunction={onCreateJunction} />
        </ReactFlow>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {
    const [configState, setConfigState] = useState<ConfigState>("wiring")
    const { closePanel } = usePanelControlContext();

    const selectedAssembly = useMemo(() => {
        const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
        if (miraObjs.length > 0) {
            return miraObjs[0][1] as MirabufSceneObject
        } else {
            // TEMPORARY: Will be moved to config panel to ensure selected assembly
            Global_AddToast?.("warning", "Missing Robot", "Must have at least one robot spawned for selection.")
            closePanel(panelId)
        }
    }, [closePanel, panelId])

    const simConfig = useMemo(() => {
        if (!selectedAssembly)
            return
        
        // Generate Default Config
        return SimConfig.Default(selectedAssembly)
    }, [selectedAssembly])

    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
            full
        >{selectedAssembly && simConfig ? (
            <div className="flex grow">
                {configState === "wiring" ? (
                    <ReactFlowProvider>
                        <WiringComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} />
                    </ReactFlowProvider>
                ) : (<></>) }
                {configState === "robotIO" ? <RobotIOComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} /> : <></>}
                {configState === "simIO" ? <SimIOComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} /> : <></>}
            </div>
        ) : (<>ERRR</>)}</Panel>
    )
}

export default WiringPanel