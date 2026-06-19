import { TextField } from "@mui/material"
import { useState, useCallback } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ZoneConfigBase from "../zones/ZoneConfigBase"
import Checkbox from "@/ui/components/Checkbox"

function attachAndPersistZone(zone: ScoringZonePreferences, field: MirabufSceneObject) {
    if (!field?.fieldPreferences) return
    if (!field.fieldPreferences.scoringZones.includes(zone)) field.fieldPreferences.scoringZones.push(zone)
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ScoringZonePreferences
    saveAllZones: () => void
}

const ScoringZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    const [points, setPoints] = useState<number>(selectedZone.points)
    const [accumulating, setAccumulating] = useState<boolean>(selectedZone.shouldPointsAccumulate)

    const applyExtrasOnSave = useCallback(
        (zone: ScoringZonePreferences) => {
            zone.points = points
            zone.shouldPointsAccumulate = accumulating
        },
        [points, accumulating]
    )

    const removeZoneObject = useCallback((field: MirabufSceneObject, zone: ScoringZonePreferences) => {
        field.removeScoringZoneObject(zone)
    }, [])

    return (
        <ZoneConfigBase
            selectedField={selectedField}
            selectedZone={selectedZone}
            attachAndPersistZone={attachAndPersistZone}
            applyExtrasOnSave={applyExtrasOnSave}
            removeZoneObject={removeZoneObject}
            saveAllZones={saveAllZones}
        >
            <TextField
                inputProps={{ type: "number" }}
                label="Points"
                placeholder="Zone points"
                defaultValue={selectedZone.points}
                onChange={v => setPoints(parseInt(v.target.value) || 0)}
            />
            <Checkbox
                label="Accumulating Points"
                tooltip="If disabled, gamepieces that exit the scoring zone will be subtracted from the score (for pick and place games)"
                checked={accumulating}
                onClick={checked => setAccumulating(checked)}
            />
        </ZoneConfigBase>
    )
}

export default ScoringZoneConfigInterface
