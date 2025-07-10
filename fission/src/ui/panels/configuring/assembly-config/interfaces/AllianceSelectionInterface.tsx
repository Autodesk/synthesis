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
        <Button
            onChange={() => {
                setAlliance(alliance === "blue" ? "red" : "blue")
                saveSetAlliance(alliance === "blue" ? "red" : "blue", selectedAssembly)
            }}
            style={{ background: alliance === "red" ? "#ff0000" : "#0000ff" }}
        >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
    )
}
