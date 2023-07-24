import React from "react"
import Modal from "../components/Modal"
import Button from "../components/Button"
import Slider from "../components/Slider"

const ConfigurationModal: React.FC = () => (
    <Modal name={"Configuration"} icon="https://placeholder.co/512x512">
        <Button value={"Test"} />
        <Slider min={0} max={100} defaultValue={50} />
    </Modal>
)

export default ConfigurationModal
