import React from "react"
import Modal, { ModalPropsImpl } from "../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import { useModalControlContext } from "../ModalContext"

const SpawningModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal name={"Spawning"} icon={<FaPlus />} modalId={modalId}>
            <Stack direction={StackDirection.Horizontal}>
                <Button value={"Robots"} onClick={() => openModal("robots")} />
                <Button value={"Fields"} onClick={() => openModal("fields")} />
            </Stack>
        </Modal>
    )
}

export default SpawningModal
