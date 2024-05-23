import React from "react"
import Modal, { ModalPropsImpl } from "../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Stack, { StackDirection } from "../components/Stack"
import Button, { ButtonSize } from "../components/Button"
import { useModalControlContext } from "../ModalContext"

const SpawningModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal name={"Spawning"} icon={<FaPlus />} modalId={modalId}>
            <Stack direction={StackDirection.Vertical}>
                <Button
                    value={"Robots"}
                    onClick={() => openModal("add-robot")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
                <Button
                    value={"Fields"}
                    onClick={() => openModal("add-field")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
            </Stack>
        </Modal>
    )
}

export default SpawningModal
