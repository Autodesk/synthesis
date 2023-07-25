import React from "react"
import Modal, { ModalPropsImpl } from "../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../components/Dropdown"

const RobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name={"Robot Selection"} icon={<FaPlus />} modalId={modalId}>
            <Dropdown options={["Dozer_v9.mira", "Team_2471_2018_v7.mira"]} />
        </Modal>
    )
}

export default RobotsModal
