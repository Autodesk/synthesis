import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { colorNameToVar } from "@/ui/ThemeContext"
import { useCallback, useEffect, useMemo, useReducer, useRef, useState } from "react"
import { DOMUnitExpression } from "@/util/Units"

const EDGE_CURVE = 0.3
const NODE_SIZE = DOMUnitExpression.fromUnit(10)
const EDGE_FLAT = NODE_SIZE.mul(DOMUnitExpression.fromUnit(Math.SQRT2)).add(DOMUnitExpression.fromUnit(10))
const NODE_LABEL_Y_OFFSET = NODE_SIZE.add(DOMUnitExpression.fromUnit(5))
const JUNCTION_SIZE = NODE_SIZE.add(DOMUnitExpression.fromUnit(2))

type NodeDirection = "in" | "out"

let nextJunctIndex = 0
let nextEdgeIndex = 0

class Node {
    public x: DOMUnitExpression
    public y: DOMUnitExpression
    public label?: string
    public direction: NodeDirection
    private _id: string

    public get id() { return this._id }

    public constructor(id: string, direction: NodeDirection, label?: string, x?: DOMUnitExpression, y?: DOMUnitExpression) {
        this.direction = direction
        this._id = id
        this.x = x ?? DOMUnitExpression.fromUnit(0)
        this.y = y ?? DOMUnitExpression.fromUnit(0)
        this.label = label
    }
}

class Junction {
    private _x: DOMUnitExpression
    private _y: DOMUnitExpression

    private _id: number

    public graph: Graph

    private _nodeIn: Node
    private _nodeOut: Node

    public get x() { return this._x }
    public get y() { return this._y }

    public get id() { return this._id }

    public get nodeIn() { return this._nodeIn }
    public get nodeOut() { return this._nodeOut }

    public set x(val: DOMUnitExpression) {
        this._x = val
        this.updateNodes()
    }
    public set y(val: DOMUnitExpression) {
        this._y = val
        this.updateNodes()
    }

    public constructor(graph: Graph, x?: DOMUnitExpression, y?: DOMUnitExpression) {
        this.graph = graph
        this._id = nextJunctIndex++
        this._x = x ?? DOMUnitExpression.fromUnit(0)
        this._y = y ?? DOMUnitExpression.fromUnit(0)
        this._nodeIn = graph.createNode(`junct_in_${this._id}`, "in")
        this._nodeOut = graph.createNode(`junct_out_${this._id}`, "out")
        this.updateNodes()
    }

    private updateNodes() {
        this._nodeIn.x = this._x.sub(JUNCTION_SIZE)
        this._nodeIn.y = this._y
        this._nodeOut.x = this._x.add(JUNCTION_SIZE)
        this._nodeOut.y = this._y

        // console.debug(`${this._nodeIn.x.toFixed(1)} ${this._nodeIn.y.toFixed(1)}`)
    }
}

function EdgeComp({ from, to, graph, element }: { from: string, to: string, graph: Graph, element: Element }) {
    const [nodeFrom, nodeTo] = useMemo(() => [ graph.nodes.get(from)!, graph.nodes.get(to)!], [from, graph, to])

    const [fromX, fromY] = [nodeFrom.x.evaluate(element), nodeFrom.y.evaluate(element)]
    const spanX = nodeTo.x.evaluate(element) - fromX
    const spanY = nodeTo.y.evaluate(element) - fromY

    let flatSign = 1

    if (spanX < 0) {
        flatSign = -1
    }

    const cmds = `
        M ${fromX} ${fromY}
        h ${flatSign * EDGE_FLAT.evaluate(element)}

    `

    // const cmds = `
    //     M ${fromX} ${fromY}
    //     h ${flatSign * EDGE_FLAT.evaluate(element)}
    //     c ${flatSign * spanX * EDGE_CURVE},0 ${spanX * (1 - (2 * flatSign * EDGE_FLAT) - flatSign * EDGE_CURVE)},${spanY} ${spanX * (1 - 2 * flatSign * EDGE_FLAT)},${spanY}
    //     h ${flatSign * spanX * EDGE_FLAT}
    // `

    return (
        <svg key={`${from}-${to}`}>
            <path strokeWidth={"0.125rem"} stroke="white" fill="none" d={cmds} />
        </svg>
    )
}

