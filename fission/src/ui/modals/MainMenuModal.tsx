import React from "react"
import Button from "@/components/Button.tsx"
import Modal, {ModalPropsImpl} from "@/components/Modal"
import {SynthesisIcons} from "../components/StyledComponents"
import {useModalControlContext} from "../helpers/UseModalManager"

const MainMenuModal: React.FC<ModalPropsImpl & {
    startSingleplayerCallback: () => void,
    startMultiplayerCallback: () => void
}> = ({
          modalId,
          startSingleplayerCallback,
          startMultiplayerCallback
      }) => {
    const {closeModal} = useModalControlContext()

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
                        closeModal()
                        startMultiplayerCallback()
                    }}
                    className="w-full mt-1 mb-3"
                />
            </div>
        </Modal>
    )
}

export default MainMenuModal
