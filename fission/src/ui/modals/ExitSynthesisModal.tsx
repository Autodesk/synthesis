import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Label from "@/components/Label"
import { SynthesisIcons } from "../components/StyledComponents"

const ExitSynthesisModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const isOnMainMenu = false
    return (
        <Modal name={"Exit Synthesis?"} icon={SynthesisIcons.Xmark} modalId={modalId} acceptName="Exit">
            <Label>
                {isOnMainMenu ? "Are you sure you wish to Exit?" : "Are you sure you wish to leave to main menu?"}
            </Label>
        </Modal>
    )
}

export default ExitSynthesisModal
