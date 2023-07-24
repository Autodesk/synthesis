import React from "react"
import Modal from "../components/Modal"
import Button from "../components/Button"
import Slider from "../components/Slider"
import { FaGear } from "react-icons/fa6"

const ConfigurationModal: React.FC = () => (
    <Modal name={"Configuration"} icon={<FaGear />}>
        <Button value={"Test"} />
        <Slider min={0} max={100} defaultValue={50} />
    </Modal>
)

export default ConfigurationModal
