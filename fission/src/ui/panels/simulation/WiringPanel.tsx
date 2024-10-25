import React from "react"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import Label, { LabelSize } from "@/ui/components/Label"
import * as d3 from "d3"

const WiringPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const data: number[] = [
        0, 3, 2, 6, 5, 4, 7, 3, 4, 4, 5, 2
    ];
    const x = d3.scaleLinear([0, data.length - 1], ["0%", "100%"]);
    const y = d3.scaleLinear([0, 8], ["100%", "0%"]);
    const line = d3.line((_, i) => x(i), y);
    
    return (
        <Panel
            name="Wiring Panel"
            icon={SynthesisIcons.SteeringWheel}
            panelId={panelId}
            openLocation={"center"}
        >
            <div className="w-[70vw] h-[70vh]">
                <Label size={LabelSize.Large}>Hunter's Messy Brain</Label>
                <svg className="absolute top-0 left-0 w-full h-full">
                    <path fill="none" stroke="currentColor" strokeWidth="1.5" d={line(data) ?? undefined} />
                    <g fill="white" stroke="currentColor" strokeWidth="1.5">
                        {data.map((d, i) => (<circle key={i} cx={x(i)} cy={y(d)} r="2.5" />))}
                    </g>
                </svg>
            </div>
        </Panel>
    )
}

export default WiringPanel
