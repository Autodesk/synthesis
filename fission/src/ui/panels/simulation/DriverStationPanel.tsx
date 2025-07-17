import type React from "react"
import { useState } from "react"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { PanelImplProps } from "@/ui/components/Panel"
import { Button, MenuItem, Select, Stack } from "@mui/material"

const DriverStationPanel: React.FC = () => {
    const [enabled, setEnabled] = useState(false)

    return (
        <Stack direction="row">
            <Button value={enabled ? "Enabled" : "Disabled"} onClick={() => setEnabled(!enabled)} />
            <Select>
                <MenuItem value="Auto">Auto</MenuItem>
                <MenuItem value="Teleop">Teleop</MenuItem>
            </Select>
        </Stack>
    )
}

export default DriverStationPanel
