import "@xyflow/react/dist/style.css"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SectionDivider, SectionLabel, SynthesisIcons } from "@/ui/components/StyledComponents"
import React, { ComponentType, useCallback, useEffect, useMemo, useReducer, useState } from "react"
import {
    ReactFlow,
    Node as FlowNode,
    Edge as FlowEdge,
    useNodesState,
    useEdgesState,
    NodeProps,
    Connection,
    FinalConnectionState,
    useReactFlow,
    ReactFlowProvider,
} from "@xyflow/react"
import {
    ConfigState,
    HandleInfo,
    handleInfoDisplayCompare,
    NODE_ID_ROBOT_IO,
    NODE_ID_SIM_IN,
    NODE_ID_SIM_OUT,
    SimConfig,
} from "./SimConfigShared"
import Label, { LabelSize } from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import Checkbox from "@/ui/components/Checkbox"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Button from "@/ui/components/Button"
import { usePanelControlContext } from "@/ui/PanelContext"
import { Global_AddToast } from "@/ui/components/GlobalUIControls"
import { SimConfigData } from "./SimConfigShared"
import FlowControls from "./FlowControls"
import WiringNode from "./WiringNode"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { isNoraDeconstructable } from "@/systems/simulation/Nora"
import InputSystem from "@/systems/input/InputSystem"
import FlowInfo from "./FlowInfo"

type ConfigComponentProps = {
    setConfigState: (state: ConfigState) => void
    selectedAssembly: MirabufSceneObject
    simConfig: SimConfigData
    reset?: () => void
}

type NodeType = ComponentType<
    NodeProps & {
        data: Record<string, unknown>
        type: string
    }
>

// This took way too long
const nodeTypes: Record<string, NodeType> = [WiringNode].reduce<{ [k: string]: NodeType }>((prev, next) => {
    prev[next.name] = next
    return prev
}, {})

function generateGraph(
    simConfig: SimConfigData,
    refreshGraph: () => void,
    setConfigState: (state: ConfigState) => void
): [FlowNode[], FlowEdge[]] {
    const nodes: Map<string, FlowNode> = new Map()
    const edges: FlowEdge[] = []

    Object.entries(simConfig.nodes).forEach(([_k, v]) => {
        let onEdit: (() => void) | undefined = undefined
        let onRefresh: (() => void) | undefined = undefined
        let onDelete: (() => void) | undefined = undefined
        let title: string = ""

        switch (v.id) {
            case NODE_ID_ROBOT_IO:
                title = "Robot IO"
                onEdit = () => setConfigState("robotIO")
                onRefresh = () => {
                    SimConfig.RefreshRobotIO(simConfig)
                    refreshGraph()
                }
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
                    if (SimConfig.RemoveNode(simConfig, v.id)) refreshGraph()
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
                onRefresh: onRefresh,
                onDelete: onDelete,
                simConfig: simConfig,
                input: [],
                output: [],
                tooltip: v.tooltip,
            },
        })
    })

    Object.entries(simConfig.handles).forEach(([_k, v]) => {
        if (!v.enabled) return
        const node = nodes.get(v.nodeId)
        if (!node) {
            console.warn("Orphaned handle found")
            return
        }
        const list = (v.isSource ? node.data.output : node.data.input) as unknown[]
        list.push(v)
    })

    Object.entries(simConfig.edges).forEach(([k, v]) => {
        const sourceHandle = simConfig.handles[v.sourceId]
        const targetHandle = simConfig.handles[v.targetId]

        if (sourceHandle?.enabled && targetHandle?.enabled) {
            edges.push({
                id: k,
                source: sourceHandle.nodeId,
                target: targetHandle.nodeId,
                sourceHandle: sourceHandle.id,
                targetHandle: targetHandle.id,
            })
        }
    })

    return [[...nodes.values()], edges]
}

