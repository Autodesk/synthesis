import { Panel as FlowPanel, useReactFlow } from "@xyflow/react"
import type { PropsWithChildren } from "react"
import type { FlowControlsProps } from "@/systems/simulation/SimConfigShared"
import { MdFitScreen, MdZoomInMap, MdZoomOutMap } from "react-icons/md"
import { FaPlus } from "react-icons/fa6"
import { Button } from "@mui/material"

function FlowControlButton({ onClick, children }: PropsWithChildren<{ onClick?: () => void }>) {
    return <Button onClick={() => onClick?.()}>{children}</Button>
}

function FlowControls({ onCreateJunction }: FlowControlsProps) {
    const { zoomIn, zoomOut, fitView } = useReactFlow()

    return (
        <FlowPanel position="bottom-left" className="flex flex-col-reverse gap-1">
            <FlowControlButton onClick={fitView}>
                <MdFitScreen className="w-full h-full" />
            </FlowControlButton>
            <FlowControlButton onClick={zoomOut}>
                <MdZoomOutMap className="w-full h-full" />
            </FlowControlButton>
            <FlowControlButton onClick={zoomIn}>
                <MdZoomInMap className="w-full h-full" />
            </FlowControlButton>
            <FlowControlButton onClick={onCreateJunction}>
                <FaPlus className="w-full h-full" />
            </FlowControlButton>
        </FlowPanel>
    )
}

export default FlowControls
