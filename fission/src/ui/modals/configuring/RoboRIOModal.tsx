import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import LabeledButton, { LabelPlacement } from "@/components/LabeledButton"
import { useModalControlContext } from "@/ui/ModalContext"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const RoboRIOModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    return (
        <Modal name="RoboRIO Configuration" icon={SynthesisIcons.CodeSquare} modalId={modalId}>
            <LabeledButton
                label="cbdbcc,ds,vsdv"
                value="Create Device"
                placement={LabelPlacement.Top}
                onClick={() => openModal("create-device")}
            />
        </Modal>
    )
}

export default RoboRIOModal
