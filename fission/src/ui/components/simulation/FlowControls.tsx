import { Panel as FlowPanel, useReactFlow } from "@xyflow/react"
import { cloneElement } from "react"
import type { FlowControlsProps } from "@/systems/simulation/SimConfigShared"
import { Button, SynthesisIcons } from "../StyledComponents"

function FlowControls({ onCreateJunction }: FlowControlsProps) {
    const { zoomIn, zoomOut, fitView } = useReactFlow()

    return (
        <FlowPanel position="bottom-left" className="flex flex-col-reverse gap-1">
            <Button variant="outlined" onClick={() => fitView()}>
                {cloneElement(SynthesisIcons.FIT_SCREEN, { className: "w-full h-full" })}
            </Button>
            <Button variant="outlined" onClick={() => zoomOut()}>
                {cloneElement(SynthesisIcons.ZOOM_OUT, { className: "w-full h-full" })}
            </Button>
            <Button variant="outlined" onClick={() => zoomIn()}>
                {cloneElement(SynthesisIcons.ZOOM_IN, { className: "w-full h-full" })}
            </Button>
            <Button variant="outlined" onClick={() => onCreateJunction?.()}>
                {cloneElement(SynthesisIcons.ADD, { className: "w-full h-full" })}
            </Button>
        </FlowPanel>
    )
}

export default FlowControls
