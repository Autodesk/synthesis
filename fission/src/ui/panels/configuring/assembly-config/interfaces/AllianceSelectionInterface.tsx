import { Box, Stack } from "@mui/material"
import { type MouseEvent, useCallback, useEffect, useState } from "react"

import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import { Button } from "@/ui/components/StyledComponents"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const AllianceSelectionInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const [alliance, setAlliance] = useState<Alliance>(selectedAssembly.alliance ?? "red")
    const [station, setStation] = useState<Station>(selectedAssembly.station ?? 1)
    const allianceColor = alliance === "red" ? "redAlliance.main" : "blueAlliance.main"
    const toggleAlliance = useCallback(() => {
        setAlliance(current => (current === "blue" ? "red" : "blue"))
    }, [])
    const selectStation = useCallback((event: MouseEvent<HTMLButtonElement>) => {
        const value = Number(event.currentTarget.value)
        if (value === 1 || value === 2 || value === 3) setStation(value)
    }, [])

    useEffect(() => {
        selectedAssembly.station = station
    }, [selectedAssembly, station])

    useEffect(() => {
        selectedAssembly.alliance = alliance
    }, [selectedAssembly, alliance])

    useEffect(() => {
        const originalAlliance = selectedAssembly.alliance
        const originalStation = selectedAssembly.station
        registerCleanupFunction(undefined, () => {
            selectedAssembly.alliance = originalAlliance
            selectedAssembly.station = originalStation
        })
    }, [registerCleanupFunction, selectedAssembly])
    return (
        <Stack gap={2}>
            <Box>
                <Label size="md">Alliance: </Label>
                <Button
                    value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                    onClick={toggleAlliance}
                    sx={{ bgcolor: allianceColor }}
                >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
            </Box>
            <div>
                <Label size="md">Station: </Label>
                <div className="flex gap-2">
                    {([1, 2, 3] as const).map(v => (
                        <Button
                            value={v}
                            key={v}
                            onClick={selectStation}
                            sx={station === v ? { bgcolor: allianceColor } : {}}
                        >
                            {v}
                        </Button>
                    ))}
                </div>
            </div>
        </Stack>
    )
}

export default AllianceSelectionInterface
