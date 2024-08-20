import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { TooltipControl, useTooltipControlContext } from "@/ui/TooltipContext"
import Dropdown from "../components/Dropdown"
import { SynthesisIcons } from "../components/StyledComponents"

type ViewType = "Orbit" | "Freecam" | "Overview" | "Driver Station"

const ViewModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext()
    const [view, setView] = useState<ViewType>("Orbit")

    const controls: { [key in ViewType]: TooltipControl[] } = {
        "Orbit": [
            { control: "LMB + Drag", description: "Orbit Camera" },
            { control: "Scroll", description: "Zoom Camera" },
        ],
        "Freecam": [
            { control: "RMB + Drag", description: "Rotate Camera" },
            { control: "RMB + WASD", description: "Move Camera" },
            { control: "Scroll", description: "Zoom Camera" },
        ],
        "Overview": [{ control: "None", description: "Cannot Move Camera" }],
        "Driver Station": [
            { control: "RMB + Drag", description: "Rotate Camera" },
            { control: "RMB + WASD", description: "Move Camera" },
            { control: "Scroll", description: "Zoom Camera" },
        ],
    }

    return (
        <Modal
            name={"Camera View"}
            icon={SynthesisIcons.MagnifyingGlass}
            modalId={modalId}
            onAccept={() => showTooltip("controls", controls[view])}
        >
            <Dropdown
                defaultValue={"Orbit"}
                options={["Orbit", "Freecam", "Overview", "Driver Station"] as ViewType[]}
                onSelect={val => {
                    setView(val as ViewType)
                }}
                label={"Camera View"}
            />
        </Modal>
    )
}

export default ViewModal
