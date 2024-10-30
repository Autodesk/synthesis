import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { useCallback, useEffect, useMemo, useReducer, useRef, useState } from "react"
import { DOMUnit, DOMUnitExpression } from "@/util/Units"
import { ReactFlow, Node as FlowNode, Edge as FlowEdge, useNodesState, useEdgesState, addEdge, Controls } from "@xyflow/react"

import '@xyflow/react/dist/style.css';

function 

function WiringPanel({ panelId }: PanelPropsImpl) {
    const initialNodes: FlowNode[] = useMemo<FlowNode[]>(() => [
        { id: "1", position: { x: 0, y: 0 }, data: { label: "Test 1" }, type: "input" },
        { id: "2", position: { x: 0, y: 100 }, data: { label: "Test 2" }, type: "output" },
    ], [])

    const initialEdges: FlowEdge[] = useMemo<FlowEdge[]>(() => [
        { id: "e1-2", source: "1", target: "2" }
    ], [])

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
