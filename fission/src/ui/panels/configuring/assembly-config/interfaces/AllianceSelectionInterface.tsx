import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import Button from "@/components/Button"
import Label from "@/components/Label"
import React, { useState } from "react"

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
        <div className="flex flex-col gap-2">
            <div>
                <Label>Alliance: </Label>
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
                <Label>Station: </Label>
                <div className="flex gap-2">
                    <Button
                        value="1"
                        onClick={() => {
                            setStation(1)
                            saveSetStation(1, selectedAssembly)
                        }}
                        colorOverrideClass={station === 1 ? `bg-match-${alliance}-alliance` : ""}
                    />
                    <Button
                        value="2"
                        onClick={() => {
                            setStation(2)
                            saveSetStation(2, selectedAssembly)
                        }}
                        colorOverrideClass={station === 2 ? `bg-match-${alliance}-alliance` : ""}
                    />
                    <Button
                        value="3"
                        onClick={() => {
                            setStation(3)
                            saveSetStation(3, selectedAssembly)
                        }}
                        colorOverrideClass={station === 3 ? `bg-match-${alliance}-alliance` : ""}
                    />
                </div>
            </div>
        </div>
    )
}

export default AllianceSelectionInterface
