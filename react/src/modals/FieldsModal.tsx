import React from "react"
import Modal from "../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../components/Dropdown"

const FieldsModal: React.FC = () => {
    return (
        <Modal name={"Field Selection"} icon={<FaPlus />}>
            <Dropdown
                options={[
                    "FRC Field 2018_v12.mira",
                    "FRC Field 2019_v10.mira",
                    "FRC Field 2020-21_v4.mira",
                    "FRC Field 2022_v4.mira",
                    "FRC_Field_2023_v7.mira",
                ]}
            />
        </Modal>
    )
}

export default FieldsModal
