import React from "react"
import Modal from "../components/Modal"
import Dropdown from "../components/Dropdown"
import { FaMagnifyingGlass } from "react-icons/fa6"

const ViewModal: React.FC = () => (
    <Modal name={"Camera View"} icon={<FaMagnifyingGlass />}>
        <Dropdown options={["Orbit", "Freecam", "Overview"]} />
    </Modal>
)

export default ViewModal
