import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { useCallback } from "react"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, addEdge, Controls } from "@xyflow/react"

import '@xyflow/react/dist/style.css';
import RobotIONode from "./RobotIONode"
import SimInputNode from "./SimInputNode"
import SimOutputNode from "./SimOutputNode"

const initialNodes: FlowNode[] = [
    // { id: "1", position: { x: 0, y: 0 }, data: { label: "Test 1" }, type: "input" },
    // { id: "2", position: { x: 0, y: 100 }, data: { label: "Test 2" }, type: "output" },
    {
        id: 'robot-io-node',
        type: 'robotIO',
        position: { x: 0, y: 0 },
        data: {
            input: [ "Encoder 1", "Encoder 2" ],
            output: [ "CAN 0", "CAN 1", "CAN 2", "CAN 3" ],
        },
    }, {
        id: 'sim-input-node',
        type: 'simInput',
        position: { x: 400, y: 0 },
        data: {
            input: [ "Wheel 1", "Wheel 2", "Wheel 3", "Wheel 4", "Wheel 5", "Wheel 6" ],
        },
    }, {
        id: 'sim-output-node',
        type: 'simOutput',
        position: { x: -400, y: 0 },
        data: {
            output: [ "Wheel 1", "Wheel 2", "Wheel 3", "Wheel 4", "Wheel 5", "Wheel 6", "IR 1" ],
        },
    }
]

const initialEdges: FlowEdge[] = []
// const initialEdges: FlowEdge[] = [
//     { id: "e1-2", source: "1", target: "2" }
// ]

const nodeTypes = { robotIO: RobotIONode, simInput: SimInputNode, simOutput: SimOutputNode }

function WiringPanel({ panelId }: PanelPropsImpl) {

    const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
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
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
            full
        >
            <div className="bg-gray-500 flex grow">
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
            </div>
            {/* <GraphComp graph={graph} /> */}
            <div className="flex flex-row justify-between">
                <Label className="text-interactive-element-solid font-medium" size={LabelSize.Large}>Stimuli</Label>
                <Label className="text-interactive-element-solid font-medium" size={LabelSize.Large}>Code IO</Label>
                <Label className="text-interactive-element-solid font-medium" size={LabelSize.Large}>Drivers</Label>
            </div>
        </Panel>
    )
}

export default WiringPanel
