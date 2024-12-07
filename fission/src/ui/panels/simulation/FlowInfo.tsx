import { Panel as FlowPanel } from "@xyflow/react"
import { CustomTooltip } from "@/ui/components/StyledComponents";

function FlowInfo() {
    
    return (
        <FlowPanel
            position="top-center"
            className="flex flex-col-reverse gap-1"
        >
            {CustomTooltip("Click and drag to make connection to your robot's IO. Use the controls in the bottom left to zoom in/out, fit to the nodes in the graph, and add junction nodes for an easier experience connecting many motors to many joints. Holding ALT while dropping an edge over nothing will break out the edge into it's separate components, if it has multiple.")}
        </FlowPanel>
    )
}

export default FlowInfo