import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { colorNameToVar } from "@/ui/ThemeContext"
import { useCallback, useEffect, useMemo, useReducer, useRef } from "react"
import { DOMUnit, DOMUnitExpression } from "@/util/Units"

const EDGE_FLAT = new DOMUnit(0.1, "h")
const EDGE_CURVE = new DOMUnit(0.3, "h")
const NODE_SIZE = 10
const NODE_LABEL_Y_OFFSET = NODE_SIZE + 5
const JUNCTION_SIZE = NODE_SIZE + 2

type NodeDirection = "in" | "out"

class Node {
    public x: DOMUnit | DOMUnitExpression
    public y: DOMUnit | DOMUnitExpression
    public label?: string
    public direction: NodeDirection

    public constructor(direction: NodeDirection, x?: DOMUnit | DOMUnitExpression, y?: DOMUnit | DOMUnitExpression, label?: string) {
        this.direction = direction
        this.x = x ?? new DOMUnit(0, "px")
        this.y = y ?? new DOMUnit(0, "px")
        this.label = label
    }
}

class Junction {
    private _x: DOMUnit | DOMUnitExpression
    private _y: DOMUnit | DOMUnitExpression

    private _nodeIn: Node
    private _nodeOut: Node

    public get x() { return this._x }
    public get y() { return this._y }

    public get nodeIn() { return this._nodeIn }
    public get nodeOut() { return this._nodeOut }

    public set x(val: DOMUnit | DOMUnitExpression) {
        this._x = val
        this.updateNodes()
    }
    public set y(val: DOMUnit | DOMUnitExpression) {
        this._y = val
        this.updateNodes()
    }

    public constructor(x?: DOMUnit | DOMUnitExpression, y?: DOMUnit | DOMUnitExpression) {
        this._x = x ?? new DOMUnit(0, "px")
        this._y = y ?? new DOMUnit(0, "px")
        this._nodeIn = new Node("in")
        this._nodeOut = new Node("out")
        this.updateNodes()
    }

    private updateNodes() {
        this._nodeIn.x = this._x - JUNCTION_SIZE
        this._nodeIn.y = this._y
        this._nodeOut.x = this._x + JUNCTION_SIZE
        this._nodeOut.y = this._y

        // console.debug(`${this._nodeIn.x.toFixed(1)} ${this._nodeIn.y.toFixed(1)}`)
    }
}

class Edge {
    private _nodeA: Node | Junction
    private _nodeB: Node | Junction

    public get nodeA() { return this._nodeA }
    public get nodeB() { return this._nodeB }

    public constructor(nodeA: Node | Junction, nodeB: Node | Junction) {
        if (nodeA instanceof Junction && nodeB instanceof Junction) {
            throw new Error("Both nodes cannot be junctions. Can't tell which is in, which is out")
        }
        this._nodeA = nodeA
        this._nodeB = nodeB
    }
}

function EdgeComp({ edge }: { edge: Edge }) {
    const spanX = edge.nodeB.x - edge.nodeA.x
    const spanY = edge.nodeB.y - edge.nodeA.y

    const cmds = `
        M ${edge.nodeA.x} ${edge.nodeA.y}
        h ${spanX * EDGE_FLAT}
        c ${spanX * EDGE_CURVE},0 ${spanX * (1 - (2 * EDGE_FLAT) - EDGE_CURVE)},${spanY} ${spanX * (1 - 2 * EDGE_FLAT)},${spanY}
        h ${spanX * EDGE_FLAT}
    `

    return (
        <svg>
            <path strokeWidth={"0.125rem"} stroke="white" fill="none" d={cmds} />
        </svg>
    )
}

function JunctionComp({ junct }: { junct: Junction }) {
    return (
        <>
            <path strokeWidth={"0.125rem"} stroke="white" fill="none" d={`M ${junct.nodeIn.x} ${junct.nodeIn.y} L ${junct.nodeOut.x} ${junct.nodeOut.y}`} />
            <NodeComp node={junct.nodeIn} />
            <NodeComp node={junct.nodeOut} />
        </>
    )
}

function NodeComp({ node }: { node: Node }) {

    const { x, y, label, direction } = node;

    const onClick = useCallback((e: React.MouseEvent<SVGCircleElement>) => {
        console.debug(`Clicked node: ${node.label ? node.label : `${node.x.toFixed(1)}, ${node.y.toFixed(1)}`}`)
    }, [node])

    const pathCmds = direction === "in" ? `
        M ${x - (Math.SQRT2 * NODE_SIZE)} ${y}
        l ${(NODE_SIZE / Math.SQRT2)} ${-(NODE_SIZE / Math.SQRT2)}
        a ${NODE_SIZE} ${NODE_SIZE} 270 1 1 0 ${2 * (NODE_SIZE / Math.SQRT2)}
        Z
    ` : `
        M ${x + (Math.SQRT2 * NODE_SIZE)} ${y}
        l ${-(NODE_SIZE / Math.SQRT2)} ${-(NODE_SIZE / Math.SQRT2)}
        a ${NODE_SIZE} ${NODE_SIZE} 270 1 0 0 ${2 * (NODE_SIZE / Math.SQRT2)}
        Z
    `

    return (
        <svg>
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
                x={x} y={y - NODE_LABEL_Y_OFFSET}>
                    {label}
                </text>
            ) : (<></>)}
        </svg>
    )
}

class Graph {
    private _nodes: Node[]
    private _juncts: Junction[]
    private _edges: Edge[]

    public get nodes() { return this._nodes }
    public get juncts() { return this._juncts }
    public get edges() { return this._edges }

    public constructor() {
        this._nodes = []
        this._juncts = []
        this._edges = []
    }
}

function GraphComp({ graph }: { graph: Graph }) {

    const svgRef = useRef<SVGSVGElement | null>(null)

    const [renderHook, forceRenderer] = useReducer(x => !x, false)
    const nodeA = useMemo<Node>(() => new Node("out"), [])
    const nodeB = useMemo<Node>(() => new Node("in"), [])
    const junct = useMemo<Junction>(() => new Junction(), [])
    const edgeA = useMemo<Edge>(() => new Edge(nodeA, junct), [nodeA, junct])
    const edgeB = useMemo<Edge>(() => new Edge(junct, nodeB), [nodeB, junct])

    useEffect(() => {
        const anim = () => {
            if (svgRef.current != null) {
                const width = svgRef.current.clientWidth
                const height = svgRef.current.clientHeight

                nodeA.x = width * 0.1
                nodeA.y = height * 0.4
                nodeB.x = width * 0.9
                nodeB.y = height * 0.6
                junct.x = width * 0.5
                junct.y = height * 0.5

                forceRenderer()
            }
            
            cancelAnimationFrame(handle)
            handle = requestAnimationFrame(anim)
        }

        let handle = requestAnimationFrame(anim)

        return () => {
            cancelAnimationFrame(handle)
        }
    }, [nodeA, nodeB, junct])

    const comps = useMemo(() => {
        return (
            <>
                {graph.edges.map(x => <EdgeComp edge={x} />)}
                {graph.nodes.map(x => <NodeComp node={x} />)}
                {graph.juncts.map(x => <JunctionComp junct={x} />)}
            </>
        )
    }, [renderHook])

    return (
        <svg ref={svgRef} className="absolute top-0 left-0 w-full h-full">
            {comps}
        </svg>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {
    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
        >
            <div className="w-[70vw] h-[70vh]">
                <Label size={LabelSize.Large}>Hunter's Messy Brain</Label>
                <GraphComp />
            </div>
        </Panel>
    )
}

export default WiringPanel
