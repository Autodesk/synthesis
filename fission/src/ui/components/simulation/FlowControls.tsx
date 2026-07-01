import { Panel as FlowPanel, useReactFlow } from "@xyflow/react"
import { FaPlus } from "react-icons/fa6"
import { MdFitScreen, MdZoomInMap, MdZoomOutMap } from "react-icons/md"
import type { FlowControlsProps } from "@/systems/simulation/SimConfigShared"
import { Button } from "../StyledComponents"

function FlowControls({ onCreateJunction }: FlowControlsProps) {
    const { zoomIn, zoomOut, fitView } = useReactFlow()

    return (
        <FlowPanel position="bottom-left" className="flex flex-col-reverse gap-1">
            <Button variant="outlined" onClick={() => fitView()}>
                <MdFitScreen className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => zoomOut()}>
                <MdZoomOutMap className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => zoomIn()}>
                <MdZoomInMap className="w-full h-full" />
            </Button>
            <Button variant="outlined" onClick={() => onCreateJunction?.()}>
                <FaPlus className="w-full h-full" />
            </Button>
        </FlowPanel>
    )
}

export default FlowControls
