import React from "react"
import Modal from "../components/Modal"
import Label from "../components/Label"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import { BsCodeSquare } from "react-icons/bs"

const CreateDeviceModal: React.FC = () => (
    <Modal name="Create Device" icon={<BsCodeSquare />}>
        <Label>Device Type</Label>
        <Stack direction={StackDirection.Horizontal}>
            <Button value="PWM" />
            <Button value="Encoder" />
        </Stack>
    </Modal>
)

export default CreateDeviceModal
