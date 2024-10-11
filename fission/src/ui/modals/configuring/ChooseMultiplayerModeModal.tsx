import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Button from "@/components/Button"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const ChooseMultiplayerModeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal
            name="Choose Mode"
            icon={SynthesisIcons.Gear}
            modalId={modalId}
            cancelEnabled={false}
            acceptEnabled={false}
        >
            <Button value="Server Test Mode" />
            <Button value="Coming Soon" className="cursor-disabled" />
        </Modal>
    )
}

export default ChooseMultiplayerModeModal