function JunctionComp({ junct, element }: { junct: Junction, element: Element }) {
    return (
        <>
            <path key={junct.id}
                strokeWidth={"0.125rem"}
                stroke="white"
                fill="none"
                d={`
                    M ${junct.nodeIn.x.evaluate(element)} ${junct.nodeIn.y.evaluate(element)}
                    L ${junct.nodeOut.x.evaluate(element)} ${junct.nodeOut.y.evaluate(element)}
                `}
            />
            {/* <NodeComp node={junct.nodeIn} element={element} />
            <NodeComp node={junct.nodeOut} element={element} /> */}
        </>
    )
}

function NodeComp({ node, element }: { node: Node, element: Element }) {

    const { label, direction } = node;
    const x = node.x.evaluate(element)
    const y = node.y.evaluate(element)

    const nodeSize = NODE_SIZE.evaluate(element)
    const nodeLabelOffset = NODE_LABEL_Y_OFFSET.evaluate(element)

    const onClick = useCallback((e: React.MouseEvent<SVGCircleElement>) => {
        console.debug(`Clicked node: ${node.label ? node.label : `${x.toFixed(1)}, ${y.toFixed(1)}`}`)
    }, [node.label, x, y])

    const pathCmds = direction === "in" ? `
        M ${x - (Math.SQRT2 * nodeSize)} ${y}
        l ${(nodeSize / Math.SQRT2)} ${-(nodeSize / Math.SQRT2)}
        a ${nodeSize} ${nodeSize} 270 1 1 0 ${2 * (nodeSize / Math.SQRT2)}
        Z
    ` : `
        M ${x + (Math.SQRT2 * nodeSize)} ${y}
        l ${-(nodeSize / Math.SQRT2)} ${-(nodeSize / Math.SQRT2)}
        a ${nodeSize} ${nodeSize} 270 1 0 0 ${2 * (nodeSize / Math.SQRT2)}
        Z
    `

    return (
        <svg key={node.id}>
            <path
                d={pathCmds}
                onClick={onClick}
                fill={colorNameToVar("InteractiveBackground")}
                strokeWidth={"0.125rem"}
                stroke={colorNameToVar("InteractiveElementSolid")}
            />
            {label ? (<text
                style={{ userSelect: "none" }}
                fontSize="1rem"
                textAnchor="middle"
                fill="white"
                x={x} y={y - nodeLabelOffset}>
                    {label}
                </text>
            ) : (<></>)}
        </svg>
    )
}

class Graph {
    private _nodes: Map<string, Node>
    private _juncts: Map<number, Junction>
    private _edges: Map<number, { from: string, to: string }>
    private _adjacency: Map<string, Set<number>>

    public get nodes() { return this._nodes }
    public get juncts() { return this._juncts }
    public get edges() { return this._edges }

    public constructor() {
        this._nodes = new Map<string, Node>()
        this._juncts = new Map<number, Junction>()
        this._edges = new Map<number, { from: string, to: string }>
        this._adjacency = new Map<string, Set<number>>()
    }

    public createNode(id: string, direction: NodeDirection, label?: string, x?: DOMUnitExpression, y?: DOMUnitExpression): Node {
        const node = new Node(id, direction, label, x, y)
        this._nodes.set(id, node)
        this._adjacency.set(id, new Set<number>())
        return node
    }

    public createJunction(x?: DOMUnitExpression, y?: DOMUnitExpression): Junction {
        const junct = new Junction(this, x, y)
        this._juncts.set(junct.id, junct)
        return junct
    }

