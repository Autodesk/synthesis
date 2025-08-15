import { TextField } from "@mui/material"
import { useState } from "react"
import Checkbox from "@/ui/components/Checkbox"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ZoneConfigBase from "../zones/ZoneConfigBase"

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

const ZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    const [persistent, setPersistent] = useState<boolean>(selectedZone.persistentPoints)

    return (
        <ZoneConfigBase
            selectedField={selectedField}
            selectedZone={selectedZone}
            attachAndPersistZone={attachAndPersistZone}
            applyExtrasOnSave={zone => {
                zone.points = zone.points ?? selectedZone.points
                zone.destroyGamepiece = selectedZone.destroyGamepiece
                zone.persistentPoints = persistent
            }}
            removeZoneObject={(field, zone) => field.removeScoringZoneObject(zone)}
            saveAllZones={saveAllZones}
        >
            <TextField
                inputProps={{ type: "number" }}
                label="Points"
                placeholder="Zone points"
                defaultValue={selectedZone.points}
                onChange={v => {
                    const parsed = parseInt(v.target.value)
                    selectedZone.points = Number.isNaN(parsed) ? 1 : parsed
                }}
            />
            <Checkbox label="Persistent Points" checked={persistent} onClick={checked => setPersistent(checked)} />
        </ZoneConfigBase>
    )
}

export default ZoneConfigInterface
