import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import { colorNameToVar } from "@/ui/ThemeContext"
import { useCallback, useEffect, useRef, useState } from "react"

const EDGE_FLAT = 0.1
const EDGE_CURVE = 0.3
const NODE_SIZE = 10
const NODE_LABEL_Y_OFFSET = NODE_SIZE + 5

type NodeProps = {
    x: number,
    y: number,
    label?: string
}

type EdgeProps = {
    nodeA: NodeProps,
    nodeB: NodeProps
}

function Edge({ nodeA, nodeB }: EdgeProps) {
    const spanX = nodeB.x - nodeA.x
    const spanY = nodeB.y - nodeA.y

    const cmds = `
        M ${nodeA.x} ${nodeA.y}
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

function Node({ x, y, label }: NodeProps) {

    const onClick = useCallback((e: React.MouseEvent<SVGCircleElement>) => {
        console.debug(`Clicked node: ${label ? label : `${x.toFixed(1)}, ${y.toFixed(1)}`}`)
    }, [label, x, y])

    return (
        <svg>
            <circle
                onClick={onClick}
                r={NODE_SIZE}
                cx={x} cy={y}
                fill={colorNameToVar("InteractiveBackground")}
                strokeWidth={"0.125rem"}
                stroke={colorNameToVar("InteractiveElementSolid")}
            />
            {label ? (<text
                className="Rrrrr"
                fontSize="1rem"
                textAnchor="middle"
                style={{ transform: "translate(0, 0px)" }}
                fill="white"
                x={x} y={y - NODE_LABEL_Y_OFFSET}>
                    {label}
                </text>
            ) : (<></>)}
        </svg>
    )
}

function WiringPanel({ panelId }: PanelPropsImpl) {

    const svgRef = useRef<SVGSVGElement | null>(null)

    const [nodeA, setNodeA] = useState<NodeProps>({x: 0, y: 0})
    const [nodeB, setNodeB] = useState<NodeProps>({x: 0, y: 0})
    
    useEffect(() => {
        const anim = () => {
            if (svgRef.current != null) {
                const width = svgRef.current.clientWidth
                const height = svgRef.current.clientHeight

                setNodeA({
                    x: width * 0.1, y: height * 0.4
                })
                setNodeB({
                    x: width * 0.9, y: height * 0.6
                })
            }
            
            cancelAnimationFrame(handle)
            handle = requestAnimationFrame(anim)
        }

        let handle = requestAnimationFrame(anim)

        return () => {
            cancelAnimationFrame(handle)
        }
    }, [nodeA, nodeB])

    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
        >
            <div className="w-[70vw] h-[70vh]">
                <Label size={LabelSize.Large}>Hunter's Messy Brain</Label>
                <svg ref={svgRef} className="absolute top-0 left-0 w-full h-full">
                    <Edge nodeA={nodeA} nodeB={nodeB} />
                    <Node x={nodeA.x} y={nodeA.y} label="Node A" />
                    <Node x={nodeB.x} y={nodeB.y} label="Node B" />
                </svg>
            </div>
        </Panel>
    )
}

export default WiringPanel
