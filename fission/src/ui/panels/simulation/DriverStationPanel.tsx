import React, { useState } from "react"
import Button from "@/components/Button"
import Dropdown from "@/components/Dropdown"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Stack, { StackDirection } from "@/components/Stack"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

const DriverStationPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [enabled, setEnabled] = useState(false)

    return (
        <Panel
            name="Driver Station (Not Connected)"
            icon={SynthesisIcons.STEERING_WHEEL}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
        >
            <Stack direction={StackDirection.HORIZONTAL}>
                <Button value={enabled ? "Enabled" : "Disabled"} onClick={() => setEnabled(!enabled)} />
                <Dropdown options={["Auto", "Teleop"]} onSelect={() => {}} />
            </Stack>
        </Panel>
    )
}

export default DriverStationPanel
