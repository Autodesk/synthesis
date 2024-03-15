import { FaPlus } from "react-icons/fa6"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import Stack, { StackDirection } from "../../components/Stack"
import Button from "../../components/Button"
import { useModalControlContext } from "../../ModalContext"

const SpawningModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    return (
        <Modal
            name="Spawning"
            icon={<FaPlus />}
            modalId={modalId}
            acceptEnabled={false}
        >
            <Stack direction={StackDirection.Horizontal} spacing={8}>
                <Button value="Robot" onClick={() => openModal("add-robot")} />
                <Button value="Field" onClick={() => openModal("add-field")} />
            </Stack>
        </Modal>
    )
}

export default SpawningModal
