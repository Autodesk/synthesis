import React, { useState } from "react"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { GiSteeringWheel } from "react-icons/gi"
import Stack, { StackDirection } from "../components/Stack"
import Button from "../components/Button"
import Dropdown from "../components/Dropdown"

const DriverStationPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [enabled, setEnabled] = useState(false)

    return (
        <Panel name="Driver Station (Not Connected)" icon={<GiSteeringWheel />} panelId={panelId}>
            <Stack direction={StackDirection.Horizontal}>
                <Button
                    value={enabled ? "Enabled" : "Disabled"}
                    onClick={() => setEnabled(!enabled)}
                />
                <Dropdown options={["Auto", "Teleop"]} />
            </Stack>
        </Panel>
    )
}

export default DriverStationPanel
