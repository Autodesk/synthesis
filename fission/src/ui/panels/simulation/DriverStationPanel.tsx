import { MenuItem, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button, Select } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const DriverStationPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [enabled, setEnabled] = useState(false)

    useEffect(() => {
        // TODO: update Not Connected dynamically when implemented
        configureScreen(panel!, { title: "Driver Station (Not Connected)" }, {})
    }, [])

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
