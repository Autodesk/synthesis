import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ZoneConfigBase from "../zones/ZoneConfigBase"
import { TextField } from "@mui/material"
import { useState, useCallback } from "react"
import Checkbox from "@/ui/components/Checkbox"

/**
 * @param points Number of points the zone is worth.
 * @param destroy Destroy gamepiece setting.
 * @param persistent Persistent points setting.
 */

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
    const [persistent, setPersistent] = useState<boolean>(selectedZone.persistentPoints)

    const applyExtrasOnSave = useCallback(
        (zone: ScoringZonePreferences) => {
            zone.points = points
            zone.persistentPoints = persistent
        },
        [points, persistent]
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
                onChange={v => setPoints(parseInt(v.target.value) || 1)}
            />
            <Checkbox label="Persistent Points" checked={persistent} onClick={checked => setPersistent(checked)} />
        </ZoneConfigBase>
    )
}

export default ScoringZoneConfigInterface
