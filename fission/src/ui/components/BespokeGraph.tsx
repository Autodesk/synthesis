/**
 * This was originally written before using ReactFlow for the node editor.
 * It's mostly working (missing some interaction aspects) and does some
 * interesting work with SVGs that I think would be really useful for a
 * more advance 2D overlay for directing the users attention to key points
 * in the 3D scene.
 */

import { useCallback, useEffect, useMemo, useReducer, useRef } from "react"
import { DOMUnitExpression } from "@/util/Units"

const DEBUG_EDGE_CONTROL_LINES = false

const EDGE_FLAT = 0.1
const EDGE_CURVE = 0.3
const EDGE_CURVE_MIN = DOMUnitExpression.fromUnit(2, "rem")
const NODE_SIZE = DOMUnitExpression.fromUnit(0.5, "rem")
const EDGE_FLAT_MAX = NODE_SIZE.mul(DOMUnitExpression.fromUnit(Math.SQRT2)).add(DOMUnitExpression.fromUnit(1, "rem"))
const NODE_LABEL_Y_OFFSET = NODE_SIZE.add(DOMUnitExpression.fromUnit(0.325, "rem"))
const JUNCTION_SIZE = NODE_SIZE.add(DOMUnitExpression.fromUnit(0.125, "rem"))
const MODULE_HEIGHT = DOMUnitExpression.fromUnit(1, "h").sub(DOMUnitExpression.fromUnit(0.125, "rem"))
const MODULE_HEIGHT_HALF = MODULE_HEIGHT.div(DOMUnitExpression.fromUnit(2))
const MODULE_SPAN_Y = MODULE_HEIGHT.sub(DOMUnitExpression.fromUnit(4, "rem"))
const MODULE_SPAN_Y_HALF = MODULE_SPAN_Y.div(DOMUnitExpression.fromUnit(2, "px"))
const MODULE_NODE_OFFSET = DOMUnitExpression.fromUnit(6, "rem")
const MODULE_CORNER_RADIUS = DOMUnitExpression.fromUnit(1, "rem")

type NodeDirection = "in" | "out"

let nextId = 0

class Node {
    public x: DOMUnitExpression
    public y: DOMUnitExpression
    public label?: string
    public direction: NodeDirection
    private _id: string

    public get id() {
        return this._id
    }

