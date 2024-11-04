import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { useCallback } from "react"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, addEdge, Controls } from "@xyflow/react"

import '@xyflow/react/dist/style.css';
import TextUpdaterNode from "./TextUpdaterNode"

const initialNodes: FlowNode[] = [
    // { id: "1", position: { x: 0, y: 0 }, data: { label: "Test 1" }, type: "input" },
    // { id: "2", position: { x: 0, y: 100 }, data: { label: "Test 2" }, type: "output" },
    {
        id: 'node-1',
        type: 'textUpdater',
        position: { x: 0, y: 0 },
        data: { value: 123 },
    }
]

const initialEdges: FlowEdge[] = []
// const initialEdges: FlowEdge[] = [
//     { id: "e1-2", source: "1", target: "2" }
// ]

const nodeTypes = { textUpdater: TextUpdaterNode }

function WiringPanel({ panelId }: PanelPropsImpl) {
    

    const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
    const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);
    
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
