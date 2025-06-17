import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import Button from "@/components/Button.tsx"
import { useModalControlContext } from "@/ui/ModalContext"

const MainMenuModal: React.FC<ModalPropsImpl & { startSingleplayerCallback: () => void }> = ({
    modalId,
    startSingleplayerCallback,
}) => {
    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Welcome"}
            icon={SynthesisIcons.Gamepad}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
        >
            <div className="flex flex-col">
                <Button
                    value={"Singleplayer"}
                    onClick={() => {
                        closeModal()
                        startSingleplayerCallback()
                    }}
                    className="w-full my-1"
                />
                <Button
                    value={"Multiplayer"}
                    onClick={() => {
                        // todo
                        alert("Multiplayer is not yet supported")
                        // closeModal()
                    }}
                    className="w-full mt-1 mb-3"
                />
            </div>
        </Modal>
    )
}

export default MainMenuModal
