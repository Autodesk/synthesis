import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Alliance } from "@/systems/preferences/PreferenceTypes"
import Button from "@/components/Button"
import { useState } from "react"

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
            value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
            onClick={() => {
                setAlliance(alliance == "blue" ? "red" : "blue")
                saveSetAlliance(alliance == "blue" ? "red" : "blue", selectedAssembly)
            }}
            colorOverrideClass={`bg-match-${alliance}-alliance`}
        />
    )
}
