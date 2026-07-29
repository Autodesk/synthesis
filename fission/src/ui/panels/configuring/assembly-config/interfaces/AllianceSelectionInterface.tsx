import { Box, Stack } from "@mui/material"
import { useEffect, useState } from "react"

import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import { Button } from "@/ui/components/StyledComponents"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const AllianceSelectionInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const [alliance, setAlliance] = useState<Alliance>(selectedAssembly.alliance ?? "red")
    const [station, setStation] = useState<Station>(selectedAssembly.station ?? 1)

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
                    onClick={() => {
                        setAlliance(alliance == "blue" ? "red" : "blue")
                    }}
                    sx={{ bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" }}
                >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
            </Box>
            <div>
                <Label size="md">Station: </Label>
                <div className="flex gap-2">
                    {([1, 2, 3] as const).map(v => (
                        <Button
                            value={v}
                            key={v}
                            onClick={() => {
                                setStation(v)
                            }}
                            sx={station === v ? { bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
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
