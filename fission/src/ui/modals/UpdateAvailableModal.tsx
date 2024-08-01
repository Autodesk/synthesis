import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Label from "@/components/Label"
import { SynthesisIcons } from "../components/StyledComponents"

const UpdateAvailableModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name={"Exit Synthesis"} icon={SynthesisIcons.Xmark} modalId={modalId} acceptName="Update">
            <Label>
                A new update is available. <br />
                Would you like to update?
            </Label>
        </Modal>
    )
}

export default UpdateAvailableModal
