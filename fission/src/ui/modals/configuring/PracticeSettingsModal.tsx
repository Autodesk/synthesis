import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import Stack, { StackDirection } from "@/components/Stack"
import Dropdown from "@/components/Dropdown"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const PracticeSettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    return (
        <Modal name="Practice Settings" icon={SynthesisIcons.GEAR} modalId={modalId}>
            <Label size={LabelSize.LARGE}>Gamepiece Spawning</Label>
            <Stack direction={StackDirection.HORIZONTAL}>
                <Dropdown options={["Sphere", "Cube", "Capsule"]} onSelect={() => {}} />
                <Button value="Spawn" />
            </Stack>
            <Button value="Gamepiece Spawnpoint" />
            <Label size={LabelSize.LARGE}>Reset</Label>
            <Stack direction={StackDirection.HORIZONTAL}>
                <Button value="Reset All" />
                <Button value="Reset Gamepieces" />
            </Stack>
        </Modal>
    )
}

export default PracticeSettingsModal
