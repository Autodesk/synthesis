import React from "react"
import Modal, { ModalPropsImpl } from "../components/Modal"
import Dropdown from "../components/Dropdown"
import { FaMagnifyingGlass } from "react-icons/fa6"

const ViewModal: React.FC<ModalPropsImpl> = ({ modalId }) => (
    <Modal name={"Camera View"} icon={<FaMagnifyingGlass />} modalId={modalId}>
        <Dropdown options={["Orbit", "Freecam", "Overview"]} />
    </Modal>
)

export default ViewModal
