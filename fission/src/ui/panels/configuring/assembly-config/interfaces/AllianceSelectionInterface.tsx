import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Alliance } from "@/systems/preferences/PreferenceTypes"
import Dropdown from "@/components/Dropdown"

type AllianceSelectionInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

const setAlliance = (alliance: Alliance, assembly: MirabufSceneObject) => {
    assembly.alliance = alliance
}

export default function AllianceSelectionInterface({ selectedAssembly }: AllianceSelectionInterfaceProps) {
    return (
        <Dropdown
            options={["red", "blue"]}
            defaultValue={selectedAssembly.alliance}
            onSelect={alliance => {
                setAlliance(alliance as "red" | "blue", selectedAssembly)
            }}
        />
    )
}
