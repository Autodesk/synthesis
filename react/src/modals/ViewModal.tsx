import React from 'react'
import Modal from '../components/Modal'
import Dropdown from '../components/Dropdown'

const ViewModal: React.FC = () => (
    <Modal name={"Camera View"} icon="https://placeholder.co/512x512">
        <Dropdown options={["Orbit", "Freecam", "Overview"]} />
    </Modal>
)

export default ViewModal;
