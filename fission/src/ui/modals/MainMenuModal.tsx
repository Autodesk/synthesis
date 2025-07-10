import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import Button from "@/components/Button.tsx"
import { useModalControlContext } from "../helpers/UseModalManager"
import { globalAddToast } from "@/components/GlobalUIControls.ts"

const MainMenuModal: React.FC<ModalPropsImpl & { startSingleplayerCallback: () => void }> = ({
    modalId,
    startSingleplayerCallback,
}) => {
    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Welcome"}
            icon={SynthesisIcons.GAMEPAD}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
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
                        globalAddToast?.("error", "Not Supported", "Multiplayer is not yet supported. Come back soon!")
                    }}
                    className="w-full mt-1 mb-3"
                />
            </div>
        </Modal>
    )
}

export default MainMenuModal
