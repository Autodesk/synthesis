import { Panel as FlowPanel, useReactFlow } from "@xyflow/react"
import { PropsWithChildren } from "react"
import { FlowControlsProps } from "./SimConfigShared"
import { MdFitScreen, MdZoomInMap, MdZoomOutMap } from "react-icons/md"
import { FaPlus } from "react-icons/fa6"

function FlowControlButton({ onClick, children }: PropsWithChildren<{ onClick?: () => void }>) {
    return (
        <button
            className="border-[0.0625rem] border-interactive-element-solid bg-background p-1 w-8 h-8 flex flex-col justify-center items-center"
            onClick={() => onClick?.()}
        >
            {children}
        </button>
    )
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
