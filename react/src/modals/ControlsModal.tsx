import React from 'react';
import Modal from '../components/Modal';
import LabeledButton, { LabelPlacement } from '../components/LabeledButton';

const ControlsModal: React.FC = () => (
    <Modal name={"Robot Controls"} icon="https://placeholder.co/512x512">
        <LabeledButton label={"Robot Forward"} value="W" placement={LabelPlacement.Right} />
        <LabeledButton label={"Robot Backward"} value="S" placement={LabelPlacement.Right} />
        <LabeledButton label={"Robot Left"} value="A" placement={LabelPlacement.Right} />
        <LabeledButton label={"Robot Right"} value="D" placement={LabelPlacement.Right} />
    </Modal>
)

export default ControlsModal;
