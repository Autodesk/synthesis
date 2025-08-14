import { Grid, Stack } from "@mui/material"
import {
    type Connection,
    type FinalConnectionState,
    type Edge as FlowEdge,
    type Node as FlowNode,
    type NodeProps,
    ReactFlow,
    ReactFlowProvider,
    useEdgesState,
    useNodesState,
    useReactFlow,
} from "@xyflow/react"
import type React from "react"
import { type ComponentType, useCallback, useEffect, useMemo, useReducer, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import { isNoraDeconstructable } from "@/systems/simulation/Nora"
import {
    type ConfigState,
    type HandleInfo,
    handleInfoDisplayCompare,
    NODE_ID_ROBOT_IO,
    NODE_ID_SIM_IN,
    NODE_ID_SIM_OUT,
    SimConfig,
    type SimConfigData,
} from "@/systems/simulation/SimConfigShared"
import { SimType } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import World from "@/systems/World.ts"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import ScrollView from "@/ui/components/ScrollView"
import { Button } from "@/ui/components/StyledComponents"
import FlowControls from "@/ui/components/simulation/FlowControls"
import FlowInfo from "@/ui/components/simulation/FlowInfo"
import { useUIContext } from "../../helpers/UIProviderHelpers"
import WiringNode from "./WiringNode"

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

const nodeTypes: Record<string, NodeType> = [WiringNode].reduce<{
    [k: string]: NodeType
}>((prev, next) => {
    prev[next.name] = next as NodeType
    return prev
}, {})

function generateGraph(
    simConfig: SimConfigData,
    refreshGraph: () => void,
    setConfigState: (state: ConfigState) => void
): [FlowNode[], FlowEdge[]] {
    const nodes: Map<string, FlowNode> = new Map()
    const edges: FlowEdge[] = []

    for (const [_k, v] of Object.entries(simConfig.nodes)) {
        let onEdit: (() => void) | undefined
        let onRefresh: (() => void) | undefined
        let onDelete: (() => void) | undefined
        let title = ""

        switch (v.id) {
            case NODE_ID_ROBOT_IO:
                title = "Robot IO"
                onEdit = () => setConfigState("robotIO")
                onRefresh = () => {
                    SimConfig.refreshRobotIO(simConfig)
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
            ...v,
            data: {
                title,
                onEdit,
                onRefresh,
                onDelete,
                simConfig,
                input: [],
                output: [],
                tooltip: v.tooltip,
            },
        })
    }

    for (const [_k, v] of Object.entries(simConfig.handles)) {
        if (!v.enabled) break
        const node = nodes.get(v.nodeId)
        if (!node) {
            console.warn("Orphaned handle found")
            break
        }
        const list = (v.isSource ? node.data.output : node.data.input) as unknown[]
        list.push(v)
    }

    for (const [k, v] of Object.entries(simConfig.edges)) {
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
    }

    return [[...nodes.values()], edges]
}

const SimIoComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig }) => {
    const simOut: HandleInfo[] = []
    const simIn: HandleInfo[] = []
    for (const [_k, v] of Object.entries(simConfig.handles)) {
        if (v.nodeId === NODE_ID_SIM_OUT || v.nodeId === NODE_ID_SIM_IN) {
            const list = v.isSource ? simOut : simIn
            list.push(v)
        }
    }

    return (
        <Stack gap={4}>
            <Label size="md">Configure the Simulation's IO Modules</Label>
            <Grid>
                <Stack>
                    <Label size="sm">Output</Label>
                    <ScrollView>
                        {simOut.sort(handleInfoDisplayCompare).map(handle => (
                            <Checkbox
                                label={`${handle.displayName}`}
                                key={handle.id}
                                checked={handle.enabled}
                                onClick={checked => {
                                    handle.enabled = checked
                                }}
                            />
                        ))}
                    </ScrollView>
                </Stack>
                <Stack>
                    <Label size="sm">Input</Label>
                    <ScrollView>
                        {simIn.sort(handleInfoDisplayCompare).map(handle => (
                            <Checkbox
                                label={`${handle.displayName}`}
                                key={handle.id}
                                checked={handle.enabled}
                                onClick={checked => {
                                    handle.enabled = checked
                                }}
                            />
                        ))}
                    </ScrollView>
                </Stack>
            </Grid>
            <Button onClick={() => setConfigState("wiring")}>Back to wiring view</Button>
        </Stack>
    )
}

const RobotIoComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig }) => {
    const [canEncoders, canMotors, pwmDevices, accelerometers] = useMemo(() => {
        const canEncoders: JSX.Element[] = []
        const canMotors: JSX.Element[] = []
        const pwmDevices: JSX.Element[] = []
        const accelerometers: JSX.Element[] = []

        for (const [_k, v] of Object.entries(simConfig.handles)) {
            if (v.nodeId !== NODE_ID_ROBOT_IO) return []

            const checkbox = (
                <Checkbox
                    label={v.displayName}
                    key={v.id}
                    checked={v.enabled}
                    onClick={enabled => {
                        v.enabled = enabled
                    }}
                />
            )

            switch (v.originType) {
                case SimType.CAN_MOTOR:
                    canMotors.push(checkbox)
                    break
                case SimType.PWM:
                    pwmDevices.push(checkbox)
                    break
                case SimType.CAN_ENCODER:
                    pwmDevices.push(checkbox)
                    break
                case SimType.ACCELEROMETER:
                    pwmDevices.push(checkbox)
                    break
            }
        }

        return [canEncoders, canMotors, pwmDevices, accelerometers]
    }, [simConfig])

    return (
        <Stack gap={4}>
            <Label size="md">Configure your Robot's IO Module</Label>
            <Grid>
                <Stack>
                    <Label size="sm">Input</Label>
                    <ScrollView>
                        <Label size="md">CAN Encoders</Label>
                        {canEncoders}
                        <Label size="md">Accelerometers</Label>
                        {accelerometers}
                    </ScrollView>
                </Stack>
                <Stack>
                    <Label size="sm">Output</Label>
                    <ScrollView>
                        <Label size="md">CAN Motors</Label>
                        {canMotors}
                        <Label size="md">PWM Devices</Label>
                        {pwmDevices}
                    </ScrollView>
                </Stack>
            </Grid>
            <Button onClick={() => setConfigState("wiring")}>Back to wiring view</Button>
        </Stack>
    )
}

const WiringComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig, reset }) => {
    const { screenToFlowPosition } = useReactFlow()
    const [nodes, setNodes, onNodesChange] = useNodesState([] as FlowNode[])
    const [edges, setEdges, onEdgesChange] = useEdgesState([] as FlowEdge[])
    const [_refreshHook, refreshGraph] = useReducer(x => !x, false) // Whenever I use reducers, it's always sketch. -Hunter

    // Essentially a callback, but it can use itself
    useEffect(() => {
        const [nodes, edges] = generateGraph(simConfig, refreshGraph, setConfigState)
        setNodes(nodes)
        setEdges(edges)
    }, [setConfigState, setEdges, setNodes, simConfig])

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
    }, [simConfig])

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

const WiringPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const [configState, setConfigState] = useState<ConfigState>("wiring")
    const { addToast, configureScreen } = useUIContext()
    const [simConfig, setSimConfig] = useState<SimConfigData | undefined>(undefined)

    const selectedAssembly = useMemo(() => {
        const miraObj = World.sceneRenderer.mirabufSceneObjects.getRobots()[0]
        if (miraObj != null) {
            return miraObj
        }
        addToast("warning", "Missing Robot", "Must have at least one robot spawned for selection.")
        // closePanel(panel!.id, CloseType.Cancel)
    }, [])

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

            selectedAssembly.updateSimConfig(simConfig)
        }
    }, [selectedAssembly, simConfig])

    const reset = useCallback(() => {
        if (selectedAssembly) {
            setSimConfig(SimConfig.Default(selectedAssembly))
        }
    }, [selectedAssembly])

    useEffect(() => {
        configureScreen(panel!, { title: "Wiring Panel" }, { onBeforeAccept: save })
    }, [])

    return (
        <>
            {selectedAssembly && simConfig ? (
                <div className="flex grow">
                    {configState === "wiring" && (
                        <ReactFlowProvider>
                            <WiringComponent
                                reset={reset}
                                simConfig={simConfig}
                                selectedAssembly={selectedAssembly}
                                setConfigState={setConfigState}
                            />
                        </ReactFlowProvider>
                    )}
                    {configState === "robotIO" && (
                        <RobotIoComponent
                            simConfig={simConfig}
                            selectedAssembly={selectedAssembly}
                            setConfigState={setConfigState}
                        />
                    )}
                    {configState === "simIO" && (
                        <SimIoComponent
                            simConfig={simConfig}
                            selectedAssembly={selectedAssembly}
                            setConfigState={setConfigState}
                        />
                    )}
                </div>
            ) : (
                "ERRR"
            )}
        </>
    )
}

export default WiringPanel
