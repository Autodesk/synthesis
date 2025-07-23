import { Button } from "@mui/material"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { Alliance } from "@/systems/preferences/PreferenceTypes"

type AllianceSelectionInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

const saveSetAlliance = (alliance: Alliance, assembly: MirabufSceneObject) => {
    assembly.alliance = alliance
}

export default function AllianceSelectionInterface({ selectedAssembly }: AllianceSelectionInterfaceProps) {
    const [alliance, setAlliance] = useState<Alliance>(selectedAssembly.alliance ?? "red")

    return (
        <div className="flex flex-col gap-2">
            <div>
                <Typography>Alliance: </Typography>
                <Button
                    value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                    onClick={() => {
                        setAlliance(alliance == "blue" ? "red" : "blue")
                        saveSetAlliance(alliance == "blue" ? "red" : "blue", selectedAssembly)
                    }}
                    colorOverrideClass={`bg-match-${alliance}-alliance`}
                />
            </div>
            <div>
                <Typography>Station: </Typography>
                <div className="flex gap-2">
                    <Button
                        value="1"
                        onClick={() => {
                            setStation(1)
                            saveSetStation(1, selectedAssembly)
                        }}
                        style={station === 1 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        1
                    </Button>
                    <Button
                        value="2"
                        onClick={() => {
                            setStation(2)
                            saveSetStation(2, selectedAssembly)
                        }}
                        style={station === 2 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        2
                    </Button>
                    <Button
                        value="3"
                        onClick={() => {
                            setStation(3)
                            saveSetStation(3, selectedAssembly)
                        }}
                        style={station === 3 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                    >
                        3
                    </Button>
                </div>
            </div>
        </div>
    )
}
