import React from "react"
import Modal from "../components/Modal"
import { FaCar } from "react-icons/fa6"
import Dropdown from "../components/Dropdown"

const DrivetrainModal: React.FC = () => {
    return (
        <Modal name="Change Drivetrain" icon={<FaCar />}>
            <Dropdown
                label="Type"
                options={["None", "Tank", "Arcade", "Swerve"]}
            />
        </Modal>
    )
}

export default DrivetrainModal
