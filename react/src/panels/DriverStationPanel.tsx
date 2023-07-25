import React, { useState } from "react"
import Modal from "../components/Modal"
import { GiSteeringWheel } from "react-icons/gi"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import Dropdown from "../components/Dropdown"

const DriverStationPanel: React.FC = () => {
    const [enabled, setEnabled] = useState(false)

    return (
        <Modal name="Driver Station (Not Connected)" icon={<GiSteeringWheel />}>
            <Stack direction={StackDirection.Horizontal}>
                <Button
                    value={enabled ? "Enabled" : "Disabled"}
                    onClick={() => setEnabled(!enabled)}
                />
                <Dropdown options={["Auto", "Teleop"]} />
            </Stack>
        </Modal>
    )
}

export default DriverStationPanel