    public constructor(
        id: string,
        direction: NodeDirection,
        label?: string,
        x?: DOMUnitExpression,
        y?: DOMUnitExpression
    ) {
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

    public get x() {
        return this._x
    }
    public get y() {
        return this._y
    }

    public get id() {
        return this._id
    }

    public get nodeIn() {
        return this._nodeIn
    }
    public get nodeOut() {
        return this._nodeOut
    }

    public get nodeIdIn() {
        return this._nodeIn.id
    }
    public get nodeIdOut() {
        return this._nodeOut.id
    }

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
        this._id = nextId++
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

const EdgeComp: React.FC<{ from: string; to: string; graph: Graph; element: Element }> = ({
    from,
    to,
    graph,
    element,
}) => {
    const [nodeFrom, nodeTo] = useMemo(() => [graph.nodes.get(from)!, graph.nodes.get(to)!], [from, graph, to])

    const [fromX, fromY] = [nodeFrom.x.evaluate(element), nodeFrom.y.evaluate(element)]
    const [toX, toY] = [nodeTo.x.evaluate(element), nodeTo.y.evaluate(element)]
    const spanX = Math.abs(toX - fromX)

    const flatMax = EDGE_FLAT_MAX.evaluate(element)
    const flat = Math.min(spanX * EDGE_FLAT, flatMax)
    const curveMin = EDGE_CURVE_MIN.evaluate(element)
    const curve = Math.max(spanX * EDGE_CURVE, curveMin)

    const cmds = `
        M ${fromX} ${fromY}
        h ${flat}
        C ${fromX + flat + curve},${fromY} ${toX - flat - curve},${toY} ${toX - flat},${toY}
        L ${toX} ${toY}
    `

    const debugCmds = `
        M ${fromX + flat} ${fromY}
        L ${fromX + flat + curve},${fromY} ${toX - flat - curve},${toY} ${toX - flat},${toY}
    `

    // const cmds = `
    //     M ${fromX} ${fromY}
    //     h ${flatSign * EDGE_FLAT.evaluate(element)}
    //     c ${flatSign * spanX * EDGE_CURVE},0 ${spanX * (1 - (2 * flatSign * EDGE_FLAT) - flatSign * EDGE_CURVE)},${spanY} ${spanX * (1 - 2 * flatSign * EDGE_FLAT)},${spanY}
    //     h ${flatSign * spanX * EDGE_FLAT}
    // `

    return (
        <svg key={`${from}-${to}`}>
            <path strokeWidth={"0.125rem"} stroke="white" strokeOpacity="0.7" fill="none" d={cmds} />
            {DEBUG_EDGE_CONTROL_LINES ? (
                <path
                    strokeWidth={"0.125rem"}
                    strokeDasharray="10,4"
                    strokeOpacity="0.3"
                    stroke="white"
                    fill="none"
                    d={debugCmds}
                />
            ) : (
                <></>
            )}
        </svg>
    )
}

const JunctionComp: React.FC<{ junct: Junction; element: Element }> = ({ junct, element }) => {
    return (
        <>
            <path
                key={junct.id}
                strokeWidth={"0.125rem"}
                stroke="white"
                fill="none"
                d={`
                    M ${junct.nodeIn.x.evaluate(element)} ${junct.nodeIn.y.evaluate(element)}
                    L ${junct.nodeOut.x.evaluate(element)} ${junct.nodeOut.y.evaluate(element)}
                `}
            />
        </>
    )
}

const NodeComp: React.FC<{ node: Node; graph: Graph; element: Element }> = ({ node, graph, element }) => {
    const { label, direction } = node
    const x = node.x.evaluate(element)
    const y = node.y.evaluate(element)

    const nodeSize = NODE_SIZE.evaluate(element)
    const nodeLabelOffset = NODE_LABEL_Y_OFFSET.evaluate(element)

    const onClick = useCallback(
        (_: React.MouseEvent<SVGCircleElement>) => {
            console.debug(`Clicked node: ${node.label ? node.label : `${x.toFixed(1)}, ${y.toFixed(1)}`}`)
        },
        [node.label, x, y]
    )

    const pathCmds =
        direction === "in"
            ? `
        M ${x - Math.SQRT2 * nodeSize} ${y}
        l ${nodeSize / Math.SQRT2} ${-(nodeSize / Math.SQRT2)}
        a ${nodeSize} ${nodeSize} 270 1 1 0 ${2 * (nodeSize / Math.SQRT2)}
        Z
    `
            : `
        M ${x + Math.SQRT2 * nodeSize} ${y}
        l ${-(nodeSize / Math.SQRT2)} ${-(nodeSize / Math.SQRT2)}
        a ${nodeSize} ${nodeSize} 270 1 0 0 ${2 * (nodeSize / Math.SQRT2)}
        Z
    `

    return (
        <svg key={node.id}>
            <path
                d={pathCmds}
                onClick={onClick}
                // TODO: theme
                fill={"black"}
                strokeWidth={"0.125rem"}
                stroke={
                    // TODO: theme (change blue)
                    (graph.adjacency.get(node.id)?.size ?? 0) > 0 ? "blue" : "white"
                }
            />
            {label ? (
                <text
                    style={{ userSelect: "none" }}
                    fontSize="0.75rem"
                    textAnchor={node.direction === "out" ? "end" : "start"}
                    fill="white"
                    dominantBaseline="central"
                    x={x + (node.direction === "out" ? -1 : 1) * nodeLabelOffset}
                    y={y /*- nodeLabelOffset*/}
                >
                    {label}
                </text>
            ) : (
                <></>
            )}
        </svg>
    )
}

class Module {
    private _inNodes: string[]
    private _outNodes: string[]

    private _id: number

    public x: DOMUnitExpression
    public y: DOMUnitExpression

    public graph: Graph

    public get inNodes() {
        return this._inNodes
    }
    public get outNodes() {
        return this._outNodes
    }

    public get id() {
        return this._id
    }

    public constructor(
        graph: Graph,
        inNodes: { id: string; label: string }[],
        outNodes: { id: string; label: string }[],
        x?: DOMUnitExpression,
        y?: DOMUnitExpression
    ) {
        this.graph = graph
        this._id = nextId++
        this.x = x ?? DOMUnitExpression.fromUnit(0.5, "w")
        this.y = y ?? DOMUnitExpression.fromUnit(0.5, "h")
        this._inNodes = []
        this._outNodes = []

        inNodes.forEach((x, i) => {
            const y = MODULE_SPAN_Y.mul(DOMUnitExpression.fromUnit((i + 1) / (inNodes.length + 1), "px"))
            const node = graph.createNode(
                x.id,
                "in",
                x.label,
                this.x.sub(MODULE_NODE_OFFSET),
                this.y.sub(MODULE_SPAN_Y_HALF).add(y)
            )
            this._inNodes.push(node.id)
        })
        outNodes.forEach((x, i) => {
            const y = MODULE_SPAN_Y.mul(DOMUnitExpression.fromUnit((i + 1) / (outNodes.length + 1), "px"))
            const node = graph.createNode(
                x.id,
                "out",
                x.label,
                this.x.add(MODULE_NODE_OFFSET),
                this.y.sub(MODULE_SPAN_Y_HALF).add(y)
            )
            this._outNodes.push(node.id)
        })
    }
}

const ModuleComp: React.FC<{ module: Module; element: Element }> = ({ module, element }) => {
    const x = module.x.evaluate(element)
    const y = module.y.evaluate(element)

    const heightHalf = MODULE_HEIGHT_HALF.evaluate(element)
    const widthHalf = MODULE_NODE_OFFSET.evaluate(element)
    const cornerRad = MODULE_CORNER_RADIUS.evaluate(element)

    const cmds =
        module.inNodes.length != 0 && module.outNodes.length != 0
            ? `
        M ${x - widthHalf} ${y - heightHalf + cornerRad}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x - widthHalf + cornerRad} ${y - heightHalf}
        L ${x + widthHalf - cornerRad} ${y - heightHalf}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x + widthHalf} ${y - heightHalf + cornerRad}
        L ${x + widthHalf} ${y + heightHalf - cornerRad}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x + widthHalf - cornerRad} ${y + heightHalf}
        L ${x - widthHalf + cornerRad} ${y + heightHalf}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x - widthHalf} ${y + heightHalf - cornerRad}
        Z
    `
            : module.inNodes.length == 0 && module.outNodes.length != 0
              ? `
        M ${x} ${y - heightHalf}
        L ${x + widthHalf - cornerRad} ${y - heightHalf}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x + widthHalf} ${y - heightHalf + cornerRad}
        L ${x + widthHalf} ${y + heightHalf - cornerRad}
        A ${cornerRad} ${cornerRad} 90 0 1 ${x + widthHalf - cornerRad} ${y + heightHalf}
        L ${x} ${y + heightHalf}
    `
              : module.outNodes.length == 0 && module.inNodes.length != 0
                ? `
        M ${x} ${y - heightHalf}
        L ${x - widthHalf + cornerRad} ${y - heightHalf}
        A ${cornerRad} ${cornerRad} 90 0 0 ${x - widthHalf} ${y - heightHalf + cornerRad}
        L ${x - widthHalf} ${y + heightHalf - cornerRad}
        A ${cornerRad} ${cornerRad} 90 0 0 ${x - widthHalf + cornerRad} ${y + heightHalf}
        L ${x} ${y + heightHalf}
    `
                : ""

    return (
        <>
            <path
                key={module.id}
                strokeWidth={"0.125rem"}
                stroke="white"
                strokeOpacity={0.5}
                // strokeDasharray="20 10"
                fill="none"
                d={cmds}
            />
        </>
    )
}

export class Graph {
    private _nodes: Map<string, Node>
    private _juncts: Map<number, Junction>
    private _edges: Map<number, { from: string; to: string }>
    private _adjacency: Map<string, Set<number>>
    private _modules: Module[]

    public get nodes() {
        return this._nodes
    }
    public get juncts() {
        return this._juncts
    }
    public get edges() {
        return this._edges
    }
    public get modules() {
        return this._modules
    }
    public get adjacency() {
        return this._adjacency
    }

    public constructor() {
        this._nodes = new Map<string, Node>()
        this._juncts = new Map<number, Junction>()
        this._edges = new Map<number, { from: string; to: string }>()
        this._adjacency = new Map<string, Set<number>>()
        this._modules = []
    }

    public createNode(
        id: string,
        direction: NodeDirection,
        label?: string,
        x?: DOMUnitExpression,
        y?: DOMUnitExpression
    ): Node {
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
        const edgeId = nextId++
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

    public createModule(
        inNodes: { id: string; label: string }[],
        outNodes: { id: string; label: string }[],
        x?: DOMUnitExpression,
        y?: DOMUnitExpression
    ): Module {
        const module = new Module(this, inNodes, outNodes, x, y)
        this._modules.push(module)
        return module
    }

    public removeEdge(edgeId: number): boolean {
        const edge = this._edges.get(edgeId)
        if (!edge) {
            return false
        }
        return (
            (this._adjacency.get(edge.from)?.delete(edgeId) ?? false) &&
            (this._adjacency.get(edge.to)?.delete(edgeId) ?? false) &&
            this._edges.delete(edgeId)
        )
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

const GraphComp: React.FC<{ graph: Graph }> = ({ graph }) => {
    const svgRef = useRef<SVGSVGElement | null>(null)

    const [_renderHook, forceRenderer] = useReducer(x => !x, false)

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

    const comps = useMemo(() => {
        return svgRef.current != null ? (
            <>
                {graph.modules.map(x => (
                    <ModuleComp key={x.id} module={x} element={svgRef.current!} />
                ))}
                {[...graph.edges.values()].map(x => (
                    <EdgeComp
                        key={x.from + "" + x.to}
                        from={x.from}
                        to={x.to}
                        graph={graph}
                        element={svgRef.current!}
                    />
                ))}
                {[...graph.juncts.values()].map(x => (
                    <JunctionComp key={x.id} junct={x} element={svgRef.current!} />
                ))}
                {[...graph.nodes.values()].map(x => (
                    <NodeComp key={x.id} node={x} graph={graph} element={svgRef.current!} />
                ))}
            </>
        ) : (
            <></>
        )
    }, [graph])

    return (
        <svg ref={svgRef} className="flex grow w-full">
            {comps}
        </svg>
    )
}

export default GraphComp