function SimIOComponent({ setConfigState, simConfig }: ConfigComponentProps) {
    const simOut: HandleInfo[] = []
    const simIn: HandleInfo[] = []
    Object.entries(simConfig.handles).forEach(([_k, v]) => {
        if (v.nodeId == NODE_ID_SIM_OUT || v.nodeId == NODE_ID_SIM_IN) {
            const list = v.isSource ? simOut : simIn
            list.push(v)
        }
    })

    return (
        <div className="flex flex-col w-full gap-4">
            <Label className="text-center" size={LabelSize.Medium}>
                Configure the Simulation's IO Modules
            </Label>
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "1rem",
                }}
            >
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Output</Label>
                    <ScrollView className="h-full px-2">
                        {simOut.sort(handleInfoDisplayCompare).map(handle => (
                            <Checkbox
                                key={handle.id}
                                label={`${handle.displayName}`}
                                defaultState={handle.enabled}
                                onClick={checked => {
                                    handle.enabled = checked
                                }}
                            />
                        ))}
                    </ScrollView>
                </div>
                <div className="flex flex-col justify-center grow">
                    <Label className="text-center">Input</Label>
                    <ScrollView className="h-full px-2">
                        {simIn.sort(handleInfoDisplayCompare).map(handle => (
                            <Checkbox
                                key={handle.id}
                                label={`${handle.displayName}`}
                                defaultState={handle.enabled}
                                onClick={checked => {
                                    handle.enabled = checked
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

        Object.entries(simConfig.handles).forEach(([_k, v]) => {
            if (v.nodeId != NODE_ID_ROBOT_IO) return

            const checkbox = (
                <Checkbox
                    key={v.id}
                    label={`${v.displayName}`}
                    defaultState={v.enabled}
                    onClick={checked => {
                        v.enabled = checked
                    }}
                />
            )

            switch (v.originType) {
                case SimType.CANMotor:
                    canMotors.push(checkbox)
                    break
                case SimType.PWM:
                    pwmDevices.push(checkbox)
                    break
                case SimType.CANEncoder:
                    canEncoders.push(checkbox)
                    break
                case SimType.Accel:
                    accelerometers.push(checkbox)
                    break
            }
        })

        return [canEncoders, canMotors, pwmDevices, accelerometers]
    }, [simConfig])

    return (
        <div className="flex flex-col w-full gap-4">
            <Label className="text-center" size={LabelSize.Medium}>
                Configure your Robot's IO Module
            </Label>
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    columnGap: "1rem",
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

function WiringComponent({ setConfigState, simConfig, reset }: ConfigComponentProps) {
    const { screenToFlowPosition } = useReactFlow()
    const [nodes, setNodes, onNodesChange] = useNodesState([] as FlowNode[])
    const [edges, setEdges, onEdgesChange] = useEdgesState([] as FlowEdge[])
    const [refreshHook, refreshGraph] = useReducer(x => !x, false) // Whenever I use reducers, it's always sketch. -Hunter

    // Essentially a callback, but it can use itself.
    useEffect(() => {
        const [nodes, edges] = generateGraph(simConfig, refreshGraph, setConfigState)
        setNodes(nodes)
        setEdges(edges)
    }, [setConfigState, setEdges, setNodes, simConfig, refreshHook])

    const onEdgeDoubleClick = useCallback(
        (_: React.MouseEvent, edge: FlowEdge) => {
            if (SimConfig.DeleteConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)) {
                refreshGraph()
            }
        },
        [simConfig]
    )

    const onNodeDragStop = useCallback(
        (_event: React.MouseEvent, node: FlowNode, _nodes: FlowNode[]) => {
            const nodeInfo = simConfig.nodes[node.id]
            if (!nodeInfo) {
                console.warn(`Unregistered Node detected: ${node.id}`)
                return
            }
            nodeInfo.position = node.position
        },
        [simConfig]
    )

    const onConnect = useCallback(
        (connection: Connection) => {
            const sourceId = connection.sourceHandle
            const targetId = connection.targetHandle
            if (SimConfig.MakeConnection(simConfig, sourceId!, targetId!)) {
                refreshGraph()
            }
        },
        [simConfig]
    )

    const onConnectEnd = useCallback(
        (event: MouseEvent | TouchEvent, state: FinalConnectionState) => {
            if (state.isValid || state.fromHandle == null) return

            if (!(InputSystem.isKeyPressed("AltRight") || InputSystem.isKeyPressed("AltLeft"))) return

            const { clientX, clientY } = "changedTouches" in event ? event.changedTouches[0] : event

            const handleInfo = simConfig.handles[state.fromHandle.id!]
            if (!handleInfo || !isNoraDeconstructable(handleInfo.noraType)) {
                return
            }

            const newHandleId = (handleInfo.isSource ? SimConfig.AddDeconstructorNode : SimConfig.AddConstructorNode)(
                simConfig,
                handleInfo.noraType,
                screenToFlowPosition({ x: clientX, y: clientY })
            )
            if (!newHandleId) return

            if (
                handleInfo.isSource
                    ? SimConfig.MakeConnection(simConfig, handleInfo.id, newHandleId)
                    : SimConfig.MakeConnection(simConfig, newHandleId, handleInfo.id)
            )
                refreshGraph()
        },
        [screenToFlowPosition, simConfig]
    )

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
            <FlowInfo reset={reset ?? (() => {})} />
        </ReactFlow>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {
    const [configState, setConfigState] = useState<ConfigState>("wiring")
    const { closePanel } = usePanelControlContext()
    const [simConfig, setSimConfig] = useState<SimConfigData | undefined>(undefined)

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

    useEffect(() => {
        if (!selectedAssembly) return

        const existingConfig = selectedAssembly.simConfigData
        if (existingConfig) {
            setSimConfig(JSON.parse(JSON.stringify(existingConfig))) // Create copy to not force a save
        } else {
            setSimConfig(SimConfig.Default(selectedAssembly))
        }
    }, [selectedAssembly])

    const save = useCallback(() => {
        if (simConfig && selectedAssembly) {
            const flows = SimConfig.Compile(simConfig, selectedAssembly)
            if (!flows) {
                console.error("Compilation Failed")
                return
            }
            console.debug(`${flows.length} Flows Successfully Compiled!`)

            selectedAssembly.UpdateSimConfig(simConfig)
        }
    }, [selectedAssembly, simConfig])

    const reset = useCallback(() => {
        if (selectedAssembly) {
            setSimConfig(SimConfig.Default(selectedAssembly))
        }
    }, [selectedAssembly])

    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
            full
            onAccept={save}
        >
            {selectedAssembly && simConfig ? (
                <div className="flex grow">
                    {configState === "wiring" ? (
                        <ReactFlowProvider>
                            <WiringComponent
                                reset={reset}
                                simConfig={simConfig}
                                selectedAssembly={selectedAssembly}
                                setConfigState={setConfigState}
                            />
                        </ReactFlowProvider>
                    ) : (
                        <></>
                    )}
                    {configState === "robotIO" ? (
                        <RobotIOComponent
                            simConfig={simConfig}
                            selectedAssembly={selectedAssembly}
                            setConfigState={setConfigState}
                        />
                    ) : (
                        <></>
                    )}
                    {configState === "simIO" ? (
                        <SimIOComponent
                            simConfig={simConfig}
                            selectedAssembly={selectedAssembly}
                            setConfigState={setConfigState}
                        />
                    ) : (
                        <></>
                    )}
                </div>
            ) : (
                <>ERRR</>
            )}
        </Panel>
    )
}

export default WiringPanel
