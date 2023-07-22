import React from 'react';
import Modal from '../components/Modal';
import Label from '../components/Label';
import Stack, { StackDirection } from '../components/Stack';
import Button from '../components/Button';

const CreateDeviceModal: React.FC = () => (
    <Modal name="Create Device" icon="https://placeholder.co/512x512">
        <Label>Device Type</Label>
        <Stack direction={StackDirection.Horizontal}>
            <Button value="PWM" />
            <Button value="Encoder" />
        </Stack>
    </Modal>
)

export default CreateDeviceModal;
