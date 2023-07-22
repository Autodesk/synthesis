import React from 'react';
import Modal from '../components/Modal';
import LabeledButton, { LabelPlacement } from '../components/LabeledButton';
import { useModalControlContext } from '../ModalContext';

const RoboRIOModal: React.FC = () => {
    const { openModal, closeModal } = useModalControlContext();
    return (<Modal name="RoboRIO Configuration" icon="https://placeholder.co/512x512">
        <LabeledButton label="cbdbcc,ds,vsdv" value="Create Device" placement={LabelPlacement.Top} onClick={() => openModal("create-device")} />
    </Modal >)
}

export default RoboRIOModal;
