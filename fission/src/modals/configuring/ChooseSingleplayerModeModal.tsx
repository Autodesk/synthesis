import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaGear } from "react-icons/fa6"
import Button from "../../components/Button"

const ChooseSingleplayerModeModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name="Choose Mode" icon={<FaGear />} modalId={modalId} cancelEnabled={false} acceptEnabled={false}>
            <Button value="Practice Mode" />
            <Button value="Match Mode" />
        </Modal>
    )
}

export default ChooseSingleplayerModeModal
