import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useCallback, useMemo, useState } from "react"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, addEdge, Controls } from "@xyflow/react"

import '@xyflow/react/dist/style.css';
import RobotIONode from "./RobotIONode"
import SimInputNode from "./SimInputNode"
import SimOutputNode from "./SimOutputNode"
import WPILibBrain, { simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { ConfigState } from "./SimConfigControls"
import Label, { LabelSize } from "@/ui/components/Label";
import ScrollView from "@/ui/components/ScrollView";
import Checkbox from "@/ui/components/Checkbox";
import Driver from "@/systems/simulation/driver/Driver";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import World from "@/systems/World";

const initialEdges: FlowEdge[] = []
// const initialEdges: FlowEdge[] = [
//     { id: "e1-2", source: "1", target: "2" }
// ]

const nodeTypes = { robotIO: RobotIONode, simInput: SimInputNode, simOutput: SimOutputNode }

type ConfigComponentProps = {
    setConfigState: (state: ConfigState) => void,
}

function getDriverSignals(): Driver[] {
    const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(x => x[1] instanceof MirabufSceneObject)
    if (miraObjs.length > 0) {
        const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
        const simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
        return simLayer?.drivers ?? []
        // brain = simLayer?.brain as WPILibBrain
    }
    return []
}

function getCANDevices(): [string, Map<string, number | boolean | string>][] {
    const cans = simMap.get(SimType.CANMotor) ?? new Map<string, Map<string, number>>()
    return [...cans.entries()].filter(([_, data]) => data.get("<init")).reverse()
}

function generateNodes(setConfigState: (state: ConfigState) => void): FlowNode[] {
    const ioNode = {
        id: 'robot-io-node',
        type: 'robotIO',
        position: { x: 0, y: 0 },
        data: {
            onEdit: () => setConfigState("robotIO"),
            input: [ "Encoder 1", "Encoder 2" ],
            output: [ "CAN 0", "CAN 1", "CAN 2", "CAN 3" ],
        },
    }

    const outNode = {
        id: 'sim-output-node',
        type: 'simOutput',
        position: { x: -400, y: 0 },
        data: {
            onEdit: () => setConfigState("simOut"),
            output: [ "Wheel 1", "Wheel 2", "Wheel 3", "Wheel 4", "Wheel 5", "Wheel 6", "IR 1" ],
        },
    }

    const driverSignals = getDriverSignals()

    const inNode = {
        id: 'sim-input-node',
        type: 'simInput',
        position: { x: 400, y: 0 },
        data: {
            onEdit: () => setConfigState("simIn"),
            input: driverSignals.map(x => `${x.constructor.name} ${x.info?.name && "(" + x.info!.name + ")"}`),
        },
    }

    return [ioNode, outNode, inNode]
}

function RobotIOComponent({ setConfigState }: ConfigComponentProps) {
    const canDevices = useMemo(() => {
        return getCANDevices()
    }, [])

    return (
        <div className="flex flex-col w-full">
            <Label className="text-center" size={LabelSize.Medium}>Configure your Robot Input/Output</Label>
            <div className="grid grid-flow-col gap-4">
                <div>
                    <Label>CAN Devices</Label>
                    <ScrollView className="h-full px-2">
                        {canDevices.map(([p, _]) => (
                            <Checkbox
                                key={p}
                                label={`${p.toString()}`}
                                defaultState={false}
                                // onClick={checked => {
                                //     if (checked && !checkedDrivers.includes(driver)) {
                                //         setCheckedDrivers([...checkedDrivers, driver])
                                //     } else if (!checked && checkedDrivers.includes(driver)) {
                                //         setCheckedDrivers(checkedDrivers.filter(a => a != driver))
                                //     }
                                // }}
                            />
                        ))}
                    </ScrollView>
                </div>
                <div>
                    <Label>Output Signals</Label>
                    <ScrollView className="h-full px-2">
                        {/* {drivers.map((driver, idx) => (
                            <Checkbox
                                key={`${driver.constructor.name}-${idx}`}
                                label={`${driver.constructor.name} ${driver.info?.name && "(" + driver.info!.name + ")"}`}
                                defaultState={false}
                                // onClick={checked => {
                                //     if (checked && !checkedDrivers.includes(driver)) {
                                //         setCheckedDrivers([...checkedDrivers, driver])
                                //     } else if (!checked && checkedDrivers.includes(driver)) {
                                //         setCheckedDrivers(checkedDrivers.filter(a => a != driver))
                                //     }
                                // }}
                            />
                        ))} */}
                    </ScrollView>
                </div>
            </div>
        </div>
    )
}

function WiringComponent({ setConfigState }: ConfigComponentProps) {
    const [nodes, setNodes, onNodesChange] = useNodesState(generateNodes(setConfigState));
    const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);

    const onEdgeDoubleClick = useCallback((_: React.MouseEvent, edge: FlowEdge) => {
        setEdges(edges.filter(x => x.id != edge.id))
    }, [edges, setEdges])
    
    const onConnect = useCallback(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (params: any) => setEdges((eds) => addEdge(params, eds)),
        [setEdges],
    );

    return (
        <ReactFlow
            colorMode="dark"
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onEdgeDoubleClick={onEdgeDoubleClick}
            nodeTypes={nodeTypes}
            fitView
        >
            <Controls />
        </ReactFlow>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {
    const [configState, setConfigState] = useState<ConfigState>("wiring")

    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
            full
        >
            <div className="flex grow">
                {configState === "wiring" ? <WiringComponent setConfigState={setConfigState} /> : <></>}
                {configState === "robotIO" ? <RobotIOComponent setConfigState={setConfigState} /> : <></>}
            </div>
        </Panel>
    )
}

export default WiringPanel
