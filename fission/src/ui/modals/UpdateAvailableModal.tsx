import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import Label from "@/components/Label"

const UpdateAvailableModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name={"Exit Synthesis"} icon={<GrFormClose />} modalId={modalId} acceptName="Update">
            <Label>
                A new update is available. <br />
                Would you like to update?
            </Label>
        </Modal>
    )
}

export default UpdateAvailableModal
