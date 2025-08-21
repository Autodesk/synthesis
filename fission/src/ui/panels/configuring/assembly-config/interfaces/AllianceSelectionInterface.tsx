import { Box, Stack } from "@mui/material"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import { Button } from "@/ui/components/StyledComponents"

type AllianceSelectionInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

const saveSetAlliance = (alliance: Alliance, assembly: MirabufSceneObject) => {
    assembly.alliance = alliance
}

const saveSetStation = (station: Station, assembly: MirabufSceneObject) => {
    assembly.station = station
}

const AllianceSelectionInterface: React.FC<AllianceSelectionInterfaceProps> = ({ selectedAssembly }) => {
    const [alliance, setAlliance] = useState<Alliance>(selectedAssembly.alliance ?? "red")
    const [station, setStation] = useState<Station>(selectedAssembly.station ?? 1)

    return (
        <Stack gap={2}>
            <Box>
                <Label size="md">Alliance: </Label>
                <Button
                    value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                    onClick={() => {
                        setAlliance(alliance == "blue" ? "red" : "blue")
                        saveSetAlliance(alliance == "blue" ? "red" : "blue", selectedAssembly)
                    }}
                    sx={{ bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" }}
                >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
            </Box>
            <div>
                <Label size="md">Station: </Label>
                <div className="flex gap-2">
                    <Button
                        value="1"
                        onClick={() => {
                            setStation(1)
                            saveSetStation(1, selectedAssembly)
                        }}
                        sx={station === 1 ? { bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        1
                    </Button>
                    <Button
                        value="2"
                        onClick={() => {
                            setStation(2)
                            saveSetStation(2, selectedAssembly)
                        }}
                        sx={station === 2 ? { bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        2
                    </Button>
                    <Button
                        value="3"
                        onClick={() => {
                            setStation(3)
                            saveSetStation(3, selectedAssembly)
                        }}
                        sx={station === 3 ? { bgcolor: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        3
                    </Button>
                </div>
            </div>
        </Stack>
    )
}

export default AllianceSelectionInterface
