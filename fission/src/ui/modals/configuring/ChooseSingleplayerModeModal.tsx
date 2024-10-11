import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Button from "@/components/Button"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const ChooseSingleplayerModeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal
            name="Choose Mode"
            icon={SynthesisIcons.Gear}
            modalId={modalId}
            cancelEnabled={false}
            acceptEnabled={false}
        >
            <Button value="Practice Mode" />
            <Button value="Match Mode" />
        </Modal>
    )
}

export default ChooseSingleplayerModeModal
