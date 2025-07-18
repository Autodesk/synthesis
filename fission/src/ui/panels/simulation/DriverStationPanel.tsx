import type React from "react"
import { useState } from "react"
import { Button, MenuItem, Select, Stack } from "@mui/material"

const DriverStationPanel: React.FC = () => {
    const [enabled, setEnabled] = useState(false)

    return (
        <Stack direction="row">
            <Button onClick={() => setEnabled(!enabled)}>{enabled ? "Enabled" : "Disabled"}</Button>
            <Select>
                <MenuItem value="Auto">Auto</MenuItem>
                <MenuItem value="Teleop">Teleop</MenuItem>
            </Select>
        </Stack>
    )
}

export default DriverStationPanel
