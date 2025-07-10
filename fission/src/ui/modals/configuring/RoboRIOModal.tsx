import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import LabeledButton, { LabelPlacement } from "@/components/LabeledButton"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const RoboRIOModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    return (
        <Modal name="RoboRIO Configuration" icon={SynthesisIcons.CODE_SQUARE} modalId={modalId}>
            <LabeledButton
                label="cbdbcc,ds,vsdv"
                value="Create Device"
                placement={LabelPlacement.TOP}
                onClick={() => openModal("create-device")}
            />
        </Modal>
    )
}

export default RoboRIOModal
