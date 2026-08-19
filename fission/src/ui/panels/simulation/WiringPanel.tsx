import "@xyflow/react/dist/style.css"
import { Box, Stack, useTheme } from "@mui/material"
import {
    type Connection,
    type FinalConnectionState,
    type Edge as FlowEdge,
    type Node as FlowNode,
    ReactFlow,
    ReactFlowProvider,
    useEdgesState,
    useNodesState,
    useReactFlow,
} from "@xyflow/react"
import type React from "react"
import { useCallback, useEffect, useMemo, useReducer, useState, type JSX } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import {
    removeConnection,
    type HandleInfo,
    handleInfoDisplayCompare,
    makeConnection,
    removeNode,
    type SimConfigData,
} from "@/systems/simulation/wiring/SimGraph"
import { compile } from "@/systems/simulation/wiring/Compile"
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
import { NODE_ID_ROBOT_IO, NODE_ID_SIM_IN, NODE_ID_SIM_OUT, nodeTypes } from "@/systems/simulation/wiring/NodeKinds"
import {
    addConstructorNode,
    addDeconstructorNode,
    addJunctionNode,
    defaultConfig,
    syncRobotIOHandles,
    syncSimIOHandles,
} from "@/systems/simulation/wiring/Factories"
import { titleCase } from "@/util/Utility"

export type ConfigState = "wiring" | "simIO" | "robotIO"

/**
 * WARNING: Please test *thoroughly* when making changes. React Flow is very temperamental with how nodes
 * and object references are maintained.
 */

type ConfigComponentProps = {
    setConfigState: (state: ConfigState) => void
    selectedAssembly: MirabufSceneObject
    simConfig: SimConfigData
    reset?: () => void
}

