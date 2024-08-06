import Button, { ButtonSize } from "@/ui/components/Button"
import Modal, { ModalPropsImpl } from "@/ui/components/Modal"
import Stack, { StackDirection } from "@/ui/components/Stack"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"

export const ConfigureRobotModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openPanel } = usePanelControlContext()
    const { closeModal } = useModalControlContext()

    return (
        <Modal name={"Configure Robot"} icon={SynthesisIcons.Gear} modalId={modalId}>
            <Stack direction={StackDirection.Vertical}>
                <Button
                    value={"Intake"}
                    onClick={() => {
                        openPanel("config-gamepiece-pickup")
                        closeModal()
                    }}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
                <Button
                    value={"Ejector"}
                    onClick={() => {
                        openPanel("config-shot-trajectory")
                        closeModal()
                    }}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
            </Stack>
        </Modal>
    )
}

export default ConfigureRobotModal
