import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../../components/Dropdown"
import { useTooltipControlContext } from "@/TooltipContext"

const RobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext();

    return (
        <Modal
            name={"Robot Selection"}
            icon={<FaPlus />}
            modalId={modalId}
            onAccept={() => showTooltip("controls", 2_000, [{ control: "Scroll", description: "Rotate Robot" }, { control: "Shift", description: "Hold to Snap" }])}
        >
            <Dropdown
                options={["Dozer_v9.mira", "Team_2471_2018_v7.mira"]}
                onSelect={() => { }}
            />
        </Modal>
    )
}

export default RobotsModal