function generateGraph(
    simConfig: SimConfigData,
    refreshGraph: () => void,
    setConfigState: (state: ConfigState) => void
): [FlowNode[], FlowEdge[]] {
    const nodes: Map<string, FlowNode> = new Map()
    const edges: FlowEdge[] = []

    Object.entries(simConfig.nodes).forEach(([_k, v]) => {
        let onEdit: (() => void) | undefined
        let onRefresh: (() => void) | undefined
        let onDelete: (() => void) | undefined
        let title = ""

        switch (v.id) {
            case NODE_ID_ROBOT_IO:
                title = "Robot IO"
                onEdit = () => setConfigState("robotIO")
                onRefresh = () => {
                    syncRobotIOHandles(simConfig)
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
                title = titleCase(v.kind)
                onDelete = () => {
                    if (removeNode(simConfig, v.id)) refreshGraph()
                }
                break
        }

        nodes.set(v.id, {
            id: v.id,
            type: v.kind,
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
        list.push({ ...v })
    })

    Object.entries(simConfig.edges).forEach(([k, v]) => {
        const sourceHandle = simConfig.handles[v.sourceId]
        const targetHandle = simConfig.handles[v.targetId]

        if (
            sourceHandle?.enabled &&
            targetHandle?.enabled &&
            nodes.has(sourceHandle.nodeId) &&
            nodes.has(targetHandle.nodeId)
        ) {
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

const SimIoComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig }) => {
    const theme = useTheme()

    const [simOut, setSimOut] = useState<Record<string, HandleInfo>>({})
    const [simIn, setSimIn] = useState<Record<string, HandleInfo>>({})

    useEffect(() => {
        const simOut: Record<string, HandleInfo> = {}
        const simIn: Record<string, HandleInfo> = {}
        for (const [_k, v] of Object.entries(simConfig.handles)) {
            if (v.nodeId === NODE_ID_SIM_OUT || v.nodeId === NODE_ID_SIM_IN) {
                if (v.isSource) {
                    simOut[v.id] = { ...v }
                } else {
                    simIn[v.id] = { ...v }
                }
            }
        }
        setSimOut(simOut)
        setSimIn(simIn)
    }, [simConfig])

    return (
        <Stack gap={4} direction={"column"} sx={{ width: "stretch" }}>
            <Label size="lg">Configure the Simulation's IO Modules</Label>
            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1px 1fr",
                    columnGap: "0.5rem",
                }}
            >
                <Stack>
                    <Label size="md">Output</Label>
                    <ScrollView>
                        {Object.values(simOut)
                            .sort(handleInfoDisplayCompare)
                            .map(handle => (
                                <Checkbox
                                    label={`${handle.displayName}`}
                                    key={handle.id}
                                    checked={handle.enabled}
                                    onClick={checked => {
                                        handle.enabled = checked
                                        setSimOut({ ...simOut, [handle.id]: handle })
                                    }}
                                />
                            ))}
                    </ScrollView>
                </Stack>
                <Box sx={{ backgroundColor: theme.palette.text.primary, height: "100%" }} />
                <Stack>
                    <Label size="md">Input</Label>
                    <ScrollView>
                        {Object.values(simIn)
                            .sort(handleInfoDisplayCompare)
                            .map(handle => (
                                <Checkbox
                                    label={`${handle.displayName}`}
                                    key={handle.id}
                                    checked={handle.enabled}
                                    onClick={checked => {
                                        handle.enabled = checked
                                        setSimIn({ ...simIn, [handle.id]: handle })
                                    }}
                                />
                            ))}
                    </ScrollView>
                </Stack>
            </Box>
            <Button sx={{ width: "fit-content", alignSelf: "center" }} onClick={() => setConfigState("wiring")}>
                Back to wiring view
            </Button>
        </Stack>
    )
}

const RobotIoComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig }) => {
    const theme = useTheme()

    const [refreshHook, refreshCheckboxes] = useReducer(x => !x, false)

    const [canEncoders, canMotors, pwmDevices, accelerometers] = useMemo(() => {
        const canEncoders: JSX.Element[] = []
        const canMotors: JSX.Element[] = []
        const pwmDevices: JSX.Element[] = []
        const accelerometers: JSX.Element[] = []

        Object.entries(simConfig.handles).forEach(([_k, v]) => {
            if (v.nodeId !== NODE_ID_ROBOT_IO) return

            const checkbox = (
                <Checkbox
                    label={v.displayName}
                    key={v.id}
                    checked={v.enabled}
                    onClick={enabled => {
                        v.enabled = enabled
                        refreshCheckboxes()
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
                    canEncoders.push(checkbox)
                    break
                case SimType.ACCELEROMETER:
                    accelerometers.push(checkbox)
                    break
            }
        })

        return [canEncoders, canMotors, pwmDevices, accelerometers]
    }, [simConfig, refreshHook])

    return (
        <Stack gap={4}>
            <Label size="lg">Configure your Robot's IO Module</Label>
            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1px 1fr",
                    columnGap: "0.5rem",
                }}
            >
                <Stack>
                    <Label size="md">Input</Label>
                    <ScrollView>
                        <Label size="md">CAN Encoders</Label>
                        {canEncoders}
                        <Label size="md">Accelerometers</Label>
                        {accelerometers}
                    </ScrollView>
                </Stack>
                <Box sx={{ backgroundColor: theme.palette.text.primary, height: "100%" }} />
                <Stack>
                    <Label size="md">Output</Label>
                    <ScrollView>
                        <Label size="md">CAN Motors</Label>
                        {canMotors}
                        <Label size="md">PWM Devices</Label>
                        {pwmDevices}
                    </ScrollView>
                </Stack>
            </Box>
            <Button sx={{ width: "fit-content", alignSelf: "center" }} onClick={() => setConfigState("wiring")}>
                Back to wiring view
            </Button>
        </Stack>
    )
}

const WiringComponent: React.FC<ConfigComponentProps> = ({ setConfigState, simConfig, reset }) => {
    const { screenToFlowPosition } = useReactFlow()
    const [nodes, setNodes, onNodesChange] = useNodesState([] as FlowNode[])
    const [edges, setEdges, onEdgesChange] = useEdgesState([] as FlowEdge[])
    const [refreshHook, refreshGraph] = useReducer(x => !x, false) // Whenever I use reducers, it's always sketch. -Hunter

    // Essentially a callback, but it can use itself
    useEffect(() => {
        const [nodes, edges] = generateGraph(simConfig, refreshGraph, setConfigState)
        setNodes(nodes)
        setEdges(edges)
    }, [setConfigState, setEdges, setNodes, simConfig, refreshHook])

    const onEdgeDoubleClick = useCallback(
        (_: React.MouseEvent, edge: FlowEdge) => {
            if (removeConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)) {
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
            if (makeConnection(simConfig, sourceId!, targetId!)) {
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
            // null means wildcard, so any type can connect to handle.
            // we can't deconstruct wildcard handles
            if (!handleInfo || handleInfo.noraType === null) {
                return
            }

            const newHandleId = (handleInfo.isSource ? addDeconstructorNode : addConstructorNode)(
                simConfig,
                handleInfo.noraType!,
                screenToFlowPosition({ x: clientX, y: clientY })
            )
            if (!newHandleId) return

            if (
                handleInfo.isSource
                    ? makeConnection(simConfig, handleInfo.id, newHandleId)
                    : makeConnection(simConfig, newHandleId, handleInfo.id)
            )
                refreshGraph()
        },
        [screenToFlowPosition, simConfig]
    )

    const onCreateJunction = useCallback(() => {
        addJunctionNode(simConfig)
        refreshGraph()
    }, [refreshGraph, simConfig])

    const onEdgesDelete = useCallback(
        (edges: FlowEdge[]) => {
            edges.forEach(edge => {
                removeConnection(simConfig, edge.sourceHandle!, edge.targetHandle!)
            })
            refreshGraph()
        },
        [refreshGraph, simConfig]
    )

    return (
        <ReactFlow
            colorMode="dark"
            nodes={nodes}
            edges={edges}
            onNodeDragStop={onNodeDragStop}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onEdgeDoubleClick={onEdgeDoubleClick}
            onEdgesDelete={onEdgesDelete}
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
    }, [addToast])

    useEffect(() => {
        if (!selectedAssembly) return

        const existingConfig = selectedAssembly.simConfigData
        if (existingConfig) {
            console.debug("Existing SimConfig found")
            const config = JSON.parse(JSON.stringify(existingConfig)) as SimConfigData
            syncRobotIOHandles(config)
            syncSimIOHandles(config, selectedAssembly)
            setSimConfig(config)
        } else {
            console.debug("No SimConfig found, creating default...")
            setSimConfig(defaultConfig(selectedAssembly))
        }
    }, [selectedAssembly])

    const save = useCallback(() => {
        if (simConfig && selectedAssembly) {
            const { flows, error } = compile(simConfig, selectedAssembly)
            if (flows) {
                console.debug(`${flows.length} Flows Successfully Compiled!`)
            } else {
                addToast("error", "Compilation Failed", error)
            }

            selectedAssembly.updateSimConfig(simConfig)
        } else {
            console.warn("Failed to save SimConfig", simConfig, selectedAssembly)
        }
    }, [addToast, selectedAssembly, simConfig])

    const reset = useCallback(() => {
        if (selectedAssembly) {
            setSimConfig(defaultConfig(selectedAssembly))
        }
    }, [selectedAssembly])

    useEffect(() => {
        configureScreen(panel!, { title: "Wiring Panel", acceptText: "Apply" }, { onBeforeAccept: save })
    }, [configureScreen, panel, save])

    return (
        <>
            {selectedAssembly && simConfig ? (
                <Box
                    sx={{
                        display: "flex",
                        width: "70vw",
                        height: "70vh",
                    }}
                >
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
                </Box>
            ) : (
                "ERRR"
            )}
        </>
    )
}

export default WiringPanel