    public createEdge(nodeA: string, nodeB: string) {
        const edgeId = nextEdgeIndex++
        const a = this._nodes.get(nodeA)
        const b = this._nodes.get(nodeB)
        if (!(a && b)) {
            throw new Error("Nodes not found")
        }
        if (a.direction === b.direction) {
            throw new Error("Incompatible directions")
        }
        if (a.direction === "out") {
            this._edges.set(edgeId, { from: nodeA, to: nodeB })
        } else {
            this._edges.set(edgeId, { from: nodeB, to: nodeA })
        }
        this._adjacency.get(nodeA)!.add(edgeId)
        this._adjacency.get(nodeB)!.add(edgeId)
    }

    public removeEdge(edgeId: number): boolean {
        const edge = this._edges.get(edgeId)
        if (!edge) {
            return false
        }
        return (this._adjacency.get(edge.from)?.delete(edgeId) ?? false)
            && (this._adjacency.get(edge.to)?.delete(edgeId) ?? false)
            && this._edges.delete(edgeId)
    }

    public disconnectNode(nodeA: string): boolean {
        const adjEdges = new Set<number>(this._adjacency.get(nodeA))
        if (!adjEdges) {
            return false
        }
        return [...adjEdges].reduce<boolean>((prev, curr) => prev && this.removeEdge(curr), true)
    }

    public removeNode(nodeA: string): boolean {
        if (!this._nodes.has(nodeA)) {
            return false
        }
        return this.disconnectNode(nodeA) && this._nodes.delete(nodeA)
    }
}

function GraphComp({ graph }: { graph: Graph }) {

    const svgRef = useRef<SVGSVGElement | null>(null)

    const [renderHook, forceRenderer] = useReducer(x => !x, false)
    const [test, setTest] = useState<boolean>(false)

    useEffect(() => {
        const anim = () => {
            forceRenderer()
            
            cancelAnimationFrame(handle)
            handle = requestAnimationFrame(anim)
        }

        let handle = requestAnimationFrame(anim)

        return () => {
            cancelAnimationFrame(handle)
        }
    }, [])

    if (!test && svgRef.current != null) {
        setTest(true)
        const out = EDGE_FLAT.evaluate(svgRef.current, true)
        console.debug(`Final Evaluation: ${out} px`)
    }

    const comps = useMemo(() => {
        return svgRef.current != null ? (
            <>
                {[...graph.edges.values()].map(x => <EdgeComp from={x.from} to={x.to} graph={graph} element={svgRef.current!} />)}
                {[...graph.juncts.values()].map(x => <JunctionComp junct={x} element={svgRef.current!} />)}
                {[...graph.nodes.values()].map(x => <NodeComp node={x} element={svgRef.current!} />)}
            </>
        ) : (<></>)
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [renderHook, graph])

    return (
        <svg ref={svgRef} className="absolute top-0 left-0 w-full h-full">
            {comps}
        </svg>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {

    const graph = useMemo(() => {
        const graph = new Graph()
        graph.createNode("A", "out", "Node A",
            DOMUnitExpression.fromUnit(0.1, "w"),
            DOMUnitExpression.fromUnit(0.5, "h")
        )
        graph.createNode("B", "in", "Node B",
            DOMUnitExpression.fromUnit(0.9, "w"),
            DOMUnitExpression.fromUnit(0.6, "h")
        )
        graph.createNode("X", "out", "Node X",
            DOMUnitExpression.fromUnit(0.5, "w"),
            DOMUnitExpression.fromUnit(0.7, "h")
        )
        graph.createNode("Y", "in", "Node Y",
            DOMUnitExpression.fromUnit(0.5, "w"),
            DOMUnitExpression.fromUnit(0.9, "h")
        )

        const junct = graph.createJunction(
            DOMUnitExpression.fromUnit(0.5, "w"),
            DOMUnitExpression.fromUnit(0.4, "h")
        )

        graph.createEdge("A", "B")
        graph.createEdge("A", junct.nodeIn.id)
        graph.createEdge(junct.nodeOut.id, "B")
        graph.createEdge("X", "Y")

        return graph
    }, [])

    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
        >
            <div className="w-[70vw] h-[70vh]">
                <Label size={LabelSize.Large}>Hunter's Messy Brain</Label>
                <GraphComp graph={graph} />
            </div>
        </Panel>
    )
}

export default WiringPanel
