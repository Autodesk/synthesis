import { Panel as FlowPanel, useReactFlow } from "@xyflow/react"
import { Button, SynthesisIcons } from "../StyledComponents"

export type FlowControlsProps = {
    onCreateJunction?: () => void
}

function FlowControls({ onCreateJunction }: FlowControlsProps) {
    const { zoomIn, zoomOut, fitView } = useReactFlow()

    return (
        <FlowPanel position="bottom-left" className="flex flex-col-reverse gap-1">
            <Button variant="outlined" onClick={() => fitView()}>
                <SynthesisIcons.FIT_SCREEN className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => zoomOut()}>
                <SynthesisIcons.ZOOM_OUT className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => zoomIn()}>
                <SynthesisIcons.ZOOM_IN className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => onCreateJunction?.()}>
                <SynthesisIcons.ADD className="w-full h-full" />
            </Button>
        </FlowPanel>
    )
}

export default FlowControls
