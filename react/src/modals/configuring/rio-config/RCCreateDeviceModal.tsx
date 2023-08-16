import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "../../../components/Modal"
import { useModalControlContext } from "../../../ModalContext"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../../../components/Dropdown"

const RCCreateDeviceModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext();
    const [type, setType] = useState<string>("");

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Next"
            onAccept={() => {
                switch (type) {
                    case "PWM":
                        openModal("config-pwm");
                        break;
                    case "Encoder":
                        openModal("config-encoder");
                        break;
                    default:
                        break;
                }
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Dropdown
                label={"Type"}
                options={["PWM", "Encoder"]}
                onSelect={(selected) => { setType(selected) }}
            />
        </Modal>
    )
}

export default RCCreateDeviceModal 
