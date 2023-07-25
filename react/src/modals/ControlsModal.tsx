import React from "react"
import Modal, { ModalPropsImpl } from "../components/Modal"
import LabeledButton, { LabelPlacement } from "../components/LabeledButton"
import { IoGameControllerOutline } from "react-icons/io5"

const ControlsModal: React.FC<ModalPropsImpl> = ({ modalId }) => (
    <Modal name={"Robot Controls"} icon={<IoGameControllerOutline />} modalId={modalId}>
        <LabeledButton
            label={"Robot Forward"}
            value="W"
            placement={LabelPlacement.Right}
        />
        <LabeledButton
            label={"Robot Backward"}
            value="S"
            placement={LabelPlacement.Right}
        />
        <LabeledButton
            label={"Robot Left"}
            value="A"
            placement={LabelPlacement.Right}
        />
        <LabeledButton
            label={"Robot Right"}
            value="D"
            placement={LabelPlacement.Right}
        />
    </Modal>
)

export default ControlsModal
