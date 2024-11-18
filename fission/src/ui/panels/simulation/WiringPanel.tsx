/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-explicit-any */
import '@xyflow/react/dist/style.css'
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SectionDivider, SectionLabel, SynthesisIcons } from "@/ui/components/StyledComponents"
import React, { ComponentType, useCallback, useEffect, useMemo, useReducer, useState } from "react"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, addEdge, NodeProps } from "@xyflow/react"
import { ConfigState, NODE_ID_ROBOT_IO, NODE_ID_SIM_IN, NODE_ID_SIM_OUT, SimConfig } from "./SimConfigShared"
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
import JunctionNode from "./JunctionNode";
import WiringNode from "./WiringNode";
import { SimType } from '@/systems/simulation/wpilib_brain/WPILibBrain'

type ConfigComponentProps = {
    setConfigState: (state: ConfigState) => void,
    selectedAssembly: MirabufSceneObject,
    simConfig: SimConfigData,
}

type NodeType = ComponentType<NodeProps & {
    data: any;
    type: any;
}>

// This took way too long
const nodeTypes: Record<string, NodeType> = [
        WiringNode,
        JunctionNode
    ].reduce<{ [k: string]: NodeType }>((prev, next) => { prev[next.name] = next; return prev }, {})
const initialEdges: FlowEdge[] = []

function generateNodes(assembly: MirabufSceneObject, simConfig: SimConfigData, refreshGraph: () => void, setConfigState: (state: ConfigState) => void): FlowNode[] {
    const ioNode = {
        id: NODE_ID_ROBOT_IO,
        type: WiringNode.name,
        position: simConfig.robotIOPosition,
        data: {
            title: "Robot IO",
            onEdit: () => setConfigState("robotIO"),
            simConfig: simConfig,
            input: [...simConfig.robotIn.entries()].filter(([_, x]) => x.enabled),
            output: [...simConfig.robotOut.entries()].filter(([_, x]) => x.enabled),
        },
    }

    const outNode = {
        id: NODE_ID_SIM_OUT,
        type: WiringNode.name,
        position: simConfig.simOutPosition,
        data: {
            title: "Sim Output",
            onEdit: () => setConfigState("simIO"),
            simConfig: simConfig,
            output: [...simConfig.simOut.entries()].filter(([_, x]) => x.enabled),
        },
    }

    const inNode = {
        id: NODE_ID_SIM_IN,
        type: WiringNode.name,
        position: simConfig.simInPosition,
        data: {
            title: "Sim Input",
            onEdit: () => setConfigState("simIO"),
            input: [...simConfig.simIn.entries()].filter(([_, x]) => x.enabled),
        },
    }

    const junctions: FlowNode[] = [...simConfig.junctions.entries()].map(([k, v]) => {
        return {
            id: k,
            type: JunctionNode.name,
            position: v.position,
            data: {
                onDelete: () => {
                    SimConfig.DeleteJunction(simConfig, k)
                    refreshGraph()
                }
            }
        }
    })

    return [ioNode, outNode, inNode, ...junctions]
}

function SimIOComponent({ setConfigState, selectedAssembly, simConfig }: ConfigComponentProps) {
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
                        {[...simConfig.simOut.entries()].map(([k, v]) => (
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
                        {[...simConfig.simIn.entries()].map(([k, v]) => (
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

function RobotIOComponent({ setConfigState, selectedAssembly, simConfig }: ConfigComponentProps) {

    const [canEncoders, canMotors, pwmDevices, accelerometers] = useMemo(() => {
        const canEncoders: JSX.Element[] = []
        const canMotors: JSX.Element[] = []
        const pwmDevices: JSX.Element[] = []
        const accelerometers: JSX.Element[] = []

        simConfig.robotIn.forEach((v, k) => {
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

            switch (v.type!) {
                case SimType.CANEncoder:
                    canEncoders.push(checkbox)
                    break
                case SimType.Accel:
                    accelerometers.push(checkbox)
            }
        })

        simConfig.robotOut.forEach((v, k) => {
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

            switch (v.type!) {
                case SimType.CANMotor:
                    canMotors.push(checkbox)
                    break
                case SimType.PWM:
                    pwmDevices.push(checkbox)
                    break
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

function WiringComponent({ setConfigState, selectedAssembly, simConfig }: ConfigComponentProps) {
    const [nodes, setNodes, onNodesChange] = useNodesState([] as FlowNode[]);
    const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);
    const [refreshHook, refreshGraph] = useReducer(x => !x, false) // Whenever I use reducers, it's always sketch. -Hunter

    // Essentially a callback, but it can use it's self.
    useEffect(() => {
        setNodes(generateNodes(selectedAssembly, simConfig, refreshGraph, setConfigState))
    }, [selectedAssembly, setConfigState, setNodes, simConfig, refreshHook])

    const onEdgeDoubleClick = useCallback((_: React.MouseEvent, edge: FlowEdge) => {
        setEdges(edges.filter(x => x.id != edge.id))
    }, [edges, setEdges])

    const onNodeDragStop = useCallback((_event: React.MouseEvent, node: FlowNode, _nodes: FlowNode[]) => {
        switch (node.id) {
            case NODE_ID_ROBOT_IO:
                simConfig.robotIOPosition = node.position
                break
            case NODE_ID_SIM_IN:
                simConfig.simInPosition = node.position
                break
            case NODE_ID_SIM_OUT:
                simConfig.simOutPosition = node.position
                break
            default: {
                const junct = simConfig.junctions.get(node.id)
                if (junct) {
                    junct.position = node.position
                }
                break
            }
        }
    }, [simConfig])
    
    const onConnect = useCallback(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (params: any) => setEdges((eds) => addEdge(params, eds)),
        [setEdges],
    );

    const onCreateJunction = useCallback(() => {
        SimConfig.MakeJunction(simConfig)
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
            onConnect={onConnect}
            onEdgeDoubleClick={onEdgeDoubleClick}
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
                {configState === "wiring" ? <WiringComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} /> : <></>}
                {configState === "robotIO" ? <RobotIOComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} /> : <></>}
                {configState === "simIO" ? <SimIOComponent simConfig={simConfig} selectedAssembly={selectedAssembly} setConfigState={setConfigState} /> : <></>}
            </div>
        ) : (<>ERRR</>)}</Panel>
    )
}

export default WiringPanel