import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaCar } from "react-icons/fa6"
import Dropdown from "../../components/Dropdown"

const DrivetrainModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name="Change Drivetrain" icon={<FaCar />} modalId={modalId}>
            <Dropdown
                label="Type"
                options={["None", "Tank", "Arcade", "Swerve"]}
                onSelect={() => { }}
            />
        </Modal>
    )
}

export default DrivetrainModal
